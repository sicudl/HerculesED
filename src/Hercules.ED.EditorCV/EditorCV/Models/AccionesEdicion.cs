﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using GuardadoCV.Models.API.Input;
using GuardadoCV.Models.API.Response;
using GuardadoCV.Models.API.Templates;
using GuardadoCV.Models.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using System.Text;
using EditorCV.Models.API.Response;
using System.Threading.Tasks;
using System.Net.Http;
using EditorCV.Models.Enrichment;
using EditorCV.Controllers;
using static EditorCV.Models.Enrichment.EnrichmentResponse;
using Hercules.ED.DisambiguationEngine.Models;

namespace GuardadoCV.Models
{
    /// <summary>
    /// Clase utilizada para las acciones de recuperación de datos de edición
    /// </summary>
    public class AccionesEdicion
    {
        /// <summary>
        /// API
        /// </summary>
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configOAuth/OAuthV3.config");
        private static readonly CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configOAuth/OAuthV3.config");

        private static Tuple<Dictionary<string, string>, Dictionary<string, string>> tuplaTesauro;

        #region Métodos públicos

        /// <summary>
        /// Obtiene un listado de sugerencias con datos existentes para esa propiedad
        /// </summary>
        /// <param name="pSearch">Texto por el que se van a buscar sugerencias</param>
        /// <param name="pProperty">Propiedad en la que se quiere buscar</param>
        /// <param name="pRdfType">Rdf:type de la entidad en la que se quiere buscar</param>
        /// <param name="pGraph">Grafo en el que se encuentra la propiedad</param>
        /// <param name="pGetEntityID">Obtiene el ID de la entidad además del valor de la propiedad</param>
        /// <returns></returns>
        public object GetAutocomplete(string pSearch, string pProperty, string pRdfType, string pGraph, bool pGetEntityID, List<string> pLista)
        {
            string searchText = pSearch.Trim();
            string filter = "";
            if (!pSearch.EndsWith(' '))
            {
                string[] splitSearch = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (splitSearch.Length > 1)
                {
                    searchText = searchText.Substring(0, searchText.LastIndexOf(' '));
                    if (splitSearch.Last().Length > 3)
                    {
                        searchText += " " + splitSearch.Last() + "*";
                    }
                    else
                    {
                        filter = $" AND lcase(?o) like \"% { splitSearch.Last() }%\" ";
                    }
                }
                else if (searchText.Length > 3)
                {
                    searchText += "*";
                }
                else // Si tiene menos de 4 caracteres y no termina en espacio, buscamos por like
                {
                    filter = $"  lcase(?o) like \"{ searchText }%\" OR lcase(?o) like \"% { searchText }%\" ";
                    searchText = "";
                }
            }
            if (searchText != "")
            {
                filter = $"bif:contains(?o, \"'{ searchText }'\"){filter}";
            }
            string select = "SELECT DISTINCT ?s ?o ";
            string where = $"WHERE {{ ?s a <{ pRdfType }>. ?s <{ pProperty }> ?o . FILTER( {filter} ) }} ORDER BY ?o";
            SparqlObject sparqlObjectAux = mResourceApi.VirtuosoQuery(select, where, pGraph);
            if (!pGetEntityID)
            {
                var resultados = sparqlObjectAux.results.bindings.Select(x => x["o"].value).Distinct();
                if (pLista != null)
                {
                    resultados = resultados.Except(pLista, StringComparer.OrdinalIgnoreCase);
                }
                return resultados.ToList();
            }
            else
            {
                Dictionary<string, string> respuesta = new Dictionary<string, string>();
                foreach (Dictionary<string, Data> fila in sparqlObjectAux.results.bindings)
                {
                    string s = fila["s"].value;
                    string o = fila["o"].value;
                    if (pLista == null || respuesta.Keys.Intersect(pLista).Count() == 0)
                    {
                        respuesta.Add(s, o);
                    }
                }
                return respuesta;
            }
        }

        /// <summary>
        /// Obtiene una sección de pestaña del CV
        /// </summary>
        /// <param name="pId">Identificador de la entidad de la sección</param>
        /// <param name="pRdfType">Rdf:type de la entidad de la sección</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <returns></returns>
        public Object GetTab(string pId, string pRdfType, string pLang)
        {

            //Obtenemos el template
            API.Templates.Tab template = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfType);
            Object respuesta = null;
            if (!template.personalData)
            {
                //Obtenemos los datos necesarios para el pintado
                Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetTabData(pId, template, pLang);
                //Obtenemos el modelo para devolver
                respuesta = GetTabModel(pId, data, template, pLang);
            }
            else
            {

                respuesta = GetEditModel(pId, template.personalDataSections, pLang);
            }
            return respuesta;
        }

        /// <summary>
        /// Obtiene una minificha de una entidad de un listado
        /// </summary>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntity">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        public TabSectionItem GetItemMini(string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            TabSectionPresentationListItems presentationListItem = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab).sections.First(x => x.property == pIdSection).presentation.listItemsPresentation;
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetItemMiniData(pEntityID, presentationListItem.listItem, pLang);
            return GetItem(pEntityID, data, presentationListItem, pLang);
        }


        /// <summary>
        /// Obtiene los datos de edición de una entidad
        /// </summary>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntityID">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        public EntityEdit GetEdit(string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            TabSectionPresentation tabSectionPresentation = UtilityCV.TabTemplates.First(x => x.rdftype == pRdfTypeTab).sections.First(x => x.property == pIdSection).presentation;
            ItemEdit templateEdit = null;
            string property = "";
            if (tabSectionPresentation.listItemsPresentation != null)
            {
                templateEdit = tabSectionPresentation.listItemsPresentation.listItemEdit;
                property = tabSectionPresentation.listItemsPresentation.property;
            }
            else if (tabSectionPresentation.itemPresentation != null)
            {
                templateEdit = tabSectionPresentation.itemPresentation.itemEdit;
                property = tabSectionPresentation.itemPresentation.property;
            }
            string entityID = pEntityID;

            //obtenemos la entidad correspondiente
            List<PropertyData> propertyData = new List<PropertyData>() { new PropertyData() { property = property } };
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataAux = UtilityCV.GetProperties(new HashSet<string>() { pEntityID }, "curriculumvitae", propertyData, pLang, new Dictionary<string, SparqlObject>());
            if (pEntityID != null && dataAux.ContainsKey(pEntityID))
            {
                entityID = dataAux[pEntityID].First()["o"].value;
            }
            if (tabSectionPresentation.listItemsPresentation != null && !string.IsNullOrEmpty(tabSectionPresentation.listItemsPresentation.property_cv))
            {
                return GetEditModel(entityID, templateEdit, pLang, pEntityID, tabSectionPresentation.listItemsPresentation.property_cv);
            }
            else
            {
                return GetEditModel(entityID, templateEdit, pLang);
            }
        }

        /// <summary>
        /// Obtiene todos los combos de edición que hay configurados en una serie de propiedades
        /// </summary>
        /// <param name="pProperties"></param>
        /// <returns></returns>
        private List<ItemEditSectionRowPropertyCombo> GetEditCombos(List<ItemEditSectionRowProperty> pProperties)
        {
            List<ItemEditSectionRowPropertyCombo> listCombosConfig = new List<ItemEditSectionRowPropertyCombo>();
            foreach (ItemEditSectionRowPropertyCombo comboConfig in pProperties.Select(x => x.combo).Where(x => x != null))
            {
                if (!listCombosConfig.Exists(x =>
                        UtilityCV.GetPropComplete(x.property) == UtilityCV.GetPropComplete(comboConfig.property) &&
                        x.graph == comboConfig.graph &&
                        x.rdftype == comboConfig.rdftype &&
                        x.filter == comboConfig.filter
                    ))
                {

                    listCombosConfig.Add(comboConfig);
                }
            }
            foreach (ItemEditSectionRowProperty rowProperty in pProperties)
            {
                if (rowProperty.auxEntityData != null && rowProperty.auxEntityData.rows != null)
                {
                    foreach (ItemEditSectionRow row in rowProperty.auxEntityData.rows)
                    {
                        List<ItemEditSectionRowPropertyCombo> aux = GetEditCombos(row.properties);
                        foreach (ItemEditSectionRowPropertyCombo comboConfig in aux)
                        {
                            if (!listCombosConfig.Exists(x =>
                                    UtilityCV.GetPropComplete(x.property) == UtilityCV.GetPropComplete(comboConfig.property) &&
                                    x.graph == comboConfig.graph &&
                                    x.rdftype == comboConfig.rdftype &&
                                    x.filter == comboConfig.filter
                                ))
                            {

                                listCombosConfig.Add(comboConfig);
                            }
                        }
                    }
                }
            }
            return listCombosConfig;
        }

        /// <summary>
        /// Obtiene todos los tesauros de edición que hay configurados en una serie de propiedades
        /// </summary>
        /// <param name="pProperties"></param>
        /// <returns></returns>
        private List<string> GetEditThesaurus(List<ItemEditSectionRowProperty> pProperties)
        {
            HashSet<string> listThesaurusConfig = new HashSet<string>();
            foreach (string thesaurusConfig in pProperties.Select(x => x.thesaurus).Where(x => x != null))
            {
                listThesaurusConfig.Add(thesaurusConfig);
            }

            foreach (ItemEditSectionRowProperty rowProperty in pProperties)
            {
                if (rowProperty.auxEntityData != null && rowProperty.auxEntityData.rows != null)
                {
                    foreach (ItemEditSectionRow row in rowProperty.auxEntityData.rows)
                    {
                        listThesaurusConfig.UnionWith(GetEditThesaurus(row.properties));
                    }
                }
            }
            return listThesaurusConfig.ToList();
        }


        public EntityEdit GetEditEntity(string pRdfType, string pEntityID, string pLang)
        {
            ItemEdit templateEdit = UtilityCV.EntityTemplates.First(x => x.rdftype == pRdfType);
            string entityID = pEntityID;


            return GetEditModel(entityID, templateEdit, pLang);
        }

        public ItemsLoad LoadProps(ItemsLoad pItemsLoad, string pLang)
        {
            if (pItemsLoad.items != null && pItemsLoad.items.Count > 0)
            {
                foreach (LoadProp loadProp in pItemsLoad.items)
                {
                    KeyValuePair<string, PropertyData> propertyData = loadProp.GenerarPropertyData();
                    Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = UtilityCV.GetProperties(new HashSet<string>() { loadProp.id }, propertyData.Key, new List<PropertyData>() { propertyData.Value }, pLang, new Dictionary<string, SparqlObject>());
                    loadProp.values = GetPropValues(loadProp.id, loadProp.GetPropComplete(), data);
                }
            }
            return pItemsLoad;
        }

        public Dictionary<string, List<Person>> ValidateSignatures(string pSignatures, string pCVID, string pPersonID, string pLang)
        {
            Disambiguation.mResourceApi = mResourceApi;
            Dictionary<string, List<Person>> listaPersonas = new Dictionary<string, List<Person>>();

            if (!string.IsNullOrEmpty(pSignatures))
            {
                float scoreDocument = 0.1f;
                float scoreProject = 0.1f;
                float scoreDepartment = 0.2f;
                Dictionary<string, int> colaboradoresDocumentos = ObtenerColaboradoresPublicaciones(pPersonID);
                Dictionary<string, int> colaboradoresProyectos = ObtenerColaboradoresProyectos(pPersonID);
                HashSet<string> colaboradoresDepartament = ObtenerColaboradoresDepartamento(pPersonID);

                List<string> signaturesList = pSignatures.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Distinct().Select(x => x.Trim()).ToList();
                Dictionary<string, List<Person>> listaPersonasAux = new Dictionary<string, List<Person>>();
                Parallel.ForEach(signaturesList, new ParallelOptions { MaxDegreeOfParallelism = 5 }, firma =>
                {
                    if (firma.Trim() != "")
                    {
                        List<Person> personasBBDD = ObtenerPersonasFirma(firma.Trim());
                        Person personaActual = new Person();
                        personaActual.name = firma.Trim();
                        personaActual.ID = Guid.NewGuid().ToString();
                        List<DisambiguableEntity> entidadesActuales = new List<DisambiguableEntity>();
                        entidadesActuales.Add(personaActual);
                        List<DisambiguableEntity> entidadesBBDD = new List<DisambiguableEntity>();
                        foreach (Person personBBDD in personasBBDD)
                        {
                            personBBDD.ID = personBBDD.personid;
                            entidadesBBDD.Add(personBBDD);
                        }
                        Dictionary<string, Dictionary<string, float>> resultadoSimilaridad = Disambiguation.SimilarityBBDDScores(entidadesActuales, entidadesBBDD, 0, 0.5f);
                        foreach (string idBBDD in resultadoSimilaridad[personaActual.ID].Keys)
                        {
                            personasBBDD.First(x => x.ID == idBBDD).score = resultadoSimilaridad[personaActual.ID][idBBDD];
                        }
                        //Ordenamos y nos quedamos sólo con asl que tengan algo de score
                        personasBBDD = personasBBDD.Where(x => x.score > 0f).OrderByDescending(x => x.score).ToList();

                        foreach (Person person in personasBBDD)
                        {
                            float max = person.score + (1 - person.score) * person.score;
                            if (colaboradoresDocumentos.ContainsKey(person.ID))
                            {
                                for (int i = 0; i < colaboradoresDocumentos[person.ID]; i++)
                                {
                                    person.score += (max - person.score) * scoreDocument;
                                }
                            }
                            if (colaboradoresProyectos.ContainsKey(person.ID))
                            {
                                for (int i = 0; i < colaboradoresProyectos[person.ID]; i++)
                                {

                                    person.score += (max - person.score) * scoreProject;
                                }
                            }
                            if (colaboradoresDepartament.Contains(person.ID))
                            {
                                person.score += (max - person.score) * scoreDepartment;
                            }
                            if (person.score > max)
                            {
                                person.score = max;
                            }
                        }
                        personasBBDD = personasBBDD.OrderByDescending(x => x.score).ToList();
                        if(personasBBDD.Count>20)
                        {
                            personasBBDD = personasBBDD.GetRange(0, 20);
                        }

                        listaPersonasAux[firma.Trim()] = personasBBDD;
                    }
                });
                foreach (string firma in signaturesList)
                {
                    listaPersonas.Add(firma, listaPersonasAux[firma]);
                }
            }

            List<Guid> listaIDs = listaPersonas.SelectMany(x => x.Value).Select(x => mResourceApi.GetShortGuid(x.personid)).Distinct().ToList();
            if (listaIDs.Count > 0)
            {
                List<ResponseGetUrl> urls = mResourceApi.GetUrl(listaPersonas.SelectMany(x => x.Value).Select(x => mResourceApi.GetShortGuid(x.personid)).Distinct().ToList(), pLang);
                foreach (string key in listaPersonas.Keys)
                {
                    foreach (Person person in listaPersonas[key])
                    {
                        person.url = urls.First(x => x.resource_id == mResourceApi.GetShortGuid(person.personid)).url;
                    }
                }
            }
            return listaPersonas;
        }

        public Dictionary<string, int> ObtenerColaboradoresPublicaciones(string pPersonID)
        {
            Dictionary<string, int> colaboradoresPublicaciones = new Dictionary<string, int>();
            int limit = 10000;
            int offset = 0;
            while (true)
            {
                string select = $@"SELECT * WHERE {{select ?personOtherID count(distinct ?s) as ?num ";
                string where = $@"where
                            {{ 
                                ?s a <http://purl.org/ontology/bibo/Document>.
                                ?s <http://purl.org/ontology/bibo/authorList> ?author.     
                                ?author <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> <{pPersonID}>.
                                ?s <http://purl.org/ontology/bibo/authorList> ?authorOther.     
                                ?authorOther <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?personOtherID.                                
                            }}order by desc(?num) desc (?personOtherID) }} LIMIT {limit} OFFSET {offset}";
                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "document");
                offset += limit;
                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                {
                    colaboradoresPublicaciones[fila["personOtherID"].value] = int.Parse(fila["num"].value);
                }
                if (sparqlObject.results.bindings.Count < limit)
                {
                    break;
                }
            }
            return colaboradoresPublicaciones;
        }

        public Dictionary<string, int> ObtenerColaboradoresProyectos(string pPersonID)
        {
            Dictionary<string, int> colaboradoresProyectos = new Dictionary<string, int>();
            int limit = 10000;
            int offset = 0;
            while (true)
            {
                string select = $@"SELECT * WHERE {{select ?personOtherID count(distinct ?s) as ?num ";
                string where = $@"where
                            {{ 
                                ?s a <http://vivoweb.org/ontology/core#Project>.
                                ?s <http://vivoweb.org/ontology/core#relates> ?miembro.     
                                ?miembro <http://w3id.org/roh/roleOf> <{pPersonID}>.
                                ?s <http://vivoweb.org/ontology/core#relates> ?miembroOther.     
                                ?miembroOther  <http://w3id.org/roh/roleOf> ?personOtherID.                                
                            }}order by desc(?num) desc (?personOtherID) }} LIMIT {limit} OFFSET {offset}";
                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "project");
                offset += limit;
                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                {
                    colaboradoresProyectos[fila["personOtherID"].value] = int.Parse(fila["num"].value);
                }
                if (sparqlObject.results.bindings.Count < limit)
                {
                    break;
                }
            }

            return colaboradoresProyectos;
        }

        public HashSet<string> ObtenerColaboradoresDepartamento(string pPersonID)
        {
            HashSet<string> colaboradoresDepartamento = new HashSet<string>();
            int limit = 10000;
            int offset = 0;
            while (true)
            {
                string select = $@"SELECT * WHERE {{select distinct ?personOtherID from <{mResourceApi.GraphsUrl}department.owl>";
                string where = $@"where
                            {{ 
                                <{pPersonID}> <http://vivoweb.org/ontology/core#departmentOrSchool> ?depID.
                                ?personOtherID <http://vivoweb.org/ontology/core#departmentOrSchool> ?depID.            
                            }}order by desc (?personOtherID) }} LIMIT {limit} OFFSET {offset}";
                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "person");
                offset += limit;
                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                {
                    colaboradoresDepartamento.Add(fila["personOtherID"].value);
                }
                if (sparqlObject.results.bindings.Count < limit)
                {
                    break;
                }
            }
            return colaboradoresDepartamento;
        }

        public List<Person> ObtenerPersonasFirma(string firma)
        {
            List<Person> listaPersonas = new List<Person>();

            string texto = Disambiguation.ObtenerTextosNombresNormalizados(firma);
            string[] wordsTexto = texto.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (wordsTexto.Length > 0)
            {
                #region Buscamos en nombres
                {
                    List<string> unions = new List<string>();
                    foreach (string word in wordsTexto)
                    {
                        StringBuilder sbUnion = new StringBuilder();
                        sbUnion.AppendLine("   ?personID <http://xmlns.com/foaf/0.1/name> ?name.");
                        sbUnion.AppendLine($@" ?name bif:contains ""'{word}'"" BIND({word.Length} as ?num)");
                        unions.Add(sbUnion.ToString());
                    }

                    string select = $@"select distinct ?signature ?personID ?ORCID ?name ?num ?departamento from <{mResourceApi.GraphsUrl}person.owl> from <{mResourceApi.GraphsUrl}department.owl>";
                    string where = $@"where
                            {{
                                {{
                                    select ?personID ?ORCID ?name ?signature sum(?num) as ?num
                                    where
                                    {{
                                        ?personID  a <http://xmlns.com/foaf/0.1/Person>.                                        
                                        {{{string.Join("}UNION{", unions)}}}
                                        OPTIONAL{{?personID <http://w3id.org/roh/ORCID> ?ORCID}}
                                    }}
                                }}
                                OPTIONAL
                                {{
                                    ?s a <http://purl.org/ontology/bibo/Document>.
                                    ?s <http://purl.org/ontology/bibo/authorList> ?author.
                                    ?author <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?personID.
                                    ?author <http://xmlns.com/foaf/0.1/nick> ?signature.    
                                }}
                                OPTIONAL
                                {{
                                    ?personID <http://vivoweb.org/ontology/core#departmentOrSchool> ?depID.
                                    ?depID <http://purl.org/dc/elements/1.1/title> ?departamento.
                                }}
                            }}order by desc (?num)limit 1000";
                    SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "document");
                    foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                    {
                        string personID = fila["personID"].value;
                        string name = fila["name"].value;
                        Person persona = listaPersonas.FirstOrDefault(x => x.personid == personID);
                        if (persona == null)
                        {
                            persona = new Person();
                            persona.name = name;
                            persona.personid = personID;
                            persona.signatures = new HashSet<string>();
                            listaPersonas.Add(persona);
                        }
                        if (fila.ContainsKey("signature"))
                        {
                            string signature = fila["signature"].value;
                            persona.signatures.Add(signature);
                        }
                        if (fila.ContainsKey("ORCID"))
                        {
                            persona.orcid = fila["ORCID"].value;
                        }
                        if (fila.ContainsKey("departamento"))
                        {
                            persona.department = fila["departamento"].value;
                        }
                    }
                }
                #endregion
            }
            return listaPersonas;
        }



        #endregion

        #region Métodos para pestañas
        /// <summary>
        /// Obtiene los datos de una pestaña 
        /// </summary>
        /// <param name="pId">Identificador de la entidad de la sección</param>
        /// <param name="pTemplate">Plantilla a utilizar</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetTabData(string pId, API.Templates.Tab pTemplate, string pLang)
        {
            List<PropertyData> propertyDatas = new List<PropertyData>();
            string graph = "curriculumvitae";
            foreach (API.Templates.TabSection templateSection in pTemplate.sections)
            {
                propertyDatas.Add(templateSection.GenerarPropertyData(graph));
            }
            return UtilityCV.GetProperties(new HashSet<string>() { pId }, graph, propertyDatas, pLang, new Dictionary<string, SparqlObject>());
        }

        /// <summary>
        /// Obtiene los datos de un item dentro de un listado
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pListItemConfig">Configuración del item</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetItemMiniData(string pId, TabSectionListItem pListItemConfig, string pLang)
        {
            string graph = "curriculumvitae";

            List<PropertyData> propertyDatas = pListItemConfig.GenerarPropertyData(graph).childs;
            //Editabilidad
            foreach (string propEditabilidad in Utils.UtilityCV.PropertyNotEditable.Keys)
            {
                PropertyData propertyItem = propertyDatas.FirstOrDefault(x => x.property == "http://vivoweb.org/ontology/core#relatedBy");
                if (propertyItem != null)
                {
                    propertyItem.childs.Add(
                        //Editabilidad
                        new Utils.PropertyData()
                        {
                            property = propEditabilidad,
                            childs = new List<Utils.PropertyData>()
                        }
                    );
                }

            }

            return UtilityCV.GetProperties(new HashSet<string>() { pId }, graph, propertyDatas, pLang, new Dictionary<string, SparqlObject>());
        }

        /// <summary>
        /// Obtiene una sección de pestaña del CV una vez que tenemos los datos cargados
        /// </summary>
        /// <param name="pId">Identificador de la entidad de la sección</param>
        /// <param name="pData">Datos cargados de BBDD</param>
        /// <param name="pTemplate">Plantilla para generar el template</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private API.Response.Tab GetTabModel(string pId, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, API.Templates.Tab pTemplate, string pLang)
        {
            API.Response.Tab tab = new API.Response.Tab()
            {
                sections = new List<API.Response.TabSection>()
            };
            foreach (API.Templates.TabSection templateSection in pTemplate.sections)
            {
                if (templateSection.presentation != null)
                {
                    API.Response.TabSection tabSection = new API.Response.TabSection()
                    {
                        identifier = templateSection.property,
                        title = UtilityCV.GetTextLang(pLang, templateSection.presentation.title),
                        orders = new List<TabSectionPresentationOrder>()
                    };
                    switch (templateSection.presentation.type)
                    {
                        case TabSectionPresentationType.listitems:
                            {
                                if (templateSection.presentation.listItemsPresentation.listItem.orders != null)
                                {
                                    foreach (TabSectionListItemOrder listItemOrder in templateSection.presentation.listItemsPresentation.listItem.orders)
                                    {
                                        TabSectionPresentationOrder presentationOrderTabSection = new TabSectionPresentationOrder()
                                        {
                                            name = UtilityCV.GetTextLang(pLang, listItemOrder.name),
                                            properties = new List<TabSectionPresentationOrderProperty>()
                                        };

                                        if (listItemOrder.properties != null)
                                        {
                                            foreach (TabSectionListItemOrderProperty listItemConfigOrderProperty in listItemOrder.properties)
                                            {
                                                TabSectionPresentationOrderProperty presentationOrderTabSectionProperty = new TabSectionPresentationOrderProperty()
                                                {
                                                    property = UtilityCV.GetPropComplete(listItemConfigOrderProperty),
                                                    asc = listItemConfigOrderProperty.asc
                                                };
                                                presentationOrderTabSection.properties.Add(presentationOrderTabSectionProperty);
                                            }
                                        }
                                        tabSection.orders.Add(presentationOrderTabSection);
                                    }
                                }
                                tabSection.items = new Dictionary<string, TabSectionItem>();
                                string propiedadIdentificador = templateSection.property;
                                if (pData.ContainsKey(pId))
                                {
                                    foreach (string idEntity in pData[pId].Where(x => x["p"].value == templateSection.property).Select(x => x["o"].value).Distinct())
                                    {
                                        tabSection.items.Add(idEntity, GetItem(idEntity, pData, templateSection.presentation.listItemsPresentation, pLang));
                                    }
                                }
                                tab.sections.Add(tabSection);
                            }
                            break;
                        case TabSectionPresentationType.item:
                            {
                                string id = null;
                                if (pData.ContainsKey(pId))
                                {
                                    foreach (string idEntity in pData[pId].Where(x => x["p"].value == templateSection.property).Select(x => x["o"].value).Distinct())
                                    {
                                        id = idEntity;
                                    }
                                    if (id != null && pData.ContainsKey(id))
                                    {
                                        foreach (string idEntity in pData[id].Where(x => x["p"].value == templateSection.presentation.itemPresentation.property).Select(x => x["o"].value).Distinct())
                                        {
                                            id = idEntity;
                                        }
                                    }
                                }
                                tabSection.item = GetEditModel(id, templateSection.presentation.itemPresentation.itemEdit, pLang);
                                if (string.IsNullOrEmpty(tabSection.item.entityID))
                                {
                                    tabSection.item.entityID = Guid.NewGuid().ToString().ToLower();
                                }
                                tab.sections.Add(tabSection);
                            }
                            break;
                        default:
                            throw new Exception("No está implementado el código para el tipo " + templateSection.presentation.type.ToString());
                    }

                }
            }
            return tab;
        }


        /// <summary>
        /// Obtiene un item de un listado de una sección
        /// </summary>
        /// <param name="pId">Identificador del item</param>
        /// <param name="pData">Datos cargados</param>
        /// <param name="pListItemConfig">Configuración del item</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private TabSectionItem GetItem(string pId, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, TabSectionPresentationListItems pListItemConfig, string pLang)
        {
            TabSectionItem item = new TabSectionItem()
            {
                title = GetPropValues(pId, UtilityCV.GetPropComplete(pListItemConfig.listItem.propertyTitle), pData).FirstOrDefault()
            };
            if (item.title == null)
            {
                item.title = "";
            }
            item.identifier = mResourceApi.GetShortGuid(GetPropValues(pId, pListItemConfig.listItem.propertyTitle.property, pData).FirstOrDefault()).ToString().ToLower();
            //Editabilidad
            item.iseditable = true;
            if (!string.IsNullOrEmpty(pId))
            {
                foreach (string propEditabilidad in Utils.UtilityCV.PropertyNotEditable.Keys)
                {
                    string valorPropiedad = GetPropValues(pId, pListItemConfig.property + "@@@" + propEditabilidad, pData).FirstOrDefault();
                    if ((Utils.UtilityCV.PropertyNotEditable[propEditabilidad] == null || Utils.UtilityCV.PropertyNotEditable[propEditabilidad].Count == 0) && !string.IsNullOrEmpty(valorPropiedad))
                    {
                        item.iseditable = false;
                    }
                    else if (Utils.UtilityCV.PropertyNotEditable[propEditabilidad].Contains(valorPropiedad))
                    {
                        item.iseditable = false;
                    }
                }
            }

            //Visibilidad
            string valorVisibilidad = GetPropValues(pId, UtilityCV.PropertyIspublic, pData).FirstOrDefault();
            item.ispublic = false;
            if (!string.IsNullOrEmpty(valorVisibilidad) && valorVisibilidad == "true")
            {
                item.ispublic = true;
            }
            item.properties = new List<TabSectionItemProperty>();
            if (pListItemConfig.listItem.properties != null)
            {
                foreach (TabSectionListItemProperty property in pListItemConfig.listItem.properties)
                {
                    string propertyIn = "";
                    TabSectionItemProperty itemProperty = new TabSectionItemProperty()
                    {
                        showMini = property.showMini,
                        showMiniBold = property.showMiniBold,
                        name = UtilityCV.GetTextLang(pLang, property.name)
                    };
                    if (property.childOR != null && property.childOR.Count > 0)
                    {
                        propertyIn = "";
                        string aux = "";
                        foreach (PropertyDataTemplate propertyData in property.childOR)
                        {
                            propertyIn += aux + UtilityCV.GetPropComplete(propertyData);
                            aux = "||";
                        }
                    }
                    else
                    {
                        propertyIn = UtilityCV.GetPropComplete(property.child);
                    }
                    itemProperty.type = property.type.ToString();
                    itemProperty.values = GetPropValues(pId, propertyIn, pData);
                    if (property.type == DataTypeListItem.number)
                    {
                        itemProperty.values = itemProperty.values.Select(x => UtilityCV.GetTextNumber(x)).ToList();
                    }
                    else if (property.type == DataTypeListItem.boolean)
                    {
                        List<string> valuesAux = new List<string>(itemProperty.values);
                        itemProperty.values = new List<string>();
                        string si = "";
                        string no = "";
                        switch (pLang)
                        {
                            case "es":
                                si = "Sí";
                                no = "No";
                                break;
                            case "en":
                                si = "Yes";
                                no = "No";
                                break;
                            default:
                                si = "Sí";
                                no = "No";
                                break;
                        }
                        foreach (string value in valuesAux)
                        {
                            if (value.ToLower() == "true")
                            {
                                itemProperty.values.Add(si);
                            }
                            else if (value.ToLower() == "false")
                            {
                                itemProperty.values.Add(no);
                            }
                        }
                    }
                    item.properties.Add(itemProperty);
                }
            }
            item.orderProperties = new List<TabSectionItemOrderProperty>();
            if (pListItemConfig.listItem.orders != null)
            {
                foreach (TabSectionListItemOrder order in pListItemConfig.listItem.orders)
                {
                    if (order.properties != null)
                    {
                        foreach (TabSectionListItemOrderProperty data in order.properties)
                        {
                            TabSectionItemOrderProperty itemOrderProperty = new TabSectionItemOrderProperty()
                            {
                                property = UtilityCV.GetPropComplete(data),
                                values = GetPropValues(pId, UtilityCV.GetPropComplete(data), pData)
                            };
                            if (!item.orderProperties.Exists(x => x.property == itemOrderProperty.property))
                            {
                                item.orderProperties.Add(itemOrderProperty);
                            }
                        }
                    }
                }
            }
            return item;
        }


        #endregion

        #region Métodos para edición de entidades
        /// <summary>
        /// Obtiene todos los datos de una entidad de BBDD para su posterior edición
        /// </summary>
        /// <param name="pId">Identificador</param>
        /// <param name="pItemEdit">Configuración de edición</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pEntityCV">Entidad del cv desde la que se apunta a la entidad</param>
        /// <param name="pPropertyCV">Propiedad que apunta a la entidad en el CV</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetEditData(string pId, ItemEdit pItemEdit, string pGraph, string pLang, string pEntityCV = null, string pPropertyCV = null)
        {
            List<PropertyData> propertyDatas = pItemEdit.GenerarPropertyDatas(pGraph);
            //Editabilidad
            foreach (string propEditabilidad in Utils.UtilityCV.PropertyNotEditable.Keys)
            {
                propertyDatas.Add(new Utils.PropertyData()
                {
                    property = propEditabilidad,
                    childs = new List<Utils.PropertyData>()
                });
            }
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> respuesta = UtilityCV.GetProperties(new HashSet<string>() { pId }, pGraph, propertyDatas, pLang, new Dictionary<string, SparqlObject>());

            List<PropertyData> propertyDatasCV = pItemEdit.GenerarPropertyDatas(pGraph, true);
            if (propertyDatasCV.Count > 0)
            {
                SparqlObject idEntityInCV = mResourceApi.VirtuosoQuery("select ?id", "where{<" + pEntityCV + "> <" + pPropertyCV + "> ?id}", "curriculumvitae");
                if (idEntityInCV.results.bindings.Count > 0)
                {
                    string id = idEntityInCV.results.bindings[0]["id"].value;
                    Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> respuestaCV = UtilityCV.GetProperties(new HashSet<string>() { id }, "curriculumvitae", propertyDatasCV, pLang, new Dictionary<string, SparqlObject>());
                    foreach (string idrespuesta in respuestaCV.Keys)
                    {
                        string idAux = idrespuesta;
                        if (idrespuesta == id)
                        {
                            idAux = pId;
                        }
                        if (!respuesta.ContainsKey(idAux))
                        {
                            respuesta.Add(idAux, new List<Dictionary<string, Data>>());
                        }
                        foreach (Dictionary<string, SparqlObject.Data> fila in respuestaCV[idrespuesta])
                        {
                            Dictionary<string, SparqlObject.Data> filaAux = new Dictionary<string, Data>();
                            filaAux.Add("s", fila["s"]);
                            filaAux.Add("p", fila["p"]);
                            filaAux.Add("o", fila["o"]);
                            filaAux["s"].value = filaAux["s"].value.Replace(id, pId);
                            respuesta[idAux].Add(filaAux);
                        }
                    }
                }
            }

            return respuesta;
        }

        /// <summary>
        /// Genera el modelo de edición de una entidad una vez tenemos todos los datos de la entidad cargados
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pPresentationEdit">Configuración de presentación</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pEntityCV">Entidad del cv desde la que se apunta a la entidad</param>
        /// <param name="pPropertyCV">Propiedad que apunta a la entidad en el CV</param>
        /// <returns></returns>
        private EntityEdit GetEditModel(string pId, ItemEdit pPresentationEdit, string pLang, string pEntityCV = null, string pPropertyCV = null)
        {
            Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> data = GetEditData(pId, pPresentationEdit, pPresentationEdit.graph, pLang, pEntityCV, pPropertyCV);

            List<ItemEditSectionRowPropertyCombo> listCombosConfig = GetEditCombos(pPresentationEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).ToList());
            Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> combos = new Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>>();
            Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> combosDependency = new Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>>();

            foreach (ItemEditSectionRowPropertyCombo combo in listCombosConfig)
            {
                Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> dataCombo = GetSubjectsCombo(combo, pLang);
                Dictionary<string, string> valoresCombo = new Dictionary<string, string>() { { "", "-" } };
                Dictionary<string, string> valoresDependency = new Dictionary<string, string>() { };
                foreach (string id in dataCombo.Keys)
                {
                    valoresCombo.Add(id, GetPropValues(id, UtilityCV.GetPropComplete(combo.property), dataCombo).First());

                    if (combo.dependency != null)
                    {
                        valoresDependency.Add(id, GetPropValues(id, combo.dependency.property, dataCombo).First());
                    }
                }

                combos.Add(combo, valoresCombo);
                if (combo.dependency != null)
                {
                    combosDependency.Add(combo, valoresDependency);
                }
            }

            List<string> listaTesaurosConfig = GetEditThesaurus(pPresentationEdit.sections.SelectMany(x => x.rows).SelectMany(x => x.properties).ToList());
            Dictionary<string, List<ThesaurusItem>> tesauros = GetTesauros(listaTesaurosConfig);

            EntityEdit entityEdit = new EntityEdit()
            {
                entityID = pId,
                ontology = pPresentationEdit.graph,
                rdftype = pPresentationEdit.rdftype,
                sections = new List<EntityEditSection>()
            };

            foreach (ItemEditSection itemEditSection in pPresentationEdit.sections)
            {
                EntityEditSection entityEditSection = new EntityEditSection()
                {
                    title = UtilityCV.GetTextLang(pLang, itemEditSection.title),
                    rows = GetRowsEdit(pId, itemEditSection.rows, data, combos, combosDependency, tesauros, pLang, pPresentationEdit.graph)
                };
                entityEdit.sections.Add(entityEditSection);
            }

            //Editabilidad
            entityEdit.iseditable = true;
            if (!string.IsNullOrEmpty(pId))
            {
                foreach (string propEditabilidad in Utils.UtilityCV.PropertyNotEditable.Keys)
                {
                    string valorPropiedad = GetPropValues(pId, propEditabilidad, data).FirstOrDefault();
                    if ((Utils.UtilityCV.PropertyNotEditable[propEditabilidad] == null || Utils.UtilityCV.PropertyNotEditable[propEditabilidad].Count == 0) && !string.IsNullOrEmpty(valorPropiedad))
                    {
                        entityEdit.iseditable = false;
                    }
                    else if (Utils.UtilityCV.PropertyNotEditable[propEditabilidad].Contains(valorPropiedad))
                    {
                        entityEdit.iseditable = false;
                    }
                }
            }
            return entityEdit;
        }


        /// <summary>
        /// Genera el modelo de edición de las filas de edición de una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pItemEditSectionRows">Filas de edición configuradas</param>
        /// <param name="pData">Datos de la entidad</param>        
        /// <param name="pCombos">Combos</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <returns></returns>
        private List<EntityEditSectionRow> GetRowsEdit(string pId, List<ItemEditSectionRow> pItemEditSectionRows, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombos, Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombosDependency, Dictionary<string, List<ThesaurusItem>> pTesauros, string pLang, string pGraph)
        {
            List<EntityEditSectionRow> entityEditSectionRows = new List<EntityEditSectionRow>();
            foreach (ItemEditSectionRow itemEditSectionRow in pItemEditSectionRows)
            {
                EntityEditSectionRow entityEditSectionRow = new EntityEditSectionRow()
                {
                    properties = new List<EntityEditSectionRowProperty>()
                };
                foreach (ItemEditSectionRowProperty itemEditSectionRowProperty in itemEditSectionRow.properties)
                {
                    if (string.IsNullOrEmpty(itemEditSectionRowProperty.compossed))
                    {
                        EntityEditSectionRowProperty entityEditSectionRowProperty = GetPropertiesEdit(pId, itemEditSectionRowProperty, pData, pCombos, pCombosDependency, pTesauros, pLang, pGraph);
                        entityEditSectionRow.properties.Add(entityEditSectionRowProperty);
                    }
                }
                entityEditSectionRows.Add(entityEditSectionRow);
            }
            return entityEditSectionRows;
        }

        /// <summary>
        /// Genera el modelo de edición de una propiedad de edición de una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pItemEditSectionRowProperty">Propiedad de edición configuradas</param>
        /// <param name="pData">Datos de la entidad</param>        
        /// <param name="pCombos">Combos</param>
        /// <param name="pLang">Idioma</param>
        /// <param name="pGraph">Grafo de la entidad</param>
        /// <returns></returns>
        private EntityEditSectionRowProperty GetPropertiesEdit(string pId, ItemEditSectionRowProperty pItemEditSectionRowProperty, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData, Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombos, Dictionary<ItemEditSectionRowPropertyCombo, Dictionary<string, string>> pCombosDependency, Dictionary<string, List<ThesaurusItem>> pTesauros, string pLang, string pGraph)
        {
            if (pItemEditSectionRowProperty.type == DataTypeEdit.auxEntityAuthorList)
            {
                EntityEditSectionRowProperty entityEditSectionRowProperty = new EntityEditSectionRowProperty()
                {
                    property = pItemEditSectionRowProperty.property,
                    width = pItemEditSectionRowProperty.width,
                    required = pItemEditSectionRowProperty.required,
                    multiple = true,
                    autocomplete = pItemEditSectionRowProperty.autocomplete,
                    title = UtilityCV.GetTextLang(pLang, pItemEditSectionRowProperty.title),
                    type = DataTypeEdit.auxEntityAuthorList.ToString(),
                    values = new List<string>(),
                    entityAuxData = new EntityEditAuxEntity()
                    {
                        childsOrder = new Dictionary<string, int>(),
                        rdftype = "http://purl.obolibrary.org/obo/BFO_0000023",
                        propertyOrder = "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment",
                        titleConfig = new EntityEditRepresentativeProperty()
                        {
                            route = "http://xmlns.com/foaf/0.1/nick"
                        },
                        propertiesConfig = new List<EntityEditRepresentativeProperty>()
                        {
                            new EntityEditRepresentativeProperty()
                            {
                                //TODO
                                name = "Nombre",
                                route = "http://www.w3.org/1999/02/22-rdf-syntax-ns#member||person||http://xmlns.com/foaf/0.1/name"
                            },
                            new EntityEditRepresentativeProperty()
                            {
                                //TODO
                                name = "ORCID",
                                route = "http://www.w3.org/1999/02/22-rdf-syntax-ns#member||person||http://w3id.org/roh/ORCID"
                            }
                        }
                    }
                };
                if (pId != null && pData.ContainsKey(pId))
                {
                    foreach (string value in pData[pId].Where(x => x["p"].value == entityEditSectionRowProperty.property).Select(x => x["o"].value).Distinct())
                    {
                        entityEditSectionRowProperty.values.Add(value);
                    }
                }

                entityEditSectionRowProperty.entityAuxData.entities = new Dictionary<string, List<EntityEditSectionRow>>();

                List<ItemEditSectionRow> rowsEdit = new List<ItemEditSectionRow>() {
                    new ItemEditSectionRow()
                    {
                        properties = new List<ItemEditSectionRowProperty>(){
                            new ItemEditSectionRowProperty(){
                                //TODO
                                title = new Dictionary<string, string>() { { "es", "Firma" } },
                                type = DataTypeEdit.text,
                                required = true,
                                property = "http://xmlns.com/foaf/0.1/nick",
                                width = 2
                            },
                            new ItemEditSectionRowProperty(){
                                //TODO
                                title = new Dictionary<string, string>() { { "es", "Persona" } },
                                type = DataTypeEdit.entity,
                                required = true,
                                property = "http://www.w3.org/1999/02/22-rdf-syntax-ns#member",
                                width = 2,
                                entityData=new ItemEditEntityData()
                                {
                                    rdftype="http://xmlns.com/foaf/0.1/Person",
                                    graph="person",
                                    propertyTitle= new PropertyDataTemplate(){ property="http://xmlns.com/foaf/0.1/name"},
                                    properties=new List<ItemEditEntityProperty>(){
                                        new ItemEditEntityProperty()
                                        {
                                            name=new Dictionary<string, string>(){ { "es", "ORCID" } },
                                            child=new PropertyDataTemplate()
                                            {
                                                property="http://w3id.org/roh/ORCID"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };



                entityEditSectionRowProperty.entityAuxData.rows = GetRowsEdit(null, rowsEdit, pData, pCombos, pCombosDependency, pTesauros, pLang, pGraph);
                entityEditSectionRowProperty.entityAuxData.titles = new Dictionary<string, EntityEditRepresentativeProperty>();
                entityEditSectionRowProperty.entityAuxData.properties = new Dictionary<string, List<EntityEditRepresentativeProperty>>();
                foreach (string id in entityEditSectionRowProperty.values)
                {
                    entityEditSectionRowProperty.entityAuxData.entities.Add(id, GetRowsEdit(id, rowsEdit, pData, pCombos, pCombosDependency, pTesauros, pLang, pGraph));
                    if (!string.IsNullOrEmpty(entityEditSectionRowProperty.entityAuxData.propertyOrder))
                    {
                        string orden = GetPropertiesEdit(id, new ItemEditSectionRowProperty() { property = entityEditSectionRowProperty.entityAuxData.propertyOrder }, pData, pCombos, pCombosDependency, pTesauros, pLang, pGraph).values.FirstOrDefault();
                        int.TryParse(orden, out int ordenInt);
                        entityEditSectionRowProperty.entityAuxData.childsOrder[id] = ordenInt;
                    }

                    //Title
                    string title = GetPropValues(id, "http://xmlns.com/foaf/0.1/nick", pData).FirstOrDefault();
                    if (string.IsNullOrEmpty(title))
                    {
                        title = "";
                    }
                    entityEditSectionRowProperty.entityAuxData.titles.Add(id, new EntityEditRepresentativeProperty()
                    {
                        value = title,
                        route = "http://xmlns.com/foaf/0.1/nick"
                    });


                    entityEditSectionRowProperty.entityAuxData.properties[id] = new List<EntityEditRepresentativeProperty>()
                    {
                        new EntityEditRepresentativeProperty()
                        {
                            //TODO
                            name = "Nombre",
                            value = string.Join(", ", GetPropValues(id, "http://www.w3.org/1999/02/22-rdf-syntax-ns#member@@@http://xmlns.com/foaf/0.1/name", pData)),
                            route = "http://www.w3.org/1999/02/22-rdf-syntax-ns#member||person||http://xmlns.com/foaf/0.1/name"
                        },
                        new EntityEditRepresentativeProperty()
                        {
                            //TODO
                            name = "ORCID",
                            value = string.Join(", ", GetPropValues(id, "http://www.w3.org/1999/02/22-rdf-syntax-ns#member@@@http://w3id.org/roh/ORCID", pData)),
                            route = "http://www.w3.org/1999/02/22-rdf-syntax-ns#member||person||http://w3id.org/roh/ORCID"
                        }
                    };
                }

                return entityEditSectionRowProperty;
            }
            else
            {
                EntityEditSectionRowProperty entityEditSectionRowProperty = new EntityEditSectionRowProperty()
                {
                    property = pItemEditSectionRowProperty.property,
                    width = pItemEditSectionRowProperty.width,
                    required = pItemEditSectionRowProperty.required,
                    multiple = pItemEditSectionRowProperty.multiple,
                    autocomplete = pItemEditSectionRowProperty.autocomplete,
                    title = UtilityCV.GetTextLang(pLang, pItemEditSectionRowProperty.title),
                    placeholder = UtilityCV.GetTextLang(pLang, pItemEditSectionRowProperty.placeholder),
                    type = pItemEditSectionRowProperty.type.ToString(),
                    values = new List<string>()
                };
                if (pItemEditSectionRowProperty.auxEntityData != null)
                {
                    entityEditSectionRowProperty.entityAuxData = new EntityEditAuxEntity()
                    {
                        childsOrder = new Dictionary<string, int>(),
                        rdftype = pItemEditSectionRowProperty.auxEntityData.rdftype,
                        propertyOrder = pItemEditSectionRowProperty.auxEntityData.propertyOrder,
                    };

                    if (pItemEditSectionRowProperty.auxEntityData.propertyTitle != null)
                    {
                        entityEditSectionRowProperty.entityAuxData.titleConfig = new EntityEditRepresentativeProperty()
                        {
                            route = pItemEditSectionRowProperty.auxEntityData.propertyTitle.GetRoute()
                        };
                    }

                    entityEditSectionRowProperty.entityAuxData.propertiesConfig = new List<EntityEditRepresentativeProperty>();
                    if (pItemEditSectionRowProperty.auxEntityData.properties != null)
                    {
                        foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.auxEntityData.properties)
                        {
                            entityEditSectionRowProperty.entityAuxData.propertiesConfig.Add(new EntityEditRepresentativeProperty()
                            {
                                name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                                route = entityProperty.child.GetRoute()
                            });
                        }
                    }
                }
                if (pItemEditSectionRowProperty.entityData != null)
                {
                    entityEditSectionRowProperty.entityData = new EntityEditEntity()
                    {
                        rdftype = pItemEditSectionRowProperty.entityData.rdftype,
                        titleConfig = new EntityEditRepresentativeProperty()
                    };
                    if (pItemEditSectionRowProperty.entityData.propertyTitle != null)
                    {
                        entityEditSectionRowProperty.entityData.titleConfig = new EntityEditRepresentativeProperty()
                        {
                            route = pItemEditSectionRowProperty.entityData.graph + "||" + pItemEditSectionRowProperty.entityData.propertyTitle.GetRoute()
                        };
                    }
                    entityEditSectionRowProperty.entityData.propertiesConfig = new List<EntityEditRepresentativeProperty>();
                    foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.entityData.properties)
                    {
                        entityEditSectionRowProperty.entityData.propertiesConfig.Add(new EntityEditRepresentativeProperty()
                        {
                            name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                            route = pItemEditSectionRowProperty.entityData.graph + "||" + entityProperty.child.GetRoute()
                        });
                    }
                }

                if (pId != null && pData.ContainsKey(pId))
                {
                    foreach (string value in pData[pId].Where(x => x["p"].value == entityEditSectionRowProperty.property).Select(x => x["o"].value).Distinct())
                    {
                        entityEditSectionRowProperty.values.Add(value);
                    }
                }
                if (pItemEditSectionRowProperty.type == DataTypeEdit.boolean)
                {
                    entityEditSectionRowProperty.comboValues = new Dictionary<string, string>();
                    entityEditSectionRowProperty.comboValues[""] = "-";
                    string si = "";
                    string no = "";
                    switch (pLang)
                    {
                        case "es":
                            si = "Sí";
                            no = "No";
                            break;
                        case "en":
                            si = "Yes";
                            no = "No";
                            break;
                        default:
                            si = "Sí";
                            no = "No";
                            break;
                    }
                    entityEditSectionRowProperty.comboValues["true"] = si;
                    entityEditSectionRowProperty.comboValues["false"] = no;
                }
                else if (pItemEditSectionRowProperty.combo != null)
                {
                    entityEditSectionRowProperty.comboValues = pCombos.FirstOrDefault(x =>
                      UtilityCV.GetPropComplete(x.Key.property) == UtilityCV.GetPropComplete(pItemEditSectionRowProperty.combo.property) &&
                       x.Key.graph == pItemEditSectionRowProperty.combo.graph &&
                       x.Key.rdftype == pItemEditSectionRowProperty.combo.rdftype && x.Key.filter == pItemEditSectionRowProperty.combo.filter
                    ).Value;

                    if (pItemEditSectionRowProperty.combo.dependency != null)
                    {
                        Dictionary<string, string> parentDependency = pCombosDependency.FirstOrDefault(x =>
                          UtilityCV.GetPropComplete(x.Key.property) == UtilityCV.GetPropComplete(pItemEditSectionRowProperty.combo.property) &&
                           x.Key.graph == pItemEditSectionRowProperty.combo.graph &&
                           x.Key.rdftype == pItemEditSectionRowProperty.combo.rdftype && x.Key.dependency == pItemEditSectionRowProperty.combo.dependency
                        ).Value;

                        if (parentDependency != null)
                        {
                            entityEditSectionRowProperty.comboDependency = new ComboDependency { parent = pItemEditSectionRowProperty.combo.dependency.propertyValue, parentDependency = parentDependency };
                        }
                    }
                }

                if (pItemEditSectionRowProperty.autocompleteConfig != null)
                {
                    entityEditSectionRowProperty.autocompleteConfig = new AutocompleteConfig()
                    {
                        property = UtilityCV.GetPropComplete(pItemEditSectionRowProperty.autocompleteConfig.property),
                        rdftype = pItemEditSectionRowProperty.autocompleteConfig.rdftype,
                        graph = pItemEditSectionRowProperty.autocompleteConfig.graph,
                        getEntityId = !string.IsNullOrEmpty(pItemEditSectionRowProperty.autocompleteConfig.propertyEntity),
                        mandatory = pItemEditSectionRowProperty.autocompleteConfig.mandatory,
                    };
                }

                if (pItemEditSectionRowProperty.autocompleteConfig != null && !string.IsNullOrEmpty(pItemEditSectionRowProperty.autocompleteConfig.propertyEntity))
                {
                    entityEditSectionRowProperty.propertyEntity = pItemEditSectionRowProperty.autocompleteConfig.propertyEntity;
                    if (pId != null && pData.ContainsKey(pId))
                    {
                        entityEditSectionRowProperty.propertyEntityValue = pData[pId].Where(x => x["p"].value == entityEditSectionRowProperty.propertyEntity).Select(x => x["o"].value).Distinct().FirstOrDefault();
                    }
                }

                if (pItemEditSectionRowProperty.autocompleteConfig != null && pItemEditSectionRowProperty.type == DataTypeEdit.entityautocomplete)
                {
                    entityEditSectionRowProperty.propertyEntityValue = "";
                    if (pId != null && pData.ContainsKey(pId))
                    {
                        string entity = pData[pId].Where(x => x["p"].value == entityEditSectionRowProperty.property).Select(x => x["o"].value).Distinct().FirstOrDefault();
                        if (!string.IsNullOrEmpty(entity))
                        {
                            string entityText = pData[entity].Where(x => x["p"].value == pItemEditSectionRowProperty.autocompleteConfig.property.property).Select(x => x["o"].value).Distinct().FirstOrDefault();
                            entityEditSectionRowProperty.propertyEntityValue = entityText;
                        }
                    }
                }

                if (pItemEditSectionRowProperty.type == DataTypeEdit.thesaurus)
                {
                    entityEditSectionRowProperty.thesaurus = pTesauros[pItemEditSectionRowProperty.thesaurus];
                    entityEditSectionRowProperty.entityAuxData = new EntityEditAuxEntity()
                    {
                        childsOrder = new Dictionary<string, int>(),
                        rdftype = "http://w3id.org/roh/CategoryPath",
                        titleConfig = new EntityEditRepresentativeProperty()
                        {
                            //TODO
                            //route = "http://xmlns.com/foaf/0.1/nick"
                        }
                    };
                    entityEditSectionRowProperty.entityAuxData.entities = new Dictionary<string, List<EntityEditSectionRow>>();
                    List<ItemEditSectionRow> rowsEdit = new List<ItemEditSectionRow>() {
                        new ItemEditSectionRow()
                        {
                            properties = new List<ItemEditSectionRowProperty>(){
                                new ItemEditSectionRowProperty(){
                                    //TODO
                                    title = new Dictionary<string, string>() { { "es", "Categoria" } },
                                    type = DataTypeEdit.text,
                                    multiple=true,
                                    property = "http://w3id.org/roh/categoryNode",
                                    width = 1
                                }
                            }
                        }
                    };
                    entityEditSectionRowProperty.entityAuxData.rows = GetRowsEdit(null, rowsEdit, pData, pCombos, pCombosDependency, pTesauros, pLang, pGraph);
                    entityEditSectionRowProperty.entityAuxData.titles = new Dictionary<string, EntityEditRepresentativeProperty>();
                    entityEditSectionRowProperty.entityAuxData.properties = new Dictionary<string, List<EntityEditRepresentativeProperty>>();
                    foreach (string id in entityEditSectionRowProperty.values)
                    {
                        entityEditSectionRowProperty.entityAuxData.entities.Add(id, GetRowsEdit(id, rowsEdit, pData, pCombos, pCombosDependency, pTesauros, pLang, pGraph));
                    }
                    return entityEditSectionRowProperty;
                }

                if (pItemEditSectionRowProperty.type == DataTypeEdit.auxEntity)
                {
                    if (pItemEditSectionRowProperty.auxEntityData != null && pItemEditSectionRowProperty.auxEntityData.rows != null && pItemEditSectionRowProperty.auxEntityData.rows.Count > 0)
                    {
                        entityEditSectionRowProperty.entityAuxData.entities = new Dictionary<string, List<EntityEditSectionRow>>();
                        entityEditSectionRowProperty.entityAuxData.rows = GetRowsEdit(null, pItemEditSectionRowProperty.auxEntityData.rows, pData, pCombos, pCombosDependency, pTesauros, pLang, pGraph);
                        entityEditSectionRowProperty.entityAuxData.titles = new Dictionary<string, EntityEditRepresentativeProperty>();
                        entityEditSectionRowProperty.entityAuxData.properties = new Dictionary<string, List<EntityEditRepresentativeProperty>>();
                        foreach (string id in entityEditSectionRowProperty.values)
                        {
                            entityEditSectionRowProperty.entityAuxData.entities.Add(id, GetRowsEdit(id, pItemEditSectionRowProperty.auxEntityData.rows, pData, pCombos, pCombosDependency, pTesauros, pLang, pGraph));
                            if (!string.IsNullOrEmpty(entityEditSectionRowProperty.entityAuxData.propertyOrder))
                            {
                                string orden = GetPropertiesEdit(id, new ItemEditSectionRowProperty() { property = entityEditSectionRowProperty.entityAuxData.propertyOrder }, pData, pCombos, pCombosDependency, pTesauros, pLang, pGraph).values.FirstOrDefault();
                                int.TryParse(orden, out int ordenInt);
                                entityEditSectionRowProperty.entityAuxData.childsOrder[id] = ordenInt;
                            }

                            if (pItemEditSectionRowProperty.auxEntityData.propertyTitle != null)
                            {
                                string title = GetPropValues(id, UtilityCV.GetPropComplete(pItemEditSectionRowProperty.auxEntityData.propertyTitle), pData).FirstOrDefault();
                                string routeTitle = pItemEditSectionRowProperty.auxEntityData.propertyTitle.GetRoute();
                                if (string.IsNullOrEmpty(title))
                                {
                                    title = "";
                                }
                                entityEditSectionRowProperty.entityAuxData.titles.Add(id, new EntityEditRepresentativeProperty() { value = title, route = routeTitle });
                            }

                            if (pItemEditSectionRowProperty.auxEntityData.properties != null)
                            {
                                foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.auxEntityData.properties)
                                {
                                    List<string> valores = GetPropValues(id, UtilityCV.GetPropComplete(entityProperty.child), pData);
                                    string routeProp = entityProperty.child.GetRoute();
                                    if (!entityEditSectionRowProperty.entityAuxData.properties.ContainsKey(id))
                                    {
                                        entityEditSectionRowProperty.entityAuxData.properties[id] = new List<EntityEditRepresentativeProperty>();
                                    }

                                    entityEditSectionRowProperty.entityAuxData.properties[id].Add(new EntityEditRepresentativeProperty()
                                    {
                                        name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                                        value = string.Join(", ", valores),
                                        route = routeProp
                                    });

                                }
                            }
                        }
                    }
                }

                if (pItemEditSectionRowProperty.type == DataTypeEdit.entity)
                {
                    if (pItemEditSectionRowProperty.entityData != null)
                    {
                        entityEditSectionRowProperty.entityData.titles = new Dictionary<string, EntityEditRepresentativeProperty>();
                        entityEditSectionRowProperty.entityData.properties = new Dictionary<string, List<EntityEditRepresentativeProperty>>();
                        foreach (string id in entityEditSectionRowProperty.values)
                        {
                            if (pItemEditSectionRowProperty.entityData.propertyTitle != null)
                            {
                                string title = GetPropValues(id, UtilityCV.GetPropComplete(pItemEditSectionRowProperty.entityData.propertyTitle), pData).FirstOrDefault();
                                string routeTitle = pItemEditSectionRowProperty.entityData.propertyTitle.GetRoute();
                                if (string.IsNullOrEmpty(title))
                                {
                                    title = "";
                                }
                                entityEditSectionRowProperty.entityData.titles.Add(id, new EntityEditRepresentativeProperty() { value = title, route = pItemEditSectionRowProperty.entityData.graph + "||" + routeTitle });
                            }

                            if (pItemEditSectionRowProperty.entityData.properties != null)
                            {
                                foreach (ItemEditEntityProperty entityProperty in pItemEditSectionRowProperty.entityData.properties)
                                {
                                    List<string> valores = GetPropValues(id, UtilityCV.GetPropComplete(entityProperty.child), pData);
                                    string routeProp = entityProperty.child.GetRoute();
                                    if (!entityEditSectionRowProperty.entityData.properties.ContainsKey(id))
                                    {
                                        entityEditSectionRowProperty.entityData.properties[id] = new List<EntityEditRepresentativeProperty>();
                                    }

                                    entityEditSectionRowProperty.entityData.properties[id].Add(new EntityEditRepresentativeProperty()
                                    {
                                        name = UtilityCV.GetTextLang(pLang, entityProperty.name),
                                        value = string.Join(", ", valores),
                                        route = pItemEditSectionRowProperty.entityData.graph + "||" + routeProp
                                    });

                                }
                            }
                        }
                    }
                }

                if (pItemEditSectionRowProperty.dependency != null)
                {
                    entityEditSectionRowProperty.dependency = new Dependency()
                    {
                        parent = pItemEditSectionRowProperty.dependency.property
                    };
                    if (!string.IsNullOrEmpty(pItemEditSectionRowProperty.dependency.propertyValue))
                    {
                        entityEditSectionRowProperty.dependency.parentDependencyValue = pItemEditSectionRowProperty.dependency.propertyValue.Replace("{GraphsUrl}", mResourceApi.GraphsUrl);
                    }
                    if (!string.IsNullOrEmpty(pItemEditSectionRowProperty.dependency.propertyValueDistinct))
                    {
                        entityEditSectionRowProperty.dependency.parentDependencyValueDistinct = pItemEditSectionRowProperty.dependency.propertyValueDistinct.Replace("{GraphsUrl}", mResourceApi.GraphsUrl);
                    }
                }
                entityEditSectionRowProperty.entity_cv = pItemEditSectionRowProperty.entity_cv;

                return entityEditSectionRowProperty;
            }
        }
        #endregion

        #region Métodos de recolección de datos

        private Dictionary<string, List<ThesaurusItem>> GetTesauros(List<string> pListaTesauros)
        {
            Dictionary<string, List<ThesaurusItem>> elementosTesauros = new Dictionary<string, List<ThesaurusItem>>();

            foreach (string tesauro in pListaTesauros)
            {
                string select = "select * ";
                string where = @$"where {{
                    ?s a <http://www.w3.org/2008/05/skos#Concept>.
                    ?s <http://www.w3.org/2008/05/skos#prefLabel> ?nombre.
                    ?s <http://purl.org/dc/elements/1.1/source> '{tesauro}'
                    OPTIONAL {{ ?s <http://www.w3.org/2008/05/skos#broader> ?padre }}
                }} ORDER BY ?padre ?s ";
                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "taxonomy");

                List<ThesaurusItem> items = sparqlObject.results.bindings.Select(x => new ThesaurusItem()
                {
                    id = x["s"].value,
                    name = x["nombre"].value,
                    parentId = x.ContainsKey("padre") ? x["padre"].value : ""
                }).ToList();

                elementosTesauros.Add(tesauro, items);
            }

            return elementosTesauros;
        }


        /// <summary>
        /// Obtiene los datos para los combos (entidad y texto)
        /// </summary>
        /// <param name="pItemEditSectionRowPropertyCombo">Configuración de un combo para edición</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        private Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> GetSubjectsCombo(ItemEditSectionRowPropertyCombo pItemEditSectionRowPropertyCombo, string pLang)
        {
            int paginacion = 10000;
            int offset = 0;
            int limit = paginacion;
            HashSet<string> ids = new HashSet<string>();
            while (limit == paginacion)
            {
                string filter = "";
                if (pItemEditSectionRowPropertyCombo.filter != null)
                {
                    filter = $" . ?s <{pItemEditSectionRowPropertyCombo.filter.property}> '{pItemEditSectionRowPropertyCombo.filter.value}'";
                }

                //Obtenemos los IDS
                string select = "select * where{select distinct ?s ";
                string where = $"where{{?s a <{pItemEditSectionRowPropertyCombo.rdftype}> {filter} }} order by asc(?s)}} limit {limit} offset {offset}";
                SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, pItemEditSectionRowPropertyCombo.graph);
                limit = sparqlObject.results.bindings.Count;
                offset += sparqlObject.results.bindings.Count;
                foreach (Dictionary<string, SparqlObject.Data> fila in sparqlObject.results.bindings)
                {
                    ids.Add(fila["s"].value);
                }
            }
            List<PropertyData> propertyDatas = new List<PropertyData>() { pItemEditSectionRowPropertyCombo.property.GenerarPropertyData(pItemEditSectionRowPropertyCombo.graph) };


            if (pItemEditSectionRowPropertyCombo.dependency != null)
            {
                propertyDatas.Add(new Utils.PropertyData()
                {
                    property = pItemEditSectionRowPropertyCombo.dependency.property,
                    order = null,
                    childs = new List<Utils.PropertyData>()
                });
            }

            return UtilityCV.GetProperties(ids, pItemEditSectionRowPropertyCombo.graph, propertyDatas, pLang, new Dictionary<string, SparqlObject>());
        }

        /// <summary>
        /// Obtiene los valores de una propiedad para una entidad
        /// </summary>
        /// <param name="pId">Identificador de la entidad</param>
        /// <param name="pProp">Propiedad</param>
        /// <param name="pData">Datos cargados</param>
        /// <returns></returns>
        public static List<string> GetPropValues(string pId, string pProp, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData)
        {
            List<string> idAux = new List<string>();

            List<string> propInList = pProp.Split(new string[] { "||" }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
            if (propInList.Count > 1)
            {
                List<PropertyData> propertyDataAux = new List<PropertyData>();
                foreach (string propIn in propInList)
                {
                    PropertyData last = null;
                    string[] props = propIn.Split(new string[] { "@@@" }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (string prop in props)
                    {
                        if (last == null)
                        {
                            if (propertyDataAux.Exists(x => x.property == prop))
                            {
                                last = propertyDataAux.First(x => x.property == prop);
                            }
                            else
                            {
                                last = new PropertyData()
                                {
                                    property = prop,
                                    childs = new List<PropertyData>()
                                };
                                propertyDataAux.Add(last);
                            }
                        }
                        else
                        {
                            if (last.childs.Exists(x => x.property == prop))
                            {
                                last = last.childs.First(x => x.property == prop);
                            }
                            else
                            {
                                PropertyData aux = last;
                                last = new PropertyData()
                                {
                                    property = prop,
                                    childs = new List<PropertyData>()
                                };
                                aux.childs.Add(last);
                            }
                        }
                    }
                }
                List<string> finalList = new List<string>();
                foreach (PropertyData propertyData in propertyDataAux)
                {
                    idAux.AddRange(GetPropValuesAux(new List<string>() { pId }, propertyData, pData));
                }
            }
            else
            {
                idAux = new List<string>() { pId };
                string[] props = pProp.Split(new string[] { "@@@" }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string prop in props)
                {
                    List<string> idAux2 = new List<string>();
                    foreach (string id in idAux)
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            if (pData.ContainsKey(id))
                            {
                                idAux2.AddRange(pData[id].Where(x => x["p"].value == prop).Select(x => x["o"].value).Distinct().ToList());
                            }
                        }
                    }
                    idAux = idAux2;
                }
            }
            return idAux;
        }

        /// <summary>
        /// Método auxiliar para cuando la propiedad es un OR
        /// </summary>
        /// <param name="pIds">Identificadores</param>
        /// <param name="pPropertyData">Propiedades a recuperar</param>
        /// <param name="pData">Datos cargados</param>
        /// <returns></returns>
        private static List<string> GetPropValuesAux(List<string> pIds, PropertyData pPropertyData, Dictionary<string, List<Dictionary<string, SparqlObject.Data>>> pData)
        {
            List<string> values = new List<string>();
            List<string> aux = new List<string>();
            foreach (string id in pIds)
            {
                aux = GetPropValues(id, pPropertyData.property, pData);
            }
            if (pPropertyData.childs != null && pPropertyData.childs.Count > 0)
            {
                foreach (string id in aux)
                {
                    List<string> aux2 = new List<string>();
                    foreach (PropertyData propertyData in pPropertyData.childs)
                    {
                        aux2 = GetPropValuesAux(new List<string>() { id }, propertyData, pData);
                        if (aux2.Count > 0)
                        {
                            break;
                        }
                    }
                    values.AddRange(aux2);
                }
            }
            else
            {
                values = aux;
            }
            return values;
        }

        /// <summary>
        /// Hace la petición al servicio para obtener los descriptores temáticos y específicos.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        /// <param name="pTitulo">Título.</param>
        /// <param name="pDesc">Descripción.</param>
        /// <param name="pUrlPdf">URL del PDF.</param>
        /// <returns>Objeto con los datos obtenidos.</returns>
        /// <exception cref="Exception">StatusCode del error (Error al hacer la petición).</exception>
        public EnrichmentResponseGlobal GetEnrichment(ConfigService pConfig, string pTitulo, string pDesc, string pUrlPdf)
        {
            // Obtención del tesáuro.
            if (tuplaTesauro == null)
            {
                tuplaTesauro = ObtenerDatosTesauro();
            }

            // Cliente.
            EnrichmentResponseGlobal respuesta = new EnrichmentResponseGlobal();
            respuesta.tags = new EnrichmentResponse();
            respuesta.tags.topics = new List<EnrichmentResponseItem>();
            respuesta.categories = new EnrichmentResponseCategory();
            respuesta.categories.topics = new List<List<EnrichmentResponseCategory.EnrichmentResponseItem>>();
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromDays(1);

            // Si la descripción llega nula o vacía...
            if (string.IsNullOrEmpty(pDesc))
            {
                pDesc = string.Empty;
            }

            EnrichmentData enrichmentData = new EnrichmentData() { rotype = "papers", title = pTitulo, abstract_ = pDesc, pdfurl = pUrlPdf };

            // Conversión de los datos.
            string informacion = JsonConvert.SerializeObject(enrichmentData,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            StringContent contentData = new StringContent(informacion, System.Text.Encoding.UTF8, "application/json");

            #region --- Tópicos Específicos (Tags)
            var responseSpecific = client.PostAsync(pConfig.GetUrlSpecificEnrichment(), contentData).Result;

            if (responseSpecific.IsSuccessStatusCode)
            {
                respuesta.tags = responseSpecific.Content.ReadAsAsync<EnrichmentResponse>().Result;
            }
            else
            {
                return respuesta;
            }
            #endregion

            #region --- Tópicos Temáticos (Categorías)            
            var responseThematic = client.PostAsync(pConfig.GetUrlThematicEnrichment(), contentData).Result;

            EnrichmentResponse categoriasObtenidas = null;

            if (responseThematic.IsSuccessStatusCode)
            {
                categoriasObtenidas = responseThematic.Content.ReadAsAsync<EnrichmentResponse>().Result;
                EnrichmentResponseCategory listaCategoria = new EnrichmentResponseCategory();
                listaCategoria.topics = new List<List<EnrichmentResponseCategory.EnrichmentResponseItem>>();

                foreach (EnrichmentResponseItem cat in categoriasObtenidas.topics)
                {
                    List<EnrichmentResponseCategory.EnrichmentResponseItem> listaTesauro = new List<EnrichmentResponseCategory.EnrichmentResponseItem>();

                    if (tuplaTesauro.Item2.ContainsKey("general " + cat.word.ToLower().Trim()))
                    {
                        string idTesauro = tuplaTesauro.Item2["general " + cat.word.ToLower().Trim()];
                        listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });

                        while (!idTesauro.EndsWith(".0.0.0"))
                        {
                            idTesauro = ObtenerIdTesauro(idTesauro);
                            listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });
                        }
                    }
                    else if (tuplaTesauro.Item2.ContainsKey(cat.word.ToLower().Trim()))
                    {
                        string idTesauro = tuplaTesauro.Item2[cat.word.ToLower().Trim()];
                        listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });

                        while (!idTesauro.EndsWith(".0.0.0"))
                        {
                            idTesauro = ObtenerIdTesauro(idTesauro);
                            listaTesauro.Add(new EnrichmentResponseCategory.EnrichmentResponseItem() { id = idTesauro });
                        }
                    }


                    if (listaTesauro.Count > 0)
                    {
                        listaCategoria.topics.Add(listaTesauro);
                    }
                }
                respuesta.categories = listaCategoria;
            }
            else
            {
                return respuesta;
            }
            #endregion

            return respuesta;
        }

        /// <summary>
        /// Obtiene la ctaegoría padre.
        /// </summary>
        /// <param name="pIdTesauro">Categoría a consultar.</param>
        /// <returns>Categoría padre.</returns>
        private string ObtenerIdTesauro(string pIdTesauro)
        {
            string idTesauro = pIdTesauro.Split(new[] { "researcharea_" }, StringSplitOptions.None)[1];
            int num1 = Int32.Parse(idTesauro.Split('.')[0]);
            int num2 = Int32.Parse(idTesauro.Split('.')[1]);
            int num3 = Int32.Parse(idTesauro.Split('.')[2]);
            int num4 = Int32.Parse(idTesauro.Split('.')[3]);

            if (num4 != 0)
            {
                idTesauro = $@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.{num2}.{num3}.0";
            }
            else if (num3 != 0 && num4 == 0)
            {
                idTesauro = $@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.{num2}.0.0";
            }
            else if (num2 != 0 && num3 == 0 && num4 == 0)
            {
                idTesauro = $@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.0.0.0";
            }

            return idTesauro;
        }

        /// <summary>
        /// Obtiene las categorías del tesáuro.
        /// </summary>
        /// <returns>Tupla con (clave) diccionario de las idCategorias-idPadre y (valor) diccionario de nombreCategoria-idCategoria.</returns>
        private static Tuple<Dictionary<string, string>, Dictionary<string, string>> ObtenerDatosTesauro()
        {
            Dictionary<string, string> dicAreasBroader = new Dictionary<string, string>();
            Dictionary<string, string> dicAreasNombre = new Dictionary<string, string>();

            string select = @"SELECT DISTINCT * ";
            string where = @$"WHERE {{
                ?concept a <http://www.w3.org/2008/05/skos#Concept>.
                ?concept <http://www.w3.org/2008/05/skos#prefLabel> ?nombre.
                ?concept <http://purl.org/dc/elements/1.1/source> 'researcharea'
                OPTIONAL{{?concept <http://www.w3.org/2008/05/skos#broader> ?broader}}
                }}";
            SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "taxonomy");

            foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
            {
                string concept = fila["concept"].value;
                string nombre = fila["nombre"].value;
                string broader = "";
                if (fila.ContainsKey("broader"))
                {
                    broader = fila["broader"].value;
                }
                dicAreasBroader.Add(concept, broader);
                if (!dicAreasNombre.ContainsKey(nombre.ToLower()))
                {
                    dicAreasNombre.Add(nombre.ToLower(), concept);
                }
            }

            Dictionary<string, string> dicAreasUltimoNivel = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> item in dicAreasNombre)
            {
                bool tieneHijos = false;
                string id = item.Value.Split(new[] { "researcharea_" }, StringSplitOptions.None)[1];
                int num1 = Int32.Parse(id.Split('.')[0]);
                int num2 = Int32.Parse(id.Split('.')[1]);
                int num3 = Int32.Parse(id.Split('.')[2]);
                int num4 = Int32.Parse(id.Split('.')[3]);

                if (num2 == 0 && num3 == 0 && num4 == 0)
                {
                    tieneHijos = dicAreasNombre.ContainsValue($@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.1.0.0");
                }
                else if (num3 == 0 && num4 == 0)
                {
                    tieneHijos = dicAreasNombre.ContainsValue($@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.{num2}.1.0");
                }
                else if (num4 == 0)
                {
                    tieneHijos = dicAreasNombre.ContainsValue($@"{mResourceApi.GraphsUrl}items/researcharea_{num1}.{num2}.{num3}.1");
                }

                if (!tieneHijos)
                {
                    dicAreasUltimoNivel.Add(item.Key, item.Value);
                }
            }

            return new Tuple<Dictionary<string, string>, Dictionary<string, string>>(dicAreasBroader, dicAreasUltimoNivel);
        }

        #endregion



    }
}