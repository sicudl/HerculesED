﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Harvester.Models;
using Hercules.MA.ServicioExterno.Controllers.Utilidades;
using Newtonsoft.Json;
using OAI_PMH.Models.SGI.Organization;
using OAI_PMH.Models.SGI.PersonalData;
using OAI_PMH.Models.SGI.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Harvester
{
    public class Loader
    {
        private static Harvester harvester;
        private static IHarvesterServices harvesterServices;
        private static ReadConfig _Config;

        private static string RUTA_PREFIJOS = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Utilidades/prefijos.json";
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));

        //Resource API
        public static ResourceApi mResourceApi { get; set; }

        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="pResourceApi">ResourceAPI.</param>
        public Loader(ResourceApi pResourceApi)
        {
            harvesterServices = new IHarvesterServices();
            harvester = new Harvester(harvesterServices);
            _Config = new ReadConfig();
            mResourceApi = pResourceApi;
        }

        /// <summary>
        /// Carga las entidades principales.
        /// </summary>
        public void LoadMainEntities()
        {
            Dictionary<string, string> dicOrganizaciones = GetEntityBBDD("http://xmlns.com/foaf/0.1/Organization", "organization");
            Dictionary<string, string> dicPersonas = GetEntityBBDD("http://xmlns.com/foaf/0.1/Person", "person");
            Dictionary<string, string> dicProyectos = GetEntityBBDD("http://vivoweb.org/ontology/core#Project", "project");

            mResourceApi.ChangeOntoly("organization");
            ProcesarFichero(_Config, "Organizacion", dicOrganizaciones);
            mResourceApi.ChangeOntoly("person");
            ProcesarFichero(_Config, "Persona", dicPersonas);
            mResourceApi.ChangeOntoly("project");
            ProcesarFichero(_Config, "Proyecto", dicProyectos);

            string fecha = DateTime.Now.ToString("yyyy-MM-ddT00:00:00") + "Z";
            fecha = "2022-01-01T00:00:00Z";

            GuardarIdentificadores(_Config, "Organizacion", fecha);
            GuardarIdentificadores(_Config, "Persona", fecha);
            GuardarIdentificadores(_Config, "Proyecto", fecha);
            GuardarIdentificadores(_Config, "PRC", fecha, true);

            UpdateLastDate(_Config, fecha);

            mResourceApi.ChangeOntoly("organization");
            ProcesarFichero(_Config, "Organizacion", dicOrganizaciones);
            mResourceApi.ChangeOntoly("person");
            ProcesarFichero(_Config, "Persona", dicPersonas);
            mResourceApi.ChangeOntoly("project");
            ProcesarFichero(_Config, "Proyecto", dicProyectos);
            ProcesarFichero(_Config, "PRC", dicProyectos);
        }

        /// <summary>
        /// Obtiene los identificadores de los datos modificados.
        /// </summary>
        /// <param name="pConfig"></param>
        /// <param name="pSet"></param>
        /// <param name="pFecha"></param>
        public void GuardarIdentificadores(ReadConfig pConfig, string pSet, string pFecha, bool pPRC = false)
        {
            if (pPRC == false)
            {
                harvester.Harvest(pConfig, pSet, pFecha);
            }
            else
            {
                harvester.HarvestPRC(pConfig, pSet, pFecha);
            }
        }

        /// <summary>
        /// Obtiene los datos de los ficheros y los carga.
        /// </summary>
        /// <param name="pConfig"></param>
        /// <param name="pSet"></param>
        /// <param name="pDicRecursosCargados"></param>
        public void ProcesarFichero(ReadConfig pConfig, string pSet, Dictionary<string, string> pDicRecursosCargados)
        {
            string directorioPendientes = $@"{pConfig.GetLogCargas()}/{pSet}/pending/";
            string directorioProcesados = $@"{pConfig.GetLogCargas()}/{pSet}/processed/";

            if (Directory.Exists(directorioPendientes))
            {
                foreach (string fichero in Directory.EnumerateFiles(directorioPendientes))
                {
                    string ficheroProcesado = directorioProcesados + fichero.Substring(fichero.LastIndexOf("/"));
                    List<string> idsACargar = File.ReadAllLines(fichero).ToList();

                    if (File.Exists(ficheroProcesado))
                    {
                        List<string> listaIdsCargados = File.ReadAllLines(ficheroProcesado).ToList();
                        idsACargar = idsACargar.Except(listaIdsCargados).ToList();
                    }
                    idsACargar.Sort();

                    string xmlResult = string.Empty;
                    XmlSerializer xmlSerializer = null;
                    ComplexOntologyResource resource = null;

                    foreach (string id in idsACargar)
                    {
                        continue;
                        switch (pSet)
                        {
                            case "Organizacion":

                                // Obtención de datos en bruto.
                                Empresa organization = new Empresa();
                                xmlResult = harvesterServices.GetRecord(id);

                                if (string.IsNullOrEmpty(xmlResult))
                                {
                                    File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                    continue;
                                }

                                xmlSerializer = new(typeof(Empresa));
                                using (StringReader sr = new(xmlResult))
                                {
                                    organization = (Empresa)xmlSerializer.Deserialize(sr);
                                }

                                // Cambio de modelo. TODO: Mirar propiedades.
                                OrganizationOntology.Organization empresaOntology = CrearOrganizacion(organization);

                                resource = empresaOntology.ToGnossApiResource(mResourceApi, null);
                                if (pDicRecursosCargados.ContainsKey(empresaOntology.Roh_crisIdentifier))
                                {
                                    // Modificación.
                                    //mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                                }
                                else
                                {
                                    // Carga.                   
                                    //mResourceApi.LoadComplexSemanticResource(resource, false, false);
                                    //pDicRecursosCargados[empresaOntology.Roh_crisIdentifier] = resource.GnossId;
                                }

                                // Guardamos el ID cargado.
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                break;

                            case "Persona":

                                // Obtención de datos en bruto.
                                Persona persona = new Persona();
                                xmlResult = harvesterServices.GetRecord(id);

                                if (string.IsNullOrEmpty(xmlResult))
                                {
                                    File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                    continue;
                                }

                                xmlSerializer = new(typeof(Persona));
                                using (StringReader sr = new(xmlResult))
                                {
                                    persona = (Persona)xmlSerializer.Deserialize(sr);
                                }

                                // Cambio de modelo. TODO: Mirar propiedades.
                                PersonOntology.Person personOntology = CrearPersona(persona);

                                resource = personOntology.ToGnossApiResource(mResourceApi, null);
                                if (pDicRecursosCargados.ContainsKey(personOntology.Roh_crisIdentifier))
                                {
                                    // Modificación.
                                    //mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                                }
                                else
                                {
                                    // Carga.                   
                                    //mResourceApi.LoadComplexSemanticResource(resource, false, false);
                                    //pDicRecursosCargados[personOntology.Roh_crisIdentifier] = resource.GnossId;
                                }

                                // Guardamos el ID cargado.
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                break;

                            case "Proyecto":

                                // Obtención de datos en bruto.
                                Proyecto proyecto = new Proyecto();
                                xmlResult = harvesterServices.GetRecord(id);

                                if (string.IsNullOrEmpty(xmlResult))
                                {
                                    File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                    continue;
                                }

                                xmlSerializer = new(typeof(Proyecto));
                                using (StringReader sr = new(xmlResult))
                                {
                                    proyecto = (Proyecto)xmlSerializer.Deserialize(sr);
                                }

                                // Cambio de modelo. TODO: Mirar propiedades.
                                //ProjectOntology.Project projectOntology = CrearProyecto(proyecto);

                                //resource = projectOntology.ToGnossApiResource(mResourceApi, null);
                                //if (pDicRecursosCargados.ContainsKey(projectOntology.Roh_crisIdentifier))
                                //{
                                //    // Modificación.
                                //    mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                                //}
                                //else
                                //{
                                //    // Carga.                   
                                //    mResourceApi.LoadComplexSemanticResource(resource, false, false);
                                //    pDicRecursosCargados[projectOntology.Roh_crisIdentifier] = resource.GnossId;
                                //}

                                // Guardamos el ID cargado.
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                break;

                            case "PRC":
                                string idRecurso = id.Split("||")[0];
                                string estado = id.Split("||")[1];

                                Guid guid = mResourceApi.GetShortGuid(idRecurso);
                                Dictionary<string, string> data = GetValues(idRecurso);

                                foreach (KeyValuePair<string, string> item in data)
                                {
                                    if (!string.IsNullOrEmpty(item.Value))
                                    {
                                        if (item.Key == "projectAux")
                                        {
                                            //Borrado(guid, "http://w3id.org/roh/projectAux", item.Value);
                                        }
                                        else if (item.Key == "status")
                                        {
                                            //Modificacion(guid, "http://w3id.org/roh/validationStatusPRC", estado, item.Value);
                                        }
                                        else
                                        {
                                            switch (estado)
                                            {
                                                case "VALIDADO":
                                                    //Modificacion(guid, "http://w3id.org/roh/isValidated", "true", item.Value);
                                                    break;
                                                default:
                                                    //Modificacion(guid, "http://w3id.org/roh/isValidated", "false", item.Value);
                                                    break;
                                            }
                                        }
                                    }
                                }

                                // Guardamos el ID cargado.
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                break;
                        }

                        // Borra el fichero.
                        File.Delete(fichero);
                    }
                }
            }
        }

        private Dictionary<string, string> GetValues(string pIdRecurso)
        {
            Dictionary<string, string> dicResultados = new Dictionary<string, string>();
            dicResultados.Add("projectAux", "");
            dicResultados.Add("isValidated", "");
            dicResultados.Add("validationStatusPRC", ""); // TODO: Nombre de la propiedad

            string valorEnviado = string.Empty;
            StringBuilder select = new StringBuilder();
            StringBuilder where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?projectAux ?isValidated ?validationStatusPRC ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("OPTIONAL{?s roh:projectAux ?projectAux. } ");
            where.Append("OPTIONAL{?s roh:isValidated ?isValidated. } ");
            where.Append("OPTIONAL{?s roh:validationStatusPRC ?validationStatusPRC. } "); // TODO: Nombre de la propiedad
            where.Append($@"FILTER(?s = <{pIdRecurso}>) ");
            where.Append("} ");

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("projectAux"))
                    {
                        dicResultados["projectAux"] = UtilidadesAPI.GetValorFilaSparqlObject(fila, "projectAux");
                    }
                    if (fila.ContainsKey("isValidated"))
                    {
                        dicResultados["isValidated"] = UtilidadesAPI.GetValorFilaSparqlObject(fila, "isValidated");
                    }
                    if (fila.ContainsKey("validationStatusPRC"))
                    {
                        dicResultados["validationStatusPRC"] = UtilidadesAPI.GetValorFilaSparqlObject(fila, "validationStatusPRC");
                    }
                }
            }

            return dicResultados;
        }

        /// <summary>
        /// Inserta un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorNuevo"></param>
        private void Insercion(Guid pGuid, string pPropiedad, string pValorNuevo)
        {
            Dictionary<Guid, List<TriplesToInclude>> dicInsercion = new Dictionary<Guid, List<TriplesToInclude>>();
            List<TriplesToInclude> listaTriplesInsercion = new List<TriplesToInclude>();
            TriplesToInclude triple = new TriplesToInclude();
            triple.Predicate = pPropiedad;
            triple.NewValue = pValorNuevo;
            listaTriplesInsercion.Add(triple);
            dicInsercion.Add(pGuid, listaTriplesInsercion);
            mResourceApi.InsertPropertiesLoadedResources(dicInsercion);
        }

        /// <summary>
        /// Modifica un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorNuevo"></param>
        /// <param name="pValorAntiguo"></param>
        private void Modificacion(Guid pGuid, string pPropiedad, string pValorNuevo, string pValorAntiguo)
        {
            Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
            List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();
            TriplesToModify triple = new TriplesToModify();
            triple.Predicate = pPropiedad;
            triple.NewValue = pValorNuevo;
            triple.OldValue = pValorAntiguo;
            listaTriplesModificacion.Add(triple);
            dicModificacion.Add(pGuid, listaTriplesModificacion);
            mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
        }

        /// <summary>
        /// Borra un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorAntiguo"></param>
        private void Borrado(Guid pGuid, string pPropiedad, string pValorAntiguo)
        {
            Dictionary<Guid, List<RemoveTriples>> dicBorrado = new Dictionary<Guid, List<RemoveTriples>>();
            List<RemoveTriples> listaTriplesBorrado = new List<RemoveTriples>();
            RemoveTriples triple = new RemoveTriples();
            triple.Predicate = pPropiedad;
            triple.Value = pValorAntiguo;
            triple.Title = false;
            triple.Description = false;
            listaTriplesBorrado.Add(triple);
            dicBorrado.Add(pGuid, listaTriplesBorrado);
            mResourceApi.DeletePropertiesLoadedResources(dicBorrado);
        }

        /// <summary>
        /// Modifica el fichero con la última fecha.
        /// </summary>
        /// <param name="pConfig"></param>
        public void UpdateLastDate(ReadConfig pConfig, string pFecha)
        {
            File.WriteAllText(pConfig.GetLastUpdateDate(), pFecha);
        }

        /// <summary>
        /// Obtiene el ID del recurso junto a su CrisIdentifier.
        /// </summary>
        /// <param name="pOntology"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetEntityBBDD(string pRdfType, string pOntology)
        {
            Dictionary<string, string> dicResultados = new Dictionary<string, string>();

            int limit = 10000;
            int offset = 0;
            bool salirBucle = false;
            do
            {
                SparqlObject resultadoQuery = null;
                StringBuilder select = new StringBuilder(), where = new StringBuilder();

                // Consulta sparql.
                select.Append("SELECT ?s ?crisIdentifier ");
                where.Append("WHERE { ");
                where.Append($@"?s a <{pRdfType}>. ");
                where.Append("?s <http://w3id.org/roh/crisIdentifier> ?crisIdentifier. ");
                where.Append("} ");

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), pOntology);

                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    offset += limit;

                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        string id = fila["crisIdentifier"].value;
                        string nombre = fila["s"].value;
                        dicResultados.Add(id, nombre);
                    }

                    if (resultadoQuery.results.bindings.Count < limit)
                    {
                        salirBucle = true;
                    }
                }
                else
                {
                    salirBucle = true;
                }

            } while (!salirBucle);

            return dicResultados;
        }

        public static PersonOntology.Person CrearPersona(Persona pDatos)
        {
            PersonOntology.Person persona = new PersonOntology.Person();
            persona.Roh_crisIdentifier = pDatos.Id;
            persona.Roh_isSynchronized = true;
            if (!string.IsNullOrEmpty(pDatos.Nombre))
            {
                persona.Foaf_firstName = pDatos.Nombre;
            }
            if (!string.IsNullOrEmpty(pDatos.Apellidos))
            {
                persona.Foaf_lastName = pDatos.Apellidos;
            }
            if (!string.IsNullOrEmpty(pDatos.Nombre) && !string.IsNullOrEmpty(pDatos.Apellidos))
            {
                persona.Foaf_name = pDatos.Nombre + " " + pDatos.Apellidos;
            }
            if (pDatos.Emails != null && pDatos.Emails.Any())
            {
                persona.Vcard_email = new List<string>();
                foreach (Email item in pDatos.Emails)
                {
                    persona.Vcard_email.Add(item.email);
                }
            }
            if (pDatos.DatosContacto.Telefonos != null && pDatos.DatosContacto.Telefonos.Any())
            {
                persona.Vcard_hasTelephone = new List<string>();
                foreach (string item in pDatos.DatosContacto.Telefonos)
                {
                    persona.Vcard_hasTelephone.Add(item);
                }
            }
            if (pDatos.DatosContacto.Moviles != null && pDatos.DatosContacto.Moviles.Any())
            {
                //persona.Vcard_hasMobilePhone = new List<string>();
                foreach (string item in pDatos.DatosContacto.Moviles)
                {
                    //persona.Vcard_hasMobilePhone.Add(item);
                }
            }
            if (pDatos.Activo.HasValue)
            {
                persona.Roh_isActive = pDatos.Activo.Value;
            }
            if (pDatos.TipoDocumento != null && !string.IsNullOrEmpty(pDatos.TipoDocumento.Id))
            {
                switch (pDatos.TipoDocumento.Id)
                {
                    case "D":
                        //persona.Roh_dni = pDatos.TipoDocumento.Nombre;
                        break;
                    case "E":
                        //persona.Roh_nie = pDatos.TipoDocumento.Nombre;
                        break;
                }
            }
            if (pDatos.DatosPersonales != null)
            {
                //persona.Vcard_birthdate = pDatos.DatosPersonales.Fechanacimiento;
            }
            if (pDatos.Fotografia != null && !string.IsNullOrEmpty(pDatos.Fotografia.Contenido))
            {
                //persona.Foaf_img = pDatos.Fotografia.Contenido;
            }
            if (pDatos.Sexo != null && !string.IsNullOrEmpty(pDatos.Sexo.Id))
            {
                switch (pDatos.Sexo.Id)
                {
                    case "V":
                        //persona.IdFoaf_gender = $@"http://gnoss.com/items/gender_000";
                        break;
                    case "M":
                        //persona.IdFoaf_gender = $@"http://gnoss.com/items/gender_010";
                        break;
                }
            }

            // TODO: Posible cambio Treelogic
            if (pDatos.Vinculacion != null && pDatos.Vinculacion.Departamento != null && !string.IsNullOrEmpty(pDatos.Vinculacion.Departamento.Id))
            {
                persona.IdVivo_departmentOrSchool = $@"http://gnoss.com/items/department_{pDatos.Vinculacion.Departamento.Id}";
            }
            if (pDatos.Vinculacion != null && pDatos.Vinculacion.CategoriaProfesional != null && !string.IsNullOrEmpty(pDatos.Vinculacion.CategoriaProfesional.Nombre))
            {
                persona.Roh_hasPosition = pDatos.Vinculacion.CategoriaProfesional.Nombre;
            }

            return persona;
        }
        public static OrganizationOntology.Organization CrearOrganizacion(Empresa pDatos)
        {
            OrganizationOntology.Organization organization = new OrganizationOntology.Organization();
            organization.Roh_crisIdentifier = pDatos.Id;
            organization.Roh_isSynchronized = true;
            organization.Roh_title = pDatos.Nombre;
            return organization;
        }
        public static ProjectOntology.Project CrearProyecto(Proyecto pDatos, Dictionary<string, string> pDicPersonasGnossId)
        {
            ProjectOntology.Project project = new ProjectOntology.Project();
            project.Roh_crisIdentifier = pDatos.Id;
            //project.Roh_isSynchronized = true;

            if (!string.IsNullOrEmpty(pDatos.Titulo))
            {
                project.Roh_title = pDatos.Titulo;
            }
            if (!string.IsNullOrEmpty(pDatos.Observaciones))
            {
                project.Vivo_description = pDatos.Observaciones;
            }
            if (!string.IsNullOrEmpty(pDatos.Acronimo))
            {
                //project.Vivo_abbreviation = pDatos.Acronimo;
            }
            if (pDatos.Equipo != null && pDatos.Equipo.Any())
            {
                project.Vivo_relates = new List<ProjectOntology.BFO_0000023>();
                int orden = 1;
                foreach (ProyectoEquipo item in pDatos.Equipo)
                {
                    ProjectOntology.BFO_0000023 persona = new ProjectOntology.BFO_0000023();
                    persona.Rdf_comment = orden;
                    if (pDicPersonasGnossId != null && pDicPersonasGnossId.ContainsKey(item.PersonaRef))
                    {
                        //persona.IdRdf_member = pDicPersonasGnossId[item.PersonaRef];
                    }
                    //TODO: Fecha 
                    //persona.Vivo_start = item.FechaInicio;
                    //persona.Vivo_end = item.FechaFin;
                    project.Vivo_relates.Add(persona);
                    orden++;
                }
            }
            if (project.Vivo_relates != null && project.Vivo_relates.Any())
            {
                project.Roh_researchersNumber = project.Vivo_relates.Count();
            }
            else
            {
                project.Roh_researchersNumber = 0;
            }

            // TODO: Continuar el desarrollo...

            return project;
        }
    }
}