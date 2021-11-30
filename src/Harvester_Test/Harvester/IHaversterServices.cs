﻿using Harvester.Models;
using Newtonsoft.Json;
using OAI_PMH.Models.SGI.Organization;
using OAI_PMH.Models.SGI.PersonalData;
using OAI_PMH.Models.SGI.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Harvester
{
    public interface IHaversterServices
    {
        public List<IdentifierOAIPMH> ListIdentifiers(string from, string until = null, string set = null);
        public string GetRecord(string id,string file=null);

    }

    public class IHarvesterServices : IHaversterServices
    {
        public List<IdentifierOAIPMH> ListIdentifiers(string from, string until = null, string set = null)
        {

            //List<IdentifierOAIPMH> personIdList = new();
            List<IdentifierOAIPMH> idList = new();
            string uri = "https://localhost:44300/OAI_PMH?verb=ListIdentifiers&metadataPrefix=EDMA";
            if (from != null)
            {
                uri += $"&from={from}";
            }
            if (until != null)
            {
                uri += $"&until={until}";
            }
            if (set != null)
            {
                uri += $"&set={set}";
            }

            WebRequest wrGETURL = WebRequest.Create(uri);
            Stream stream = wrGETURL.GetResponse().GetResponseStream();

            XDocument XMLresponse = XDocument.Load(stream);
            XNamespace nameSpace = XMLresponse.Root.GetDefaultNamespace();
            XElement idListElement = XMLresponse.Root.Element(nameSpace + "ListIdentifiers");

            if (idListElement != null)
            {
                IEnumerable<XElement> headerList = idListElement.Descendants(nameSpace + "header");

                foreach (var header in headerList)
                {
                    header.Attribute(nameSpace + "status");
                    string identifier = header.Element(nameSpace + "identifier").Value;
                    string date = header.Element(nameSpace + "datestamp").Value;
                    string setSpec = header.Element(nameSpace + "setSpec").Value;
                    IdentifierOAIPMH identifierOAIPMH = new()
                    {
                        Date = DateTime.Parse(date),
                        Identifier = identifier,
                        Set = setSpec,
                        Deleted = false
                    };
                    idList.Add(identifierOAIPMH);
                }
            }
            return idList;
        }
        public string GetRecord(string id,String file=null)
        {
            string uri = "https://localhost:44300/OAI_PMH?verb=GetRecord&identifier=" + id + "&metadataPrefix=EDMA";

            WebRequest wrGETURL = WebRequest.Create(uri);
            Stream stream = wrGETURL.GetResponse().GetResponseStream();
            XDocument XMLresponse = XDocument.Load(stream);
            XNamespace nameSpace = XMLresponse.Root.GetDefaultNamespace();
            string record = XMLresponse.Root.Element(nameSpace + "GetRecord").Descendants(nameSpace + "metadata").First().FirstNode.ToString();
            record = record.Replace("xmlns=\"" + nameSpace + "\"", "");
            return record;
        }
    }

}

