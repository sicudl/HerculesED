﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.DisambiguationEngine.Models;
using Hercules.ED.ImportadorWebCV.Models;
using Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class TrabajosCongresos : DisambiguableEntity
    {
        public string titulo { get; set; }
        public string fecha { get; set; }
        private HashSet<string> autores { get; set; }

        private static readonly DisambiguationDataConfig configTitulo = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static readonly DisambiguationDataConfig configFecha = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        private static readonly DisambiguationDataConfig configAutores = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItemList,
            score = 0.5f
        };

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new List<DisambiguationData>
            {
                new DisambiguationData()
                {
                    property = "titulo",
                    config = configTitulo,
                    value = titulo
                },

                new DisambiguationData()
                {
                    property = "fecha",
                    config = configFecha,
                    value = fecha
                },

                new DisambiguationData()
                {
                    property = "autores",
                    config = configAutores,
                    values = autores
                }
            };

            return data;
        }

        /// <summary>
        /// Devuelve las entidades de BBDD del <paramref name="pCVID"/> de con las propiedades de <paramref name="propiedadesItem"/>
        /// </summary>
        /// <param name="pResourceApi">pResourceApi</param>
        /// <param name="pCVID">pCVID</param>
        /// <param name="graph">graph</param>
        /// <param name="propiedadesItem">propiedadesItem</param>
        /// <returns></returns>
        public static Dictionary<string, DisambiguableEntity> GetBBDD(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem, List<Entity> listadoAux)
        {
            //Obtenemos IDS
            HashSet<string> ids = Utils.UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultados = new Dictionary<string, DisambiguableEntity>();

            //Divido la lista en listas de 1.000 elementos
            List<List<string>> listaListas = Utils.UtilitySecciones.SplitList(ids.ToList(), 1000).ToList();

            foreach (List<string> lista in listaListas)//TODO -revisar consulta(autores)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.trabajosCongresosTitulo}> ?itemTitle .
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.trabajosCongresosPubFecha}> ?itemDate }} .
                                        OPTIONAL{{ 
                                            ?item <http://purl.org/ontology/bibo/authorList> ?authorList . 
                                            ?authorList <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autor
                                        }}
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    TrabajosCongresos trabajosCongresos = new TrabajosCongresos
                    {
                        ID = fila["item"].value,
                        titulo = fila["itemTitle"].value,
                        fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : ""
                    };

                    trabajosCongresos.autores = new HashSet<string>();
                    if (fila.ContainsKey("autores"))
                    {
                        string[] autores = fila["autores"].value.Split("|");
                        foreach (string autor in autores)
                        {
                            trabajosCongresos.autores.Add(autor);
                        }
                    }
                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), trabajosCongresos);
                }
            }
            HashSet<string> listaNombres = new HashSet<string>();
            ConcurrentDictionary<string, List<Persona>> listaPersonasAux = new ConcurrentDictionary<string, List<Persona>>();

            //Selecciono el nombre completo o la firma.
            foreach (Entity item in listadoAux)
            {
                for (int i = 0; i < item.autores.Count; i++)
                {
                    listaNombres.Add(item.autores[i].NombreBuscar);
                }
            }
            //TODO
            ////Divido la lista en listas de 10 elementos


            Parallel.ForEach(listaNombres, new ParallelOptions { MaxDegreeOfParallelism = 5 }, firma =>
            {
                if (firma.Trim() != "")
                {
                    List<Persona> personasBBDD = Utility.ObtenerPersonasFirma(pResourceApi, firma.Trim());
                    listaPersonasAux[firma.Trim()] = personasBBDD;
                }
            });

            //Divido la lista en listas de 1.000 elementos
            List<List<string>> listaListasIdPersonas = Utils.UtilitySecciones.SplitList(listaPersonasAux.SelectMany(x => x.Value).Select(x => x.personid).ToList(), 1000).ToList();

            foreach (List<string> lista in listaListasIdPersonas)
            {
                string select = $@"SELECT distinct ?item group_concat(?autor;separator=""|"") as ?autores ";
                string where = $@"where {{
                                        ?item a <http://purl.org/ontology/bibo/Document> . 
                                        ?item <http://purl.org/ontology/bibo/authorList> ?authorList . 
                                        ?authorList <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autorIn .                                        
                                        ?item <http://purl.org/ontology/bibo/authorList> ?authorList2 . 
                                        ?authorList2 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autor .
                                        FILTER(?autorIn in (<{string.Join(">,<", lista)}>))                                        
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    string doc = fila["item"].value;
                    HashSet<string> autores = new HashSet<string>();
                    if (fila.ContainsKey("autores"))
                    {
                        foreach (string autor in fila["autores"].value.Split("|"))
                        {
                            autores.Add(autor);
                        }
                    }
                    foreach (string autorIn in autores)
                    {
                        List<Persona> personas = listaPersonasAux.SelectMany(x => x.Value).Where(x => x.personid == autorIn).ToList();
                        foreach (Persona persona in personas)
                        {
                            persona.documentos.Add(doc);
                            persona.coautores.UnionWith(autores.Except(new List<string>() { persona.personid }));
                        }
                    }

                }
            }

            //Añado los autores de BBDD para la desambiguación
            for (int i = 0; i < listaPersonasAux.Count; i++)
            {
                Persona persona = new Persona
                {
                    nombreCompleto = listaPersonasAux.ElementAt(i).Value.Select(x => x.nombreCompleto).FirstOrDefault(),
                    firma = listaPersonasAux.ElementAt(i).Value.Select(x => x.firma).FirstOrDefault(),
                    coautores = listaPersonasAux.ElementAt(i).Value.Select(x => x.coautores).FirstOrDefault(),
                    documentos = listaPersonasAux.ElementAt(i).Value.Select(x => x.documentos).FirstOrDefault(),

                    ID = listaPersonasAux.ElementAt(i).Value.Select(x => x.personid).FirstOrDefault()
                };
                if (string.IsNullOrEmpty(persona.nombreCompleto) && string.IsNullOrEmpty(persona.firma)) { continue; }

                resultados[persona.ID] = persona;
            }

            return resultados;
        }
    }
}
