﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.GraphicEngine.Models.Graficas;
using Hercules.ED.GraphicEngine.Models.Paginas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Web;

namespace Hercules.ED.GraphicEngine.Models
{
    public static class GraphicEngine
    {
        // Prefijos.
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config\configJson\prefijos.json")));
        private static ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static CommunityApi mCommunityApi = new CommunityApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");
        private static Guid mCommunityID = mCommunityApi.GetCommunityId();
        private static List<ConfigModel> mTabTemplates;

        #region --- Páginas
        /// <summary>
        /// Obtiene los datos de la página.
        /// </summary>
        /// <param name="pIdPagina">Identificador de la página.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static Pagina GetPage(string pIdPagina, string pLang)
        {
            // Lectura del JSON de configuración.
            ConfigModel configModel = TabTemplates.FirstOrDefault(x => x.identificador == pIdPagina);

            return CrearPagina(configModel, pLang);
        }

        /// <summary>
        /// Crea el objeto página.
        /// </summary>
        /// <param name="pConfigModel">Configuración.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static Pagina CrearPagina(ConfigModel pConfigModel, string pLang)
        {
            Pagina pagina = new Pagina();
            pagina.id = pConfigModel.identificador;
            pagina.nombre = GetTextLang(pLang, pConfigModel.nombre);
            pagina.listaIdsGraficas = new List<string>();
            foreach (Grafica itemGrafica in pConfigModel.graficas)
            {
                pagina.listaIdsGraficas.Add(itemGrafica.identificador);
            }
            pagina.listaIdsFacetas = new List<string>();
            foreach (FacetaConf itemFaceta in pConfigModel.facetas)
            {
                pagina.listaIdsFacetas.Add(itemFaceta.filtro);
            }
            return pagina;
        }
        #endregion

        #region --- Gráficas
        /// <summary>
        /// Lee la configuración y obtiene los datos necesarios para el servicio de gráficas.
        /// </summary>
        /// <param name="pIdPagina">Identificador de la página.</param>
        /// <param name="pIdGrafica">Identificador de la gráfica.</param>
        /// <param name="pFiltroFacetas">Filtros de la URL.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static GraficaBase GetGrafica(string pIdPagina, string pIdGrafica, string pFiltroFacetas, string pLang)
        {
            // Lectura del JSON de configuración.
            ConfigModel configModel = TabTemplates.FirstOrDefault(x => x.identificador == pIdPagina);
            if (configModel != null)
            {
                Grafica grafica = configModel.graficas.FirstOrDefault(x => x.identificador == pIdGrafica);
                return CrearGrafica(grafica, configModel.filtro, pFiltroFacetas, pLang);
            }

            return null;
        }

        /// <summary>
        /// Crea el objeto gráfica.
        /// </summary>
        /// <param name="pGrafica">Configuración.</param>
        /// <param name="pFiltroBase">Filtros base.</param>
        /// <param name="pFiltroFacetas">Filtros de las facetas.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static GraficaBase CrearGrafica(Grafica pGrafica, string pFiltroBase, string pFiltroFacetas, string pLang)
        {
            pFiltroFacetas = HttpUtility.UrlDecode(pFiltroFacetas);
            switch (pGrafica.tipoGrafica)
            {
                case EnumGraficas.Barras:
                    // TODO: Controlar excepciones en la configuración.
                    if (string.IsNullOrEmpty(pGrafica.configBarras.ejeX))
                    {
                        throw new Exception("No está configurada la propiedad del agrupación del eje x.");
                    }
                    if (pGrafica.configBarras.dimensiones == null || pGrafica.configBarras.dimensiones.Count() == 0)
                    {
                        throw new Exception("No se ha configurado dimensiones.");
                    }
                    return CrearGraficaBarras(pGrafica, pFiltroBase, pFiltroFacetas, pLang);
                case EnumGraficas.Circular:
                    // TODO: Controlar excepciones en la configuración.
                    return CrearGraficaCircular(pGrafica, pFiltroBase, pFiltroFacetas, pLang);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Crea el objeto de la gráfica (Gráfica de Barras).
        /// </summary>
        /// <param name="pGrafica">Configuración.</param>
        /// <param name="pFiltroBase">Filtros base.</param>
        /// <param name="pFiltroFacetas">Filtros de las facetas.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static GraficaBase CrearGraficaBarras(Grafica pGrafica, string pFiltroBase, string pFiltroFacetas, string pLang)
        {
            // Objeto a devolver.
            GraficaBase grafica = new GraficaBase();
            grafica.type = "bar"; // Por defecto, de tipo bar.

            // Asignación de Data.
            Data data = new Data();
            data.datasets = new ConcurrentBag<Dataset>();
            grafica.data = data;

            // Asignación de Options.
            Options options = new Options();

            // Orientación            
            if (!pGrafica.configBarras.orientacionVertical)
            {
                options.indexAxis = "y";
            }
            else
            {
                options.indexAxis = "x";
            }

            options.scales = new Dictionary<string, Eje>();

            // Ejes X
            foreach (EjeXConf item in pGrafica.configBarras.xAxisPrint)
            {
                options.scales.Add(item.xAxisID, new Eje() { position = item.posicion });
            }

            // Ejes Y
            foreach (EjeYConf item in pGrafica.configBarras.yAxisPrint)
            {
                options.scales.Add(item.yAxisID, new Eje() { position = item.posicion });
            }

            // Animación
            options.animation = new Animation();
            options.animation.duration = 2000;

            // Título
            options.plugins = new Plugin();
            options.plugins.title = new Title();
            options.plugins.title.display = true;
            options.plugins.title.text = GetTextLang(pLang, pGrafica.nombre);
            
            grafica.options = options;

            ConcurrentDictionary<Dimension, List<Tuple<string, string, float>>> resultadosDimension = new ConcurrentDictionary<Dimension, List<Tuple<string, string, float>>>();
            Dictionary<Dimension, Dataset> dimensionesDataset = new Dictionary<Dimension, Dataset>();

            // Invierte las dimensiones para que la grafica de línea salga por encima de la de barras.
            pGrafica.configBarras.dimensiones.Reverse();

            foreach (Dimension dim in pGrafica.configBarras.dimensiones)
            {
                resultadosDimension[dim] = null;
                dimensionesDataset[dim] = null;
            }

            Parallel.ForEach(pGrafica.configBarras.dimensiones, new ParallelOptions { MaxDegreeOfParallelism = 1 }, itemGrafica =>
            {
                // Determina si en el filtro contiene '=' para tratarlo de manera especial.
                bool filtroEspecial = false;
                if (!string.IsNullOrEmpty(itemGrafica.filtro) && !itemGrafica.filtro.Contains("="))
                {
                    filtroEspecial = true;
                }

                // Orden.
                string orden = "ASC";
                if (pGrafica.configBarras.orderDesc == true)
                {
                    orden = "DESC";
                }

                // Filtro de página.
                List<string> filtros = new List<string>();

                List<Tuple<string, string, float>> listaTuplas = new List<Tuple<string, string, float>>();
                SparqlObject resultadoQuery = null;
                StringBuilder select = new StringBuilder(), where = new StringBuilder();
                filtros.AddRange(ObtenerFiltros(new List<string>() { pGrafica.configBarras.ejeX }, "ejeX"));
                filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroBase }));
                if (!string.IsNullOrEmpty(pFiltroFacetas))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroFacetas }));
                }
                if(filtroEspecial)
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { itemGrafica.filtro }, "aux"));
                }
                else if (!string.IsNullOrEmpty(itemGrafica.filtro))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { itemGrafica.filtro }));
                }
                if (string.IsNullOrEmpty(itemGrafica.calculo))
                {
                    // Consulta sparql.                    
                    select = new StringBuilder();
                    where = new StringBuilder();

                    select.Append(mPrefijos);
                    if (filtroEspecial)
                    {
                        select.Append($@"SELECT ?ejeX ?aux COUNT(DISTINCT ?s) AS ?numero ");
                    }
                    else
                    {
                        select.Append($@"SELECT ?ejeX COUNT(DISTINCT ?s) AS ?numero ");
                    }
                    where.Append("WHERE { ");
                    foreach (string item in filtros)
                    {
                        where.Append(item);
                    }
                    if (filtroEspecial)
                    {
                        where.Append($@"FILTER(LANG(?aux) = 'es' OR LANG(?aux) = '' OR !isLiteral(?aux))");
                    }
                    where.Append($@"}} ORDER BY {orden}(?ejeX) ");
                }
                else
                {
                    // Cálculo (SUM|AVG|MIN|MAX)
                    string calculo = itemGrafica.calculo;

                    // Consulta sparql.
                    select = new StringBuilder();
                    where = new StringBuilder();

                    select.Append(mPrefijos);
                    select.Append($@"SELECT ?ejeX {calculo}(?aux) AS ?numero ");
                    where.Append("WHERE { ");
                    foreach (string item in filtros)
                    {
                        where.Append(item);
                    }
                    where.Append($@"}} ORDER BY {orden}(?ejeX) ");
                }

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        if (filtroEspecial && string.IsNullOrEmpty(itemGrafica.calculo))
                        {
                            listaTuplas.Add(new Tuple<string, string, float>(fila["ejeX"].value, fila["aux"].value, float.Parse(fila["numero"].value.Replace(",", "."), CultureInfo.InvariantCulture)));
                        }
                        else
                        {
                            listaTuplas.Add(new Tuple<string, string, float>(fila["ejeX"].value, string.Empty, float.Parse(fila["numero"].value.Replace(",", "."), CultureInfo.InvariantCulture)));
                        }
                    }
                }
                resultadosDimension[itemGrafica] = listaTuplas;
            });

            #region --- Cálculo de los valores del Eje X
            HashSet<string> valuesEje = new HashSet<string>();

            foreach (KeyValuePair<Dimension, List<Tuple<string, string, float>>> item in resultadosDimension)
            {
                if (item.Value != null && item.Value.Any())
                {
                    foreach (Tuple<string, string, float> item2 in item.Value)
                    {
                        valuesEje.Add(item2.Item1);
                    }                    
                }
            }

            bool isInt = valuesEje.Where(x => !int.TryParse(x, out int aux)).Count() == 0;

            if (pGrafica.configBarras.rellenarEjeX && isInt && valuesEje.Count > 0)
            {
                int numMin = valuesEje.Min(x => int.Parse(x));
                int numMax = valuesEje.Max(x => int.Parse(x));
                for (int i = numMin; i <= numMax; i++)
                {
                    valuesEje.Add(i.ToString());
                }
            }

            foreach (KeyValuePair<Dimension, List<Tuple<string, string, float>>> item in resultadosDimension)
            {
                if (item.Value != null && item.Value.Any())
                {
                    foreach (string valor in valuesEje)
                    {
                        if (!item.Value.Where(x => x.Item1.Equals(valor)).Any())
                        {
                            item.Value.Add(new Tuple<string, string, float>(valor, "", 0));
                        }
                    }
                }

                if (isInt)
                {
                    resultadosDimension[item.Key] = item.Value.OrderBy(x => int.Parse(x.Item1)).ToList();
                }
                else
                {
                    resultadosDimension[item.Key] = item.Value.OrderBy(x => x.Item1).ToList();
                }
            }

            if (isInt)
            {
                valuesEje = new HashSet<string>(valuesEje.OrderBy(item => int.Parse(item)));
            }
            else
            {
                valuesEje = new HashSet<string>(valuesEje.OrderBy(item => item));
            }
            #endregion

            // Obtención del objeto de la gráfica.
            List<string> listaLabels = valuesEje.ToList();

            foreach (KeyValuePair<Dimension, List<Tuple<string, string, float>>> item in resultadosDimension)
            {
                Dataset dataset = new Dataset();
                List<float> listaData = new List<float>();
                foreach(Tuple<string, string, float> itemAux in item.Value)
                {
                    listaData.Add(itemAux.Item3);
                }
                dataset.data = listaData;

                // Nombre del dato en leyenda.
                dataset.label = GetTextLang(pLang, item.Key.nombre);

                // Color.
                dataset.backgroundColor = ObtenerColores(dataset.data.Count(), item.Key.color);
                dataset.type = item.Key.tipoDimension;

                // Anchura.
                dataset.barPercentage = 1;
                if (item.Key.anchura != 0)
                {
                    dataset.barPercentage = item.Key.anchura;
                }

                // Stack.
                if (!string.IsNullOrEmpty(item.Key.stack))
                {
                    dataset.stack = item.Key.stack;
                }
                else
                {
                    dataset.stack = Guid.NewGuid().ToString();
                }

                // Eje Y.
                dataset.yAxisID = item.Key.yAxisID;

                // Eje X.
                dataset.xAxisID = item.Key.xAxisID;

                data.labels = listaLabels;
                data.type = item.Key.tipoDimension;
                dimensionesDataset[item.Key] = dataset;
            }

            foreach (Dimension dim in pGrafica.configBarras.dimensiones)
            {
                grafica.data.datasets.Add(dimensionesDataset[dim]);
            }

            return grafica;
        }

        public static GraficaBase CrearGraficaCircular(Grafica pGrafica, string pFiltroBase, string pFiltroFacetas, string pLang)
        {
            ConcurrentDictionary<Dimension, Dictionary<string, float>> resultadosDimension = new ConcurrentDictionary<Dimension, Dictionary<string, float>>();
            Dictionary<Dimension, Dataset> dimensionesDataset = new Dictionary<Dimension, Dataset>();

            // Invierte las dimensiones para que la grafica de línea salga por encima de la de barras.
            pGrafica.configBarras.dimensiones.Reverse();

            foreach (Dimension dim in pGrafica.configBarras.dimensiones)
            {
                resultadosDimension[dim] = null;
                dimensionesDataset[dim] = null;
            }

            Parallel.ForEach(pGrafica.configBarras.dimensiones, new ParallelOptions { MaxDegreeOfParallelism = 5 }, itemGrafica =>
            {
                SparqlObject resultadoQuery = null;
                StringBuilder select = new StringBuilder(), where = new StringBuilder();

                // Consulta sparql.
                List<string> filtros = new List<string>();
                Dictionary<string, float> dicResultados = new Dictionary<string, float>();
                filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroBase }));
                if (!string.IsNullOrEmpty(pFiltroFacetas))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroFacetas }));
                }
                if (!string.IsNullOrEmpty(itemGrafica.filtro))
                {
                    filtros.AddRange(ObtenerFiltros(new List<string>() { itemGrafica.filtro }));
                }

                select.Append(mPrefijos);
                select.Append($@"SELECT ?tipo COUNT(?s) AS ?numero ");
                where.Append("WHERE { ");
                foreach (string item in filtros)
                {
                    where.Append(item);
                }
                where.Append($@"FILTER(LANG(?tipo) = '{pLang}' OR LANG(?tipo) = '' OR !isLiteral(?tipo)) ");
                where.Append($@"}} ORDER BY DESC (?numero) ");

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {

                    }
                }
            });

            return null;
        }
        #endregion

        #region --- Facetas
        /// <summary>
        /// Lee la configuración y obtiene los datos necesarios para el servicio de facetas.
        /// </summary>
        /// <param name="pIdPagina">Identificador de la página.</param>
        /// <param name="pIdFaceta">Identificador de la faceta.</param>
        /// <param name="pFiltroFacetas">Filtros de la URL.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static Faceta GetFaceta(string pIdPagina, string pIdFaceta, string pFiltroFacetas, string pLang)
        {
            // Decode de los filtros.
            pFiltroFacetas = HttpUtility.UrlDecode(pFiltroFacetas);

            // Lectura del JSON de configuración.
            ConfigModel configModel = TabTemplates.FirstOrDefault(x => x.identificador == pIdPagina);
            if (configModel != null)
            {
                FacetaConf faceta = configModel.facetas.FirstOrDefault(x => x.filtro == pIdFaceta);
                return CrearFaceta(faceta, configModel.filtro, pFiltroFacetas, pLang);
            }

            return null;
        }

        /// <summary>
        /// Crea el objeto faceta.
        /// </summary>
        /// <param name="pFacetaConf">Configuración.</param>
        /// <param name="pFiltroBase">Filtros base.</param>
        /// <param name="pFiltroFacetas">Filtros de las facetas.</param>
        /// <param name="pLang">Idioma.</param>
        /// <returns></returns>
        public static Faceta CrearFaceta(FacetaConf pFacetaConf, string pFiltroBase, string pFiltroFacetas, string pLang)
        {
            Faceta faceta = new Faceta();

            faceta.numeroItemsFaceta = int.MaxValue;
            if (pFacetaConf.numeroItemsFaceta != 0)
            {
                faceta.numeroItemsFaceta = pFacetaConf.numeroItemsFaceta;
            }

            faceta.id = pFacetaConf.filtro;
            faceta.nombre = GetTextLang(pLang, pFacetaConf.nombre);
            faceta.items = new List<ItemFaceta>();

            // Filtro de página.
            List<string> filtros = new List<string>();
            filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroBase }));
            filtros.AddRange(ObtenerFiltros(new List<string>() { pFacetaConf.filtro }, "nombreFaceta"));
            if (!string.IsNullOrEmpty(pFiltroFacetas))
            {
                filtros.AddRange(ObtenerFiltros(new List<string>() { pFiltroFacetas }));
            }
            Dictionary<string, float> dicResultados = new Dictionary<string, float>();
            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append($@"SELECT DISTINCT ?nombreFaceta LANG(?nombreFaceta) AS ?lang COUNT(?s) AS ?numero ");
            where.Append("WHERE { ");
            foreach (string item in filtros)
            {
                where.Append(item);
            }
            where.Append($@"FILTER(LANG(?nombreFaceta) = '{pLang}' OR LANG(?nombreFaceta) = '' OR !isLiteral(?nombreFaceta)) ");
            where.Append($@"}} ORDER BY DESC (?numero) ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), mCommunityID);
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    ItemFaceta itemFaceta = new ItemFaceta();
                    itemFaceta.nombre = fila["nombreFaceta"].value;
                    itemFaceta.numero = Int32.Parse(fila["numero"].value);

                    // Comprobación si tiene idioma asignado.
                    string lang = "";
                    if (!string.IsNullOrEmpty(fila["lang"].value))
                    {
                        lang = $@"@{fila["lang"].value}";
                    }

                    // Comprobación si es literal o numerico.
                    string filtro = itemFaceta.nombre;
                    if (fila["nombreFaceta"].type == "literal")
                    {
                        filtro = $@"'{filtro}'";
                    }

                    itemFaceta.filtro = $@"{pFacetaConf.filtro}={filtro}{lang}";
                    faceta.items.Add(itemFaceta);
                }
            }

            // Ordenación.
            //faceta.items = faceta.items.OrderBy(o => o.nombre).ToList();

            return faceta;
        }
        #endregion

        #region --- Utils
        /// <summary>
        /// Obtiene la lista de configuraciones.
        /// </summary>
        public static List<ConfigModel> TabTemplates
        {
            get
            {
                if (mTabTemplates == null || mTabTemplates.Count != Directory.EnumerateFiles($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configGraficas").Count())
                {
                    mTabTemplates = new List<ConfigModel>();
                    foreach (string file in Directory.EnumerateFiles($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/configGraficas"))
                    {
                        ConfigModel tab = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(file));
                        mTabTemplates.Add(tab);
                    }
                }
                return mTabTemplates;
            }
        }

        /// <summary>
        /// Crea la lista de colores para rellenar las gráficas.
        /// </summary>
        /// <param name="pNumVeces">Número de la lista.</param>
        /// <param name="pColorHex">Color a rellenar.</param>
        /// <returns></returns>
        public static List<string> ObtenerColores(int pNumVeces, string pColorHex)
        {
            List<string> colores = new List<string>();
            for (int i = 0; i < pNumVeces; i++)
            {
                colores.Add(pColorHex);
            }
            return colores;
        }

        /// <summary>
        /// Splitea los filtros para tratarlos.
        /// </summary>
        /// <param name="pListaFiltros">Listado de filtros.</param>
        /// <param name="pNombreVar">Nombre a poner a la última variable.</param>
        /// <returns></returns>
        public static List<string> ObtenerFiltros(List<string> pListaFiltros, string pNombreVar = null)
        {
            // Split por filtro.
            List<string> listaAux = new List<string>();
            foreach (string filtro in pListaFiltros)
            {
                string[] array = filtro.Split("&", StringSplitOptions.RemoveEmptyEntries);
                listaAux.AddRange(array.ToList());
            }

            List<string> filtrosQuery = new List<string>();

            // Split por salto de ontología.
            int i = 0;
            foreach (string item in listaAux)
            {
                filtrosQuery.Add(TratarParametros(item, "?s", i, pNombreVar));
                i += 10;
            }

            return filtrosQuery;
        }

        /// <summary>
        /// Según el tipo de parametros, los trata de una manera u otra para el filtro.
        /// </summary>
        /// <param name="pFiltro">Filtro a tratar.</param>
        /// <param name="pVarAnterior">Sujeto.</param>
        /// <param name="pAux">Iterador incremental.</param>
        /// <param name="pNombreVar">Nombre de la última variable.</param>
        /// <returns></returns>
        public static string TratarParametros(string pFiltro, string pVarAnterior, int pAux, string pNombreVar = null)
        {
            StringBuilder filtro = new StringBuilder();
            string[] filtros = pFiltro.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (string parteFiltro in filtros)
            {
                i++;
                if (!parteFiltro.Contains("="))
                {
                    string varActual = $@"?{parteFiltro.Substring(parteFiltro.IndexOf(":") + 1)}{pAux}";
                    filtro.Append($@"{pVarAnterior} ");
                    filtro.Append($@"{parteFiltro} ");
                    // Si es el último, le asignamos el nombre que queramos.
                    if (i == filtros.Length && !string.IsNullOrEmpty(pNombreVar))
                    {
                        filtro.Append($@"?{pNombreVar}. ");
                    }
                    else
                    {
                        filtro.Append($@"{varActual}. ");
                    }
                    pVarAnterior = varActual;
                    pAux++;
                }
                else
                {
                    string varActual = $@"{parteFiltro.Split("=")[1]}";
                    if (varActual.StartsWith("'"))
                    {
                        filtro.Append($@"{pVarAnterior} ");
                        filtro.Append($@"{parteFiltro.Split("=")[0]} ");
                        filtro.Append($@"{varActual}. ");
                    }
                    else
                    {
                        // Si el valor es númerico, se le asigna con el FILTER.
                        string varActualAux = $@"?{parteFiltro.Split("=")[0].Substring(parteFiltro.IndexOf(":") + 1)}{pAux}";

                        filtro.Append($@"{pVarAnterior} ");
                        filtro.Append($@"{parteFiltro.Split("=")[0]} ");
                        filtro.Append($@"{varActualAux}. ");
                        filtro.Append($@"FILTER({varActualAux} = {varActual}) ");
                    }

                }
            }
            return filtro.ToString();
        }

        /// <summary>
        /// Obtiene el idioma de del diccionario de idiomas.
        /// </summary>
        /// <param name="pLang">Idioma.</param>
        /// <param name="pValores">Diccionario.</param>
        /// <returns></returns>
        public static string GetTextLang(string pLang, Dictionary<string, string> pValores)
        {
            if (pValores == null)
            {
                return "";
            }
            else if (pValores.ContainsKey(pLang))
            {
                return pValores[pLang];
            }
            else if (pValores.ContainsKey("es"))
            {
                return pValores["es"];
            }
            else if (pValores.Count > 0)
            {
                return pValores.Values.First();
            }
            else
            {
                return "";
            }
        }
        #endregion
    }
}
