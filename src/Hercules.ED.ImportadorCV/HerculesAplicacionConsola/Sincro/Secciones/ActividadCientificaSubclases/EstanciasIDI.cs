﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.DisambiguationEngine.Models;
using HerculesAplicacionConsola.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace HerculesAplicacionConsola.Sincro.Secciones.ActividadCientifica
{
    class EstanciasIDI : DisambiguableEntity
    {
        public string descripcion { get; set; }
        public string fecha { get; set; }
        public string entidadRealizacion { get; set; }

        private static DisambiguationDataConfig configDescripcion = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static DisambiguationDataConfig configFecha = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };
        
        private static DisambiguationDataConfig configER = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new List<DisambiguationData>();

            data.Add(new DisambiguationData()
            {
                property = "descripcion",
                config = configDescripcion,
                value = descripcion
            });

            data.Add(new DisambiguationData()
            {
                property = "fecha",
                config = configFecha,
                value = fecha
            });

            data.Add(new DisambiguationData()
            {
                property = "entidadRealizacion",
                config = configER,
                value = entidadRealizacion
            });
            return data;
        }

        public static Dictionary<string, DisambiguableEntity> GetBBDD(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultados = new Dictionary<string, DisambiguableEntity>();

            //Divido la lista en listas de 1.000 elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), 1000).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemER ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.estanciasIDITareasContrastables}> ?itemTitle . 
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.estanciasIDIFechaInicioEntidadRealizacion}> ?itemDate }} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.estanciasIDIEntidadRealizacionNombre}> ?itemER }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    EstanciasIDI estanciasIDI = new EstanciasIDI();
                    estanciasIDI.ID = fila["item"].value;
                    estanciasIDI.descripcion = fila["itemTitle"].value;
                    estanciasIDI.fecha = "";
                    if (fila.ContainsKey("itemDate"))
                    {
                        estanciasIDI.fecha = fila["itemDate"].value;
                    }
                    estanciasIDI.entidadRealizacion = "";
                    if (fila.ContainsKey("itemER"))
                    {
                        estanciasIDI.entidadRealizacion = fila["itemER"].value;
                    }
                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), estanciasIDI);
                }
            }

            return resultados;
        }
    }
}
