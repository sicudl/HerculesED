﻿using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.ExperienciaCientificaSubclases
{
    public class ProyectosIDI:SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience", 
            "http://w3id.org/roh/competitiveProjects", "http://w3id.org/roh/relatedCompetitiveProjectCV",
            "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "project";
        public ProyectosIDI(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }
        /// <summary>
        /// Exporta los datos de la sección "050.020.010.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="seccion"></param>
        /// <param name="secciones"></param>
        /// <param name="preimportar"></param>
        public void ExportaProyectosIDI(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidadesCV(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntityCV(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "050.020.010.000",
                    Items = new List<CVNObject>()
                };


                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDINombre),
                    "050.020.010.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIModalidadProyecto),
                    "050.020.010.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIAmbitoProyecto),
                    "050.020.010.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIAmbitoProyectoOtros),
                    "050.020.010.050", keyValue.Value);

                string numInvestigadores = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDINumInvestigadores))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDINumInvestigadores)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numInvestigadores))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.010.150", numInvestigadores);
                }

                string numPersonasAnio = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDINumPersonasAnio))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDINumPersonasAnio)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numPersonasAnio))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.010.160", numPersonasAnio);
                }

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDINombreProgramaFinanciacion),
                    "050.020.010.250", keyValue.Value);
               
                UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDICodEntidadFinanciacion),
                    "050.020.010.260", keyValue.Value);

                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIFechaInicio),
                    "050.020.010.270", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean, "050.020.010.280", keyValue.Value);
                string cuantiaTotal = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDICuantiaTotal))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDICuantiaTotal)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(cuantiaTotal))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.010.290", cuantiaTotal);
                }
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIResultadosRelevantes),
                    "050.020.010.340", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, seccion ,UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIFechaFinalizacion),
                    "050.020.010.410", keyValue.Value);

                // Properties_cv
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIGradoContribucion),
                    "050.020.010.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIGradoContribucionOtros),
                    "050.020.010.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoParticipacion),
                    "050.020.010.230", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoParticipacionOtros),
                    "050.020.010.240", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIRegimenDedicacion),
                    "050.020.010.430", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString_cv(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIAportacionSolicitante),
                    "050.020.010.420", keyValue.Value);

                // Entidad realizacion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacionNombre),
                    "050.020.010.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoEntidadRealizacion),
                    "050.020.010.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoEntidadRealizacionOtros),
                    "050.020.010.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIPaisEntidadRealizacion),
                    "050.020.010.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDICCAAEntidadRealizacion),
                    "050.020.010.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDICiudadEntidadRealizacion),
                    "050.020.010.090", keyValue.Value);

                //// Resultados relevantes palabras clave TODO
                //UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIResultadosRelevantesPalabrasClave),
                //    "050.020.010.350", entity);
                //// Palabras clave TODO
                //UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIPalabrasClave), 
                //    "050.020.010.020", entity);

                // Financiacion
                string cuantiaSubproyecto = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDICuantiaSubproyecto))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDICuantiaSubproyecto)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(cuantiaSubproyecto))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.010.300", cuantiaSubproyecto);
                }
                string porcentajeSubvencion = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeSubvencion))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeSubvencion)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(porcentajeSubvencion))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.010.310", porcentajeSubvencion);
                }
                string porcentajeCredito = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeCredito))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeCredito)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(porcentajeCredito))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.010.320", porcentajeCredito);
                }
                string porcentajeMixto = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeMixto))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeMixto)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(porcentajeMixto))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "050.020.010.330", porcentajeMixto);
                }

                // Entidades participantes
                UtilityExportar.AddCvnItemBeanCvnEntityBeanList(itemBean, UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadParticipanteNombre),
                    "050.020.010.400", keyValue.Value);

                // Autores
                Dictionary<string, string> listadoAutores = new Dictionary<string, string>();
                listadoAutores.Add("Firma", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIFirmaIP));
                listadoAutores.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDINombreIP));
                listadoAutores.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIPrimerApellidoIP));
                listadoAutores.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDISegundoApellidoIP));
                UtilityExportar.AddCvnItemBeanCvnAuthorBeanList(itemBean, listadoAutores, "050.020.010.140", keyValue.Value);

                // Entidad financiadora
                List<Tuple<string, string, string>> dicEntidad = new List<Tuple<string, string, string>>();
                dicEntidad.Add(new Tuple<string, string, string>("EntityBean", "050.020.010.190",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadFinanciadoraNombre)));
                dicEntidad.Add(new Tuple<string, string, string>("String", "050.020.010.210",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoEntidadFinanciadora)));
                dicEntidad.Add(new Tuple<string, string, string>("String", "050.020.010.220",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoEntidadFinanciadoraOtros)));
                dicEntidad.Add(new Tuple<string, string, string>("String", "050.020.010.360",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDIPaisEntidadFinanciadora)));
                dicEntidad.Add(new Tuple<string, string, string>("String", "050.020.010.370",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDICCAAEntidadFinanciadora)));
                dicEntidad.Add(new Tuple<string, string, string>("String", "050.020.010.390",
                    UtilityExportar.EliminarRDF(Variables.ExperienciaCientificaTecnologica.proyectosIDICiudadEntidadFinanciadora)));

                UtilityExportar.AddCvnItemBeanCvnCodeGroup(itemBean, dicEntidad,
                   "050.020.010.190", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
