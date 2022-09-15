﻿using Newtonsoft.Json;
using OAI_PMH.Controllers;
using OAI_PMH.Models.SGI.GruposInvestigacion;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OAI_PMH.Services
{
    public class InvestigationGroup
    {
        /// <summary>
        /// Obtiene los IDs de los grupos modificads.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="pConfig"></param>
        /// <returns></returns>
        public static Dictionary<string, DateTime> GetModifiedGrupos(string from, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/modificados-ids?q=fechaModificacion=ge=\"" + from + "\""); // TODO: Revisar url petición.
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!string.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    string idMod = "Grupo_" + id.Replace("\"", "");
                    if (!idDictionary.ContainsKey(idMod))
                    {
                        idDictionary.Add(idMod, DateTime.UtcNow);
                    }
                }
            }
            return idDictionary;
        }

        /// <summary>
        /// Obtiene la información de un grupo mediante un ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pConfig"></param>
        /// <returns></returns>
        public static Grupo GetGrupos(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            string identifier = id.Split("_")[1];
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            Grupo grupo = new Grupo();
            try
            {
                grupo = JsonConvert.DeserializeObject<Grupo>(response.Content);
                grupo.equipo = GetGrupoEquipo(identifier, pConfig);
                grupo.investigadoresPrincipales = GetInvestigadoresPrincipales(identifier, pConfig);
                grupo.investigadoresPrincipalesMaxParticipacion = GetInvestigadoresPrincipalesMax(identifier, pConfig);
                grupo.palabrasClave = GetPalabrasClave(identifier, pConfig);
                grupo.lineasClasificacion = GetLineasClasificacion(identifier, pConfig);
                if (grupo.lineasClasificacion != null && grupo.lineasClasificacion.Any())
                {
                    grupo.lineasInvestigacion = new List<LineaInvestigacion>();

                    foreach (LineaClasificacion linea in grupo.lineasClasificacion)
                    {
                        grupo.lineasInvestigacion.AddRange(GetLineasInvestigacion(identifier, pConfig));
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }

            return grupo;
        }

        private static List<LineaClasificacion> GetLineasClasificacion(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<LineaClasificacion> lineas = new();
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "gruposlineasinvestigacion/" + id + "/clasificaciones");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            lineas = JsonConvert.DeserializeObject<List<LineaClasificacion>>(response.Content);
            return lineas;
        }

        private static List<LineaInvestigacion> GetLineasInvestigacion(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<LineaInvestigacion> lineas = new();
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/" + id + "/lineasinvestigacion");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            lineas = JsonConvert.DeserializeObject<List<LineaInvestigacion>>(response.Content);
            return lineas;
        }

        private static List<GrupoPalabraClave> GetPalabrasClave(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<GrupoPalabraClave> palabras = new();
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/" + id + "/palabrasclave");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            palabras = JsonConvert.DeserializeObject<List<GrupoPalabraClave>>(response.Content);
            return palabras;
        }

        private static List<string> GetInvestigadoresPrincipalesMax(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<string> investigadores = new();
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/" + id + "/investigadoresprincipalesmaxparticipacion");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            investigadores = JsonConvert.DeserializeObject<List<string>>(response.Content);
            return investigadores;
        }

        private static List<string> GetInvestigadoresPrincipales(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<string> investigadores = new();
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/" + id + "/investigadoresprincipales");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            investigadores = JsonConvert.DeserializeObject<List<string>>(response.Content);
            return investigadores;
        }

        private static List<GrupoEquipo> GetGrupoEquipo(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<GrupoEquipo> grupoEquipo = new();
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/" + id + "/miembrosequipo"); // TODO: Revisar url petición.
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            grupoEquipo = JsonConvert.DeserializeObject<List<GrupoEquipo>>(response.Content);
            return grupoEquipo;
        }

        private static List<string> GetGrupoPalabrasClave(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<string> palabras = new();
            RestClient client = new(pConfig.GetUrlBaseGrupos() + "grupos/" + id + "/palabrasclave"); // TODO: Revisar url petición.
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            palabras = JsonConvert.DeserializeObject<List<string>>(response.Content);
            return palabras;
        }
    }
}
