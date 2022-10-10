﻿using DepartmentOntology;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Harvester;
using Harvester.Models.ModelsBBDD;
using Harvester.Models.RabbitMQ;
using Newtonsoft.Json;
using OAI_PMH.Models.SGI.ActividadDocente;
using OAI_PMH.Models.SGI.FormacionAcademica;
using OAI_PMH.Models.SGI.Organization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Utilidades;

namespace OAI_PMH.Models.SGI.PersonalData
{
    /// <summary>
    /// Persona
    /// </summary>
    public class Persona : SGI_Base
    {
        private static string RUTA_PREFIJOS = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Utilidades/prefijos.json";
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));

        public override ComplexOntologyResource ToRecurso(IHarvesterServices pHarvesterServices, ReadConfig pConfig, ResourceApi pResourceApi, Dictionary<string, HashSet<string>> pDicIdentificadores, Dictionary<string, Dictionary<string, string>> pDicRutas, RabbitServiceWriterDenormalizer pRabbitConf, bool pFusionarPersona = false, string pIdPersona = null)
        {
            PersonOntology.Person persona = CrearPersonOntology(pRabbitConf, pHarvesterServices, pConfig, pResourceApi, pDicIdentificadores, pDicRutas);

            if (pFusionarPersona && !string.IsNullOrEmpty(pIdPersona))
            {
                PersonOntology.Person personaAux = DatosPersonaNoBorrar(pIdPersona, pResourceApi);
                FusionarPersonas(persona, personaAux);
            }

            return persona.ToGnossApiResource(pResourceApi, null);
        }

        public override string ObtenerIDBBDD(ResourceApi pResourceApi)
        {
            Dictionary<string, string> respuesta = ObtenerPersonasBBDD(new HashSet<string>() { Id.ToString() }, pResourceApi);
            if (respuesta.ContainsKey(Id.ToString()) && !string.IsNullOrEmpty(respuesta[Id.ToString()]))
            {
                return respuesta[Id.ToString()];
            }
            return null;
        }

        public static Dictionary<string, string> ObtenerPersonasBBDD(HashSet<string> pListaIds, ResourceApi pResourceApi)
        {
            List<List<string>> listasPersonas = SplitList(pListaIds.ToList(), 1000).ToList();
            Dictionary<string, string> dicPersonasBBDD = new Dictionary<string, string>();
            foreach (string persona in pListaIds)
            {
                dicPersonasBBDD[persona] = "";
            }
            foreach (List<string> listaItem in listasPersonas)
            {
                List<string> listaAux = new List<string>();
                foreach (string item in listaItem)
                {
                    if (item.Contains("_"))
                    {
                        listaAux.Add(item.Split("_")[1]);
                    }
                    else
                    {
                        listaAux.Add(item);
                    }
                }
                string selectPerson = $@"SELECT DISTINCT ?s ?crisIdentifier ";
                string wherePerson = $@"WHERE {{ 
                            ?s <http://w3id.org/roh/crisIdentifier> ?crisIdentifier. 
                            FILTER(?crisIdentifier in ('{string.Join("', '", listaAux.Select(x => x))}')) }}";
                SparqlObject resultadoQueryPerson = pResourceApi.VirtuosoQuery(selectPerson, wherePerson, "person");
                if (resultadoQueryPerson != null && resultadoQueryPerson.results != null && resultadoQueryPerson.results.bindings != null && resultadoQueryPerson.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQueryPerson.results.bindings)
                    {
                        dicPersonasBBDD[fila["crisIdentifier"].value] = fila["s"].value;
                    }
                }
            }

            return dicPersonasBBDD;
        }

        public static Persona GetPersonaSGI(IHarvesterServices pHarvesterServices, ReadConfig pConfig, string pId, Dictionary<string, Dictionary<string, string>> pDicRutas)
        {
            // Obtención de datos en bruto.
            Persona persona = new Persona();
            string xmlResult = pHarvesterServices.GetRecord(pId, pConfig);

            if (string.IsNullOrEmpty(xmlResult))
            {
                return null;
            }

            XmlSerializer xmlSerializer = new(typeof(Persona));
            using (StringReader sr = new(xmlResult))
            {
                persona = (Persona)xmlSerializer.Deserialize(sr);
            }

            return persona;
        }

        public PersonOntology.Person CrearPersonOntology(RabbitServiceWriterDenormalizer pRabbitConf, IHarvesterServices pHarvesterServices, ReadConfig pConfig, ResourceApi pResourceApi, Dictionary<string, HashSet<string>> pDicIdentificadores, Dictionary<string, Dictionary<string, string>> pDicRutas)
        {
            PersonOntology.Person persona = new PersonOntology.Person();

            // TESIS
            List<TesisBBDD> listaTesisBBDD = ObtenerTesisBBDD(pResourceApi, this.Id);
            List<TesisBBDD> listaTesisSGI = ObtenerTesisSGI(pRabbitConf, this.Tesis, pHarvesterServices, pConfig, pResourceApi, pDicIdentificadores, pDicRutas);

            // Cargar --> Tesis que estén en listaTesisSGI y no en listaTesisBBDD [Por CrisIdentifier]
            List<TesisBBDD> listaTesisCargar = listaTesisSGI.Where(x => !listaTesisBBDD.Any(y => x.crisIdentifier == y.crisIdentifier)).ToList();
            

            // Elimitar --> Tesis que estén en listaTesisBBDD y no en listaTesisSGI [Por CrisIdentifier]
            // Modificar --> Tesis que estén en listaTesisBBDD y en listaTesisSGI [Preguntar Álvaro Comparator]

            // Crisidentifier (Se corresponde al DNI sin letra)
            persona.Roh_crisIdentifier = this.Id;

            // Sincronización.
            persona.Roh_isSynchronized = true;

            // Nombre.
            if (!string.IsNullOrEmpty(this.Nombre))
            {
                persona.Foaf_firstName = this.Nombre;
            }

            // Apellidos.
            if (!string.IsNullOrEmpty(this.Apellidos))
            {
                persona.Foaf_lastName = this.Apellidos;
            }

            // Sexo.
            if (this.Sexo != null && !string.IsNullOrEmpty(this.Sexo.Id))
            {
                if (this.Sexo.Id == "V")
                {
                    persona.IdFoaf_gender = $@"{pResourceApi.GraphsUrl}items/gender_000";
                }
                else
                {
                    persona.IdFoaf_gender = $@"{pResourceApi.GraphsUrl}items/gender_010";
                }
            }

            // Nombre completo.
            if (!string.IsNullOrEmpty(this.Nombre) && !string.IsNullOrEmpty(this.Apellidos))
            {
                persona.Foaf_name = this.Nombre + " " + this.Apellidos;
            }

            // Correos.
            if (this.Emails != null && this.Emails.Any())
            {
                persona.Vcard_email = new List<string>();
                foreach (Email item in this.Emails)
                {
                    persona.Vcard_email.Add(item.email);
                }
            }

            // Dirección de contacto.
            if (!string.IsNullOrEmpty(this.DatosContacto?.PaisContacto?.Nombre) || !string.IsNullOrEmpty(this.DatosContacto?.ComAutonomaContacto?.Nombre)
                || !string.IsNullOrEmpty(this.DatosContacto?.CiudadContacto) || !string.IsNullOrEmpty(this.DatosContacto?.CodigoPostalContacto)
                || !string.IsNullOrEmpty(this.DatosContacto?.DireccionContacto))
            {
                string direccionContacto = string.IsNullOrEmpty(this.DatosContacto?.DireccionContacto) ? "" : this.DatosContacto.DireccionContacto;
                direccionContacto += string.IsNullOrEmpty(this.DatosContacto?.CodigoPostalContacto) ? "" : ", " + this.DatosContacto.CodigoPostalContacto;
                direccionContacto += string.IsNullOrEmpty(this.DatosContacto?.CiudadContacto) ? "" : ", " + this.DatosContacto.CiudadContacto;
                direccionContacto += string.IsNullOrEmpty(this.DatosContacto?.ProvinciaContacto?.Nombre) ? "" : ", " + this.DatosContacto.ProvinciaContacto.Nombre;
                direccionContacto += string.IsNullOrEmpty(this.DatosContacto?.PaisContacto?.Nombre) ? "" : ", " + this.DatosContacto.PaisContacto.Nombre;

                persona.Vcard_address = direccionContacto;
            }

            // Teléfonos.
            HashSet<string> telefonos = new HashSet<string>();
            if (this.DatosContacto?.Telefonos != null && this.DatosContacto.Telefonos.Any())
            {
                foreach (string item in this.DatosContacto.Telefonos)
                {
                    telefonos.Add(item);
                }
            }
            if (this.DatosContacto?.Moviles != null && this.DatosContacto.Moviles.Any())
            {
                foreach (string item in this.DatosContacto.Telefonos)
                {
                    telefonos.Add(item);
                }
            }
            persona.Vcard_hasTelephone = telefonos.ToList();

            // Activo.
            if (this.Activo.HasValue)
            {
                persona.Roh_isActive = this.Activo.Value;
            }

            // Departamentos.
            if (!string.IsNullOrEmpty(this.Vinculacion?.Departamento?.Id) && !string.IsNullOrEmpty(this.Vinculacion?.Departamento?.Nombre))
            {
                bool deptEncontrado = ComprobarDepartamentoBBDD(this.Vinculacion?.Departamento?.Id, pResourceApi);

                if (!deptEncontrado)
                {
                    // Si no existe, se carga el departamento como entidad secundaria.
                    CargarDepartment(this.Vinculacion.Departamento.Id, this.Vinculacion.Departamento.Nombre, pResourceApi);
                }

                persona.IdVivo_departmentOrSchool = $@"{pResourceApi.GraphsUrl}items/department_{this.Vinculacion.Departamento.Id}";
            }

            // Cargo en la universidad.
            if (!string.IsNullOrEmpty(this.Vinculacion?.VinculacionCategoriaProfesional?.categoriaProfesional?.nombre))
            {
                persona.Roh_hasPosition = this.Vinculacion?.VinculacionCategoriaProfesional?.categoriaProfesional?.nombre;
            }

            // Fecha de actualización.
            persona.Roh_lastUpdatedDate = DateTime.UtcNow;

            return persona;
        }

        /// <summary>
        /// Obtiene los datos que no queremos borrar de la persona.
        /// </summary>
        /// <param name="pIdRecurso"></param>
        /// <param name="pResourceApi"></param>
        /// <returns></returns>
        private static PersonOntology.Person DatosPersonaNoBorrar(string pIdRecurso, ResourceApi pResourceApi)
        {
            // Objeto Persona Final
            PersonOntology.Person persona = new PersonOntology.Person();
            persona.Roh_metricPage = new List<PersonOntology.MetricPage>();
            persona.Roh_ignorePublication = new List<PersonOntology.IgnorePublication>();

            HashSet<string> listaMetricPage = new HashSet<string>();
            HashSet<string> listaIgnorePublications = new HashSet<string>();

            List<MetricPageBBDD> listaMetricPagesBBDD = new List<MetricPageBBDD>();

            #region --- Datos de la Persona
            string selectPerson = $@"{mPrefijos} SELECT DISTINCT ?isOtriManager ?isGraphicManager ?ORCID ?scopusId ?researcherId ?semanticScholarId ?gnossUser ?usuarioFigShare ?tokenFigShare ?usuarioGitHub ?tokenGitHub ?metricPage ?useMatching ?ignorePublication ";
            string wherePerson = $@"WHERE {{ 
                            ?s a foaf:Person. 
                            OPTIONAL {{?s roh:isOtriManager ?isOtriManager. }}
                            OPTIONAL {{?s roh:isGraphicManager ?isGraphicManager. }} 
                            OPTIONAL {{?s roh:ORCID ?ORCID. }}
                            OPTIONAL {{?s vivo:scopusId ?scopusId. }}
                            OPTIONAL {{?s vivo:researcherId ?researcherId. }}
                            OPTIONAL {{?s roh:semanticScholarId ?semanticScholarId. }}
                            OPTIONAL {{?s roh:gnossUser ?gnossUser. }}
                            OPTIONAL {{?s roh:usuarioFigShare ?usuarioFigShare. }}
                            OPTIONAL {{?s roh:tokenFigShare ?tokenFigShare. }}
                            OPTIONAL {{?s roh:usuarioGitHub ?usuarioGitHub. }}
                            OPTIONAL {{?s roh:tokenGitHub ?tokenGitHub. }}
                            OPTIONAL {{?s roh:metricPage ?metricPage. }}
                            OPTIONAL {{?s roh:useMatching ?useMatching. }}
                            OPTIONAL {{?s roh:ignorePublication ?ignorePublication. }}
                            FILTER(?s = <{pIdRecurso}>)}} ";

            SparqlObject resultadoQueryPerson = pResourceApi.VirtuosoQuery(selectPerson, wherePerson, "person");

            if (resultadoQueryPerson != null && resultadoQueryPerson.results != null && resultadoQueryPerson.results.bindings != null && resultadoQueryPerson.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQueryPerson.results.bindings)
                {
                    if (fila.ContainsKey("isOtriManager") && !string.IsNullOrEmpty(fila["isOtriManager"].value))
                    {
                        persona.Roh_isOtriManager = bool.Parse(fila["isOtriManager"].value);
                    }
                    if (fila.ContainsKey("isGraphicManager") && !string.IsNullOrEmpty(fila["isGraphicManager"].value))
                    {
                        persona.Roh_isGraphicManager = bool.Parse(fila["isGraphicManager"].value);
                    }
                    if (fila.ContainsKey("ORCID") && !string.IsNullOrEmpty(fila["ORCID"].value))
                    {
                        persona.Roh_ORCID = fila["ORCID"].value;
                    }
                    if (fila.ContainsKey("scopusId") && !string.IsNullOrEmpty(fila["scopusId"].value))
                    {
                        persona.Vivo_scopusId = fila["scopusId"].value;
                    }
                    if (fila.ContainsKey("researcherId") && !string.IsNullOrEmpty(fila["researcherId"].value))
                    {
                        persona.Vivo_researcherId = fila["researcherId"].value;
                    }
                    if (fila.ContainsKey("semanticScholarId") && !string.IsNullOrEmpty(fila["semanticScholarId"].value))
                    {
                        persona.Roh_semanticScholarId = fila["semanticScholarId"].value;
                    }
                    if (fila.ContainsKey("gnossUser") && !string.IsNullOrEmpty(fila["gnossUser"].value))
                    {
                        persona.IdRoh_gnossUser = fila["gnossUser"].value;
                    }
                    if (fila.ContainsKey("usuarioFigShare") && !string.IsNullOrEmpty(fila["usuarioFigShare"].value))
                    {
                        persona.Roh_usuarioFigShare = fila["usuarioFigShare"].value;
                    }
                    if (fila.ContainsKey("tokenFigShare") && !string.IsNullOrEmpty(fila["tokenFigShare"].value))
                    {
                        persona.Roh_tokenFigShare = fila["tokenFigShare"].value;
                    }
                    if (fila.ContainsKey("usuarioGitHub") && !string.IsNullOrEmpty(fila["usuarioGitHub"].value))
                    {
                        persona.Roh_usuarioGitHub = fila["usuarioGitHub"].value;
                    }
                    if (fila.ContainsKey("tokenGitHub") && !string.IsNullOrEmpty(fila["tokenGitHub"].value))
                    {
                        persona.Roh_tokenGitHub = fila["tokenGitHub"].value;
                    }
                    if (fila.ContainsKey("metricPage") && !string.IsNullOrEmpty(fila["metricPage"].value))
                    {
                        listaMetricPage.Add(fila["metricPage"].value);
                    }
                    if (fila.ContainsKey("useMatching") && !string.IsNullOrEmpty(fila["useMatching"].value))
                    {
                        persona.Roh_useMatching = bool.Parse(fila["useMatching"].value);
                    }
                    if (fila.ContainsKey("ignorePublication") && !string.IsNullOrEmpty(fila["ignorePublication"].value))
                    {
                        listaIgnorePublications.Add(fila["ignorePublication"].value);
                    }
                }
            }
            #endregion

            #region --- Datos del IgnorePublication
            string selectIgnorePublication = $@"{mPrefijos} SELECT DISTINCT ?id ?tipo ";
            string whereIgnorePublication = $@"WHERE {{ 
                            ?s a roh:IgnorePublication. 
                            ?s roh:title ?id. 
                            ?s foaf:topic ?tipo. 
                            FILTER(?s in (<{string.Join(">, <", listaIgnorePublications.Select(x => x))}>))}} ";

            SparqlObject resultadoQueryIgnorePublications = pResourceApi.VirtuosoQuery(selectIgnorePublication, whereIgnorePublication, "person");

            if (resultadoQueryIgnorePublications != null && resultadoQueryIgnorePublications.results != null && resultadoQueryIgnorePublications.results.bindings != null && resultadoQueryIgnorePublications.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQueryIgnorePublications.results.bindings)
                {
                    PersonOntology.IgnorePublication ignorePub = new PersonOntology.IgnorePublication();
                    ignorePub.Foaf_topic = fila["tipo"].value;
                    ignorePub.Roh_title = fila["id"].value;
                    persona.Roh_ignorePublication.Add(ignorePub);
                }
            }
            #endregion

            #region --- Datos del MetricPage
            string selecMetricPage = $@"{mPrefijos} SELECT DISTINCT ?order ?title ?metricGraphic ";
            string whereMetricPage = $@"WHERE {{ 
                            ?s a roh:MetricPage. 
                            ?s roh:order ?order. 
                            ?s roh:title ?title. 
                            OPTIONAL {{?s roh:metricGraphic ?metricGraphic. }}
                            FILTER(?s in (<{string.Join(">, <", listaMetricPage.Select(x => x))}>))}} ";

            SparqlObject resultadoQueryMetricPage = pResourceApi.VirtuosoQuery(selecMetricPage, whereMetricPage, "person");

            if (resultadoQueryMetricPage != null && resultadoQueryMetricPage.results != null && resultadoQueryMetricPage.results.bindings != null && resultadoQueryMetricPage.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQueryMetricPage.results.bindings)
                {
                    MetricPageBBDD metricPage = new MetricPageBBDD();
                    metricPage.listaGraficas = new List<PersonOntology.MetricGraphic>();
                    metricPage.order = Int32.Parse(fila["order"].value);
                    metricPage.title = fila["title"].value;

                    if (fila.ContainsKey("metricGraphic") && !string.IsNullOrEmpty(fila["metricGraphic"].value))
                    {
                        if (listaMetricPagesBBDD.Any(x => x.order == Int32.Parse(fila["order"].value) && x.title == fila["title"].value))
                        {
                            listaMetricPagesBBDD.First(x => x.order == Int32.Parse(fila["order"].value) && x.title == fila["title"].value).idsMetricGraphic.Add(fila["metricGraphic"].value);
                        }
                        else
                        {
                            metricPage.idsMetricGraphic = new List<string>() { fila["metricGraphic"].value };
                            listaMetricPagesBBDD.Add(metricPage);
                        }
                    }
                    else
                    {
                        listaMetricPagesBBDD.Add(metricPage);
                    }
                }
            }
            #endregion

            #region --- MetricGraphic
            foreach (MetricPageBBDD item in listaMetricPagesBBDD)
            {
                string selectMetricGraphic = $@"{mPrefijos} SELECT DISTINCT ?title ?order ?pageId ?graphicId ?filters ?width ?scales ";
                string whereMetricGraphic = $@"WHERE {{ 
                            ?s a roh:MetricGraphic.
                            ?s roh:title ?title.
                            ?s roh:order ?order.
                            ?s roh:pageId ?pageId.
                            ?s roh:graphicId ?graphicId.
                            OPTIONAL{{ ?s roh:filters ?filters. }}
                            ?s roh:width ?width.
                            OPTIONAL{{ ?s roh:scales ?scales. }}
                            FILTER(?s in (<{string.Join(">, <", item.idsMetricGraphic.Select(x => x))}>))}} ";

                SparqlObject resultadoQueryMetricGraphic = pResourceApi.VirtuosoQuery(selectMetricGraphic, whereMetricGraphic, "person");

                if (resultadoQueryMetricGraphic != null && resultadoQueryMetricGraphic.results != null && resultadoQueryMetricGraphic.results.bindings != null && resultadoQueryMetricGraphic.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQueryMetricGraphic.results.bindings)
                    {
                        PersonOntology.MetricGraphic metricGraphic = new PersonOntology.MetricGraphic();
                        metricGraphic.Roh_title = fila["title"].value;
                        metricGraphic.Roh_order = Int32.Parse(fila["order"].value);
                        metricGraphic.Roh_pageId = fila["pageId"].value;
                        metricGraphic.Roh_graphicId = fila["graphicId"].value;
                        metricGraphic.Roh_width = fila["width"].value;

                        if (fila.ContainsKey("filters") && !string.IsNullOrEmpty(fila["filters"].value))
                        {
                            metricGraphic.Roh_filters = fila["filters"].value;
                        }

                        if (fila.ContainsKey("scales") && !string.IsNullOrEmpty(fila["scales"].value))
                        {
                            metricGraphic.Roh_scales = fila["scales"].value;
                        }

                        item.listaGraficas.Add(metricGraphic);
                    }
                }
            }
            #endregion

            // Construcción de objetos finales.
            foreach (MetricPageBBDD item in listaMetricPagesBBDD)
            {
                PersonOntology.MetricPage metricPage = new PersonOntology.MetricPage();
                metricPage.Roh_order = item.order;
                metricPage.Roh_title = item.title;
                metricPage.Roh_metricGraphic = item.listaGraficas;
                persona.Roh_metricPage.Add(metricPage);
            }

            return persona;
        }

        /// <summary>
        /// Junta la información de BBDD con la persona obtenida del SGI.
        /// </summary>
        /// <param name="pPersonaSGI">Persona del SGI.</param>
        /// <param name="pPersonaBBDD">Persona de BBDD.</param>
        public void FusionarPersonas(PersonOntology.Person pPersonaSGI, PersonOntology.Person pPersonaBBDD)
        {
            pPersonaSGI.Roh_isOtriManager = pPersonaBBDD.Roh_isOtriManager;
            pPersonaSGI.Roh_isGraphicManager = pPersonaBBDD.Roh_isGraphicManager;
            pPersonaSGI.Roh_ORCID = pPersonaBBDD.Roh_ORCID;
            pPersonaSGI.Vivo_scopusId = pPersonaBBDD.Vivo_scopusId;
            pPersonaSGI.Vivo_researcherId = pPersonaBBDD.Vivo_researcherId;
            pPersonaSGI.Roh_semanticScholarId = pPersonaBBDD.Roh_semanticScholarId;
            pPersonaSGI.IdRoh_gnossUser = pPersonaBBDD.IdRoh_gnossUser;
            pPersonaSGI.Roh_usuarioFigShare = pPersonaBBDD.Roh_usuarioFigShare;
            pPersonaSGI.Roh_tokenFigShare = pPersonaBBDD.Roh_tokenFigShare;
            pPersonaSGI.Roh_usuarioGitHub = pPersonaBBDD.Roh_usuarioGitHub;
            pPersonaSGI.Roh_tokenGitHub = pPersonaBBDD.Roh_tokenGitHub;
            pPersonaSGI.Roh_metricPage = pPersonaBBDD.Roh_metricPage;
            pPersonaSGI.Roh_useMatching = pPersonaBBDD.Roh_useMatching;
            pPersonaSGI.Roh_ignorePublication = pPersonaBBDD.Roh_ignorePublication;
        }

        private static void CargarDepartment(string pCodigoDept, string pNombreDept, ResourceApi pResourceApi)
        {
            string ontology = "department";

            // Cambio de ontología.
            pResourceApi.ChangeOntoly(ontology);

            // Creación del objeto a cargar.
            Department dept = new Department();
            dept.Dc_identifier = pCodigoDept;
            dept.Dc_title = pNombreDept;

            // Carga.
            var cargado = pResourceApi.LoadSecondaryResource(dept.ToGnossApiResource(pResourceApi, ontology + "_" + dept.Dc_identifier));
        }

        private static bool ComprobarDepartamentoBBDD(string pIdentificadorDept, ResourceApi pResourceApi)
        {
            string idSecundaria = $@"http://gnoss.com/items/department_{pIdentificadorDept}";

            SparqlObject resultadoQuery = null;

            // Consulta sparql.
            string select = "SELECT * ";
            string where = $@"WHERE {{ 
                                <{idSecundaria}> ?p ?o. 
                            }}";

            resultadoQuery = pResourceApi.VirtuosoQuery(select, where, "department");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                return true;
            }

            return false;
        }

        public List<TesisBBDD> ObtenerTesisBBDD(ResourceApi pResourceApi, string pCrisIdentfierPerson)
        {
            List<TesisBBDD> listaTesis = new List<TesisBBDD>();
            Dictionary<string, List<string>> dicCodirectores = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> dicCategoryPaths = new Dictionary<string, List<string>>();

            string select = string.Empty;
            string where = string.Empty;
            SparqlObject resultadoQuery = null;

            #region --- Obtención datos básicos Tesis
            select = "SELECT * ";
            where = $@"WHERE {{ 
                                ?s <http://w3id.org/roh/owner> ?persona.
                                ?s <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                ?s <http://w3id.org/roh/title> ?title.
                                OPTIONAL {{ ?s <http://purl.org/dc/terms/issued> ?issued. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/studentNick> ?studentNick. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/studentName> ?studentName. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/studentFirstSurname> ?studentFirstSurname. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/studentSecondSurname> ?studentSecondSurname. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/promotedByTitle> ?promotedByTitle. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/promotedBy> ?promotedBy. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/promotedByType> ?promotedByType. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/promotedByTypeOther> ?promotedByTypeOther. }}
                                OPTIONAL {{ ?s <http://www.w3.org/2006/vcard/ns#locality> ?locality. }}
                                OPTIONAL {{ ?s <http://www.w3.org/2006/vcard/ns#hasCountryName> ?hasCountryName. }}
                                OPTIONAL {{ ?s <http://www.w3.org/2006/vcard/ns#hasRegion> ?hasRegion. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/projectCharacterType> ?projectCharacterType. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/projectCharacterTypeOther> ?projectCharacterTypeOther. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/codirector> ?codirector. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/qualification> ?qualification. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/qualityMention> ?qualityMention. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/qualityMentionDate> ?qualityMentionDate. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/europeanDoctorate> ?europeanDoctorate. }}
                                OPTIONAL {{ ?s <http://w3id.org/roh/europeanDoctorateDate> ?europeanDoctorateDate. }}
                                OPTIONAL {{ ?s <http://vivoweb.org/ontology/core#freeTextKeyword> ?freeTextKeyword. }}
                                ?persona <http://w3id.org/roh/crisIdentifier> '{pCrisIdentfierPerson}'.
                            }}";

            resultadoQuery = pResourceApi.VirtuosoQueryMultipleGraph(select, where, new List<string>() { "person", "thesissupervision" });
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    TesisBBDD tesis = new TesisBBDD();
                    tesis.idGnoss = fila["s"].value;
                    tesis.crisIdentifier = fila["s"].value;
                    tesis.title = fila["title"].value;
                    if (fila.ContainsKey("issued") && !string.IsNullOrEmpty(fila["issued"].value))
                    {
                        int anio = int.Parse(fila["issued"].value.Substring(0, 4));
                        int mes = int.Parse(fila["issued"].value.Substring(4, 2));
                        int dia = int.Parse(fila["issued"].value.Substring(6, 2));
                        tesis.issued = new DateTime(anio, mes, dia, 0, 0, 0, DateTimeKind.Utc);
                    }
                    if (fila.ContainsKey("studentNick") && !string.IsNullOrEmpty(fila["studentNick"].value))
                    {
                        tesis.studentNick = fila["studentNick"].value;
                    }
                    if (fila.ContainsKey("studentName") && !string.IsNullOrEmpty(fila["studentName"].value))
                    {
                        tesis.studentName = fila["studentName"].value;
                    }
                    if (fila.ContainsKey("studentFirstSurname") && !string.IsNullOrEmpty(fila["studentFirstSurname"].value))
                    {
                        tesis.studentFirstSurname = fila["studentFirstSurname"].value;
                    }
                    if (fila.ContainsKey("studentSecondSurname") && !string.IsNullOrEmpty(fila["studentSecondSurname"].value))
                    {
                        tesis.studentSecondSurname = fila["studentSecondSurname"].value;
                    }
                    if (fila.ContainsKey("promotedByTitle") && !string.IsNullOrEmpty(fila["promotedByTitle"].value))
                    {
                        tesis.promotedByTitle = fila["promotedByTitle"].value;
                    }
                    if (fila.ContainsKey("promotedBy") && !string.IsNullOrEmpty(fila["promotedBy"].value))
                    {
                        tesis.promotedBy = fila["promotedBy"].value;
                    }
                    if (fila.ContainsKey("promotedByType") && !string.IsNullOrEmpty(fila["promotedByType"].value))
                    {
                        tesis.promotedByType = fila["promotedByType"].value;
                    }
                    if (fila.ContainsKey("promotedByTypeOther") && !string.IsNullOrEmpty(fila["promotedByTypeOther"].value))
                    {
                        tesis.promotedByTypeOther = fila["promotedByTypeOther"].value;
                    }
                    if (fila.ContainsKey("codirector") && !string.IsNullOrEmpty(fila["codirector"].value))
                    {
                        if (dicCodirectores.ContainsKey(fila["s"].value))
                        {
                            dicCodirectores[fila["s"].value].Add(fila["codirector"].value);
                        }
                        else
                        {
                            tesis.codirector = new List<Codirector>();
                            dicCodirectores[fila["s"].value] = new List<string>() { fila["codirector"].value };
                        }
                    }
                    if (fila.ContainsKey("locality") && !string.IsNullOrEmpty(fila["locality"].value))
                    {
                        tesis.locality = fila["locality"].value;
                    }
                    if (fila.ContainsKey("hasCountryName") && !string.IsNullOrEmpty(fila["hasCountryName"].value))
                    {
                        tesis.hasCountryName = fila["hasCountryName"].value;
                    }
                    if (fila.ContainsKey("hasRegion") && !string.IsNullOrEmpty(fila["hasRegion"].value))
                    {
                        tesis.hasRegion = fila["hasRegion"].value;
                    }
                    if (fila.ContainsKey("projectCharacterType") && !string.IsNullOrEmpty(fila["projectCharacterType"].value))
                    {
                        tesis.projectCharacterType = fila["projectCharacterType"].value;
                    }
                    if (fila.ContainsKey("projectCharacterTypeOther") && !string.IsNullOrEmpty(fila["projectCharacterTypeOther"].value))
                    {
                        tesis.projectCharacterTypeOther = fila["projectCharacterTypeOther"].value;
                    }
                    if (fila.ContainsKey("qualityMention") && !string.IsNullOrEmpty(fila["qualityMention"].value))
                    {
                        tesis.qualityMention = bool.Parse(fila["qualityMention"].value);
                    }
                    if (fila.ContainsKey("qualityMentionDate") && !string.IsNullOrEmpty(fila["qualityMentionDate"].value))
                    {
                        int anio = int.Parse(fila["qualityMentionDate"].value.Substring(0, 4));
                        int mes = int.Parse(fila["qualityMentionDate"].value.Substring(4, 2));
                        int dia = int.Parse(fila["qualityMentionDate"].value.Substring(6, 2));
                        tesis.qualityMentionDate = new DateTime(anio, mes, dia, 0, 0, 0, DateTimeKind.Utc);
                    }
                    if (fila.ContainsKey("europeanDoctorate") && !string.IsNullOrEmpty(fila["europeanDoctorate"].value))
                    {
                        tesis.europeanDoctorate = bool.Parse(fila["europeanDoctorate"].value);
                    }
                    if (fila.ContainsKey("europeanDoctorateDate") && !string.IsNullOrEmpty(fila["europeanDoctorateDate"].value))
                    {
                        int anio = int.Parse(fila["europeanDoctorateDate"].value.Substring(0, 4));
                        int mes = int.Parse(fila["europeanDoctorateDate"].value.Substring(4, 2));
                        int dia = int.Parse(fila["europeanDoctorateDate"].value.Substring(6, 2));
                        tesis.europeanDoctorateDate = new DateTime(anio, mes, dia, 0, 0, 0, DateTimeKind.Utc);
                    }
                    if (fila.ContainsKey("freeTextKeyword") && !string.IsNullOrEmpty(fila["freeTextKeyword"].value))
                    {
                        if (dicCategoryPaths.ContainsKey(fila["s"].value))
                        {
                            dicCategoryPaths[fila["s"].value].Add(fila["freeTextKeyword"].value);
                        }
                        else
                        {
                            tesis.freeTextKeyword = new List<string>();
                            dicCategoryPaths[fila["s"].value] = new List<string>() { fila["freeTextKeyword"].value };
                        }
                    }
                    listaTesis.Add(tesis);
                }
            }
            #endregion

            #region --- Obtención de los datos de codirectores
            foreach (KeyValuePair<string, List<string>> item in dicCodirectores)
            {
                select = "SELECT * ";
                where = $@"WHERE {{
                        ?s <http://www.w3.org/1999/02/22-rdf-syntax-ns#comment> ?comment.
                        ?s <http://xmlns.com/foaf/0.1/nick> ?nick.
                        ?s <http://xmlns.com/foaf/0.1/firtName> ?firstName.
                        ?s <http://xmlns.com/foaf/0.1/familyName> ?familyName.
                        ?s <http://w3id.org/roh/secondFamilyName> ?secondFamilyName.
                        FILTER(?s in (<{string.Join(">, <", item.Value.Select(x => x))}>))}}
                    }}";

                resultadoQuery = pResourceApi.VirtuosoQuery(select, where, "thesissupervision");
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        Codirector codirector = new Codirector();
                        codirector.comment = Int32.Parse(fila["comment"].value);
                        codirector.firstName = fila["firstName"].value;

                        if (fila.ContainsKey("nick") && !string.IsNullOrEmpty(fila["nick"].value))
                        {
                            codirector.nick = fila["nick"].value;
                        }
                        if (fila.ContainsKey("familyName") && !string.IsNullOrEmpty(fila["familyName"].value))
                        {
                            codirector.familyName = fila["familyName"].value;
                        }
                        if (fila.ContainsKey("secondFamilyName") && !string.IsNullOrEmpty(fila["secondFamilyName"].value))
                        {
                            codirector.secondFamilyName = fila["secondFamilyName"].value;
                        }

                        listaTesis.First(x => x.idGnoss == item.Key).codirector.Add(codirector);
                    }
                }
            }
            #endregion

            return listaTesis;
        }

        public List<TesisBBDD> ObtenerTesisSGI(RabbitServiceWriterDenormalizer pRabbitConf, List<Tesis> pListaTesisSGI, IHarvesterServices pHarvesterServices, ReadConfig pConfig, ResourceApi pResourceApi, Dictionary<string, HashSet<string>> pDicIdentificadores, Dictionary<string, Dictionary<string, string>> pDicRutas)
        {
            List<TesisBBDD> listaTesis = new List<TesisBBDD>();

            foreach (Tesis item in pListaTesisSGI)
            {
                TesisBBDD tesis = new TesisBBDD();

                tesis.crisIdentifier = item.Id;

                if (item.TipoProyecto != null && !string.IsNullOrEmpty(item.TipoProyecto.Nombre))
                {
                    tesis.projectCharacterType = ProjectCharacterType(pResourceApi, item.TipoProyecto.Nombre);
                    if (tesis.projectCharacterType.Contains("OTHERS"))
                    {
                        tesis.projectCharacterTypeOther = item.TipoProyecto.Nombre;
                    }
                }

                tesis.title = item.TituloTrabajo;
                // TODO: Código del pais no mapeable.
                //tesis.hasCountryName = IdentificadorPais(item.PaisEntidadRealizacion.Id, pResourceApi);
                //tesis.hasRegion = IdentificadorRegion(item.CcaaRegionEntidadRealizacion.Id, pResourceApi);
                //tesis.locality = item.CiudadEntidadRealizacion;
                tesis.issued = item.FechaDefensa;
                tesis.qualification = item.CalificacionObtenida;
                tesis.europeanDoctorateDate = item.FechaMencionDoctoradoEuropeo;
                tesis.qualityMention = item.MencionCalidad != null ? (bool)item.MencionCalidad : false;
                tesis.europeanDoctorate = item.DoctoradoEuropeo != null ? (bool)item.DoctoradoEuropeo : false;
                tesis.qualityMentionDate = item.FechaMencionCalidad;

                Persona alumno = GetPersonaSGI(pHarvesterServices, pConfig, "Persona_" + item.Alumno, pDicRutas);
                if (alumno != null)
                {
                    tesis.studentName = alumno.Nombre;
                    tesis.studentFirstSurname = alumno.Apellidos;
                    tesis.studentNick = alumno.Nombre + " " + alumno.Apellidos;
                }

                // ORGANIZACION
                if (item.EntidadRealizacion != null && !string.IsNullOrEmpty(item.EntidadRealizacion.EntidadRef))
                {
                    Dictionary<string, string> dicOrganizaciones = Empresa.ObtenerOrganizacionesBBDD(new HashSet<string>() { item.EntidadRealizacion.EntidadRef }, pResourceApi);
                    Dictionary<string, string> dicOrganizacionesCargadas = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, string> organizacion in dicOrganizaciones)
                    {
                        Empresa organizacionAux = Empresa.GetOrganizacionSGI(pHarvesterServices, pConfig, "Organizacion_" + organizacion.Key, pDicRutas);
                        tesis.promotedByTitle = organizacionAux.Nombre;

                        if (string.IsNullOrEmpty(organizacion.Value))
                        {
                            string idGnoss = organizacionAux.Cargar(pHarvesterServices, pConfig, pResourceApi, "organization", pDicIdentificadores, pDicRutas, pRabbitConf);
                            pDicIdentificadores["organization"].Add(idGnoss);
                            dicOrganizacionesCargadas[organizacion.Key] = idGnoss;
                        }
                        else
                        {
                            dicOrganizacionesCargadas[organizacion.Key] = organizacion.Value;
                        }

                        tesis.promotedBy = dicOrganizacionesCargadas[item.EntidadRealizacion.EntidadRef];
                    }
                }

                if (item.CoDirectorTesis != null && !string.IsNullOrEmpty(item.CoDirectorTesis.PersonaRef))
                {
                    Persona codirectorSGI = GetPersonaSGI(pHarvesterServices, pConfig, "Persona_"+item.CoDirectorTesis.PersonaRef, pDicRutas);
                    if (codirectorSGI != null)
                    {
                        Codirector codirector = new Codirector();
                        codirector.firstName = codirectorSGI.Nombre;
                        codirector.secondFamilyName = codirectorSGI.Apellidos;
                        codirector.nick = codirectorSGI.Nombre + " " + codirectorSGI.Apellidos;
                        codirector.comment = 1;
                        tesis.codirector = new List<Codirector>() { codirector };
                    }
                }

                listaTesis.Add(tesis);
            }

            return listaTesis;
        }

        private static string IdentificadorPais(string pais, ResourceApi pResourceApi)
        {
            if (!UtilidadesGeneral.DicPaisesContienePais(pais))
            {
                return null;
            }
            return pResourceApi.GraphsUrl + "items/feature_PCLD_" + UtilidadesGeneral.dicPaises[pais];
        }

        private static string IdentificadorRegion(string region, ResourceApi pResourceApi)
        {
            if (!UtilidadesGeneral.DicRegionesContieneRegion(region))
            {
                return null;
            }
            return pResourceApi.GraphsUrl + "items/feature_ADM1_" + UtilidadesGeneral.dicRegiones[region];
        }

        private static string ProjectCharacterType(ResourceApi pResourceApi, string projectCharacterType)
        {
            string id = pResourceApi.GraphsUrl + "items/projectcharactertype_";
            switch (projectCharacterType.ToLower())
            {
                case "proyecto de fin de carrera":                
                    id += "055";
                    break;
                case "proyecto final de carrera":
                    id += "055";
                    break;
                case "tesina":
                    id += "066";
                    break;
                case "tesis doctoral":
                    id += "067";
                    break;
                case "trabajo conducente a la obtención de dea":
                    id += "071";
                    break;
                case "texto de otros":
                    id += "OTHERS";
                    break;
                default:
                    return null;
            }
            return id;
        }







        /// <summary>
        /// Identificador de la persona.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Nombre de la persona.
        /// </summary>
        public string Nombre { get; set; }
        /// <summary>
        /// Apellidos de la persona.
        /// </summary>
        public string Apellidos { get; set; }
        /// <summary>
        /// Se devuelve la entidad Sexo con todos sus campos.
        /// </summary>
        public Sexo Sexo { get; set; }
        /// <summary>
        /// Número de documento de identificación personal.
        /// </summary>
        public string NumeroDocumento { get; set; }
        /// <summary>
        /// Se devuelve la entidad TipoDocumento con todos sus campos.
        /// </summary>
        public TipoDocumento TipoDocumento { get; set; }
        /// <summary>
        /// Se devuelve el identificador/referencia de la entidad Empresa.
        /// </summary>
        public string EmpresaRef { get; set; }
        /// <summary>
        /// Indica si es personal de la Universidad o no (a día de hoy).
        /// </summary>
        public bool? PersonalPropio { get; set; }
        /// <summary>
        /// Se devuelve el identificador/referencia de la entidad que representa a la UM
        /// </summary>
        public string EntidadPropiaRef { get; set; }
        /// <summary>
        /// Lista con los emails de la persona 
        /// </summary>
        public List<Email> Emails { get; set; }
        /// <summary>
        /// Indica si la persona esta activa o no
        /// </summary>
        public bool? Activo { get; set; }
        /// <summary>
        /// Datos personales
        /// </summary>
        public DatosPersonales DatosPersonales { get; set; }
        /// <summary>
        /// Datos de contacto
        /// </summary>
        public DatosContacto DatosContacto { get; set; }
        /// <summary>
        /// Vinculación
        /// </summary>
        public Vinculacion Vinculacion { get; set; }
        /// <summary>
        /// Datos academicos
        /// </summary>
        public DatosAcademicos DatosAcademicos { get; set; }
        /// <summary>
        /// Colectivo
        /// </summary>
        public Colectivo Colectivo { get; set; }
        /// <summary>
        /// Fotografía
        /// </summary>
        public Fotografia Fotografia { get; set; }
        /// <summary>
        /// Sexenios
        /// </summary>
        public Sexenio Sexenios { get; set; }
        /// <summary>
        /// Posgrado
        /// </summary>
        public List<Posgrado> Posgrado { get; set; }
        /// <summary>
        /// Ciclos
        /// </summary>
        public List<Ciclos> Ciclos { get; set; }
        /// <summary>
        /// Doctorados
        /// </summary>
        public List<Doctorados> Doctorados { get; set; }
        /// <summary>
        /// Formación especializada
        /// </summary>
        public List<FormacionEspecializada> FormacionEspecializada { get; set; }
        /// <summary>
        /// Tesis
        /// </summary>
        public List<Tesis> Tesis { get; set; }
        /// <summary>
        /// Seminarios/Cursos
        /// </summary>
        public List<SeminariosCursos> SeminariosCursos { get; set; }
        /// <summary>
        /// Formación academica impartida
        /// </summary>
        public List<FormacionAcademicaImpartida> FormacionAcademicaImpartida { get; set; }
    }
}
