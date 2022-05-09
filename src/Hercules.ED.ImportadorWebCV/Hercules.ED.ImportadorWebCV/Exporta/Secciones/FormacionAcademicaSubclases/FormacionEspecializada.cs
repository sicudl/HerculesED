﻿using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.FormacionAcademicaSubclases
{
    public class FormacionEspecializada : SeccionBase
    {
        private List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications",
            "http://w3id.org/roh/specialisedTraining", "http://vivoweb.org/ontology/core#relatedBy" };
        private string graph = "academicdegree";

        public FormacionEspecializada(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        public void ExportaFormacionEspecializada(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "020.020.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeTipoFormacion),
                    "020.020.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeTipoFormacionOtros),
                    "020.020.000.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeTituloFormacion),
                    "020.020.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspePaisEntidadTitulacion),
                    "020.020.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeCCAAEntidadTitulacion),
                    "020.020.000.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeCiudadEntidadTitulacion),
                    "020.020.000.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeObjetivosEntidad),
                    "020.020.000.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDurationHours(itemBean,"020.020.000.140",keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeFechaFinalizacion),
                    "020.020.000.150", keyValue.Value);

                //Entidad titulacion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeEntidadTitulacionNombre),
                    "020.020.000.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeTipoEntidadTitulacion),
                    "020.020.000.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeTipoEntidadTitulacionOtros),
                    "020.020.000.110", keyValue.Value);

                //Responsable
                Dictionary<string, string> listadoPropiedades = new Dictionary<string, string>();
                listadoPropiedades.Add("Firma",Variables.FormacionAcademica.formacionEspeResponsableFirma);
                listadoPropiedades.Add("Nombre",Variables.FormacionAcademica.formacionEspeResponsableNombre);
                listadoPropiedades.Add("PrimerApellido",Variables.FormacionAcademica.formacionEspeResponsablePrimerApellido);
                listadoPropiedades.Add("SegundoApellido",Variables.FormacionAcademica.formacionEspeResponsableSegundoApellido);

                UtilityExportar.AddCvnItemBeanCvnAuthorBean(itemBean, listadoPropiedades, "020.020.000.130", entity);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}