﻿using CurriculumvitaeOntology;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DesnormalizadorHercules.Models
{
    /// <summary>
    /// Clase para actualizar propiedades de CVs
    /// </summary>
    class ActualizadorCV : ActualizadorBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pResourceApi">API Wrapper de GNOSS</param>
        public ActualizadorCV(ResourceApi pResourceApi) : base(pResourceApi)
        {
        }

        /// <summary>
        /// Crea un currículum para todos los investigadores activos (http://w3id.org/roh/isActive 'true')
        /// NO se realizan eliminaciones
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        public void CrearCVs(string pPerson = null)
        {
            string filter = "";
            if (!string.IsNullOrEmpty(pPerson))
            {
                filter = $" FILTER(?person =<{pPerson}>)";
            }

            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                while (true)
                {
                    //Creamos CVs
                    int limit = 50;
                    //TODO eliminar from
                    String select = @"SELECT distinct ?person from <http://gnoss.com/curriculumvitae.owl> ";
                    String where = @$"  where{{
                                            {filter}
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?person <http://w3id.org/roh/isActive> 'true'.
                                            MINUS{{ ?cv  <http://w3id.org/roh/cvOf> ?person}}
                                        }} limit {limit}";

                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV y deberían tenerlo
                    List<string> persons = new();
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        persons.Add(fila["person"].value);
                    }

                    // Obtenemos los CV a cargar
                    mResourceApi.ChangeOntoly("curriculumvitae");
                    List<CV> listaCVCargar = GenerateCVFromPersons(persons);
                    Parallel.ForEach(listaCVCargar, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, cv =>
                    {
                        ComplexOntologyResource resource = cv.ToGnossApiResource(mResourceApi, new());
                        if (listaCVCargar.Last() == cv)
                        {
                            mResourceApi.LoadComplexSemanticResource(resource, true, true);
                        }
                        else
                        {
                            mResourceApi.LoadComplexSemanticResource(resource);
                        }
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Insertamos/eliminamos en los CV las publicaciones de las que el dueño del CV es autor con la privacidad correspondiente
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>        
        /// <param name="pPerson">ID de la persona</param>
        /// <param name="pDocument">ID del documento</param>
        /// <param name="pCV">ID del CV</param>
        public void ModificarDocumentos(string pPerson = null, string pDocument = null, string pCV = null)
        {
            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pPerson))
                {
                    filter = $" FILTER(?person =<{pPerson}>)";
                }
                if (!string.IsNullOrEmpty(pDocument))
                {
                    filter = $" FILTER(?document =<{pDocument}>)";
                }
                if (!string.IsNullOrEmpty(pCV))
                {
                    filter = $" FILTER(?cv =<{pCV}>)";
                }

                while (true)
                {
                    //Añadimos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificActivity ?document ?isValidated ?typeDocument  from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificactivitydocument.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?scientificActivity ?document ?isValidated ?typeDocument
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?document a <http://purl.org/ontology/bibo/Document>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                            ?document <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?document <http://w3id.org/roh/scientificActivityDocument> ?scientificActivityDocument.
                                            ?document <http://w3id.org/roh/isValidated> ?isValidated.
                                            ?scientificActivityDocument <http://purl.org/dc/elements/1.1/identifier> ?typeDocument.
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/scientificPublications> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD1"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedConferences> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD2"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedSeminars> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD3"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/otherDisseminationActivities> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD4"" as ?typeDocument)
                                        }}
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarDocumentosCV(resultado, graphsUrl);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificActivity ?item ?typeDocument from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificactivitydocument.owl>  ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/scientificPublications> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD1"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedConferences> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD2"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedSeminars> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD3"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/otherDisseminationActivities> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD4"" as ?typeDocument)
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?scientificActivity ?document ?typeDocument
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?document a <http://purl.org/ontology/bibo/Document>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                            ?document <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?document <http://w3id.org/roh/scientificActivityDocument> ?scientificActivityDocument.
                                            ?scientificActivityDocument <http://purl.org/dc/elements/1.1/identifier> ?typeDocument.
                                        }}                                        
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarDocumentosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Modifica la privacidad de las publicaciones de los CV en caso de que haya que hacerlo
        /// (Solo convierte en públicos aquellos documentos que sean privados pero deberían ser públicos)
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        /// <param name="pDocument">ID del documento</param>
        /// <param name="pCV">ID del CV</param>
        public void CambiarPrivacidadDocumentos(string pPerson = null, string pDocument = null, string pCV = null)
        {
            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pPerson))
                {
                    filter = $" FILTER(?person =<{pPerson}>)";
                }
                if (!string.IsNullOrEmpty(pDocument))
                {
                    filter = $" FILTER(?document =<{pDocument}>)";
                }
                if (!string.IsNullOrEmpty(pCV))
                {
                    filter = $" FILTER(?cv =<{pCV}>)";
                }

                while (true)
                {
                    //Publicamos los documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificActivity ?propItem ?item from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?document <http://w3id.org/roh/isValidated> 'true'.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.                                        
                                        ?scientificActivity ?propItem ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                        ?item <http://w3id.org/roh/isPublic> 'false'.
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    PublicarDocumentosCV(resultado, graphsUrl);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Insertamos/eliminamos en los CV los proyectos oficiales (con http://w3id.org/roh/crisIdentifier ) de los que el dueño del CV es miembro y les ponemos privacidad pública
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>        
        /// <param name="pPerson">ID de la persona</param>
        /// <param name="pProyecto">ID del documento</param>
        /// <param name="pCV">ID del CV</param>
        public void ModificarProyectos(string pPerson = null, string pProyecto = null, string pCV = null)
        {
            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pPerson))
                {
                    filter = $" FILTER(?person =<{pPerson}>)";
                }
                if (!string.IsNullOrEmpty(pProyecto))
                {
                    filter = $" FILTER(?project =<{pProyecto}>)";
                }
                if (!string.IsNullOrEmpty(pCV))
                {
                    filter = $" FILTER(?cv =<{pCV}>)";
                }
                while (true)
                {
                    //Añadimos proyectos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificExperience ?project ?typeProject from <http://gnoss.com/project.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificexperienceproject.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        #DESEABLES
                                        select distinct ?cv ?scientificExperience ?project ?typeProject 
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?project <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                            ?project ?propRol ?rol.
                                            FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?project <http://w3id.org/roh/scientificExperienceProject> ?scientificExperienceProject.
                                            ?scientificExperienceProject <http://purl.org/dc/elements/1.1/identifier> ?typeProject.
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?project <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                        {{
                                                ?scientificExperience <http://w3id.org/roh/competitiveProjects> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                                BIND(""SEP1"" as ?typeProject)
                                        }}
                                        UNION
                                        {{
                                                ?scientificExperience <http://w3id.org/roh/nonCompetitiveProjects> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                                BIND(""SEP2"" as ?typeProject)
                                        }}
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarProyectosCV(resultado, graphsUrl);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos proyectos
                    int limit = 500;
                    //TODO eliminar from select distinct ?cv ?scientificActivity ?item ?typeDocument
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificExperience ?project ?item ?typeProject from <http://gnoss.com/project.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificexperienceproject.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?project <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                        {{
                                                ?scientificExperience <http://w3id.org/roh/competitiveProjects> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                                BIND(""SEP1"" as ?typeProject)
                                        }}
                                        UNION
                                        {{
                                                ?scientificExperience <http://w3id.org/roh/nonCompetitiveProjects> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                                BIND(""SEP2"" as ?typeProject)
                                        }}                                       
                                    
                                    MINUS
                                    {{
                                        #DESEABLES
                                        select distinct ?cv ?scientificExperience ?project ?typeProject 
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?project <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                            ?project ?propRol ?rol.
                                            FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?project <http://w3id.org/roh/scientificExperienceProject> ?scientificExperienceProject.
                                            ?scientificExperienceProject <http://purl.org/dc/elements/1.1/identifier> ?typeProject.
                                        }}
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarProyectosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Insertamos/eliminamos en los CV los grupos oficiales (con http://w3id.org/roh/crisIdentifier ) de los que el dueño del CV es miembro y les ponemos privacidad pública
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>        
        /// <param name="pPerson">ID de la persona</param>
        /// <param name="pGroup">ID del documento</param>
        /// <param name="pCV">ID del CV</param>
        public void ModificarGrupos(string pPerson = null, string pGroup = null, string pCV = null)
        {
            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pPerson))
                {
                    filter = $" FILTER(?person =<{pPerson}>)";
                }
                if (!string.IsNullOrEmpty(pGroup))
                {
                    filter = $" FILTER(?group =<{pGroup}>)";
                }
                if (!string.IsNullOrEmpty(pCV))
                {
                    filter = $" FILTER(?cv =<{pCV}>)";
                }
                while (true)
                {
                    //Añadimos grupos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificExperience ?group from <http://gnoss.com/group.owl> from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter} 
                                    {{
                                        #DESEABLES                                        
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.                                            
                                        {{
                                            ?group <http://w3id.org/roh/mainResearchers> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
                                        }}
                                        UNION
                                        {{
                                            ?group <http://xmlns.com/foaf/0.1/member> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                        ?scientificExperience <http://w3id.org/roh/groups> ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?group.
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarItemsCV(resultado, graphsUrl, "http://w3id.org/roh/RelatedGroup", "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/groups", "group", "scientificExperience",true);

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos grupos
                    int limit = 500;
                    //TODO eliminar from 
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificExperience ?group ?item from <http://gnoss.com/group.owl> from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                        ?scientificExperience <http://w3id.org/roh/groups> ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?group.                                
                                    }}
                                    MINUS
                                    {{
                                        #DESEABLES                                        
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.                                            
                                        {{
                                            ?group <http://w3id.org/roh/mainResearchers> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
                                        }}
                                        UNION
                                        {{
                                            ?group <http://xmlns.com/foaf/0.1/member> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
                                        }}
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarItemsCV(resultado, "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/groups", "item", "scientificExperience");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Genera objetos CV de las personas pasadas por parámetro
        /// </summary>
        /// <param name="personsIDs"></param>
        /// <returns></returns>
        private List<CV> GenerateCVFromPersons(List<string> personsIDs)
        {
            Dictionary<string, CV> listaCV = new();
            if (personsIDs.Count > 0)
            {
                var personasIDsStr = string.Join(',', personsIDs.Select(item => "<" + item + ">"));

                //Nombre
                {
                    String select = @"SELECT DISTINCT ?person ?name ?firstName ?lastName";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <http://xmlns.com/foaf/0.1/name> ?name.
                                        OPTIONAL{{?person <http://xmlns.com/foaf/0.1/firstName> ?firstName}}
                                        OPTIONAL{{?person <http://xmlns.com/foaf/0.1/lastName> ?lastName}}
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string name = fila["name"].value;
                        string firstName = "";
                        string lastName = "";
                        if (fila.ContainsKey("firstName"))
                        {
                            firstName = fila["firstName"].value;
                        }
                        if (fila.ContainsKey("lastName"))
                        {
                            lastName = fila["lastName"].value;
                        }
                        CV cv = new();
                        if (listaCV.ContainsKey(person))
                        {
                            cv = listaCV[person];
                        }
                        else
                        {
                            listaCV.Add(person, cv);
                        }
                        cv.Foaf_name = name;
                        cv.IdRoh_cvOf = person;
                        cv.Roh_scientificExperience = new ScientificExperience() { Roh_title = "-" };
                        cv.Roh_scientificActivity = new ScientificActivity() { Roh_title = "-" };
                        cv.Roh_personalData = new PersonalData() { Foaf_firstName = firstName, Foaf_familyName = lastName };
                    }
                }

                //Email
                {
                    String select = @"SELECT DISTINCT ?person ?email";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <https://www.w3.org/2006/vcard/ns#email> ?email.
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string email = fila["email"].value;
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            cv.Roh_personalData.Vcard_email = email;
                        }
                    }
                }

                //Teléfono
                {
                    String select = @"SELECT DISTINCT ?person ?telephone";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <https://www.w3.org/2006/vcard/ns#hasTelephone> ?telephone.
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string telephone = fila["telephone"].value;
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            cv.Roh_personalData.Vcard_hasTelephone = new TelephoneType() { Vcard_hasValue = telephone };
                        }
                    }
                }

                //Página
                {
                    String select = @"SELECT DISTINCT ?person ?homepage";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <http://xmlns.com/foaf/0.1/homepage> ?homepage.
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string homepage = fila["homepage"].value;
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            cv.Roh_personalData.Foaf_homepage = homepage;
                        }
                    }
                }

                //ORCID
                //SCOPUS
                //ResearcherId
                {
                    String select = @"SELECT DISTINCT ?person ?orcid ?scopusId ?researcherId";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        OPTIONAL{{?person <http://w3id.org/roh/ORCID> ?orcid.}}
                                        OPTIONAL{{?person <http://vivoweb.org/ontology/core#scopusId> ?scopusId.}}
                                        OPTIONAL{{?person <http://vivoweb.org/ontology/core#researcherId> ?researcherId.}}
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string orcid = "";
                        string scopusId = "";
                        string researcherId = "";
                        if (fila.ContainsKey("orcid"))
                        {
                            orcid = fila["orcid"].value;
                        }
                        if (fila.ContainsKey("scopusId"))
                        {
                            scopusId = fila["scopusId"].value;
                        }
                        if (fila.ContainsKey("researcherId"))
                        {
                            researcherId = fila["researcherId"].value;
                        }
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            cv.Roh_personalData.Roh_ORCID = orcid;
                            cv.Roh_personalData.Vivo_scopusId = scopusId;
                            cv.Roh_personalData.Vivo_researcherId = researcherId;
                        }
                    }
                }

                //Otros IDs
                {
                    String select = @"SELECT DISTINCT ?person ?semanticScholarId";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <http://w3id.org/roh/semanticScholarId> ?semanticScholarId.
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string semanticScholarId = fila["semanticScholarId"].value;
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            if (cv.Roh_personalData.Roh_otherIds == null)
                            {
                                cv.Roh_personalData.Roh_otherIds = new List<Document>();
                            }
                            cv.Roh_personalData.Roh_otherIds.Add(new Document() { Foaf_topic = "SemanticScholar", Dc_title = semanticScholarId });
                        }
                    }
                }

                //Direccion
                {
                    String select = @"SELECT DISTINCT ?person ?address";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <https://www.w3.org/2006/vcard/ns#address> ?address.
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string address = fila["address"].value;
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            if (cv.Roh_personalData.Vcard_address == null)
                            {
                                cv.Roh_personalData.Vcard_address = new Address();
                            }
                            cv.Roh_personalData.Vcard_address.Vcard_locality = address;
                        }
                    }
                }
            }
            return listaCV.Values.ToList();
        }



        private void InsertarDocumentosCV(SparqlObject pDatosCargar, string graphsUrl)
        {
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificActivity = fila["scientificActivity"].value;
                string document = fila["document"].value;
                string typeDocument = fila["typeDocument"].value;
                string isValidated = fila["isValidated"].value;

                string rdftype = "";
                string property = "";
                switch (typeDocument)
                {
                    case "SAD1":
                        rdftype = "http://w3id.org/roh/RelatedScientificPublication";
                        property = "http://w3id.org/roh/scientificPublications";
                        break;
                    case "SAD2":
                        rdftype = "http://w3id.org/roh/RelatedWorkSubmittedConferences";
                        property = "http://w3id.org/roh/worksSubmittedConferences";
                        break;
                    case "SAD3":
                        rdftype = "http://w3id.org/roh/RelatedWorkSubmittedSeminars";
                        property = "http://w3id.org/roh/worksSubmittedSeminars";
                        break;
                    case "SAD4":
                        rdftype = "http://w3id.org/roh/RelatedOtherDisseminationActivity";
                        property = "http://w3id.org/roh/otherDisseminationActivities";
                        break;
                }

                //Obtenemos la auxiliar en la que cargar la entidad  
                string rdfTypePrefix = AniadirPrefijo(rdftype);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = graphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new();
                string idEntityAux = scientificActivity + "|" + idNewAux;

                //Privacidad            
                string predicadoPrivacidad = "http://w3id.org/roh/scientificActivity|" + property + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new(idEntityAux + "|" + isValidated, predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = "http://w3id.org/roh/scientificActivity|" + property + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new(idEntityAux + "|" + document, predicadoEntidad);
                listaTriples.Add(tr1);

                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToInclude.ContainsKey(idCV))
                {
                    triplesToInclude[idCV].AddRange(listaTriples);
                }
                else
                {
                    triplesToInclude.Add(mResourceApi.GetShortGuid(cv), listaTriples);
                }
            }

            Parallel.ForEach(triplesToInclude.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                List<List<TriplesToInclude>> listasDeListas = SplitList(triplesToInclude[idCV], 50).ToList();
                foreach (List<TriplesToInclude> triples in listasDeListas)
                {
                    mResourceApi.InsertPropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        private void EliminarDocumentosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificActivity = fila["scientificActivity"].value;
                string item = fila["item"].value;
                string typeDocument = fila["typeDocument"].value;

                string property = "";
                switch (typeDocument)
                {
                    case "SAD1":
                        property = "http://w3id.org/roh/scientificPublications";
                        break;
                    case "SAD2":
                        property = "http://w3id.org/roh/worksSubmittedConferences";
                        break;
                    case "SAD3":
                        property = "http://w3id.org/roh/worksSubmittedSeminars";
                        break;
                    case "SAD4":
                        property = "http://w3id.org/roh/otherDisseminationActivities";
                        break;
                }

                RemoveTriples removeTriple = new();
                removeTriple.Predicate = "http://w3id.org/roh/scientificActivity|" + property;
                removeTriple.Value = scientificActivity + "|" + item;
                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToDelete.ContainsKey(idCV))
                {
                    triplesToDelete[idCV].Add(removeTriple);
                }
                else
                {
                    triplesToDelete.Add(idCV, new() { removeTriple });
                }
            }

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                List<List<RemoveTriples>> listasDeListas = SplitList(triplesToDelete[idCV], 50).ToList();
                foreach (List<RemoveTriples> triples in listasDeListas)
                {
                    mResourceApi.DeletePropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        private void PublicarDocumentosCV(SparqlObject pDatosCargar, string graphsUrl)
        {
            Dictionary<Guid, List<TriplesToModify>> triplesToModify = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificActivity = fila["scientificActivity"].value;
                string propItem = fila["propItem"].value;
                string item = fila["item"].value;


                TriplesToModify triple = new()
                {
                    OldValue = scientificActivity + "|" + item + "|false",
                    NewValue = scientificActivity + "|" + item + "|true",
                    Predicate = "http://w3id.org/roh/scientificActivity|" + propItem + "|http://w3id.org/roh/isPublic"
                };

                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToModify.ContainsKey(idCV))
                {
                    triplesToModify[idCV].Add(triple);
                }
                else
                {
                    triplesToModify.Add(mResourceApi.GetShortGuid(cv), new List<TriplesToModify>() { triple });
                }
            }

            Parallel.ForEach(triplesToModify.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                List<List<TriplesToModify>> listasDeListas = SplitList(triplesToModify[idCV], 50).ToList();
                foreach (List<TriplesToModify> triples in listasDeListas)
                {
                    mResourceApi.ModifyPropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        private void InsertarProyectosCV(SparqlObject pDatosCargar, string graphsUrl)
        {
            //http://gnoss.com/items/scientificexperienceproject_SEP1
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificExperience = fila["scientificExperience"].value;
                string project = fila["project"].value;
                string typeProject = fila["typeProject"].value;

                string rdftype = "";
                string property = "";
                switch (typeProject)
                {
                    case "SEP1":
                        rdftype = "http://w3id.org/roh/RelatedCompetitiveProject";
                        property = "http://w3id.org/roh/competitiveProjects";
                        break;
                    case "SEP2":
                        rdftype = "http://w3id.org/roh/RelatedNonCompetitiveProject";
                        property = "http://w3id.org/roh/nonCompetitiveProjects";
                        break;
                }

                //Obtenemos la auxiliar en la que cargar la entidad     
                string rdfTypePrefix = AniadirPrefijo(rdftype);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = graphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new();
                string idEntityAux = scientificExperience + "|" + idNewAux;

                //Privacidad, true (son proyectos oficiales)
                string predicadoPrivacidad = "http://w3id.org/roh/scientificExperience|" + property + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new(idEntityAux + "|true", predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = "http://w3id.org/roh/scientificExperience|" + property + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new(idEntityAux + "|" + project, predicadoEntidad);
                listaTriples.Add(tr1);

                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToInclude.ContainsKey(idCV))
                {
                    triplesToInclude[idCV].AddRange(listaTriples);
                }
                else
                {
                    triplesToInclude.Add(mResourceApi.GetShortGuid(cv), listaTriples);
                }
            }

            Parallel.ForEach(triplesToInclude.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { idCV, triplesToInclude[idCV] } });
            });
        }

        private void EliminarProyectosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificExperience = fila["scientificExperience"].value;
                string item = fila["item"].value;
                string typeProject = fila["typeProject"].value;

                string property = "";
                switch (typeProject)
                {
                    case "SEP1":
                        property = "http://w3id.org/roh/competitiveProjects";
                        break;
                    case "SEP2":
                        property = "http://w3id.org/roh/nonCompetitiveProjects";
                        break;
                }

                RemoveTriples removeTriple = new();
                removeTriple.Predicate = "http://w3id.org/roh/scientificExperience|" + property;
                removeTriple.Value = scientificExperience + "|" + item;
                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToDelete.ContainsKey(idCV))
                {
                    triplesToDelete[idCV].Add(removeTriple);
                }
                else
                {
                    triplesToDelete.Add(idCV, new List<RemoveTriples>() { removeTriple });
                }
            }

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<RemoveTriples>>() { { idCV, triplesToDelete[idCV] } });
            });
        }

        private void InsertarItemsCV(SparqlObject pDatosCargar, string graphsUrl, string pRdfType, string pSectionProperty, string pProperty, string pVarEntity, string pVarSection,bool pPublic)
        {
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string section = fila[pVarSection].value;
                string entity = fila[pVarEntity].value;

                //Obtenemos la auxiliar en la que cargar la entidad     
                string rdfTypePrefix = AniadirPrefijo(pRdfType);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = graphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new();
                string idEntityAux = section + "|" + idNewAux;

                //Privacidad                  
                string predicadoPrivacidad = pSectionProperty + "|" + pProperty + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new(idEntityAux + "|"+ pPublic.ToString().ToLower(), predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = pSectionProperty + "|" + pProperty + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new(idEntityAux + "|" + entity, predicadoEntidad);
                listaTriples.Add(tr1);

                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToInclude.ContainsKey(idCV))
                {
                    triplesToInclude[idCV].AddRange(listaTriples);
                }
                else
                {
                    triplesToInclude.Add(mResourceApi.GetShortGuid(cv), listaTriples);
                }
            }

            Parallel.ForEach(triplesToInclude.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { idCV, triplesToInclude[idCV] } });
            });
        }
        
        private void EliminarItemsCV(SparqlObject pDatosCargar, string pSectionProperty, string pProperty, string pVarItem, string pVarSection)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string section = fila[pVarSection].value;
                string item = fila[pVarItem].value;

                RemoveTriples removeTriple = new();
                removeTriple.Predicate = pSectionProperty + "|" + pProperty;
                removeTriple.Value = section + "|" + item;
                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToDelete.ContainsKey(idCV))
                {
                    triplesToDelete[idCV].Add(removeTriple);
                }
                else
                {
                    triplesToDelete.Add(idCV, new List<RemoveTriples>() { removeTriple });
                }
            }

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<RemoveTriples>>() { { idCV, triplesToDelete[idCV] } });
            });
        }


    }
}