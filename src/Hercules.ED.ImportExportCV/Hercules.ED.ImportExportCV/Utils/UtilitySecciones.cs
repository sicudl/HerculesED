﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.ImportExportCV.Controllers;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.InteropServices;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using static Models.Entity;

namespace Utils
{
    public class UtilitySecciones
    {
        /// <summary>
        /// Identificador, {nombre(1), issn(2), eissn(3), editor(4)}
        /// </summary>
        private static Dictionary<string, Tuple<string, string, string, string>> mListaRevistas = new();

        private static readonly Dictionary<string, string> mListaPalabrasClave = new();
        private static readonly List<Tuple<string, string>> Lenguajes = new();
        private static Dictionary<string, string> mOrgsNombreIds = new();
        private static readonly Dictionary<string, string> dicTopics = new();
        private static readonly Dictionary<string, string> dicDOI = new();
        private static DateTime mDateOrgsNombreIds = DateTime.MinValue;

        private static readonly ResourceApi mResourceApi = new($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config");

        public static Dictionary<string, string> GetIDPersona(string crisID)
        {
            Dictionary<string, string> PersonaORCID = new();
            string persona = "";
            string ORCID = "";

            string select = $@"select distinct ?cv ?person ?cris ?ORCID";
            string where = $@"
where{{
    ?cv a <http://w3id.org/roh/CV> .
    ?cv ?p ?person.
    ?person a <http://xmlns.com/foaf/0.1/Person> .
    ?person <http://w3id.org/roh/crisIdentifier> ?cris .
    ?person <http://w3id.org/roh/crisIdentifier> ?ORCID .
    FILTER(?cris='{crisID}')
}} ";
            SparqlObject resultData = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new List<string> { "curriculumvitae", "person" });

            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (fila.ContainsKey("person"))
                {
                    persona = fila["person"].value;
                }
                if (fila.ContainsKey("ORCID"))
                {
                    ORCID = fila["ORCID"].value;
                }
            }

            PersonaORCID.Add(persona, ORCID);
            return PersonaORCID;
        }

        /// <summary>
        /// Devuelve el identificador de CV a partir del crisIdentifier
        /// </summary>
        /// <param name="crisID"></param>
        /// <returns></returns>
        public static string GetID(string crisID)
        {
            string select = $@"select distinct ?cv ?person ?cris";
            string where = $@"
where{{
    ?cv a <http://w3id.org/roh/CV> .
    ?cv ?p ?person.
    ?person a <http://xmlns.com/foaf/0.1/Person> .
    ?person <http://w3id.org/roh/crisIdentifier> ?cris .
    FILTER(?cris='{crisID}')
}} ";
            SparqlObject resultData = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new List<string> { "curriculumvitae", "person" });
            if (resultData == null || resultData.results == null || resultData.results.bindings == null)
            {
                return "";
            }

            Dictionary<string, Data> fila = resultData.results.bindings.First();
            if (fila.ContainsKey("cv"))
            {
                return fila["cv"].value;
            }

            return "";
        }

        public static List<Tuple<string, string>> GetLenguajes()
        {
            return Lenguajes;
        }

        /// <summary>
        /// Inicializa el listado de Lenguajes.
        /// </summary>
        /// <param name="pResourceApi"></param>
        public static void IniciarLenguajes(ResourceApi pResourceApi)
        {
            string select = $@"select distinct ?title ?ident";
            string where = $@" where {{
?s a <http://w3id.org/roh/Language> .
?s <http://purl.org/dc/elements/1.1/title> ?title FILTER(langMatches(lang(?title), ""es""))
?s <http://purl.org/dc/elements/1.1/identifier> ?ident .
}}";

            SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "language");
            if (resultData.results.bindings.Count == 0)
            {
                return;
            }

            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (fila.ContainsKey("title") && fila.ContainsKey("ident"))
                {
                    Lenguajes.Add(new Tuple<string, string>(fila["title"].value, fila["ident"].value));
                }
            }
        }

        /// <summary>
        /// Devuelve los identificadores devueltos en la consulta.
        /// </summary>
        /// <param name="pResourceApi">pResourceApi</param>
        /// <param name="pCVID">pCVID</param>
        /// <param name="propiedadesItem">propiedadesItem</param>
        /// <returns>HashSet<string></returns>
        public static HashSet<string> GetIDS(ResourceApi pResourceApi, string pCVID, List<string> propiedadesItem)
        {
            HashSet<string> ids = new();
            int offsetInt = 0;
            int limit = 10000;

            while (true)
            {
                string select = $@"SELECT distinct ?item {{ select *";
                string where = $@"where {{";

                //Compruebo que no es nulo y que tiene 1 o más valores
                if (propiedadesItem == null) { return null; }
                if (propiedadesItem.Count == 0) { return null; }

                if (propiedadesItem.Count == 1)
                {
                    where += $@" ?s <{propiedadesItem[0]}> ?item ";
                }
                if (propiedadesItem.Count > 1)
                {
                    where += $@" ?s <{propiedadesItem[0]}> ?prop1 . ";
                    for (int i = 1; i < propiedadesItem.Count - 1; i++)
                    {
                        where += $@" ?prop{i} <{propiedadesItem[i]}> ?prop{i + 1} . ";
                    }
                    where += $@" ?prop{propiedadesItem.Count - 1} <{propiedadesItem[propiedadesItem.Count - 1]}> ?item ";

                }
                where += $@" FILTER(?s = <{pCVID}>)
                            }} ORDER BY ?item
                        }}LIMIT {limit} OFFSET {offsetInt} ";

                //Si tengo más de 10.000 resultados repito la consulta
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                ids.UnionWith(resultData.results.bindings.Select(x => x["item"].value));

                offsetInt += limit;
                if (resultData.results.bindings.Count < limit)
                {
                    break;
                }
            }

            return ids;
        }

        /// <summary>
        /// Devuelve el identificador a <paramref name="nombreRevista"/>
        /// en caso de que se encuentre en la consulta, nulo en caso contrario.
        /// </summary>
        /// <param name="pResourceApi">pResourceApi</param>
        /// <param name="nombreRevista">nombreRevista</param>
        /// <returns>string</returns>
        public static string GetNombreRevista(ResourceApi pResourceApi, string nombreRevista, string issn, string eissn, string editorial)
        {
            //Si el nombre de la revista es nulo o vacio
            if (string.IsNullOrEmpty(nombreRevista))
            {
                return null;
            }

            int offsetInt = 0;
            int limit = 10000;

            if (mListaRevistas.Count == 0)
            {
                Dictionary<string, Tuple<string, string, string, string>> listaRevistasAux = new();
                while (true)
                {
                    //Si tengo más de 10.000 resultados repito la consulta, sino salgo del bucle
                    string select = $@"SELECT distinct ?identificador ?nombreRevista ?issn ?eissn ?editorial {{ select *";
                    string where = $@"where {{
                                ?identificador a <http://w3id.org/roh/MainDocument> .
                                ?identificador <http://w3id.org/roh/title> ?nombreRevista .
                                OPTIONAL{{?identificador <http://purl.org/ontology/bibo/issn> ?issn}}
                                OPTIONAL{{?identificador <http://purl.org/ontology/bibo/eissn> ?eissn}}
                                OPTIONAL{{?identificador <http://purl.org/ontology/bibo/editor> ?editorial}}
                             }} ORDER BY ?nombreRevista
                        }} LIMIT {limit} OFFSET {offsetInt} ";
                    SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "maindocument");

                    string nombreRevistaConsulta = "";
                    string identificadorRevistaConsulta = "";
                    string issnRevistaConsulta = "";
                    string eissnRevistaConsulta = "";
                    string editorialRevistaConsulta = "";
                    foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                    {
                        nombreRevistaConsulta = fila["nombreRevista"].value;
                        identificadorRevistaConsulta = fila["identificador"].value;

                        issnRevistaConsulta = "";
                        eissnRevistaConsulta = "";
                        editorialRevistaConsulta = "";
                        if (fila.ContainsKey("issn"))
                        {
                            issnRevistaConsulta = fila["issn"].value;
                        }
                        if (fila.ContainsKey("eissn"))
                        {
                            eissnRevistaConsulta = fila["eissn"].value;
                        }
                        if (fila.ContainsKey("editorial"))
                        {
                            editorialRevistaConsulta = fila["editorial"].value;
                        }
                        listaRevistasAux[nombreRevistaConsulta] = new Tuple<string, string, string, string>(identificadorRevistaConsulta, issnRevistaConsulta, eissnRevistaConsulta, editorialRevistaConsulta);

                    }
                    offsetInt += limit;
                    if (resultData.results.bindings.Count < limit)
                    {
                        break;
                    }
                }
                mListaRevistas = listaRevistasAux;
            }

            //Si tienen el mismo issn
            if (mListaRevistas.Any(x => x.Value.Item2.Equals(issn)))
            {
                return mListaRevistas.Where(x => x.Value.Item2.Equals(issn)).Select(x => x.Key).FirstOrDefault();
            }
            //Si el titulo es el mismo y el editor es el mismo
            else if (mListaRevistas.Any(x => x.Value.Item1.ToLower().Equals(nombreRevista.ToLower()))
                  && mListaRevistas.Any(x => x.Value.Item4.Equals(editorial)))
            {
                return mListaRevistas.Where(x => x.Value.Item1.ToLower().Equals(nombreRevista.ToLower()) && x.Value.Item4.Equals(editorial)).Select(x => x.Key).FirstOrDefault();
            }
            //Si el titulo es el mismo y el eissn es el mismo
            else if (mListaRevistas.Any(x => x.Value.Item1.ToLower().Equals(nombreRevista.ToLower()))
                 && mListaRevistas.Any(x => x.Value.Item3.Equals(eissn)))
            {
                return mListaRevistas.Where(x => x.Value.Item1.ToLower().Equals(nombreRevista.ToLower()) && x.Value.Item3.Equals(eissn)).Select(x => x.Key).FirstOrDefault();
            }
            else if (mListaRevistas.Any(x => x.Value.Item1.ToLower().Equals(nombreRevista.ToLower())))
            {
                return mListaRevistas.Where(x => x.Value.Item1.ToLower().Equals(nombreRevista.ToLower())).Select(x => x.Key).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Devuelve la referencia a <paramref name="nombreOrganizacion"/>
        /// en caso de que se encuentre en la consulta, nulo en caso contrario.
        /// </summary>
        /// <param name="pResourceApi">pResourceApi</param>
        /// <param name="nombreOrganizacion">nombreOrganizacion</param>
        /// <returns>string</returns>
        public static string GetOrganizacionPorNombre(ResourceApi pResourceApi, string nombreOrganizacion)
        {
            //Recalculamos cada 60 minutos
            if (mDateOrgsNombreIds.AddMinutes(60) < DateTime.Now)
            {
                Dictionary<string, string> aux = new();
                int offset = 0;
                int limit = 10000;
                while (true)
                {
                    string select = $@"select * where{{ select distinct ?nombre ?id";
                    string where = $@"where {{ 
                                ?id <http://w3id.org/roh/title> ?nombre.  
                            }}order by desc(?id) }} offset {offset} limit {limit}";

                    SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, "organization");

                    if (resultData.results.bindings.Count > 0)
                    {
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultData.results.bindings)
                        {
                            aux[fila["nombre"].value] = fila["id"].value;
                        }
                    }
                    offset += limit;
                    if (resultData.results.bindings.Count < limit)
                    {
                        break;
                    }
                }


                mOrgsNombreIds = aux;
                mDateOrgsNombreIds = DateTime.Now;
            }
            KeyValuePair<string, string> valor = mOrgsNombreIds.FirstOrDefault(x => x.Key.ToLower() == nombreOrganizacion.ToLower());
            return valor.Value;
        }

        /// <summary>
        /// Método para dividir listas
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pItems">Listado</param>
        /// <param name="pSize">Tamaño</param>
        /// <returns></returns>
        public static IEnumerable<List<T>> SplitList<T>(List<T> pItems, int pSize)
        {
            for (int i = 0; i < pItems.Count; i += pSize)
            {
                yield return pItems.GetRange(i, Math.Min(pSize, pItems.Count - i));
            }
        }

        /// <summary>
        /// Añade en la lista de propiedades de la entidad las propiedades en las 
        /// que los valores no son nulos, en caso de que los valores sean nulos se omite
        /// dicha propiedad.
        /// </summary>
        /// <param name="list"></param>
        public static List<Property> AddProperty(params Property[] list)
        {
            List<Property> listado = new();
            for (int i = 0; i < list.Length; i++)
            {
                if (!string.IsNullOrEmpty(list[i].values[0]))
                {
                    listado.Add(list[i]);
                }
            }
            return listado;
        }

        /// <summary>
        /// Devuelve si un listado de secciones contiene el codigo, en caso de que <paramref name="secciones"/> sea nulo o vacio se devuelve true.
        /// </summary>
        /// <param name="secciones"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public static bool CheckSecciones(List<string> secciones, string codigo)
        {
            if (secciones == null)
            {
                return true;
            }
            if (secciones.Count == 0)
            {
                return true;
            }
            else
            {
                return secciones.Contains(codigo);
            }
        }

        /// <summary>
        /// Añade la referencia a la entidad <paramref name="propiedadNombreEntidad"/> si esta se encuentra en BBDD.
        /// Si se pasa el valor <paramref name="aux"/> lo concatena con <paramref name="nombreEntidad"/> para formar la entidad auxiliar.
        /// </summary>
        /// <param name="mResourceApi"></param>
        /// <param name="nombreEntidad"></param>
        /// <param name="propiedadNombreEntidad"></param>
        /// <param name="propiedadEntidad"></param>
        /// <param name="entidadAux"></param>
        public static void AniadirEntidadOrganizacion(ResourceApi mResourceApi, string nombreEntidad, string propiedadNombreEntidad, string propiedadEntidad, Entity entidadAux, [Optional] string aux)
        {
            if (mResourceApi == null || string.IsNullOrEmpty(nombreEntidad) ||
                string.IsNullOrEmpty(propiedadEntidad) || string.IsNullOrEmpty(propiedadEntidad))
            { return; }

            Property propertyNombre = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadNombreEntidad);
            Property propertyEntidad = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadEntidad);

            string entidadN = GetOrganizacionPorNombre(mResourceApi, nombreEntidad);
            if (!string.IsNullOrEmpty(aux))
            {
                nombreEntidad = aux + nombreEntidad;
                entidadN = aux + entidadN;
            }

            if (!string.IsNullOrEmpty(entidadN))
            {
                CheckProperty(propertyNombre, entidadAux, nombreEntidad, propiedadNombreEntidad);
                CheckProperty(propertyEntidad, entidadAux, entidadN, propiedadEntidad);
            }
            else
            {
                CheckProperty(propertyNombre, entidadAux, nombreEntidad, propiedadNombreEntidad);
                CheckProperty(propertyEntidad, entidadAux, "", propiedadEntidad);
            }
        }

        /// <summary>
        /// Añade en <paramref name="entidadAux"/> el <paramref name="valorXML"/> en caso de que no sea nulo.
        /// Si <paramref name="property"/> no es valor nulo, se añade el <paramref name="valorXML"/> en la propiedad,
        /// sino se crea una <paramref name="propiedadXML"/> en <paramref name="entidadAux"/>
        /// </summary>
        /// <param name="property">property</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="valorXML">valorXML</param>
        /// <param name="propiedadXML">propiedadXML</param>
        public static void CheckProperty(Property property, Entity entidadAux, string valorXML, string propiedadXML)
        {
            if (valorXML == null) { return; }

            if (property != null)
            {
                property.values.Add(valorXML);
            }
            else
            {
                entidadAux.properties.AddRange(AddProperty(
                    new Property(propiedadXML, valorXML)
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de códigos UNESCO de <paramref name="listaCodUnesco"/>,
        /// en <paramref name="propiedadCodUnesco"/>.
        /// </summary>
        /// <param name="listaCodUnesco">listaCodigosUnesco</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="propiedadCodUnesco">propiedadCodigoUnesco</param>
        public static void CodigosUnesco(List<CvnItemBeanCvnString> listaCodUnesco, Entity entidadAux, string propiedadCodUnesco)
        {
            //No hago nada si no se pasa la propiedad.
            if (string.IsNullOrEmpty(propiedadCodUnesco))
            { return; }

            foreach (CvnItemBeanCvnString codigo in listaCodUnesco)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                List<string> listadoCodigos = Utility.GetPadresCodUnesco(codigo);
                //Añado Codigo UNESCO
                foreach (string codigolista in listadoCodigos)
                {
                    Property propertyCodUnesco = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadCodUnesco);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    CheckProperty(propertyCodUnesco, entidadAux, valorCodigo, propiedadCodUnesco);
                }
            }
        }

        /// <summary>
        /// Comprueba los DOI en BBDD y los elimina de <paramref name="listadoDOI"/> si están.
        /// </summary>
        /// <param name="listadoDOI"></param>
        /// <returns></returns>
        public static List<string> ComprobarDOIenBBDD(List<string> listadoDOI)
        {
            List<string> listaDOIenBBDD = new();

            string select = $@"select ?doi";
            string where = $@"where{{
                                ?document a <http://purl.org/ontology/bibo/Document> .
                                ?document <http://purl.org/ontology/bibo/doi> ?doi . 
                                FILTER( ?doi in ('{string.Join("','", listadoDOI)}') )
                            }}";
            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "document");
            if (sparqlObject.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                {
                    if (fila.ContainsKey("doi") && !string.IsNullOrEmpty(fila["doi"].value))
                    {
                        listaDOIenBBDD.Add(fila["doi"].value);
                    }
                }
            }
            //Elimino los DOI que aparecen en BBDD
            foreach (string doi in listaDOIenBBDD)
            {
                listadoDOI.Remove(doi);
            }

            return listadoDOI;
        }

        /// <summary>
        /// Llamada al servicio de Fuentes externas para la inserción de la publicación con doi <paramref name="doi"/> del autor <paramref name="userId"/>
        /// </summary>
        /// <param name="mConfiguracion"></param>
        /// <param name="doi"></param>
        /// <param name="nombreAutor"></param>
        /// <param name="orcid"></param>
        /// <returns>True si se inserta, false en caso contrario</returns>
        public static bool EnvioFuentesExternasDOI(ConfigService mConfiguracion, string doi, string idPersona, string nombreCompletoAutor)
        {
            try
            {
                string urlEstado = mConfiguracion.GetUrlServicioExterno() + "/FuentesExternas/InsertDoiToQueue?pIdentificador=doi&pDoi=" + doi + "&pIdPersona=" + idPersona + "&pNombreCompletoAutor=" + nombreCompletoAutor;
                HttpClient httpClientEstado = new();
                HttpResponseMessage responseEstado = httpClientEstado.GetAsync($"{urlEstado}").Result;

                bool status = responseEstado.IsSuccessStatusCode;
                if (status)
                {
                    return bool.Parse(responseEstado.Content.ReadAsStringAsync().Result);
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Devuelve la referencia a la publicacion con doi <paramref name="doiPublicacion"/>.
        /// </summary>
        /// <param name="doiPublicacion">DOI de publicación</param>
        /// <returns>Referencia a la publicación</returns>
        public static string GetPublicationDOI(string doiPublicacion)
        {
            if (string.IsNullOrEmpty(doiPublicacion))
            {
                return null;
            }

            int offsetInt = 0;
            int limit = 10000;

            if (dicDOI.Count == 0)
            {
                while (true)
                {
                    string select = $@"select distinct ?document ?doi";
                    string where = $@"where{{
    ?document a <http://purl.org/ontology/bibo/Document> .
    ?document <http://purl.org/ontology/bibo/doi> ?doi .
}}LIMIT {limit} OFFSET {offsetInt}";

                    SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "document");
                    if (sparqlObject.results.bindings.Count > 0)
                    {
                        foreach (Dictionary<string, Data> fila in sparqlObject.results.bindings)
                        {
                            string document = fila["document"].value;
                            string doi = fila["doi"].value;
                            dicTopics[doi] = document;
                        }
                    }

                    offsetInt += limit;
                    if (sparqlObject.results.bindings.Count < limit)
                    {
                        break;
                    }
                }
            }

            if (dicTopics.ContainsKey(doiPublicacion))
            {
                return dicTopics[doiPublicacion];
            }

            return null;
        }

        /// <summary>
        /// Diccionario con el tesauro de todas las palabras clave, 
        /// con clave el identificador de la palabra clave y valor el identificador del padre o null en caso de ser el ultimo nodo.
        /// </summary>
        /// <param name="mResourceApi"></param>
        /// <returns></returns>
        public static Dictionary<string, string> PalabrasClave(ResourceApi mResourceApi)
        {
            if (mListaPalabrasClave.Count > 0) { return mListaPalabrasClave; }

            string select = "select ?idS ?idPadre ";
            string where = @$"where {{
                                ?s a <http://www.w3.org/2008/05/skos#Concept>.
                                ?s <http://purl.org/dc/elements/1.1/source> 'tesauro_cvn'.
                                ?s <http://purl.org/dc/elements/1.1/identifier> ?idS.
                                OPTIONAL {{ ?s <http://www.w3.org/2008/05/skos#broader> ?padre. ?padre <http://purl.org/dc/elements/1.1/identifier> ?idPadre. }}
                            }} ORDER BY ?padre ?s ";
            SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "taxonomy");
            if (sparqlObject.results.bindings.Count > 0)
            {
                for (int i = 0; i < sparqlObject.results.bindings.Count; i++)
                {
                    mListaPalabrasClave[sparqlObject.results.bindings.Select(x => x["idS"].value).ElementAt(i)] = sparqlObject.results.bindings.ElementAt(i).ContainsKey("idPadre") ?
                        sparqlObject.results.bindings.Select(x => x["idPadre"].value).ElementAt(i) : null;
                }
            }

            return mListaPalabrasClave;
        }

        /// <summary>
        /// Devuelve un listado con los padres del tesauro incluido al hijo pasado como parametro.
        /// </summary>
        /// <param name="hijo">Nodo final del tesauro</param>
        /// <returns>Listado con los padres del teauro desde el hijo</returns>
        public static List<string> GetPadresTesauro(string hijo)
        {
            HashSet<string> listado = new()
            {
                hijo
            };

            //Base del recurso
            string item = hijo.Split("_").First();
            //Numeracion
            string values = hijo.Split("_").Last();
            List<string> listValues = values.Split(".").ToList();
            //Contador para concatenar los hijos
            string numCeros = "0";
            //Elimino el valor del último elemento
            listValues.RemoveAt(listValues.Count - 1);

            while (listValues.Count != 0)
            {
                string valueAux = item + "_";
                foreach (string valueIn in listValues)
                {
                    valueAux += valueIn + ".";
                }
                //Concateno los ".0" y sumo uno más
                valueAux += numCeros;
                numCeros += ".0";
                //Elimino el ultimo valor para llegar a su padre
                listValues.RemoveAt(listValues.Count - 1);
                //En caso de que no esté en el listado añado el valor
                if (!listado.Contains(valueAux))
                {
                    listado.Add(valueAux);
                }
            }

            return listado.ToList();
        }

        /// <summary>
        /// Obtiene los topics de base de datos, los guarda en dicTopics en caso de no estar inicializado y devuelve la referencia en BBDD si se encuentra en el listado.
        /// </summary>
        /// <param name="nombreTopic">Nombre del topic</param>
        /// <returns>Referencia en BBDD</returns>
        public static string ObtenerTopics(string nombreTopic)
        {
            if (string.IsNullOrEmpty(nombreTopic))
            {
                return null;
            }

            int offsetInt = 0;
            int limit = 10000;

            if (dicTopics.Count == 0)
            {
                while (true)
                {
                    //Selecciono las labels que no tengan hijos del tesauro (de último nivel), el tesauro no es multiidioma por lo que no filtro por el lenguaje.
                    string select = $@"select distinct ?skos ?label";
                    string where = $@"where{{
    ?skos a <http://www.w3.org/2008/05/skos#Concept> . 
    ?skos <http://www.w3.org/2008/05/skos#prefLabel> ?label .
    ?skos <http://purl.org/dc/elements/1.1/source> 'researcharea'.
    MINUS{{
        ?skos <http://www.w3.org/2008/05/skos#narrower> ?hijo
    }}
}}LIMIT {limit} OFFSET {offsetInt}";

                    SparqlObject sparqlObject = mResourceApi.VirtuosoQuery(select, where, "taxonomy");
                    if (sparqlObject.results.bindings.Count > 0)
                    {
                        foreach (Dictionary<string, SparqlObject.Data> fila in sparqlObject.results.bindings)
                        {
                            string skos = fila["skos"].value;
                            string label = fila["label"].value;
                            dicTopics[label] = skos;
                        }
                    }

                    offsetInt += limit;
                    if (sparqlObject.results.bindings.Count < limit)
                    {
                        break;
                    }
                }
            }
            if (dicTopics.ContainsKey(nombreTopic))
            {
                return dicTopics[nombreTopic];
            }

            return null;
        }

        /// <summary>
        /// Obtiene el identificador y nick de la persona con CV <paramref name="CVID"/>
        /// </summary>
        /// <param name="resourceApi"></param>
        /// <param name="CVID"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ObtenerIdPersona(ResourceApi resourceApi, string CVID)
        {
            Dictionary<string, string> resultado = new();
            string select = $@"SELECT distinct ?idPersona ?nombreCompleto ";
            string where = $@"where {{
                                <{CVID}> <http://w3id.org/roh/cvOf> ?idPersona . 
                                ?idPersona <http://xmlns.com/foaf/0.1/name> ?nombreCompleto 
                            }}";

            SparqlObject sparqlObject = resourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "curriculumvitae", "person" });
            if (sparqlObject.results.bindings.Count > 0)
            {
                string nick = sparqlObject.results.bindings.Any(x => x.ContainsKey("nombreCompleto")) ? sparqlObject.results.bindings.Select(x => x["nombreCompleto"].value)?.FirstOrDefault() : null;
                resultado.Add(sparqlObject.results.bindings.Select(x => x["idPersona"].value).FirstOrDefault(), nick);
            }
            return resultado;
        }

        /// <summary>
        /// Inserta los valores de los autores en cada una de sus propiedades.
        /// </summary>
        /// <param name="listaAutores">listaAutores</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="propiedadAutorFirma">propiedadAutorFirma</param>
        /// <param name="propiedadAutorOrden">propiedadAutorOrden</param>
        /// <param name="propiedadAutorNombre">propiedadAutorNombre</param>
        /// <param name="propiedadAutorPrimerApellido">propiedadAutorPrimerApellido</param>
        /// <param name="propiedadAutorSegundoApellido">propiedadAutorSegundoApellido</param>
        public static void InsertaAutorProperties(List<CvnItemBeanCvnAuthorBean> listaAutores, Entity entidadAux, string propiedadAutorFirma, string propiedadAutorOrden,
            string propiedadAutorNombre, string propiedadAutorPrimerApellido, string propiedadAutorSegundoApellido)
        {

            //No hago nada si no se pasa la propiedad.
            if (string.IsNullOrEmpty(propiedadAutorFirma) || string.IsNullOrEmpty(propiedadAutorOrden) ||
                string.IsNullOrEmpty(propiedadAutorNombre) || string.IsNullOrEmpty(propiedadAutorPrimerApellido) ||
                string.IsNullOrEmpty(propiedadAutorSegundoApellido))
            {
                return;
            }

            foreach (CvnItemBeanCvnAuthorBean autor in listaAutores)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                Property propertyFirma = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorFirma);
                Property propertyOrden = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorOrden);
                Property propertyNombre = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorNombre);
                Property propertyPrimerApellido = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorPrimerApellido);
                Property propertySegundoApellido = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorSegundoApellido);

                string valorFirma = StringGNOSSID(entityPartAux, autor.GetFirmaAutor());
                string valorOrden = StringGNOSSID(entityPartAux, autor.GetOrdenAutor());
                string valorNombre = StringGNOSSID(entityPartAux, autor.GetNombreAutor());
                string valorPrimerApellido = StringGNOSSID(entityPartAux, autor.GetPrimerApellidoAutor());
                string valorSegundoApellido = StringGNOSSID(entityPartAux, autor.GetSegundoApellidoAutor());

                //Si no tiene ningun valor no lo inserto
                if (string.IsNullOrEmpty(valorFirma) && string.IsNullOrEmpty(valorOrden) &&
                    string.IsNullOrEmpty(valorNombre) && string.IsNullOrEmpty(valorPrimerApellido) &&
                    string.IsNullOrEmpty(valorSegundoApellido))
                {
                    continue;
                }

                CheckProperty(propertyFirma, entidadAux, valorFirma, propiedadAutorFirma);
                CheckProperty(propertyOrden, entidadAux, valorOrden, propiedadAutorOrden);
                CheckProperty(propertyNombre, entidadAux, valorNombre, propiedadAutorNombre);
                CheckProperty(propertyPrimerApellido, entidadAux, valorPrimerApellido, propiedadAutorPrimerApellido);
                CheckProperty(propertySegundoApellido, entidadAux, valorSegundoApellido, propiedadAutorSegundoApellido);
            }
        }

        /// <summary>
        /// Inserta los Identificadores de publicación dependiendo del tipo
        /// 120-Handle, 040-DOI, 130-PMID, OTHERS-otros identificadores.
        /// </summary>
        /// <param name="listadoIDs">listadoIDs</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="propIdHandle">propIdHandle</param>
        /// <param name="propIdDOI">propIdDOI</param>
        /// <param name="propIdPMID">propIdPMID</param>
        /// <param name="propIdOtroPub">propIdOtroPub</param>
        /// <param name="nombreOtroPub">nombreOtroPub</param>
        public static void InsertaTiposIDPublicacion(List<CvnItemBeanCvnExternalPKBean> listadoIDs, Entity entidadAux,
            string propIdHandle, string propIdDOI, string propIdPMID, string propIdOtroPub, string nombreOtroPub)
        {
            //Si alguna propiedad es nula no hago nada
            if (string.IsNullOrEmpty(propIdHandle) && string.IsNullOrEmpty(propIdDOI)
                && string.IsNullOrEmpty(propIdPMID) && string.IsNullOrEmpty(propIdOtroPub)
                && string.IsNullOrEmpty(nombreOtroPub))
            { return; }

            foreach (CvnItemBeanCvnExternalPKBean identificador in listadoIDs)
            {
                if (identificador.Type == null)
                {
                    continue;
                }

                switch (identificador.Type)
                {
                    //Handle
                    case "120":
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(propIdHandle, identificador.Value)
                        ));
                        break;
                    //DOI
                    case "040":
                        string doi = identificador.Value;
                        doi = doi.Replace("http://dx.doi.org/", "");
                        doi = doi.Replace("http://doi.org/", "");
                        doi = doi.Replace("https://dx.doi.org/", "");
                        doi = doi.Replace("https://doi.org/", "");
                        doi = doi.Replace("doi:", "");
                        doi = doi.Replace("DOI:", "");
                        doi = doi.Trim();

                        entidadAux.properties.AddRange(AddProperty(
                            new Property(propIdDOI, doi)
                        ));
                        break;
                    //PMID
                    case "130":
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(propIdPMID, identificador.Value)
                        ));
                        break;
                    case "OTHERS":
                        Property IDOtro = entidadAux.properties.FirstOrDefault(x => x.prop == propIdOtroPub);

                        string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                        string valorID = StringGNOSSID(entityPartAux, identificador.Value);
                        CheckProperty(IDOtro, entidadAux, valorID, propIdOtroPub);

                        string valorNombre = StringGNOSSID(entityPartAux, identificador.Others);
                        CheckProperty(IDOtro, entidadAux, valorNombre, nombreOtroPub);
                        break;
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de los ISBN.
        /// </summary>
        /// <param name="listadoISBN">listadoISBN</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="propiedadISBN">propiedadISBN</param>
        public static void InsertaISBN(List<CvnItemBeanCvnExternalPKBean> listadoISBN, Entity entidadAux, string propiedadISBN)
        {
            //No hago nada si no se pasa la propiedad.
            if (string.IsNullOrEmpty(propiedadISBN))
            { return; }

            foreach (CvnItemBeanCvnExternalPKBean isbn in listadoISBN)
            {
                //Si no hay type, ignoro el valor
                if (string.IsNullOrEmpty(isbn.Type)) { continue; }

                //Si es ISBN (020)
                if (isbn.Type.Equals("020"))
                {
                    entidadAux.properties.AddRange(AddProperty(
                        new Property(propiedadISBN, isbn.Value)
                    ));
                }

            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de los ISSN.
        /// </summary>
        /// <param name="listadoISBN"></param>
        /// <param name="entidadAux"></param>
        /// <param name="propiedadISSN"></param>
        public static void InsertaISSN(List<CvnItemBeanCvnExternalPKBean> listadoISBN, Entity entidadAux, string propiedadISSN)
        {
            //No hago nada si no se pasa la propiedad.
            if (string.IsNullOrEmpty(propiedadISSN))
            { return; }

            foreach (CvnItemBeanCvnExternalPKBean issn in listadoISBN)
            {
                //Si no hay type, ignoro el valor
                if (string.IsNullOrEmpty(issn.Type)) { continue; }

                //Si es ISSN (010)
                if (issn.Type.Equals("010"))
                {
                    entidadAux.properties.AddRange(AddProperty(
                        new Property(propiedadISSN, issn.Value)
                    ));
                }

            }
        }

        /// <summary>
        /// Añade los PhoneBean de <paramref name="listado"/> en <paramref name="entidadAux"/>.
        /// </summary>
        /// <param name="listado">listado de numeros telefonicos</param>
        /// <param name="entidadAux">entidadAux</param>
        /// <param name="propiedadNumero">propiedadNumero</param>
        /// <param name="propiedadCodInternacional">propiedadCodInternacional</param>
        /// <param name="propiedadExtension">propiedadExtension</param>
        public static void InsertarListadoTelefonos(List<CvnItemBeanCvnPhoneBean> listado, Entity entidadAux, string propiedadNumero, string propiedadCodInternacional, string propiedadExtension)
        {
            //No hago nada si no se pasan las propiedades de cada parametro.
            if (string.IsNullOrEmpty(propiedadNumero) && string.IsNullOrEmpty(propiedadExtension)
                && string.IsNullOrEmpty(propiedadCodInternacional))
            { return; }

            foreach (CvnItemBeanCvnPhoneBean telefono in listado)
            {
                //No añado si no hay numero de telefono.
                if (string.IsNullOrEmpty(telefono.Number)) { continue; }

                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                //Añado Numero
                Property propertyNumero = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadNumero);

                string valorNumero = StringGNOSSID(entityPartAux, telefono.Number);

                CheckProperty(propertyNumero, entidadAux, valorNumero, propiedadNumero);

                //Añado Codigo Internacional
                Property propertyCodInternacional = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadCodInternacional);

                string valorCodInternacional = StringGNOSSID(entityPartAux, telefono.InternationalCode);

                CheckProperty(propertyCodInternacional, entidadAux, valorCodInternacional, propiedadCodInternacional);

                //Añado Extension
                Property propertyExtension = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadExtension);

                string valorExtension = StringGNOSSID(entityPartAux, telefono.Extension);

                CheckProperty(propertyExtension, entidadAux, valorExtension, propiedadExtension);
            }
        }

        /// <summary>
        /// Dada una cadena de GUID concatenados y un string en caso de que 
        /// el string no sea nulo los concatena, sino devuelve null.
        /// </summary>
        /// <param name="entityAux">GUID concatenado</param>
        /// <param name="valor">Valor del parametro</param>
        /// <returns>String de concatenar los parametros, o nulo si el valor es vacio</returns>
        public static string StringGNOSSID(string entityAux, string valor)
        {
            if (!string.IsNullOrEmpty(valor))
            {
                return entityAux + valor;
            }
            return null;
        }

        /// <summary>
        /// Dado un string, devuelve si el string tiene un formato de email correcto.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsEmailValid(string emailAddress)
        {
            try
            {
                _ = new MailAddress(emailAddress);
                return true;
            }
            catch (FormatException)
            {
                mResourceApi.Log.Error("Fallo al convertir el email");
                return false;
            }
        }

    }
}
