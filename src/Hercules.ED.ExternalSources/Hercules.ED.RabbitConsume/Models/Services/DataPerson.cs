﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hercules.ED.RabbitConsume.Models.Services
{
    public class DataPerson
    {
        // Prefijos.
        private static readonly string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}configJson{Path.DirectorySeparatorChar}prefijos.json")));
        private static readonly ResourceApi mResourceApi = new($@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config{Path.DirectorySeparatorChar}ConfigOAuth{Path.DirectorySeparatorChar}OAuthV3.config");

        public static void ModifyDate(string pIdGnoss, DateTime pDate)
        {
            // Obtención de datos antiguos.
            string fechaAntigua = string.Empty;
            string idRecurso = string.Empty;

            SparqlObject resultadoQuery;
            StringBuilder select = new(), where = new();

            // Consulta sparql.
            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?s ?fecha ");
            where.Append("WHERE { ");
            where.Append($@"FILTER(?s = <{pIdGnoss}>)");
            where.Append("OPTIONAL {?s roh:lastUpdatedDate ?fecha. } ");
            where.Append("} ");
            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("fecha"))
                    {
                        fechaAntigua = fila["fecha"].value;
                    }

                    idRecurso = fila["s"].value;
                }
            }

            // Conversión de fecha.
            string fechaFinal = $@"{pDate.ToString("yyyy/MM/dd").Replace("/", "")}000000";

            // Inserción/Modificación de triples.
            mResourceApi.ChangeOntoly("person");
            Guid guid = mResourceApi.GetShortGuid(idRecurso);

            if (!string.IsNullOrEmpty(fechaAntigua))
            {
                // Modificación.
                Dictionary<Guid, List<TriplesToModify>> dicModificacion = new();
                List<TriplesToModify> listaTriplesModificacion = new();

                // Modificación (Triples).
                TriplesToModify triple = new();
                triple.Predicate = $@"http://w3id.org/roh/lastUpdatedDate";
                triple.NewValue = fechaFinal;
                triple.OldValue = fechaAntigua;
                listaTriplesModificacion.Add(triple);

                dicModificacion.Add(guid, listaTriplesModificacion);
                mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
            }
            else
            {
                // Inserción.
                Dictionary<Guid, List<TriplesToInclude>> dicInsercion = new();
                List<TriplesToInclude> listaTriplesInsercion = new();

                // Inserción (Triples).                 
                TriplesToInclude triple = new();
                triple.Predicate = $@"http://w3id.org/roh/lastUpdatedDate";
                triple.NewValue = fechaFinal;
                listaTriplesInsercion.Add(triple);

                dicInsercion.Add(guid, listaTriplesInsercion);
                mResourceApi.InsertPropertiesLoadedResources(dicInsercion);
            }
        }
    }
}
