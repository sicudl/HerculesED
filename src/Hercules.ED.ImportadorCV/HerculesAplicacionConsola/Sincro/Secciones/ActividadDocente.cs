﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Models.Entity;
using Utils;
using Models;

namespace HerculesAplicacionConsola.Sincro.Secciones
{
    class ActividadDocente : SeccionBase
    {
        /// <summary>
        /// Añade en la lista de propiedades de la entidad las propiedades en las 
        /// que los valores no son nulos, en caso de que los valores sean nulos se omite
        /// dicha propiedad.
        /// </summary>
        /// <param name="list"></param>
        private List<Property> AddProperty(params Property[] list)
        {
            List<Property> listado = new List<Property>();
            for (int i = 0; i < list.Length; i++)
            {
                if (!string.IsNullOrEmpty(list[i].values[0]))
                {
                    listado.Add(list[i]);
                }
            }
            return listado;
        }

        private List<CvnItemBean> listadoDatos = new List<CvnItemBean>();
        private List<CvnItemBean> listadoPremios = new List<CvnItemBean>();
        public ActividadDocente(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
            listadoDatos = mCvn.GetListadoBloque("030");
            listadoPremios = mCvn.GetListadoBloque("060.030.080");
        }

        /// <summary>
        /// Dada una cadena de GUID concatenados y finalizando en "|" y un string en caso de que 
        /// el string no sea nulo los concatena, sino devuelve null.
        /// </summary>
        /// <param name="entityAux">GUID concatenado con "|"</param>
        /// <param name="valor">Valor del parametro</param>
        /// <returns>String de concatenar los parametros, o nulo si el valor es vacio</returns>
        private string StringGNOSSID(string entityAux, string valor)
        {
            if (!string.IsNullOrEmpty(valor))
            {
                return entityAux + valor;
            }
            return null;
        }

        /// <summary>
        /// 030.010.000.000
        /// </summary>
        public void SincroFormacionAcademica()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetFormacionAcademica(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";//TODO
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 030.020.000.000
        /// </summary>
        public void SincroFormacionSanitariaEspecializada()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetFormacionSanitariaEspecializada(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";//TODO
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 030.030.000.000
        /// </summary>
        public void SincroFormacionSanitariaIMasD()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetFormacionSanitariaIMasD(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";//TODO
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }            
        }

        /// <summary>
        /// 030.040.000.000
        /// </summary>
        public void SincroDireccionTesis()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetDireccionTesis(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 030.050.000.000
        /// </summary>
        public void SincroTutoriasAcademicas()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetTutoriasAcademicas(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 030.060.000.000
        /// </summary>
        public void SincroCursosSeminarios()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetCursosSeminarios(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 030.070.000.000
        /// </summary>
        public void SincroPublicacionDocentes()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetPublicacionDocentes(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 030.080.000.000
        /// </summary>
        public void SincroParticipacionProyectosInnovacionDocente()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetParticipacionProyectosInnovacionDocente(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 030.090.000.000
        /// </summary>
        public void SincroParticipacionCongresosFormacionDocente()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetParticipacionCongresosFormacionDocente(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 060.030.080.000
        /// </summary>
        public void SincroPremiosInovacionDocente()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetPremiosInovacionDocente(listadoPremios);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 030.100.000.000
        /// </summary>
        public void SincroOtrasActividades()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtrasActividades(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        /// <summary>
        /// 030.110.000.000
        /// </summary>
        public void SincroAportacionesRelevantes()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXXXX", "http://w3id.org/roh/XXXXXXXXXXXXXXXXXX", "http://vivoweb.org/ontology/core#relatedBy" };

            //1º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;

            //2º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetAportacionesRelevantes(listadoDatos);

            foreach (Entity entityXML in listadoAux)
            {
                entityXML.propTitle = "XXXXXXX";
                entityXML.ontology = "XXX";
                entityXML.rdfType = "XXXXXXXXXXX";

                UpdateListEntityAux(mCvID, "http://w3id.org/roh/XXXXXXXXXXXXXXXX", "RelatedXXXXXXXXXXXXXX", propiedadesItem, entityXML);
            }
        }

        private List<Entity> GetDireccionTesis(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoDireccionTesis = listadoDatos.Where(x => x.Code.Equals("030.040.000.000")).ToList();
            if (listadoDireccionTesis.Count > 0)
            {
                foreach (CvnItemBean item in listadoDireccionTesis)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.040.000.010")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.direccionTesisTipoProyecto, item.GetStringPorIDCampo("030.040.000.010")),//TODO
                            new Property(Variables.ActividadDocente.direccionTesisTipoProyectoOtros, item.GetStringPorIDCampo("030.040.000.020")),
                            new Property(Variables.ActividadDocente.direccionTesisTituloTrabajo, item.GetStringPorIDCampo("030.040.000.030")),
                            new Property(Variables.ActividadDocente.direccionTesisCodirectorTesis, item.GetStringPorIDCampo("030.040.000.180")),//rep//TODO - funcion autor
                            new Property(Variables.ActividadDocente.direccionTesisPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.040.000.040")),
                            new Property(Variables.ActividadDocente.direccionTesisCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.040.000.050")),
                            new Property(Variables.ActividadDocente.direccionTesisCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.040.000.070")),
                            new Property(Variables.ActividadDocente.direccionTesisEntidadRealizacion, item.GetNameEntityBeanPorIDCampo("030.040.000.080")),
                            new Property(Variables.ActividadDocente.direccionTesisTipoEntidadRealizacion, item.GetStringPorIDCampo("030.040.000.100")),//TODO - funcion other
                            new Property(Variables.ActividadDocente.direccionTesisTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.040.000.110")),//TODO
                            new Property(Variables.ActividadDocente.direccionTesisAlumno, item.GetStringPorIDCampo("030.040.000.120")),//TODO - funcion authorbean
                            new Property(Variables.ActividadDocente.direccionTesisPalabrasClave, item.GetStringPorIDCampo("030.040.000.130")),//rep//TODO
                            new Property(Variables.ActividadDocente.direccionTesisFechaDefensa, item.GetStringDatetimePorIDCampo("030.040.000.140")),
                            new Property(Variables.ActividadDocente.direccionTesisCalificacionObtenida, item.GetStringPorIDCampo("030.040.000.150")),
                            new Property(Variables.ActividadDocente.direccionTesisFechaMencionDoctUE, item.GetStringDatetimePorIDCampo("030.040.000.160")),
                            new Property(Variables.ActividadDocente.direccionTesisMencionCalidad, item.GetStringBooleanPorIDCampo("030.040.000.170")),
                            new Property(Variables.ActividadDocente.direccionTesisDoctoradoUE, item.GetStringBooleanPorIDCampo("030.040.000.190")),
                            new Property(Variables.ActividadDocente.direccionTesisFechaMencionCalidad, item.GetStringDatetimePorIDCampo("030.040.000.200"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        private List<Entity> GetFormacionAcademica(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoFormacionAcademica = listadoDatos.Where(x => x.Code.Equals("030.010.000.000")).ToList();
            if (listadoFormacionAcademica.Count > 0)
            {
                foreach (CvnItemBean item in listadoFormacionAcademica)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.010.000.010")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoDocenciaOficialidad, item.GetStringPorIDCampo("030.010.000.010")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaTitulacionUniversitaria, item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("030.010.000.020").Name),//TODO -check
                            new Property(Variables.ActividadDocente.formacionAcademicaPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.010.000.040")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.010.000.050")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.010.000.070")),
                            new Property(Variables.ActividadDocente.formacionAcademicaEntidadRealizacion, item.GetNameEntityBeanPorIDCampo("030.010.000.080")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadRealizacion, item.GetStringPorIDCampo("030.010.000.110")),//TODO - funcion other
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.010.000.120")),
                            new Property(Variables.ActividadDocente.formacionAcademicaDepartamento, item.GetStringPorIDCampo("030.010.000.130")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoPrograma, item.GetStringPorIDCampo("030.010.000.140")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoProgramaOtros, item.GetStringPorIDCampo("030.010.000.150")),
                            new Property(Variables.ActividadDocente.formacionAcademicaNombreAsignatura, item.GetStringPorIDCampo("030.010.000.160")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoDocenciaModalidad, item.GetStringPorIDCampo("030.010.000.170")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoDocenciaModalidadOtros, item.GetStringPorIDCampo("030.010.000.180")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoAsignatura, item.GetStringPorIDCampo("030.010.000.190")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoAsignaturaOtros, item.GetStringPorIDCampo("030.010.000.430")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCursoTitulacion, item.GetStringPorIDCampo("030.010.000.200")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoECTS, item.GetStringPorIDCampo("030.010.000.210")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaNumeroECTS, item.GetStringDoublePorIDCampo("030.010.000.220")),
                            new Property(Variables.ActividadDocente.formacionAcademicaIdiomaAsignatura, item.GetStringPorIDCampo("030.010.000.230")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaFrecuenciaAsignatura, item.GetStringDoublePorIDCampo("030.010.000.240")),
                            new Property(Variables.ActividadDocente.formacionAcademicaFechaFinalizacion, item.GetStringDatetimePorIDCampo("030.010.000.250")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaCompetenciasRelacionadas, item.GetStringPorIDCampo("030.010.000.260")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCategoriaProfesional, item.GetStringPorIDCampo("030.010.000.270")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCalificacionObtenida, item.GetStringPorIDCampo("030.010.000.280")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCalificacionMax, item.GetStringPorIDCampo("030.010.000.290")),
                            new Property(Variables.ActividadDocente.formacionAcademicaPaisEntidadEvaluacion, item.GetPaisPorIDCampo("030.010.000.440")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCCAAEntidadEvaluacion, item.GetRegionPorIDCampo("030.010.000.450")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCiudadEntidadEvaluacion, item.GetStringPorIDCampo("030.010.000.470")),
                            new Property(Variables.ActividadDocente.formacionAcademicaEntidadEvaluacion, item.GetNameEntityBeanPorIDCampo("030.010.000.300")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadEvaluacion, item.GetStringPorIDCampo("030.010.000.520")),//TODO - funcion other
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadEvaluacionOtros, item.GetStringPorIDCampo("030.010.000.530")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoEvaluacion, item.GetStringPorIDCampo("030.010.000.320")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoEvaluacionOtros, item.GetStringPorIDCampo("030.010.000.330")),
                            new Property(Variables.ActividadDocente.formacionAcademicaPaisEntidadFinanciadora, item.GetPaisPorIDCampo("030.010.000.480")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCCAAEntidadFinanciadora, item.GetRegionPorIDCampo("030.010.000.500")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCiudadEntidadFinanciadora, item.GetStringPorIDCampo("030.010.000.510")),
                            new Property(Variables.ActividadDocente.formacionAcademicaEntidadFinanciadora, item.GetNameEntityBeanPorIDCampo("030.010.000.350")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadFinanciadora, item.GetStringPorIDCampo("030.010.000.370")),//TODO - funcion other
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadFinanciadoraOtros, item.GetStringPorIDCampo("030.010.000.380")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanTipoConvocatoria, item.GetStringPorIDCampo("030.010.000.390")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanTipoConvocatoriaOtros, item.GetStringPorIDCampo("030.010.000.400")),
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanAmbitoGeo, item.GetStringPorIDCampo("030.010.000.410")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanAmbitoGeoOtros, item.GetStringPorIDCampo("030.010.000.420")),
                            new Property(Variables.ActividadDocente.formacionAcademicaFacultadEscuela, item.GetNameEntityBeanPorIDCampo("030.010.000.540")),
                            new Property(Variables.ActividadDocente.formacionAcademicaFechaInicio, item.GetStringDatetimePorIDCampo("030.010.000.550")),
                            new Property(Variables.ActividadDocente.formacionAcademicaFechaFinalizacion_, item.GetStringDatetimePorIDCampo("030.010.000.610"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        private List<Entity> GetFormacionSanitariaEspecializada(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoFormacionSanitariaEspecializada = listadoDatos.Where(x => x.Code.Equals("030.020.000.000")).ToList();
            if (listadoFormacionSanitariaEspecializada.Count > 0)
            {
                foreach (CvnItemBean item in listadoFormacionSanitariaEspecializada)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.020.000.020")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.formacionSaniEspeTituloEspe, item.GetStringPorIDCampo("030.020.000.020")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeTituloSubespe, item.GetStringPorIDCampo("030.020.000.030")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeTipoParticipacion, item.GetStringPorIDCampo("030.020.000.040")),//TODO
                            new Property(Variables.ActividadDocente.formacionSaniEspeTipoParticipacionOtros, item.GetStringPorIDCampo("030.020.000.050")),
                            new Property(Variables.ActividadDocente.formacionSaniEspePaisEntidadTitulacion, item.GetPaisPorIDCampo("030.020.000.060")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeCCAAEntidadTitulacion, item.GetRegionPorIDCampo("030.020.000.070")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeCiudadEntidadTitulacion, item.GetStringPorIDCampo("030.020.000.090")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeEntidadRealizacion, item.GetNameEntityBeanPorIDCampo("030.020.000.100")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeTipoEntidadRealizacion, item.GetStringPorIDCampo("030.020.000.120")),//TODO - funcion other
                            new Property(Variables.ActividadDocente.formacionSaniEspeTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.020.000.130")),
                            new Property(Variables.ActividadDocente.formacionSaniEspePaisEntidadRealizacion, item.GetPaisPorIDCampo("030.020.000.270")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.020.000.260")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.020.000.280")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeEntidadTitulacion, item.GetNameEntityBeanPorIDCampo("030.020.000.140")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeTipoEntidadTitulacion, item.GetStringPorIDCampo("030.020.000.160")),//TODO -funcion other
                            new Property(Variables.ActividadDocente.formacionSaniEspeTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("030.020.000.170")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeDepartamento, item.GetStringPorIDCampo("030.020.000.180")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeServicio, item.GetStringPorIDCampo("030.020.000.190")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeSeccion, item.GetStringPorIDCampo("030.020.000.200")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeUnidad, item.GetStringPorIDCampo("030.020.000.210")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeFechaInicio, item.GetStringDatetimePorIDCampo("030.020.000.220")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeFechaFinal, item.GetStringDatetimePorIDCampo("030.020.000.230")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeDuracionAnio, item.GetDurationAnioPorIDCampo("030.020.000.240")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeDuracionMes, item.GetDurationMesPorIDCampo("030.020.000.240")),
                            new Property(Variables.ActividadDocente.formacionSaniEspeDuracionDia, item.GetDurationDiaPorIDCampo("030.020.000.240")),
                            new Property(Variables.ActividadDocente.formacionSaniEspePerfilDestinatario, item.GetStringPorIDCampo("030.020.000.250"))//TODO
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        private List<Entity> GetFormacionSanitariaIMasD(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoFormacionSanitariaIMasD = listadoDatos.Where(x => x.Code.Equals("030.030.000.000")).ToList();
            if (listadoFormacionSanitariaIMasD.Count > 0)
            {
                foreach (CvnItemBean item in listadoFormacionSanitariaIMasD)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.030.000.010")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.formacionPosformacionTitulo, item.GetStringPorIDCampo("030.030.000.010")),
                            new Property(Variables.ActividadDocente.formacionPosformacionPaisEntidadTitulacion, item.GetPaisPorIDCampo("030.030.000.020")),
                            new Property(Variables.ActividadDocente.formacionPosformacionCCAAEntidadTitulacion, item.GetRegionPorIDCampo("030.030.000.030")),
                            new Property(Variables.ActividadDocente.formacionPosformacionCiudadEntidadTitulacion, item.GetStringPorIDCampo("030.030.000.050")),
                            new Property(Variables.ActividadDocente.formacionPosformacionEntidadRealizacion, item.GetNameEntityBeanPorIDCampo("030.030.000.060")),
                            new Property(Variables.ActividadDocente.formacionPosformacionTipoEntidadRealizacion, item.GetStringPorIDCampo("030.030.000.080")),//TODO - funcion other
                            new Property(Variables.ActividadDocente.formacionPosformacionTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.030.000.090")),
                            new Property(Variables.ActividadDocente.formacionPosformacionPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.030.000.220")),
                            new Property(Variables.ActividadDocente.formacionPosformacionCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.030.000.240")),
                            new Property(Variables.ActividadDocente.formacionPosformacionCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.030.000.230")),
                            new Property(Variables.ActividadDocente.formacionPosformacionEntidadTitulacion, item.GetNameEntityBeanPorIDCampo("030.030.000.100")),
                            new Property(Variables.ActividadDocente.formacionPosformacionTipoEntidadTitulacion, item.GetStringPorIDCampo("030.030.000.120")),//TODO - funcion other
                            new Property(Variables.ActividadDocente.formacionPosformacionTipoEntidadTitulacionOtros, item.GetStringPorIDCampo("030.030.000.130")),//TODO
                            new Property(Variables.ActividadDocente.formacionPosformacionDepartamento, item.GetStringPorIDCampo("030.030.000.140")),
                            new Property(Variables.ActividadDocente.formacionPosformacionServicio, item.GetStringPorIDCampo("030.030.000.150")),
                            new Property(Variables.ActividadDocente.formacionPosformacionSeccion, item.GetStringPorIDCampo("030.030.000.160")),
                            new Property(Variables.ActividadDocente.formacionPosformacionUnidad, item.GetStringPorIDCampo("030.030.000.170")),
                            new Property(Variables.ActividadDocente.formacionPosformacionFechaInicio, item.GetStringDatetimePorIDCampo("030.030.000.180")),
                            new Property(Variables.ActividadDocente.formacionPosformacionFechaFinal, item.GetStringDatetimePorIDCampo("030.030.000.190")),
                            new Property(Variables.ActividadDocente.formacionPosformacionDuracionAnio, item.GetDurationAnioPorIDCampo("030.030.000.200")),
                            new Property(Variables.ActividadDocente.formacionPosformacionDuracionMes, item.GetDurationMesPorIDCampo("030.030.000.200")),
                            new Property(Variables.ActividadDocente.formacionPosformacionDuracionDia, item.GetDurationDiaPorIDCampo("030.030.000.200")),
                            new Property(Variables.ActividadDocente.formacionPosformacionPerfilDestinatario, item.GetStringPorIDCampo("030.030.000.210"))//TODO
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        private List<Entity> GetTutoriasAcademicas(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoTutoriasAcademicas = listadoDatos.Where(x => x.Code.Equals("030.050.000.000")).ToList();
            if (listadoTutoriasAcademicas.Count > 0)
            {
                foreach (CvnItemBean item in listadoTutoriasAcademicas)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.050.000.010")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.tutoAcademicaNombrePrograma, item.GetStringPorIDCampo("030.050.000.010")),//TODO
                            new Property(Variables.ActividadDocente.tutoAcademicaNombreProgramaOtros, item.GetStringPorIDCampo("030.050.000.020")),
                            new Property(Variables.ActividadDocente.tutoAcademicaPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.050.000.030")),
                            new Property(Variables.ActividadDocente.tutoAcademicaCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.050.000.040")),
                            new Property(Variables.ActividadDocente.tutoAcademicaCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.050.000.060")),
                            new Property(Variables.ActividadDocente.tutoAcademicaEntidadRealizacion, item.GetNameEntityBeanPorIDCampo("030.050.000.070")),
                            new Property(Variables.ActividadDocente.tutoAcademicaTipoEntidadRealizacion, item.GetStringPorIDCampo("030.050.000.090")),//TODO-funcion others
                            new Property(Variables.ActividadDocente.tutoAcademicaTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.050.000.100")),//TODO
                            new Property(Variables.ActividadDocente.tutoAcademicaNumAlumnosTutelados, item.GetStringDoublePorIDCampo("030.050.000.110")),
                            new Property(Variables.ActividadDocente.tutoAcademicaFrecuenciaActividad, item.GetStringDoublePorIDCampo("030.050.000.120")),                            
                            new Property(Variables.ActividadDocente.tutoAcademicaNumHorasECTS, item.GetStringDoublePorIDCampo("030.050.000.130"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        private List<Entity> GetCursosSeminarios(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoCursosSeminarios = listadoDatos.Where(x => x.Code.Equals("030.060.000.000")).ToList();
            if (listadoCursosSeminarios.Count > 0)
            {
                foreach (CvnItemBean item in listadoCursosSeminarios)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.060.000.030")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoEvento, item.GetStringPorIDCampo("030.060.000.010")),//TODO
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoEventoOtros, item.GetStringPorIDCampo("030.060.000.020")),
                            new Property(Variables.ActividadDocente.cursosSeminariosNombreEvento, item.GetStringPorIDCampo("030.060.000.030")),
                            new Property(Variables.ActividadDocente.cursosSeminariosPaisEntidadOrganizadora, item.GetPaisPorIDCampo("030.060.000.040")),
                            new Property(Variables.ActividadDocente.cursosSeminariosCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("030.060.000.050")),
                            new Property(Variables.ActividadDocente.cursosSeminariosCiudadEntidadOrganizadora, item.GetStringPorIDCampo("030.060.000.070")),
                            new Property(Variables.ActividadDocente.cursosSeminariosEntidadOrganizadora, item.GetNameEntityBeanPorIDCampo("030.060.000.080")),
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoEntidadOrganizadora, item.GetStringPorIDCampo("030.060.000.100")),//TODO
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.060.000.110")),
                            new Property(Variables.ActividadDocente.cursosSeminariosObjetivosCurso, item.GetStringPorIDCampo("030.060.000.120")),
                            new Property(Variables.ActividadDocente.cursosSeminariosPerfilDestinatarios, item.GetStringPorIDCampo("030.060.000.130")),
                            new Property(Variables.ActividadDocente.cursosSeminariosIdiomaImpartio, item.GetStringPorIDCampo("030.060.000.140")),//TODO
                            new Property(Variables.ActividadDocente.cursosSeminariosFechaImparticion, item.GetStringDatetimePorIDCampo("030.060.000.150")),
                            new Property(Variables.ActividadDocente.cursosSeminariosHorasImpartidas, item.GetStringDoublePorIDCampo("030.060.000.160")),
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoParticipacion, item.GetStringPorIDCampo("030.060.000.170")),//TODO
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoParticipacionOtros, item.GetStringPorIDCampo("030.060.000.180")),
                            new Property(Variables.ActividadDocente.cursosSeminariosISBN, item.GetStringPorIDCampo("030.060.000.190")),//rep//TODO funcion isbn/issn
                            new Property(Variables.ActividadDocente.cursosSeminariosAutorCorrespondencia, item.GetStringBooleanPorIDCampo("030.060.000.200")),
                            new Property(Variables.ActividadDocente.cursosSeminariosIdentificadorPublicacion, item.GetStringPorIDCampo("030.060.000.210")),//rep//TODO
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoIdentificadorPublicacion, item.GetStringPorIDCampo("030.060.000.220")),//rep//TODO
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoIdentificadorPublicacionOtros, item.GetStringPorIDCampo("030.060.000.230"))//rep//TODO
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        private List<Entity> GetPublicacionDocentes(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoPublicacionDocentes = listadoDatos.Where(x => x.Code.Equals("030.070.000.000")).ToList();
            if (listadoPublicacionDocentes.Count > 0)
            {
                foreach (CvnItemBean item in listadoPublicacionDocentes)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.070.000.010")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.publicacionDocenteNombre, item.GetStringPorIDCampo("030.070.000.010")),
                            new Property(Variables.ActividadDocente.publicacionDocentePerfilDestinatario, item.GetStringPorIDCampo("030.070.000.020")),
                            new Property(Variables.ActividadDocente.publicacionDocenteAutores, item.GetStringPorIDCampo("030.070.000.030")),//rep//TODO -autores?
                            new Property(Variables.ActividadDocente.publicacionDocentePosicionFirma, item.GetStringDoublePorIDCampo("030.070.000.040")),
                            new Property(Variables.ActividadDocente.publicacionDocenteFechaElaboracion, item.GetStringDatetimePorIDCampo("030.070.000.050")),
                            new Property(Variables.ActividadDocente.publicacionDocenteTipologiaSoporte, item.GetStringPorIDCampo("030.070.000.060")),//TODO
                            new Property(Variables.ActividadDocente.publicacionDocenteTipologiaSoporteOtros, item.GetStringPorIDCampo("030.070.000.070")),//TODO
                            new Property(Variables.ActividadDocente.publicacionDocenteTituloPublicacion, item.GetStringPorIDCampo("030.070.000.080")),
                            new Property(Variables.ActividadDocente.publicacionDocenteNombrePublicacion, item.GetStringPorIDCampo("030.070.000.190")),
                            new Property(Variables.ActividadDocente.publicacionDocenteVolumenPublicacion, item.GetVolumenPorIDCampo("030.070.000.090")),//TODO
                            new Property(Variables.ActividadDocente.publicacionDocentePagIniPublicacion, item.GetPaginaInicialPorIDCampo("030.070.000.100")),
                            new Property(Variables.ActividadDocente.publicacionDocentePagFinalPublicacion, item.GetPaginaFinalPorIDCampo("030.070.000.100")),
                            new Property(Variables.ActividadDocente.publicacionDocenteEditorialPublicacion, item.GetStringPorIDCampo("030.070.000.110")),
                            new Property(Variables.ActividadDocente.publicacionDocentePaisPublicacion, item.GetPaisPorIDCampo("030.070.000.120")),
                            new Property(Variables.ActividadDocente.publicacionDocenteCCAAPublicacion, item.GetRegionPorIDCampo("030.070.000.130")),
                            new Property(Variables.ActividadDocente.publicacionDocenteFechaPublicacion, item.GetStringDatetimePorIDCampo("030.070.000.150")),//TODO - check
                            new Property(Variables.ActividadDocente.publicacionDocenteURLPublicacion, item.GetStringPorIDCampo("030.070.000.160")),
                            new Property(Variables.ActividadDocente.publicacionDocenteISBNPublicacion, item.GetStringPorIDCampo("030.070.000.170")),//TODO - externalPKBean //rep
                            new Property(Variables.ActividadDocente.publicacionDocenteDepositoLegal, item.GetElementoPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.070.000.180").Value),
                            new Property(Variables.ActividadDocente.publicacionDocenteJustificacionMaterial, item.GetStringPorIDCampo("030.070.000.200")),
                            new Property(Variables.ActividadDocente.publicacionDocenteGradoContribucion, item.GetStringPorIDCampo("030.070.000.210")),//TODO
                            new Property(Variables.ActividadDocente.publicacionDocenteAutorCorrespondencia, item.GetStringBooleanPorIDCampo("030.070.000.220")),
                            new Property(Variables.ActividadDocente.publicacionDocenteIdentificadorPublicacion, item.GetStringPorIDCampo("030.070.000.230")),//TODO - externalPKBean //rep
                            new Property(Variables.ActividadDocente.publicacionDocenteTipoIdentificadorPublicacion, item.GetStringPorIDCampo("030.070.000.240")),//TODO//rep
                            new Property(Variables.ActividadDocente.publicacionDocenteTipoIdentificadorPublicacionOtros, item.GetStringPorIDCampo("030.070.000.250"))//TODO//rep
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        private List<Entity> GetParticipacionProyectosInnovacionDocente(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoParticipacionProyectosInnovacionDocente = listadoDatos.Where(x => x.Code.Equals("030.080.000.000")).ToList();
            if (listadoParticipacionProyectosInnovacionDocente.Count > 0)
            {
                foreach (CvnItemBean item in listadoParticipacionProyectosInnovacionDocente)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.080.000.010")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.participacionInnovaTitulo, item.GetStringPorIDCampo("030.080.000.010")),
                            new Property(Variables.ActividadDocente.participacionInnovaPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.080.000.020")),
                            new Property(Variables.ActividadDocente.participacionInnovaCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.080.000.030")),
                            new Property(Variables.ActividadDocente.participacionInnovaCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.080.000.250")),
                            new Property(Variables.ActividadDocente.participacionInnovaTipoParticipacion, item.GetStringPorIDCampo("030.080.000.050")),//TODO
                            new Property(Variables.ActividadDocente.participacionInnovaTipoParticipacionOtros, item.GetStringPorIDCampo("030.080.000.060")),
                            new Property(Variables.ActividadDocente.participacionInnovaAportacionProyecto, item.GetStringPorIDCampo("030.080.000.070")),
                            new Property(Variables.ActividadDocente.participacionInnovaRegimenDedicacion, item.GetStringPorIDCampo("030.080.000.080")),//TODO
                            new Property(Variables.ActividadDocente.participacionInnovaEntidadFinanciadora, item.GetNameEntityBeanPorIDCampo("030.080.000.090")),
                            new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadFinanciadora, item.GetStringPorIDCampo("030.080.000.110")),//TODO
                            new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadFinanciadoraOtros, item.GetStringPorIDCampo("030.080.000.120")),
                            new Property(Variables.ActividadDocente.participacionInnovaTipoConvocatoria, item.GetStringPorIDCampo("030.080.000.130")),//TODO
                            new Property(Variables.ActividadDocente.participacionInnovaTipoConvocatoriaOtros, item.GetStringPorIDCampo("030.080.000.140")),
                            new Property(Variables.ActividadDocente.participacionInnovaEntidadParticipante, item.GetNameEntityBeanPorIDCampo("030.080.000.150")),//rep
                            new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadParticipante, item.GetStringPorIDCampo("030.080.000.170")),//rep//TODO
                            new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadParticipanteOtros, item.GetStringPorIDCampo("030.080.000.180")),//rep
                            new Property(Variables.ActividadDocente.participacionInnovaTipoDuracionRelacionLaboral, item.GetStringPorIDCampo("030.080.000.190")),//TODO
                            new Property(Variables.ActividadDocente.participacionInnovaDuracionParticipacionAnio, item.GetDurationAnioPorIDCampo("030.080.000.200")),
                            new Property(Variables.ActividadDocente.participacionInnovaDuracionParticipacionMes, item.GetDurationMesPorIDCampo("030.080.000.200")),
                            new Property(Variables.ActividadDocente.participacionInnovaDuracionParticipacionDia, item.GetDurationDiaPorIDCampo("030.080.000.200")),
                            new Property(Variables.ActividadDocente.participacionInnovaFechaFinalizacionParticipacion, item.GetStringDatetimePorIDCampo("030.080.000.210")),
                            new Property(Variables.ActividadDocente.participacionInnovaNombreIP, item.GetStringPorIDCampo("030.080.000.220")),//TODO - funcion autor
                            new Property(Variables.ActividadDocente.participacionInnovaNumParticipantes, item.GetStringDoublePorIDCampo("030.080.000.230")),
                            new Property(Variables.ActividadDocente.participacionInnovaImporteConcedido, item.GetStringDoublePorIDCampo("030.080.000.240")),
                            new Property(Variables.ActividadDocente.participacionInnovaAmbitoProyecto, item.GetStringPorIDCampo("030.080.000.260")),//TODO
                            new Property(Variables.ActividadDocente.participacionInnovaAmbitoProyectoOtros, item.GetStringPorIDCampo("030.080.000.270")),
                            new Property(Variables.ActividadDocente.participacionInnovaFechaInicio, item.GetStringDatetimePorIDCampo("030.080.000.280"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        private List<Entity> GetParticipacionCongresosFormacionDocente(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoParticipacionCongresosFormacionDocente = listadoDatos.Where(x => x.Code.Equals("030.090.000.000")).ToList();
            if (listadoParticipacionCongresosFormacionDocente.Count > 0)
            {
                foreach (CvnItemBean item in listadoParticipacionCongresosFormacionDocente)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.090.000.010")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.participaCongresosTipoEvento, item.GetStringPorIDCampo("030.090.000.010")),//TODO
                            new Property(Variables.ActividadDocente.participaCongresosTipoEventoOtros, item.GetStringPorIDCampo("030.090.000.020")),
                            new Property(Variables.ActividadDocente.participaCongresosNombreEvento, item.GetStringPorIDCampo("030.090.000.030")),
                            new Property(Variables.ActividadDocente.participaCongresosPaisEvento, item.GetPaisPorIDCampo("030.090.000.040")),
                            new Property(Variables.ActividadDocente.participaCongresosCCAAEvento, item.GetRegionPorIDCampo("030.090.000.050")),
                            new Property(Variables.ActividadDocente.participaCongresosCiudadEvento, item.GetStringPorIDCampo("030.090.000.070")),
                            new Property(Variables.ActividadDocente.participaCongresosEntidadOrganizadora, item.GetNameEntityBeanPorIDCampo("030.090.000.080")),
                            new Property(Variables.ActividadDocente.participaCongresosTipoEntidadOrganizadora, item.GetStringPorIDCampo("030.090.000.100")),//TODO 
                            new Property(Variables.ActividadDocente.participaCongresosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.090.000.110")),//TODO 
                            new Property(Variables.ActividadDocente.participaCongresosPaisEntidadOrganizadora, item.GetPaisPorIDCampo("030.090.000.190")),
                            new Property(Variables.ActividadDocente.participaCongresosCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("030.090.000.200")),
                            new Property(Variables.ActividadDocente.participaCongresosCiudadEntidadOrganizadora, item.GetStringPorIDCampo("030.090.000.210")),
                            new Property(Variables.ActividadDocente.participaCongresosObjetivosEvento, item.GetStringPorIDCampo("030.090.000.120")),
                            new Property(Variables.ActividadDocente.participaCongresosPerfilDestinatarios, item.GetStringPorIDCampo("030.090.000.130")),
                            new Property(Variables.ActividadDocente.participaCongresosIdiomaPresentacion, item.GetStringPorIDCampo("030.090.000.140")),//TODO
                            new Property(Variables.ActividadDocente.participaCongresosFechaPresentacion, item.GetStringDatetimePorIDCampo("030.090.000.150")),
                            new Property(Variables.ActividadDocente.participaCongresosTipoParticipacion, item.GetStringPorIDCampo("030.090.000.160")),//TODO
                            new Property(Variables.ActividadDocente.participaCongresosTipoParticipacionOtros, item.GetStringPorIDCampo("030.090.000.170")),
                            new Property(Variables.ActividadDocente.participaCongresosTipoPublicacion, item.GetStringPorIDCampo("030.090.000.220")),//TODO
                            new Property(Variables.ActividadDocente.participaCongresosTituloPublicacion, item.GetStringPorIDCampo("030.090.000.230")),
                            new Property(Variables.ActividadDocente.participaCongresosNombrePublicacion, item.GetStringPorIDCampo("030.090.000.330")),
                            new Property(Variables.ActividadDocente.participaCongresosVolumenPublicacion, item.GetVolumenPorIDCampo("030.090.000.240")),//TODO
                            new Property(Variables.ActividadDocente.participaCongresosPagIniPublicacion, item.GetPaginaInicialPorIDCampo("030.090.000.250")),
                            new Property(Variables.ActividadDocente.participaCongresosPagFinalPublicacion, item.GetPaginaFinalPorIDCampo("030.090.000.250")),
                            new Property(Variables.ActividadDocente.participaCongresosEditorialPublicacion, item.GetStringPorIDCampo("030.090.000.260")),
                            new Property(Variables.ActividadDocente.participaCongresosPaisPublicacion, item.GetPaisPorIDCampo("030.090.000.270")),
                            new Property(Variables.ActividadDocente.participaCongresosCCAAPublicacion, item.GetRegionPorIDCampo("030.090.000.280")),
                            new Property(Variables.ActividadDocente.participaCongresosFechaPublicacion, item.GetStringDatetimePorIDCampo("030.090.000.300")),
                            new Property(Variables.ActividadDocente.participaCongresosURLPublicacion, item.GetStringPorIDCampo("030.090.000.310")),
                            new Property(Variables.ActividadDocente.participaCongresosISBNPublicacion, item.GetStringPorIDCampo("030.090.000.180")),//rep//TODO externalpkbean
                            new Property(Variables.ActividadDocente.participaCongresosDepositoLegalPublicacion, item.GetElementoPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.090.000.320").Value),
                            new Property(Variables.ActividadDocente.participaCongresosNumHorasPublicacion, item.GetDurationHorasPorIDCampo("030.090.000.340")),
                            new Property(Variables.ActividadDocente.participaCongresosFechaInicioPublicacion, item.GetStringDatetimePorIDCampo("030.090.000.350")),
                            new Property(Variables.ActividadDocente.participaCongresosFechaFinalPublicacion, item.GetStringDatetimePorIDCampo("030.090.000.360")),
                            new Property(Variables.ActividadDocente.participaCongresosIdentificadorPublicacion, item.GetElementoPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.090.000.370").Value),//rep//TODO-funcion
                            new Property(Variables.ActividadDocente.participaCongresosTipoIDPublicacion, item.GetStringPorIDCampo("030.090.000.380")),//rep//TODO-funcion
                            new Property(Variables.ActividadDocente.participaCongresosTipoIDSPublicacionOtros, item.GetStringPorIDCampo("030.090.000.390")),//rep//TODO-funcion
                            new Property(Variables.ActividadDocente.participaCongresosAutorCorrespondencia, item.GetStringBooleanPorIDCampo("030.090.000.400"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        private List<Entity> GetPremiosInovacionDocente(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoPremiosInovacionDocente = listadoDatos.Where(x => x.Code.Equals("060.030.080.000")).ToList();
            if (listadoPremiosInovacionDocente.Count > 0)
            {
                foreach (CvnItemBean item in listadoPremiosInovacionDocente)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.080.010")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.premiosInnovaNombre, item.GetStringPorIDCampo("060.030.080.010")),
                            new Property(Variables.ActividadDocente.premiosInnovaPaisEntidadConcesionaria, item.GetPaisPorIDCampo("060.030.080.020")),
                            new Property(Variables.ActividadDocente.premiosInnovaCCAAEntidadConcesionaria, item.GetRegionPorIDCampo("060.030.080.030")),
                            new Property(Variables.ActividadDocente.premiosInnovaCiudadEntidadConcesionaria, item.GetStringPorIDCampo("060.030.080.110")),
                            new Property(Variables.ActividadDocente.premiosInnovaEntidadConcesionaria, item.GetNameEntityBeanPorIDCampo("060.030.080.050")),
                            new Property(Variables.ActividadDocente.premiosInnovaTipoEntidadConcesionaria, item.GetStringPorIDCampo("060.030.080.070")),//TODO - funcion other
                            new Property(Variables.ActividadDocente.premiosInnovaTipoEntidadConcesionariaOtros, item.GetStringPorIDCampo("060.030.080.080")),//TODO
                            new Property(Variables.ActividadDocente.premiosInnovaPropuestaDe, item.GetStringPorIDCampo("060.030.080.090")),
                            new Property(Variables.ActividadDocente.premiosInnovaFechaConcesion, item.GetStringDatetimePorIDCampo("060.030.080.100"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        private List<Entity> GetOtrasActividades(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOtrasActividades = listadoDatos.Where(x => x.Code.Equals("030.100.000.000")).ToList();
            if (listadoOtrasActividades.Count > 0)
            {
                foreach (CvnItemBean item in listadoOtrasActividades)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.100.000.010")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.otrasActividadesDescripcion, item.GetStringPorIDCampo("030.100.000.010")),
                            new Property(Variables.ActividadDocente.otrasActividadesPalabrasClave, item.GetStringPorIDCampo("030.100.000.020")),//TODO //rep
                            new Property(Variables.ActividadDocente.otrasActividadesPaisRealizacion, item.GetPaisPorIDCampo("030.100.000.030")),
                            new Property(Variables.ActividadDocente.otrasActividadesCCAARealizacion, item.GetRegionPorIDCampo("030.100.000.040")),
                            new Property(Variables.ActividadDocente.otrasActividadesCiudadRealizacion, item.GetStringPorIDCampo("030.100.000.060")),
                            new Property(Variables.ActividadDocente.otrasActividadesEntidadOrganizadora, item.GetNameEntityBeanPorIDCampo("030.100.000.070")),
                            new Property(Variables.ActividadDocente.otrasActividadesTipoEntidadOrganizadora, item.GetStringPorIDCampo("030.100.000.090")),//TODO - funcion
                            new Property(Variables.ActividadDocente.otrasActividadesTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.100.000.100")),//TODO
                            new Property(Variables.ActividadDocente.otrasActividadesFechaFinalizacion, item.GetStringDatetimePorIDCampo("030.100.000.110"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }
        
        private List<Entity> GetAportacionesRelevantes(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoAportacionesRelevantes = listadoDatos.Where(x => x.Code.Equals("030.110.000.000")).ToList();
            if (listadoAportacionesRelevantes.Count > 0)
            {
                foreach (CvnItemBean item in listadoAportacionesRelevantes)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.110.000.010")))//TODO-check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.ActividadDocente.aportacionesCVDescripcion, item.GetStringPorIDCampo("030.110.000.010")),
                            new Property(Variables.ActividadDocente.aportacionesCVPalabrasClave, item.GetStringPorIDCampo("030.110.000.020")),//rep//TODO
                            new Property(Variables.ActividadDocente.aportacionesCVPaisRealizacion, item.GetPaisPorIDCampo("030.110.000.030")),
                            new Property(Variables.ActividadDocente.aportacionesCVCCAARealizacion, item.GetRegionPorIDCampo("030.110.000.040")),
                            new Property(Variables.ActividadDocente.aportacionesCVCiudadRealizacion, item.GetStringPorIDCampo("030.110.000.060")),
                            new Property(Variables.ActividadDocente.aportacionesCVEntidadOrganizadora, item.GetNameEntityBeanPorIDCampo("030.110.000.070")),
                            new Property(Variables.ActividadDocente.aportacionesCVTipoEntidadOrganizadora, item.GetStringPorIDCampo("030.110.000.090")),//TODO-other
                            new Property(Variables.ActividadDocente.aportacionesCVTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.110.000.100")),//TODO 
                            new Property(Variables.ActividadDocente.aportacionesCVFechaFinalizacion, item.GetStringDatetimePorIDCampo("030.110.000.110"))
                        ));

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

    }
}