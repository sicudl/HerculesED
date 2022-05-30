﻿using EditorCV.Models.API;
using EditorCV.Models.API.Input;
using EditorCV.Models.Utils;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace EditorCV.Models
{
    public class AccionesExportacion
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        /// <summary>
        /// Añade el archivo enviado como array de bytes.
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="nombreCV"></param>
        /// <param name="pCVID"></param>
        /// <param name="lang"></param>
        /// <param name="listaId"></param>
        public static void AddFile(ConfigService _Configuracion, string pCVID,string nombreCV, string lang, List<string> listaId)
        {
            Guid guidCortoCVID = mResourceApi.GetShortGuid(pCVID);

            //Añado GeneratedPDFFile sin el link al archivo
            string filePredicateTitle = "http://w3id.org/roh/generatedPDFFile|http://w3id.org/roh/title";
            string filePredicateFecha = "http://w3id.org/roh/generatedPDFFile|http://purl.org/dc/terms/issued";
            string filePredicateEstado = "http://w3id.org/roh/generatedPDFFile|http://w3id.org/roh/status";

            string idEntityAux = $"{mResourceApi.GraphsUrl}items/GeneratedPDFFile_" + guidCortoCVID.ToString() + "_" + Guid.NewGuid();

            string PDFFilePDF = "CV_filePDF" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".pdf";
            string PDFFileFecha = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            string PDFFileEstado = "pendiente";

            List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();
            TriplesToInclude trTitle = new TriplesToInclude(idEntityAux + "|" + nombreCV, filePredicateTitle);
            listaTriples.Add(trTitle);
            TriplesToInclude trFecha = new TriplesToInclude(idEntityAux + "|" + PDFFileFecha, filePredicateFecha);
            listaTriples.Add(trFecha);
            TriplesToInclude trEstado = new TriplesToInclude(idEntityAux + "|" + PDFFileEstado, filePredicateEstado);
            listaTriples.Add(trEstado);

            var inserted = mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { guidCortoCVID, listaTriples } });

            Thread thread = new Thread(() => AddPDFFile(_Configuracion, pCVID, lang, listaId, idEntityAux, PDFFilePDF, guidCortoCVID, filePredicateEstado));
            thread.Start();
        }

        /// <summary>
        /// Adjunto el fichero y modifico los triples de <paramref name="idEntityAux"/> para referenciar el archivo y 
        /// modificar el estado a "procesado". En caso de error durante el proceso cambio el estado a "error".
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="pCVID">Identificador del CV</param>
        /// <param name="lang">Lenguaje del CV</param>
        /// <param name="listaId">listado de identificadores</param>
        /// <param name="idEntityAux">Identificador de la entidad auxiliar a modificar</param>
        /// <param name="PDFFilePDF">nombre del fichero</param>
        /// <param name="guidCortoCVID">GUID corto del CV</param>
        /// <param name="filePredicateEstado">Predicado estado de la entidad</param>
        static void AddPDFFile(ConfigService _Configuracion, string pCVID, string lang, List<string> listaId,
            string idEntityAux, string PDFFilePDF, Guid guidCortoCVID, string filePredicateEstado)
        {
            try
            {
                //Petición al exportador
                List<KeyValuePair<string, string>> parametros = new List<KeyValuePair<string, string>>();
                parametros.Add(new KeyValuePair<string, string>("pCVID", pCVID));
                parametros.Add(new KeyValuePair<string, string>("lang", lang));
                foreach (string id in listaId)
                {
                    parametros.Add(new KeyValuePair<string, string>("listaId", id));
                }
                FormUrlEncodedContent formContent = new FormUrlEncodedContent(parametros);

                //Petición al exportador para conseguir el archivo PDF
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(1, 15, 0);
                string urlExportador = _Configuracion.GetUrlExportador();
                HttpResponseMessage response = client.PostAsync($"{urlExportador}", formContent).Result;
                response.EnsureSuccessStatusCode();
                byte[] result = response.Content.ReadAsByteArrayAsync().Result;

                //Inserto el archivo
                string filePredicate = "http://w3id.org/roh/generatedPDFFile|http://w3id.org/roh/filePDF";

                string fileName = idEntityAux + "|" + PDFFilePDF;
                List<byte[]> attachedFile = new List<byte[]>();
                attachedFile.Add(result);

                //Añado el fichero en virtuoso
                mResourceApi.AttachFileToResource(guidCortoCVID, filePredicate, fileName,
                    new List<string>() { PDFFilePDF }, new List<short>() { 0 }, attachedFile);

                //Cambio el estado a "procesado"
                string PDFFileEstado = "procesado";
                Dictionary<Guid, List<TriplesToModify>> triplesModificar = new Dictionary<Guid, List<TriplesToModify>>();
                triplesModificar[mResourceApi.GetShortGuid(pCVID)] = new List<TriplesToModify>()
                {
                    new TriplesToModify(idEntityAux + "|" + PDFFileEstado, idEntityAux + "|pendiente", filePredicateEstado)
                };
                mResourceApi.ModifyPropertiesLoadedResources(triplesModificar);

            }
            catch (Exception e)
            {
                //Cambio el estado a "error"
                string PDFFileEstado = "error";
                Dictionary<Guid, List<TriplesToModify>> triplesModificar = new Dictionary<Guid, List<TriplesToModify>>();
                triplesModificar[mResourceApi.GetShortGuid(pCVID)] = new List<TriplesToModify>()
                {
                    new TriplesToModify(idEntityAux + "|" + PDFFileEstado, idEntityAux + "|pendiente", filePredicateEstado)
                };
                mResourceApi.ModifyPropertiesLoadedResources(triplesModificar);

                throw new Exception(e.Message);
            }
        }

        public static List<Tuple<string, string, string>> GetListPDFFile(string pCVId)
        {
            List<Tuple<string,string,string>> listadoArchivos = new List<Tuple<string, string, string>>();
            string select = "SELECT *";
            string where = $@"WHERE{{
    <{pCVId}> <http://w3id.org/roh/generatedPDFFile> ?pdfFile .
    ?pdfFile ?p ?o .
}}";

            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (!fila.ContainsKey("p") || !fila.ContainsKey("o"))
                {
                    continue;
                }
                string s = fila["s"].value;
                string p = fila["p"].value;
                string o = fila["o"].value;

                if (p != "http://www.w3.org/2000/01/rdf-schema#label" && p != "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")
                {
                    listadoArchivos.Add(new Tuple<string, string, string>(fila["s"].value, fila["p"].value, fila["o"].value));
                }                 
            }

            return listadoArchivos;
        }

        /// <summary>
        /// Devuelve todas las pestañas del CV de <paramref name="pCVId"/>
        /// </summary>
        /// <param name="pCVId"></param>
        /// <returns></returns>
        public static ConcurrentDictionary<string, string> GetAllTabs(string pCVId)
        {
            ConcurrentDictionary<string, string> dicIds = new ConcurrentDictionary<string, string>();
            string select = "SELECT *";
            string where = $@"WHERE{{
    <{pCVId}> ?p ?o .
}}";

            SparqlObject resultData = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
            foreach (Dictionary<string, Data> fila in resultData.results.bindings)
            {
                if (!fila.ContainsKey("p") || !fila.ContainsKey("o"))
                {
                    continue;
                }
                if (!IsValidTab(fila["p"].value))
                {
                    continue;
                }

                string property = fila["p"].value.Split("/").Last();
                string uri = fila["p"].value.Split(property).First();
                dicIds.TryAdd(FirstLetterUpper(uri, property), fila["o"].value);
            }

            return dicIds;
        }

        /// <summary>
        /// Devuelve true si <paramref name="tab"/> se encuentra en:
        /// "http://w3id.org/roh/personalData",
        /// "http://w3id.org/roh/scientificExperience",
        /// "http://w3id.org/roh/scientificActivity",
        /// "http://w3id.org/roh/teachingExperience",
        /// "http://w3id.org/roh/qualifications",
        /// "http://w3id.org/roh/professionalSituation",
        /// "http://w3id.org/roh/freeTextSummary"
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        private static bool IsValidTab(string tab)
        {
            List<string> validTabs = new List<string>()
            {
                "http://w3id.org/roh/personalData",
                "http://w3id.org/roh/scientificExperience",
                "http://w3id.org/roh/scientificActivity",
                "http://w3id.org/roh/teachingExperience",
                "http://w3id.org/roh/qualifications",
                "http://w3id.org/roh/professionalSituation",
                "http://w3id.org/roh/freeTextSummary"
            };
            return validTabs.Contains(tab);
        }

        /// <summary>
        /// Cambia la 1º letra de <paramref name="property"/> a mayuscula y la concatena con <paramref name="uri"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private static string FirstLetterUpper(string uri, string property)
        {
            if (property.Length == 0 || property.Length == 1)
            {
                return "";
            }
            string upper = property.Substring(0, 1).ToUpper();
            string substring = property.Substring(1, property.Length - 1);
            return uri + upper + substring;
        }
    }
}
