﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Models.Entity;
using Utils;
using Models;
using System.Runtime.InteropServices;
using Hercules.ED.ImportadorWebCV.Models;
using Hercules.ED.DisambiguationEngine.Models;
using ImportadorWebCV.Sincro.Secciones.ActividadDocenteSubclases;

namespace ImportadorWebCV.Sincro.Secciones
{
    class ActividadDocente : SeccionBase
    {
        private List<CvnItemBean> listadoDatos = new List<CvnItemBean>();
        private List<CvnItemBean> listadoPremios = new List<CvnItemBean>();
        private readonly string RdfTypeTab = "XXXXXXXXXX";//TODO -check

        public ActividadDocente(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
            listadoDatos = mCvn.GetListadoBloque("030");
            listadoPremios = mCvn.GetListadoBloque("060.030.080");
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Formación académica impartida".
        /// Con el codigo identificativo 030.010.000.000
        /// </summary>
        public List<SubseccionItem> SincroFormacionAcademica([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXX", "XXXXXXXXXXXXXX", "XXXXXXXXXXXX" };
            string graph = "XXXXXXXXXXXXXXXXXX";
            string propTitle = "XXXXXXXXXXXXX";
            string rdfType = "XXXXXXXXXXXXXXX";
            string rdfTypePrefix = "RelatedXXXXXXXXXXXX";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetFormacionAcademica(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO - check
            {
                ActividadDocenteSubclases.FormacionAcademica formacionAcademica = new ActividadDocenteSubclases.FormacionAcademica();
                formacionAcademica.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.formacionAcademicaNombreAsignatura)?.values.FirstOrDefault();
                formacionAcademica.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.formacionAcademicaFechaInicio)?.values.FirstOrDefault();
                formacionAcademica.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(formacionAcademica.ID, formacionAcademica);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = ActividadDocenteSubclases.FormacionAcademica.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Dirección de tesis doctorales y/o proyectos fin de carrera".
        /// Con el codigo identificativo 030.040.000.000
        /// </summary>
        public List<SubseccionItem> SincroDireccionTesis([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXX", "XXXXXXXXXXXXXX", "XXXXXXXXXXXX" };
            string graph = "XXXXXXXXXXXXXXXXXX";
            string propTitle = "XXXXXXXXXXXXX";
            string rdfType = "XXXXXXXXXXXXXXX";
            string rdfTypePrefix = "RelatedXXXXXXXXXXXX";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetDireccionTesis(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO - check
            {
                DireccionTesis direccionTesis = new DireccionTesis();
                direccionTesis.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.direccionTesisTituloTrabajo)?.values.FirstOrDefault();
                direccionTesis.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.direccionTesisFechaDefensa)?.values.FirstOrDefault();
                direccionTesis.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(direccionTesis.ID, direccionTesis);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = DireccionTesis.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Tutorías académicas de estudiantes".
        /// Con el codigo identificativo 030.050.000.000
        /// </summary>
        public List<SubseccionItem> SincroTutoriasAcademicas([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXX", "XXXXXXXXXXXXXX", "XXXXXXXXXXXX" };
            string graph = "XXXXXXXXXXXXXXXXXX";
            string propTitle = "XXXXXXXXXXXXX";
            string rdfType = "XXXXXXXXXXXXXXX";
            string rdfTypePrefix = "RelatedXXXXXXXXXXXX";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetTutoriasAcademicas(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO - check
            {
                TutoriasAcademicas tutoriasAcademicas = new TutoriasAcademicas();
                tutoriasAcademicas.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.tutoAcademicaNombrePrograma)?.values.FirstOrDefault();
                tutoriasAcademicas.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.tutoAcademicaNumHorasECTS)?.values.FirstOrDefault();
                tutoriasAcademicas.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(tutoriasAcademicas.ID, tutoriasAcademicas);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = TutoriasAcademicas.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Cursos y seminarios impartidos orientados a la formación docente universitaria".
        /// Con el codigo identificativo 030.060.000.000
        /// </summary>
        public List<SubseccionItem> SincroCursosSeminarios([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXX", "XXXXXXXXXXXXXX", "XXXXXXXXXXXX" };
            string graph = "XXXXXXXXXXXXXXXXXX";
            string propTitle = "XXXXXXXXXXXXX";
            string rdfType = "XXXXXXXXXXXXXXX";
            string rdfTypePrefix = "RelatedXXXXXXXXXXXX";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetCursosSeminarios(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO - check
            {
                CursosSeminarios cursosSeminarios = new CursosSeminarios();
                cursosSeminarios.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.cursosSeminariosNombreEvento)?.values.FirstOrDefault();
                cursosSeminarios.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.cursosSeminariosFechaImparticion)?.values.FirstOrDefault();
                cursosSeminarios.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(cursosSeminarios.ID, cursosSeminarios);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = CursosSeminarios.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Publicaciones docentes o de carácter pedagógico, libros, artículos, etc.".
        /// Con el codigo identificativo 030.070.000.000
        /// </summary>
        public List<SubseccionItem> SincroPublicacionDocentes([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXX", "XXXXXXXXXXXXXX", "XXXXXXXXXXXX" };
            string graph = "XXXXXXXXXXXXXXXXXX";
            string propTitle = "XXXXXXXXXXXXX";
            string rdfType = "XXXXXXXXXXXXXXX";
            string rdfTypePrefix = "RelatedXXXXXXXXXXXX";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetPublicacionDocentes(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO - check
            {
                PublicacionDocentes publicacionDocentes = new PublicacionDocentes();
                publicacionDocentes.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.publicacionDocenteNombre)?.values.FirstOrDefault();
                publicacionDocentes.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.publicacionDocenteFechaPublicacion)?.values.FirstOrDefault();
                publicacionDocentes.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(publicacionDocentes.ID, publicacionDocentes);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = PublicacionDocentes.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Participación en proyectos de innovación docente".
        /// Con el codigo identificativo 030.080.000.000
        /// </summary>
        public List<SubseccionItem> SincroParticipacionProyectosInnovacionDocente([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXX", "XXXXXXXXXXXXXX", "XXXXXXXXXXXX" };
            string graph = "XXXXXXXXXXXXXXXXXX";
            string propTitle = "XXXXXXXXXXXXX";
            string rdfType = "XXXXXXXXXXXXXXX";
            string rdfTypePrefix = "RelatedXXXXXXXXXXXX";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetParticipacionProyectosInnovacionDocente(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO - check
            {
                ParticipacionProyectosInnovacionDocente participacionProyectosInnovacion = new ParticipacionProyectosInnovacionDocente();
                participacionProyectosInnovacion.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.participacionInnovaTitulo)?.values.FirstOrDefault();
                participacionProyectosInnovacion.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.participacionInnovaFechaInicio)?.values.FirstOrDefault();
                participacionProyectosInnovacion.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(participacionProyectosInnovacion.ID, participacionProyectosInnovacion);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = ParticipacionProyectosInnovacionDocente.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Participación en congresos con ponencias orientadas a la formación docente".
        /// Con el codigo identificativo 030.090.000.000
        /// </summary>
        public List<SubseccionItem> SincroParticipacionCongresosFormacionDocente([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXX", "XXXXXXXXXXXXXX", "XXXXXXXXXXXX" };
            string graph = "XXXXXXXXXXXXXXXXXX";
            string propTitle = "XXXXXXXXXXXXX";
            string rdfType = "XXXXXXXXXXXXXXX";
            string rdfTypePrefix = "RelatedXXXXXXXXXXXX";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetParticipacionCongresosFormacionDocente(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO - check
            {
                ParticipacionCongresosFormacionDocente participacionCongresos = new ParticipacionCongresosFormacionDocente();
                participacionCongresos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.participaCongresosTituloPublicacion)?.values.FirstOrDefault();
                participacionCongresos.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.participaCongresosFechaPresentacion)?.values.FirstOrDefault();
                participacionCongresos.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(participacionCongresos.ID, participacionCongresos);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = ParticipacionCongresosFormacionDocente.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al bloque 
        /// "Premios de innovación docente recibidos".
        /// Con el codigo identificativo 060.030.080.000
        /// </summary>
        public List<SubseccionItem> SincroPremiosInovacionDocente([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXX", "XXXXXXXXXXXXXX", "XXXXXXXXXXXX" };
            string graph = "XXXXXXXXXXXXXXXXXX";
            string propTitle = "XXXXXXXXXXXXX";
            string rdfType = "XXXXXXXXXXXXXXX";
            string rdfTypePrefix = "RelatedXXXXXXXXXXXX";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetPremiosInovacionDocente(listadoPremios);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO - check
            {
                PremiosInnovacionDocente premiosInnovacion = new PremiosInnovacionDocente();
                premiosInnovacion.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.premiosInnovaNombre)?.values.FirstOrDefault();
                premiosInnovacion.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.premiosInnovaFechaConcesion)?.values.FirstOrDefault();
                premiosInnovacion.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(premiosInnovacion.ID, premiosInnovacion);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = PremiosInnovacionDocente.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Otras actividades/méritos no incluidos en la relación anterior".
        /// Con el codigo identificativo 030.100.000.000
        /// </summary>
        public List<SubseccionItem> SincroOtrasActividades([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXX", "XXXXXXXXXXXXXX", "XXXXXXXXXXXX" };
            string graph = "XXXXXXXXXXXXXXXXXX";
            string propTitle = "XXXXXXXXXXXXX";
            string rdfType = "XXXXXXXXXXXXXXX";
            string rdfTypePrefix = "RelatedXXXXXXXXXXXX";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtrasActividades(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO - check
            {
                OtrasActividades otrasActividades = new OtrasActividades();
                otrasActividades.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.otrasActividadesDescripcion)?.values.FirstOrDefault();
                otrasActividades.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.otrasActividadesFechaFinalizacion)?.values.FirstOrDefault();
                otrasActividades.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(otrasActividades.ID, otrasActividades);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = OtrasActividades.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Aportaciones más relevantes de su CV de docencia".
        /// Con el codigo identificativo 030.110.000.000
        /// </summary>
        public List<SubseccionItem> SincroAportacionesRelevantes([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/XXXXXXXXXXXX", "XXXXXXXXXXXXXX", "XXXXXXXXXXXX" };
            string graph = "XXXXXXXXXXXXXXXXXX";
            string propTitle = "XXXXXXXXXXXXX";
            string rdfType = "XXXXXXXXXXXXXXX";
            string rdfTypePrefix = "RelatedXXXXXXXXXXXX";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetAportacionesRelevantes(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO - check
            {
                AportacionesRelevantes aportacionesRelevantes = new AportacionesRelevantes();
                aportacionesRelevantes.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.aportacionesCVDescripcion)?.values.FirstOrDefault();
                aportacionesRelevantes.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.aportacionesCVFechaFinalizacion)?.values.FirstOrDefault();
                aportacionesRelevantes.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(aportacionesRelevantes.ID, aportacionesRelevantes);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = AportacionesRelevantes.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// 030.040.000.000
        /// </summary>
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
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.direccionTesisTipoProyecto, item.GetStringPorIDCampo("030.040.000.010")),//TODO
                            new Property(Variables.ActividadDocente.direccionTesisTipoProyectoOtros, item.GetStringPorIDCampo("030.040.000.020")),
                            new Property(Variables.ActividadDocente.direccionTesisTituloTrabajo, item.GetStringPorIDCampo("030.040.000.030")),
                            new Property(Variables.ActividadDocente.direccionTesisCodirectorTesis, item.GetStringPorIDCampo("030.040.000.180")),//rep//TODO - funcion autor
                            new Property(Variables.ActividadDocente.direccionTesisPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.040.000.040")),
                            new Property(Variables.ActividadDocente.direccionTesisCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.040.000.050")),
                            new Property(Variables.ActividadDocente.direccionTesisCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.040.000.070")),
                            new Property(Variables.ActividadDocente.direccionTesisAlumno, item.GetStringPorIDCampo("030.040.000.120")),//TODO - funcion authorbean
                            new Property(Variables.ActividadDocente.direccionTesisFechaDefensa, item.GetStringDatetimePorIDCampo("030.040.000.140")),
                            new Property(Variables.ActividadDocente.direccionTesisCalificacionObtenida, item.GetStringPorIDCampo("030.040.000.150")),
                            new Property(Variables.ActividadDocente.direccionTesisFechaMencionDoctUE, item.GetStringDatetimePorIDCampo("030.040.000.160")),
                            new Property(Variables.ActividadDocente.direccionTesisMencionCalidad, item.GetStringBooleanPorIDCampo("030.040.000.170")),
                            new Property(Variables.ActividadDocente.direccionTesisDoctoradoUE, item.GetStringBooleanPorIDCampo("030.040.000.190")),
                            new Property(Variables.ActividadDocente.direccionTesisFechaMencionCalidad, item.GetStringDatetimePorIDCampo("030.040.000.200"))
                        ));
                        DireccionTesisPalabarasClave(item, entidadAux);
                        DireccionTesisEntidadRealizacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        private void DireccionTesisEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            /*
            new Property(Variables.ActividadDocente.direccionTesisEntidadRealizacion, item.GetNameEntityBeanPorIDCampo("030.040.000.080")),
            new Property(Variables.ActividadDocente.direccionTesisTipoEntidadRealizacion, item.GetStringPorIDCampo("030.040.000.100")),
            new Property(Variables.ActividadDocente.direccionTesisTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.040.000.110")),
            */
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.040.000.080"),
                Variables.ActividadDocente.direccionTesisEntidadRealizacionNombre,
                Variables.ActividadDocente.direccionTesisEntidadRealizacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.040.000.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetStringPorIDCampo("030.040.000.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.direccionTesisTipoEntidadRealizacion, valorTipo),
                new Property(Variables.ActividadDocente.direccionTesisTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.040.000.110"))
            ));
        }

        private void DireccionTesisPalabarasClave(CvnItemBean item, Entity entidadAux)
        {
            /*                            
            new Property(Variables.ActividadDocente.direccionTesisPalabrasClave, item.GetStringPorIDCampo("030.040.000.130")),
             */
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("030.040.000.130");

            string propiedadPalabrasClave = Variables.ActividadDocente.direccionTesisPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.direccionTesisPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// 030.010.000.000
        /// </summary>
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
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoDocenciaOficialidad, item.GetStringPorIDCampo("030.010.000.010")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaTitulacionUniversitaria, item.GetElementoPorIDCampo<CvnItemBeanCvnTitleBean>("030.010.000.020").Name),//TODO -check
                            new Property(Variables.ActividadDocente.formacionAcademicaPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.010.000.040")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.010.000.050")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.010.000.070")),
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
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanTipoConvocatoria, item.GetStringPorIDCampo("030.010.000.390")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanTipoConvocatoriaOtros, item.GetStringPorIDCampo("030.010.000.400")),
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanAmbitoGeo, item.GetStringPorIDCampo("030.010.000.410")),//TODO
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanAmbitoGeoOtros, item.GetStringPorIDCampo("030.010.000.420")),
                            new Property(Variables.ActividadDocente.formacionAcademicaFacultadEscuela, item.GetNameEntityBeanPorIDCampo("030.010.000.540")),
                            new Property(Variables.ActividadDocente.formacionAcademicaFechaInicio, item.GetStringDatetimePorIDCampo("030.010.000.550")),
                            new Property(Variables.ActividadDocente.formacionAcademicaFechaFinalizacion_, item.GetStringDatetimePorIDCampo("030.010.000.610"))
                        ));
                        FormacionAcademicaEntidadRealizacion(item, entidadAux);
                        FormacionAcademicaEntidadFinanciadora(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        private void FormacionAcademicaEntidadFinanciadora(CvnItemBean item, Entity entidadAux)
        {
            /*             
            new Property(Variables.ActividadDocente.formacionAcademicaEntidadFinanciadora, item.GetNameEntityBeanPorIDCampo("030.010.000.350")),
            new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadFinanciadora, item.GetStringPorIDCampo("030.010.000.370")),
            new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadFinanciadoraOtros, item.GetStringPorIDCampo("030.010.000.380")),
             */
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.010.000.350"),
                Variables.ActividadDocente.formacionAcademicaEntidadFinanciadoraNombre,
                Variables.ActividadDocente.formacionAcademicaEntidadFinanciadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.010.000.380")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetStringPorIDCampo("030.010.000.370");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadFinanciadora, valorTipo),
                new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadFinanciadoraOtros, item.GetStringPorIDCampo("030.010.000.380"))
            ));
        }

        private void FormacionAcademicaEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            /*
            new Property(Variables.ActividadDocente.formacionAcademicaEntidadRealizacion, item.GetNameEntityBeanPorIDCampo("030.010.000.080")),
            new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadRealizacion, item.GetStringPorIDCampo("030.010.000.110")),
            new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.010.000.120")),
            */
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.010.000.080"),
                Variables.ActividadDocente.formacionAcademicaEntidadRealizacionNombre,
                Variables.ActividadDocente.formacionAcademicaEntidadRealizacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.010.000.120")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetStringPorIDCampo("030.010.000.110");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadRealizacion, valorTipo),
                new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.010.000.120"))
            ));
        }

        /// <summary>
        /// 030.050.000.000
        /// </summary>
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
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.tutoAcademicaNombrePrograma, item.GetStringPorIDCampo("030.050.000.010")),//TODO
                            new Property(Variables.ActividadDocente.tutoAcademicaNombreProgramaOtros, item.GetStringPorIDCampo("030.050.000.020")),
                            new Property(Variables.ActividadDocente.tutoAcademicaPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.050.000.030")),
                            new Property(Variables.ActividadDocente.tutoAcademicaCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.050.000.040")),
                            new Property(Variables.ActividadDocente.tutoAcademicaCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.050.000.060")),
                            new Property(Variables.ActividadDocente.tutoAcademicaNumAlumnosTutelados, item.GetStringDoublePorIDCampo("030.050.000.110")),
                            new Property(Variables.ActividadDocente.tutoAcademicaFrecuenciaActividad, item.GetStringDoublePorIDCampo("030.050.000.120")),
                            new Property(Variables.ActividadDocente.tutoAcademicaNumHorasECTS, item.GetStringDoublePorIDCampo("030.050.000.130"))
                        ));
                        TutoriasAcademicasEntidadRealizacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        private void TutoriasAcademicasEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            /*
            new Property(Variables.ActividadDocente.tutoAcademicaEntidadRealizacion, item.GetNameEntityBeanPorIDCampo("030.050.000.070")),
            new Property(Variables.ActividadDocente.tutoAcademicaTipoEntidadRealizacion, item.GetStringPorIDCampo("030.050.000.090")),
            new Property(Variables.ActividadDocente.tutoAcademicaTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.050.000.100")),
            */
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.050.000.070"),
                Variables.ActividadDocente.tutoAcademicaEntidadRealizacionNombre,
                Variables.ActividadDocente.tutoAcademicaEntidadRealizacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.050.000.100")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetStringPorIDCampo("030.050.000.090");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.tutoAcademicaTipoEntidadRealizacion, valorTipo),
                new Property(Variables.ActividadDocente.tutoAcademicaTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.050.000.100"))
            ));
        }

        /// <summary>
        /// 030.060.000.000
        /// </summary>
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
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoEvento, item.GetStringPorIDCampo("030.060.000.010")),//TODO
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoEventoOtros, item.GetStringPorIDCampo("030.060.000.020")),
                            new Property(Variables.ActividadDocente.cursosSeminariosNombreEvento, item.GetStringPorIDCampo("030.060.000.030")),
                            new Property(Variables.ActividadDocente.cursosSeminariosPaisEntidadOrganizadora, item.GetPaisPorIDCampo("030.060.000.040")),
                            new Property(Variables.ActividadDocente.cursosSeminariosCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("030.060.000.050")),
                            new Property(Variables.ActividadDocente.cursosSeminariosCiudadEntidadOrganizadora, item.GetStringPorIDCampo("030.060.000.070")),
                            new Property(Variables.ActividadDocente.cursosSeminariosObjetivosCurso, item.GetStringPorIDCampo("030.060.000.120")),
                            new Property(Variables.ActividadDocente.cursosSeminariosPerfilDestinatarios, item.GetStringPorIDCampo("030.060.000.130")),
                            new Property(Variables.ActividadDocente.cursosSeminariosIdiomaImpartio, item.GetStringPorIDCampo("030.060.000.140")),//TODO
                            new Property(Variables.ActividadDocente.cursosSeminariosFechaImparticion, item.GetStringDatetimePorIDCampo("030.060.000.150")),
                            new Property(Variables.ActividadDocente.cursosSeminariosHorasImpartidas, item.GetStringDoublePorIDCampo("030.060.000.160")),
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoParticipacion, item.GetStringPorIDCampo("030.060.000.170")),//TODO
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoParticipacionOtros, item.GetStringPorIDCampo("030.060.000.180")),
                            new Property(Variables.ActividadDocente.cursosSeminariosAutorCorrespondencia, item.GetStringBooleanPorIDCampo("030.060.000.200"))
                        ));
                        CursosSeminariosEntidadOrganizadora(item, entidadAux);
                        CursosSeminariosISBN(item, entidadAux);
                        CursosSeminariosIDPublicacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        private void CursosSeminariosIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            //new Property(Variables.ActividadDocente.cursosSeminariosIdentificadorPublicacion, item.GetStringPorIDCampo("030.060.000.210"))
            //new Property(Variables.ActividadDocente.cursosSeminariosTipoIdentificadorPublicacion, item.GetStringPorIDCampo("030.060.000.220"))
            //new Property(Variables.ActividadDocente.cursosSeminariosTipoIdentificadorPublicacionOtros, item.GetStringPorIDCampo("030.060.000.230"))

            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.060.000.210");
            string propIdHandle = Variables.ActividadDocente.cursosSeminariosIDPubDigitalHandle;
            string propIdDOI = Variables.ActividadDocente.cursosSeminariosIDPubDigitalDOI;
            string propIdPMID = Variables.ActividadDocente.cursosSeminariosIDPubDigitalPMID;
            string propIdOtroPub = Variables.ActividadDocente.cursosSeminariosIDOtroPubDigital;
            string nombreOtroPub = Variables.ActividadDocente.cursosSeminariosNombreOtroIDPubDigital;

            UtilitySecciones.InsertaTiposIDPublicacion(listadoIDs, entidadAux, propIdHandle, propIdDOI, propIdPMID, propIdOtroPub, nombreOtroPub);
        }

        private void CursosSeminariosEntidadOrganizadora(CvnItemBean item, Entity entidadAux)
        {
            /*
            new Property(Variables.ActividadDocente.cursosSeminariosEntidadOrganizadora, item.GetNameEntityBeanPorIDCampo("030.060.000.080")),
            new Property(Variables.ActividadDocente.cursosSeminariosTipoEntidadOrganizadora, item.GetStringPorIDCampo("030.060.000.100")),
            new Property(Variables.ActividadDocente.cursosSeminariosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.060.000.110")),
            */
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.060.000.080"),
                Variables.ActividadDocente.cursosSeminariosEntidadOrganizadoraNombre,
                Variables.ActividadDocente.cursosSeminariosEntidadOrganizadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.060.000.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetStringPorIDCampo("030.060.000.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.cursosSeminariosTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadDocente.cursosSeminariosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.060.000.110"))
            ));
        }

        private void CursosSeminariosISBN(CvnItemBean item, Entity entidadAux)
        {
            /*                            
            new Property(Variables.ActividadDocente.cursosSeminariosISBN, item.GetStringPorIDCampo("030.060.000.190")),
             */
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.060.000.190");
            string propiedadISBN = Variables.ActividadDocente.cursosSeminariosISBN;

            UtilitySecciones.InsertaISBN(listadoISBN, entidadAux, propiedadISBN);
        }

        /// <summary>
        /// 030.070.000.000
        /// </summary>
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
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.publicacionDocenteNombre, item.GetStringPorIDCampo("030.070.000.010")),
                            new Property(Variables.ActividadDocente.publicacionDocentePerfilDestinatario, item.GetStringPorIDCampo("030.070.000.020")),
                            new Property(Variables.ActividadDocente.publicacionDocenteAutores, item.GetStringPorIDCampo("030.070.000.030")),//rep//TODO -autores?
                            new Property(Variables.ActividadDocente.publicacionDocentePosicionFirma, item.GetStringDoublePorIDCampo("030.070.000.040")),
                            new Property(Variables.ActividadDocente.publicacionDocenteFechaElaboracion, item.GetStringDatetimePorIDCampo("030.070.000.050")),
                            new Property(Variables.ActividadDocente.publicacionDocenteTipologiaSoporte, item.GetStringPorIDCampo("030.070.000.060")),//TODO
                            new Property(Variables.ActividadDocente.publicacionDocenteTipologiaSoporteOtros, item.GetStringPorIDCampo("030.070.000.070")),//TODO
                            new Property(Variables.ActividadDocente.publicacionDocenteTituloPublicacion, item.GetStringPorIDCampo("030.070.000.080")),
                            new Property(Variables.ActividadDocente.publicacionDocenteNombrePublicacion, item.GetStringPorIDCampo("030.070.000.190")),
                            new Property(Variables.ActividadDocente.publicacionDocenteVolumenPublicacion, item.GetVolumenPorIDCampo("030.070.000.090")),
                            new Property(Variables.ActividadDocente.publicacionDocentePagIniPublicacion, item.GetPaginaInicialPorIDCampo("030.070.000.100")),
                            new Property(Variables.ActividadDocente.publicacionDocentePagFinalPublicacion, item.GetPaginaFinalPorIDCampo("030.070.000.100")),
                            new Property(Variables.ActividadDocente.publicacionDocenteEditorialPublicacion, item.GetStringPorIDCampo("030.070.000.110")),
                            new Property(Variables.ActividadDocente.publicacionDocentePaisPublicacion, item.GetPaisPorIDCampo("030.070.000.120")),
                            new Property(Variables.ActividadDocente.publicacionDocenteCCAAPublicacion, item.GetRegionPorIDCampo("030.070.000.130")),
                            new Property(Variables.ActividadDocente.publicacionDocenteFechaPublicacion, item.GetStringDatetimePorIDCampo("030.070.000.150")),//TODO - check
                            new Property(Variables.ActividadDocente.publicacionDocenteURLPublicacion, item.GetStringPorIDCampo("030.070.000.160")),
                            new Property(Variables.ActividadDocente.publicacionDocenteDepositoLegal, item.GetElementoPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.070.000.180").Value),
                            new Property(Variables.ActividadDocente.publicacionDocenteJustificacionMaterial, item.GetStringPorIDCampo("030.070.000.200")),
                            new Property(Variables.ActividadDocente.publicacionDocenteGradoContribucion, item.GetStringPorIDCampo("030.070.000.210")),//TODO
                            new Property(Variables.ActividadDocente.publicacionDocenteAutorCorrespondencia, item.GetStringBooleanPorIDCampo("030.070.000.220"))
                        ));
                        PublicacionDocentesISBN(item, entidadAux);
                        PublicacionDocentesIDPublicacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        private void PublicacionDocentesIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            //new Property(Variables.ActividadDocente.publicacionDocenteIdentificadorPublicacion, item.GetStringPorIDCampo("030.070.000.230")),
            //new Property(Variables.ActividadDocente.publicacionDocenteTipoIdentificadorPublicacion, item.GetStringPorIDCampo("030.070.000.240")),
            //new Property(Variables.ActividadDocente.publicacionDocenteTipoIdentificadorPublicacionOtros, item.GetStringPorIDCampo("030.070.000.250"))

            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.070.000.230");
            string propIdHandle = Variables.ActividadDocente.publicacionDocenteIDPubDigitalHandle;
            string propIdDOI = Variables.ActividadDocente.publicacionDocenteIDPubDigitalDOI;
            string propIdPMID = Variables.ActividadDocente.publicacionDocenteIDPubDigitalPMID;
            string propIdOtroPub = Variables.ActividadDocente.publicacionDocenteIDOtroPubDigital;
            string nombreOtroPub = Variables.ActividadDocente.publicacionDocenteNombreOtroIDPubDigital;

            UtilitySecciones.InsertaTiposIDPublicacion(listadoIDs, entidadAux, propIdHandle, propIdDOI, propIdPMID, propIdOtroPub, nombreOtroPub);
        }

        private void PublicacionDocentesISBN(CvnItemBean item, Entity entidadAux)
        {                         
            //new Property(Variables.ActividadDocente.publicacionDocenteISBNPublicacion, item.GetStringPorIDCampo("030.070.000.170"))

            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.070.000.170");
            string propiedadISBN = Variables.ActividadDocente.publicacionDocenteISBNPublicacion;

            UtilitySecciones.InsertaISBN(listadoISBN, entidadAux, propiedadISBN);
        }

        /// <summary>
        /// 030.080.000.000
        /// </summary>
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
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.participacionInnovaTitulo, item.GetStringPorIDCampo("030.080.000.010")),
                            new Property(Variables.ActividadDocente.participacionInnovaPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.080.000.020")),
                            new Property(Variables.ActividadDocente.participacionInnovaCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.080.000.030")),
                            new Property(Variables.ActividadDocente.participacionInnovaCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.080.000.250")),
                            new Property(Variables.ActividadDocente.participacionInnovaTipoParticipacion, item.GetStringPorIDCampo("030.080.000.050")),//TODO
                            new Property(Variables.ActividadDocente.participacionInnovaTipoParticipacionOtros, item.GetStringPorIDCampo("030.080.000.060")),
                            new Property(Variables.ActividadDocente.participacionInnovaAportacionProyecto, item.GetStringPorIDCampo("030.080.000.070")),
                            new Property(Variables.ActividadDocente.participacionInnovaRegimenDedicacion, item.GetStringPorIDCampo("030.080.000.080")),//TODO
                            new Property(Variables.ActividadDocente.participacionInnovaTipoConvocatoria, item.GetStringPorIDCampo("030.080.000.130")),//TODO
                            new Property(Variables.ActividadDocente.participacionInnovaTipoConvocatoriaOtros, item.GetStringPorIDCampo("030.080.000.140")),
                            new Property(Variables.ActividadDocente.participacionInnovaTipoDuracionRelacionLaboral, item.GetStringPorIDCampo("030.080.000.190")),//TODO
                            new Property(Variables.ActividadDocente.participacionInnovaDuracionParticipacionAnio, item.GetDurationAnioPorIDCampo("030.080.000.200")),
                            new Property(Variables.ActividadDocente.participacionInnovaDuracionParticipacionMes, item.GetDurationMesPorIDCampo("030.080.000.200")),
                            new Property(Variables.ActividadDocente.participacionInnovaDuracionParticipacionDia, item.GetDurationDiaPorIDCampo("030.080.000.200")),
                            new Property(Variables.ActividadDocente.participacionInnovaFechaFinalizacionParticipacion, item.GetStringDatetimePorIDCampo("030.080.000.210")),
                            new Property(Variables.ActividadDocente.participacionInnovaNombreIP, item.GetStringPorIDCampo("030.080.000.220")),//TODO - funcion autor
                            new Property(Variables.ActividadDocente.participacionInnovaNumParticipantes, item.GetStringDoublePorIDCampo("030.080.000.230")),
                            new Property(Variables.ActividadDocente.participacionInnovaImporteConcedido, item.GetStringDoublePorIDCampo("030.080.000.240")),
                            new Property(Variables.ActividadDocente.participacionInnovaAmbitoProyecto, item.GetAmbitoGestion("030.080.000.260")),//TODO - check
                            new Property(Variables.ActividadDocente.participacionInnovaAmbitoProyectoOtros, item.GetStringPorIDCampo("030.080.000.270")),
                            new Property(Variables.ActividadDocente.participacionInnovaFechaInicio, item.GetStringDatetimePorIDCampo("030.080.000.280"))
                        ));
                        ParticipacionProyectosInnovacionDocenteEntidadFinanciadora(item, entidadAux);
                        ParticipacionProyectosInnovacionDocenteEntidadParticipante(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        private void ParticipacionProyectosInnovacionDocenteEntidadFinanciadora(CvnItemBean item, Entity entidadAux)
        {                      
            //new Property(Variables.ActividadDocente.participacionInnovaEntidadFinanciadora, item.GetNameEntityBeanPorIDCampo("030.080.000.090")),
            //new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadFinanciadora, item.GetStringPorIDCampo("030.080.000.110")),
            //new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadFinanciadoraOtros, item.GetStringPorIDCampo("030.080.000.120")),

            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.080.000.090"),
                Variables.ActividadDocente.participacionInnovaEntidadFinanciadoraNombre,
                Variables.ActividadDocente.participacionInnovaEntidadFinanciadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.080.000.120")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetStringPorIDCampo("030.080.000.110");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadFinanciadora, valorTipo),
                new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadFinanciadoraOtros, item.GetStringPorIDCampo("030.080.000.120"))
            ));
        }
        private void ParticipacionProyectosInnovacionDocenteEntidadParticipante(CvnItemBean item, Entity entidadAux)
        {
            //new Property(Variables.ActividadDocente.participacionInnovaEntidadParticipante, item.GetNameEntityBeanPorIDCampo("030.080.000.150")),
            //new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadParticipante, item.GetStringPorIDCampo("030.080.000.170")),
            //new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadParticipanteOtros, item.GetStringPorIDCampo("030.080.000.180")),

            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.080.000.150"),
                Variables.ActividadDocente.participacionInnovaEntidadParticipanteNombre,
                Variables.ActividadDocente.participacionInnovaEntidadParticipante, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.080.000.180")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetStringPorIDCampo("030.080.000.170");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadParticipante, valorTipo),
                new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadParticipanteOtros, item.GetStringPorIDCampo("030.080.000.180"))
            ));
        }

        /// <summary>
        /// 030.090.000.000
        /// </summary>
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
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.090.000.030")))//TODO-check, añadir valores de URL
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.participaCongresosTipoEvento, item.GetStringPorIDCampo("030.090.000.010")),
                            new Property(Variables.ActividadDocente.participaCongresosTipoEventoOtros, item.GetStringPorIDCampo("030.090.000.020")),
                            new Property(Variables.ActividadDocente.participaCongresosNombreEvento, item.GetStringPorIDCampo("030.090.000.030")),
                            new Property(Variables.ActividadDocente.participaCongresosPaisEvento, item.GetPaisPorIDCampo("030.090.000.040")),
                            new Property(Variables.ActividadDocente.participaCongresosCCAAEvento, item.GetRegionPorIDCampo("030.090.000.050")),
                            new Property(Variables.ActividadDocente.participaCongresosCiudadEvento, item.GetStringPorIDCampo("030.090.000.070")),
                            new Property(Variables.ActividadDocente.participaCongresosPaisEntidadOrganizadora, item.GetPaisPorIDCampo("030.090.000.190")),
                            new Property(Variables.ActividadDocente.participaCongresosCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("030.090.000.200")),
                            new Property(Variables.ActividadDocente.participaCongresosCiudadEntidadOrganizadora, item.GetStringPorIDCampo("030.090.000.210")),
                            new Property(Variables.ActividadDocente.participaCongresosObjetivosEvento, item.GetStringPorIDCampo("030.090.000.120")),
                            new Property(Variables.ActividadDocente.participaCongresosPerfilDestinatarios, item.GetStringPorIDCampo("030.090.000.130")),
                            new Property(Variables.ActividadDocente.participaCongresosIdiomaPresentacion, item.GetStringPorIDCampo("030.090.000.140")),
                            new Property(Variables.ActividadDocente.participaCongresosFechaPresentacion, item.GetStringDatetimePorIDCampo("030.090.000.150")),
                            new Property(Variables.ActividadDocente.participaCongresosTipoParticipacion, item.GetStringPorIDCampo("030.090.000.160")),
                            new Property(Variables.ActividadDocente.participaCongresosTipoParticipacionOtros, item.GetStringPorIDCampo("030.090.000.170")),
                            new Property(Variables.ActividadDocente.participaCongresosTipoPublicacion, item.GetStringPorIDCampo("030.090.000.220")),
                            new Property(Variables.ActividadDocente.participaCongresosTituloPublicacion, item.GetStringPorIDCampo("030.090.000.230")),
                            new Property(Variables.ActividadDocente.participaCongresosNombrePublicacion, item.GetStringPorIDCampo("030.090.000.330")),
                            new Property(Variables.ActividadDocente.participaCongresosVolumenPublicacion, item.GetVolumenPorIDCampo("030.090.000.240")),
                            new Property(Variables.ActividadDocente.participaCongresosPagIniPublicacion, item.GetPaginaInicialPorIDCampo("030.090.000.250")),
                            new Property(Variables.ActividadDocente.participaCongresosPagFinalPublicacion, item.GetPaginaFinalPorIDCampo("030.090.000.250")),
                            new Property(Variables.ActividadDocente.participaCongresosEditorialPublicacion, item.GetStringPorIDCampo("030.090.000.260")),
                            new Property(Variables.ActividadDocente.participaCongresosPaisPublicacion, item.GetPaisPorIDCampo("030.090.000.270")),
                            new Property(Variables.ActividadDocente.participaCongresosCCAAPublicacion, item.GetRegionPorIDCampo("030.090.000.280")),
                            new Property(Variables.ActividadDocente.participaCongresosFechaPublicacion, item.GetStringDatetimePorIDCampo("030.090.000.300")),
                            new Property(Variables.ActividadDocente.participaCongresosURLPublicacion, item.GetStringPorIDCampo("030.090.000.310")),
                            new Property(Variables.ActividadDocente.participaCongresosDepositoLegalPublicacion, item.GetElementoPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.090.000.320").Value),
                            new Property(Variables.ActividadDocente.participaCongresosNumHorasPublicacion, item.GetDurationHorasPorIDCampo("030.090.000.340")),
                            new Property(Variables.ActividadDocente.participaCongresosFechaInicioPublicacion, item.GetStringDatetimePorIDCampo("030.090.000.350")),
                            new Property(Variables.ActividadDocente.participaCongresosFechaFinalPublicacion, item.GetStringDatetimePorIDCampo("030.090.000.360")),
                            new Property(Variables.ActividadDocente.participaCongresosAutorCorrespondencia, item.GetStringBooleanPorIDCampo("030.090.000.400"))
                        ));
                        ParticipacionCongresosFormacionDocenteEntidadOrganizadora(item, entidadAux);
                        ParticipacionCongresosFormacionDocenteISBN(item, entidadAux);
                        ParticipacionCongresosFormacionDocenteIDPublicacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        private void ParticipacionCongresosFormacionDocenteIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            //new Property(Variables.ActividadDocente.participaCongresosIdentificadorPublicacion, item.GetElementoPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.090.000.370").Value),
            //new Property(Variables.ActividadDocente.participaCongresosTipoIDPublicacion, item.GetStringPorIDCampo("030.090.000.380")),
            //new Property(Variables.ActividadDocente.participaCongresosTipoIDSPublicacionOtros, item.GetStringPorIDCampo("030.090.000.390")),

            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.090.000.370");
            string propIdHandle = Variables.ActividadDocente.participaCongresosIDPubDigitalHandle;
            string propIdDOI = Variables.ActividadDocente.participaCongresosIDPubDigitalDOI;
            string propIdPMID = Variables.ActividadDocente.participaCongresosIDPubDigitalPMID;
            string propIdOtroPub = Variables.ActividadDocente.participaCongresosIDOtroPubDigital;
            string nombreOtroPub = Variables.ActividadDocente.participaCongresosNombreOtroIDPubDigital;

            UtilitySecciones.InsertaTiposIDPublicacion(listadoIDs, entidadAux, propIdHandle, propIdDOI, propIdPMID, propIdOtroPub, nombreOtroPub);
        }

        private void ParticipacionCongresosFormacionDocenteISBN(CvnItemBean item, Entity entidadAux)
        {
            //new Property(Variables.ActividadDocente.participaCongresosISBNPublicacion, item.GetStringPorIDCampo("030.090.000.180"))
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.090.000.180");
            string propiedadISBN = Variables.ActividadDocente.participaCongresosISBNPublicacion;

            UtilitySecciones.InsertaISBN(listadoISBN, entidadAux, propiedadISBN);
        }

        private void ParticipacionCongresosFormacionDocenteEntidadOrganizadora(CvnItemBean item, Entity entidadAux)
        {
            /*
            new Property(Variables.ActividadDocente.participaCongresosEntidadOrganizadora, item.GetNameEntityBeanPorIDCampo("030.090.000.080")),
            new Property(Variables.ActividadDocente.participaCongresosTipoEntidadOrganizadora, item.GetStringPorIDCampo("030.090.000.100")),
            new Property(Variables.ActividadDocente.participaCongresosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.090.000.110")),
            */
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.090.000.080"),
                Variables.ActividadDocente.participaCongresosEntidadOrganizadoraNombre,
                Variables.ActividadDocente.participaCongresosEntidadOrganizadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.090.000.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetStringPorIDCampo("030.090.000.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.participaCongresosTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadDocente.participaCongresosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.090.000.110"))
            ));
        }

        /// <summary>
        /// 060.030.080.000
        /// </summary>
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
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.premiosInnovaNombre, item.GetStringPorIDCampo("060.030.080.010")),
                            new Property(Variables.ActividadDocente.premiosInnovaPaisEntidadConcesionaria, item.GetPaisPorIDCampo("060.030.080.020")),
                            new Property(Variables.ActividadDocente.premiosInnovaCCAAEntidadConcesionaria, item.GetRegionPorIDCampo("060.030.080.030")),
                            new Property(Variables.ActividadDocente.premiosInnovaCiudadEntidadConcesionaria, item.GetStringPorIDCampo("060.030.080.110")),
                            new Property(Variables.ActividadDocente.premiosInnovaPropuestaDe, item.GetStringPorIDCampo("060.030.080.090")),
                            new Property(Variables.ActividadDocente.premiosInnovaFechaConcesion, item.GetStringDatetimePorIDCampo("060.030.080.100"))
                        ));
                        PremiosInnovacionDocenteEntidadConcesionaria(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        private void PremiosInnovacionDocenteEntidadConcesionaria(CvnItemBean item, Entity entidadAux)
        {
            /*             
            new Property(Variables.ActividadDocente.premiosInnovaEntidadConcesionaria, item.GetNameEntityBeanPorIDCampo("060.030.080.050")),
            new Property(Variables.ActividadDocente.premiosInnovaTipoEntidadConcesionaria, item.GetStringPorIDCampo("060.030.080.070")),
            new Property(Variables.ActividadDocente.premiosInnovaTipoEntidadConcesionariaOtros, item.GetStringPorIDCampo("060.030.080.080")),
             */
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.080.050"),
                Variables.ActividadDocente.premiosInnovaEntidadConcesionariaNombre,
                Variables.ActividadDocente.premiosInnovaEntidadConcesionaria, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.080.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetStringPorIDCampo("060.030.080.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.premiosInnovaTipoEntidadConcesionaria, valorTipo),
                new Property(Variables.ActividadDocente.premiosInnovaTipoEntidadConcesionariaOtros, item.GetStringPorIDCampo("060.030.080.080"))
            ));
        }

        /// <summary>
        /// 030.100.000.000
        /// </summary>
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
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.otrasActividadesDescripcion, item.GetStringPorIDCampo("030.100.000.010")),
                            new Property(Variables.ActividadDocente.otrasActividadesPaisRealizacion, item.GetPaisPorIDCampo("030.100.000.030")),
                            new Property(Variables.ActividadDocente.otrasActividadesCCAARealizacion, item.GetRegionPorIDCampo("030.100.000.040")),
                            new Property(Variables.ActividadDocente.otrasActividadesCiudadRealizacion, item.GetStringPorIDCampo("030.100.000.060")),
                            new Property(Variables.ActividadDocente.otrasActividadesFechaFinalizacion, item.GetStringDatetimePorIDCampo("030.100.000.110"))
                        ));
                        OtrasActividadesPalabrasClave(item, entidadAux);
                        OtrasActividadesEntidadOrganizadora(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        private void OtrasActividadesPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            //new Property(Variables.ActividadDocente.otrasActividadesPalabrasClave, item.GetStringPorIDCampo("030.100.000.020"))
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("030.100.000.020");

            string propiedadPalabrasClave = Variables.ActividadDocente.otrasActividadesPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.otrasActividadesPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        private void OtrasActividadesEntidadOrganizadora(CvnItemBean item, Entity entidadAux)
        {
            /*
            new Property(Variables.ActividadDocente.otrasActividadesEntidadOrganizadora, item.GetNameEntityBeanPorIDCampo("030.100.000.070")),
            new Property(Variables.ActividadDocente.otrasActividadesTipoEntidadOrganizadora, item.GetStringPorIDCampo("030.100.000.090")),
            new Property(Variables.ActividadDocente.otrasActividadesTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.100.000.100")),
            */
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.100.000.070"),
                Variables.ActividadDocente.otrasActividadesEntidadOrganizadoraNombre,
                Variables.ActividadDocente.otrasActividadesEntidadOrganizadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.100.000.100")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetStringPorIDCampo("030.100.000.090");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.otrasActividadesTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadDocente.otrasActividadesTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.100.000.100"))
            ));
        }

        /// <summary>
        /// 030.110.000.000
        /// </summary>
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
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.aportacionesCVDescripcion, item.GetStringPorIDCampo("030.110.000.010")),
                            new Property(Variables.ActividadDocente.aportacionesCVPaisRealizacion, item.GetPaisPorIDCampo("030.110.000.030")),
                            new Property(Variables.ActividadDocente.aportacionesCVCCAARealizacion, item.GetRegionPorIDCampo("030.110.000.040")),
                            new Property(Variables.ActividadDocente.aportacionesCVCiudadRealizacion, item.GetStringPorIDCampo("030.110.000.060")),
                            new Property(Variables.ActividadDocente.aportacionesCVFechaFinalizacion, item.GetStringDatetimePorIDCampo("030.110.000.110"))
                        ));
                        AportacionesRelevantesPalabrasClave(item, entidadAux);
                        AportacionesRelevantesEntidadOrganizadora(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        private void AportacionesRelevantesEntidadOrganizadora(CvnItemBean item, Entity entidadAux)
        {
            /*
            new Property(Variables.ActividadDocente.aportacionesCVEntidadOrganizadora, item.GetNameEntityBeanPorIDCampo("030.110.000.070")),
            new Property(Variables.ActividadDocente.aportacionesCVTipoEntidadOrganizadora, item.GetStringPorIDCampo("030.110.000.090")),
            new Property(Variables.ActividadDocente.aportacionesCVTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.110.000.100")),
            */
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.110.000.070"),
                Variables.ActividadDocente.aportacionesCVEntidadOrganizadoraNombre,
                Variables.ActividadDocente.aportacionesCVEntidadOrganizadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.110.000.100")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetStringPorIDCampo("030.110.000.090");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.aportacionesCVTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadDocente.aportacionesCVTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.110.000.100"))
            ));
        }

        private void AportacionesRelevantesPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            //new Property(Variables.ActividadDocente.aportacionesCVPalabrasClave, item.GetStringPorIDCampo("030.110.000.020"))
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("030.110.000.020");

            string propiedadPalabrasClave = Variables.ActividadDocente.aportacionesCVPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.aportacionesCVPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }
    }
}
