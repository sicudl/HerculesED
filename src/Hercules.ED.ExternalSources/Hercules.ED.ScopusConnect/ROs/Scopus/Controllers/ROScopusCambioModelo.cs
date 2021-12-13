using System.Collections.Generic;
using ScopusConnect.ROs.Scopus.Models;
using ScopusConnect.ROs.Scopus.Models.Inicial;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;

using Newtonsoft.Json;


namespace ScopusConnect.ROs.Scopus.Controllers
{
    public class ROScopusControllerJSON //: //ROScopusLogic
    {
        public List<string> advertencia = null;
        public ROScopusLogic ScopusLogic;
        public ROScopusControllerJSON(ROScopusLogic ScopusLogic)
        {
            this.ScopusLogic = ScopusLogic;

        }

        public List<Publication> getListPublicatio(Root objInicial, string date)
        {
            List<Publication> sol = new List<Publication>();
            if (objInicial != null)
            {
                if (objInicial.SearchResults != null)
                {
                    if (objInicial.SearchResults.entry != null)
                    {
            
                        foreach (PublicacionInicial rec in objInicial.SearchResults.entry)
                        {
                            if (DateTime.Parse(rec.PrismCoverDate) > DateTime.Parse(date))
                            {
                                
                                Publication publicacion = cambioDeModeloPublicacion(rec, true);
                                if(publicacion!=null){
                                     if(this.advertencia !=null){
                                            publicacion.problema = this.advertencia;
                                            this.advertencia=null;
                                        }
                                sol.Add(publicacion);
                                }
                                
                            }
                        }
                    }
                }
            }

            return sol;
        }



        public Publication cambioDeModeloPublicacion(PublicacionInicial objInicial, Boolean publicacion_principal)
        {
            Publication publicacion = new Publication();

            if (objInicial != null)
            {
                publicacion.typeOfPublication = getType(objInicial);
                if (publicacion.typeOfPublication != null)
                {
                    publicacion.IDs = getIDs(objInicial);
                    publicacion.title = getTitle(objInicial);
                    //publicacion.Abstract = getAbstract(objInicial);
                    //publicacion.language = getLanguage(objInicial);
                    publicacion.doi = getDoi(objInicial);
                    publicacion.url = getLinks(objInicial);
                    publicacion.dataIssued = getDate(objInicial);
                    publicacion.pageStart = getPageStart(objInicial);
                    publicacion.pageEnd = getPageEnd(objInicial);
                    ///publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
                    //publicacion.freetextKeyword = getFreetextKeyword(objInicial);
                    publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
                    //publicacion.seqOfAuthors = getAuthors(objInicial);

                    publicacion.hasPublicationVenue = getJournal(objInicial);
                    publicacion.hasMetric = getPublicationMetric(objInicial);
                    return publicacion;
                }
                else { return null; }
            }
            else
            {

                return null;
            }

        }


        public string getType(PublicacionInicial objInicial)
        {

            if (objInicial.subtypeDescription != null)
            {
                string type = objInicial.subtypeDescription;
                if (type == "Article")
                {
                    return "Journal Article";
                }
                else if (type == "Book")
                {
                    return "Book";
                }
                else if (type == "Book Chapter")
                {
                    return "Chapter";
                }
                else if (type == "Conference Paper")
                {
                    return "Conference Paper";
                }
                else { return null; }
            }
            else { return null; }



        }
        public List<string> getIDs(PublicacionInicial objInicial)
        {
            List<string> ids = new List<string>();
            if (objInicial.DcIdentifier != null)
            {
                ids.Add(objInicial.DcIdentifier);
            }
            return ids;
        }

        public string getTitle(PublicacionInicial objInicial)
        {
            if (objInicial.DcTitle != null)
            {
                return objInicial.DcTitle;
            }
            return null;
        }

        // public string getAbstract(PublicacionInicial objInicial)
        // {
        //     return null;
        // }

        // public string getLanguage(PublicacionInicial objInicial)
        // {
        //     return null;
        // }
        public string getDoi(PublicacionInicial objInicial)
        {
            if (objInicial.PrismDoi != null)
            {
                if(objInicial.PrismDoi.Contains("https://doi.org/")){
                     
                                        
                                            int indice = objInicial.PrismDoi.IndexOf("org/");
                                            return objInicial.PrismDoi.Substring(indice + 4);

                                        
                }else{
                return objInicial.PrismDoi;
                }
            }
            return null;
        }
        public List<string> getLinks(PublicacionInicial objInicial)
        {
            List<string> links = new List<string>();
            if (objInicial.link != null)
            {
                foreach (Link link in objInicial.link)
                {
                    if (link.Ref == "scopus")
                    {
                        links.Add(link.Href);
                    }
                }
            }
            return links;
        }

        public DateTimeValue getDate(PublicacionInicial objInicial)
        {
            DateTimeValue date = new DateTimeValue();

            date.datimeTime = null;
            if (objInicial.PrismCoverDate != null)
            {
                date.datimeTime = objInicial.PrismCoverDate;
            }
            return date;
        }

        public string getPageStart(PublicacionInicial objInicial)
        {
            if (objInicial.PrismPageRange != null)
            {
                if (objInicial.PrismCoverDate.Contains("-"))
                {
                    string[] paginas = objInicial.PrismCoverDate.Split("-");
                    return paginas[0];
                }
            }
            return null;
        }

        public string getPageEnd(PublicacionInicial objInicial)
        {
            if (objInicial.PrismPageRange != null)
            {
                if (objInicial.PrismCoverDate.Contains("-"))
                {
                    string[] paginas = objInicial.PrismCoverDate.Split("-");
                    return paginas[1];
                }
            }
            return null;
        }

        // public List<KnowledgeArea> getKnowledgeAreas(PublicacionInicial objInicial)
        // {
        //     List<KnowledgeArea> result = new List<KnowledgeArea>();
        //     KnowledgeArea area = null;
        //     result.Add(area);
        //     return result;
        // }

        // public List<string> getFreetextKeyword(PublicacionInicial objInicial)
        // {
        //     return new List<string>();
        // }

        public Person getAuthorPrincipal(PublicacionInicial objInicial)
        {
            Person autor = new Person();
             int i = this.ScopusLogic.autores_orcid.Count;
                    string orcid = null;
                    string name = null;
                    string familia = null;
                    string completo = null;
                    string idss = null; ;
                    string links = null;
            if (objInicial.DcCreator != null)
            {
                List<string> names = new List<string>();
                names.Add(objInicial.DcCreator);
                Name nombre =new Name();
                nombre.nombre_completo=names;
                autor.name = nombre;
                completo=objInicial.DcCreator;
                  autor.id_persona=i.ToString();
                                        if(orcid!=null || name!=null ||familia!=null ||completo!=null || idss!=null || links!=null){
                                        Tuple<string,string, string, string, string, string> tupla = new Tuple<string,string, string, string, string, string>(orcid,name,familia,completo,idss,links);
                                        
                                        this.ScopusLogic.autores_orcid[i.ToString()]=tupla;}
                                       
                return autor;
            }
            return null;
        }
        // public List<Person> getAuthors(PublicacionInicial objInicial)
        // {
        //     return new List<Person>();
        // }

        public Source getJournal(PublicacionInicial objInicial)
        {
            if (objInicial.PrismPublicationName != null || objInicial.PrismIssn != null)
            {
                Source revista = new Source();
                if (objInicial.PrismPublicationName != null)
                {
                    revista.name = objInicial.PrismPublicationName;
                }
                if (objInicial.PrismIssn != null)
                {
                    List<string> issn = new List<string>();
                    issn.Add(objInicial.PrismIssn);
                    revista.issn = issn;
                }
                if(objInicial.PrismAggregationType!=null){
                    if(objInicial.PrismAggregationType=="Book"){
                        revista.type="Book";
                    }
                    else if(objInicial.PrismAggregationType=="Journal"){
                        revista.type="Journal";
                    }else{
                        //error de no identificar el type en el que esta publicado la publicacion! 
                        string ad= "No se ha identificado el tipo de recurso en el que esta publicado";
                        if(this.advertencia==null){
                            List<string> ads = new List<string>();
                            ads.Add(ad);
                            this.advertencia=ads;
                        }else{this.advertencia.Add(ad);}
                    }
                
                }
                if(objInicial.PrismIsbn!=null){
                    List<string> isbn_list=new List<string>();
                    foreach(PrismIsbn isbn in objInicial.PrismIsbn){
                        isbn_list.Add(isbn.id);
                    }
                    revista.isbn= isbn_list;
                }
                return revista;
            }
            return null;
        }

        public List<PublicationMetric> getPublicationMetric(PublicacionInicial objInicial)
        {
            List<PublicationMetric> metricList = new List<PublicationMetric>();
            PublicationMetric metricPublicacion = new PublicationMetric();
            if (objInicial.CitedbyCount != null)
            {
                metricPublicacion.citationCount = objInicial.CitedbyCount;
                metricPublicacion.metricName = "Scopus";
                metricList.Add(metricPublicacion);
                return metricList;
            }

            return null;
        }

    }
}