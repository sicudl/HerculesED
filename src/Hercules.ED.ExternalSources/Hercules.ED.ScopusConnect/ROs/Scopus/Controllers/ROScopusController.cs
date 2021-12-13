using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ScopusConnect.ROs.Scopus.Models;
using System;

namespace ScopusConnect.ROs.Scopus.Controllers
{
      public class ROScopusController : ROScopusLogic
    {
        public ROScopusController(string baseUri, string bareer, Dictionary<string, Tuple<string, string, string, string, string, string>> autores_orcid) : base(baseUri, bareer,autores_orcid)
        {
            // baseUri = "https://api.elsevier.com/";
            // _bareer = new Guid("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");
            this.baseUri = baseUri;
            this.bareer = bareer;
            this.autores_orcid=autores_orcid;
            //headers.Add("view","COMPLETE");
            //headers.Add("User-Agent", "http://developer.github.com/v3/#user-agent-required");
            //headers.Add("Accept", "application/vnd.github.mercy-preview+json");
        }
    
    }
     
}
