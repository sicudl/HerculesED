using System.Collections.Generic;
using ScopusConnect.ROs.Scopus.Models;
using ScopusConnect.ROs.Scopus.Models.Inicial;

using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;


namespace ScopusConnect.ROs.Scopus.Controllers
{
    public class ROScopusControllerJSON //: //ROScopusLogic
    {
        public ROScopusLogic scopusLogic;
        public ROScopusControllerJSON(ROScopusLogic scopusLogic)
        {
            this.scopusLogic = scopusLogic;

        }

        public List<Publication> getListPublicatio(string stringInicial)
        {
            //Console.Write(stringInicial);
            Root objInicial = JsonConvert.DeserializeObject<Root>(stringInicial);
            //Console.Write(objInicial);
            List<Publication> sol = new List<Publication>();

            //---modificacion en otro repo!  ---------------------------------------------------------
            List<Entry> lista_item = objInicial.SearchResults.entry;
            for (int i = 0; i < lista_item.Count; i++)
            {
                Publication a = new Publication();
                Entry entidad = lista_item[i];

                string[] id_code = entidad.DcIdentifier.Split(':');
                string id = id_code[1];
                Publication_root info_publicacion_root = getPublication(this.scopusLogic.getStringPublication(id));
                
                //------------------------------------------------------------------------
                //Console.Write(id);
                if (info_publicacion_root != null)
                {
                    if (entidad.subtype == "cp")
                    {
                        ConferencePaper conferencePaper = getConferencePaper(info_publicacion_root);
                        conferencePaper.bibliografia = getBibliografia(info_publicacion_root);
                        sol.Add(conferencePaper);
                    }
                    else if (entidad.subtype == "ar")
                    {
                        JournalArticle conferencePaper = getJournalArticle(info_publicacion_root);
                        conferencePaper.bibliografia = getBibliografia(info_publicacion_root);
                        sol.Add(conferencePaper);
                    }
                    else
                    {
                        Publication publicacion = getGenericPublication(info_publicacion_root);
                        publicacion.bibliografia = getBibliografia(info_publicacion_root);
                        sol.Add(publicacion);
                    }
                }
            }
            return sol;
        }

        public List<Publication> getBibliografia(Publication_root objInicial)
        {
            if(objInicial.AbstractsRetrievalResponse!=null){
            if (objInicial.AbstractsRetrievalResponse.item.bibrecord.tail != null)
            {
                //Console.Write("Punto inicial");
                List<Publication> bibliografia = new List<Publication>();
                if (objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference != null)
                {
                    //try{
                    for (int j = 0; j < objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference.Count; j++)
                    {
                        //    string scopus_id = null;
                        //Console.Write(info_publicacion_root.item.bibrecord.tail.bibliography.reference[j].RefInfo.RefdItemidlist.itemid.Idtype);
                        if (objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference[j].RefInfo.RefdItemidlist != null)
                        {
                            //Console.Write("hola!");
                            string scopus_id = null;
                            try{
                                //Console.Write(objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference[j].RefInfo.RefdItemidlist.itemid.ToString());
                                Itemid hey = JsonConvert.DeserializeObject<Itemid>(objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference[j].RefInfo.RefdItemidlist.itemid.ToString());
                                scopus_id = hey.a;
                                //Console.Write(scopus_id);
                                //Console.Write("------");
                            } catch{
                                //Console.Write(objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference[j].RefInfo.RefdItemidlist.itemid.GetType());
                                JArray hey = JsonConvert.DeserializeObject<JArray>(objInicial.AbstractsRetrievalResponse.item.bibrecord.tail.bibliography.reference[j].RefInfo.RefdItemidlist.itemid.ToString());
                                foreach(JContainer var in hey){
                                        Itemid ee = JsonConvert.DeserializeObject<Itemid>(var.ToString());
                                        if (ee.Idtype == "SGR" ^ee.Idtype=="SCOPUS"){
                                        scopus_id = ee.a;}
                                }
                            }
                            //Console.Write(scopus_id);
                            //Console.Write("------");
                            Publication_root obj_inicial = null;
                            if (scopus_id != null)
                            {
                                string publicacion_ref = this.scopusLogic.getStringPublication(scopus_id);
                                obj_inicial = getPublication(publicacion_ref);
                            }
                            if (obj_inicial != null)
                            {
                                Publication publication_def = getGenericPublication(obj_inicial);
                                bibliografia.Add(publication_def);
                            }
                        }
                    }
                    return bibliografia;
                }
                else { return null; }
            }
            else { return null; }
            }else{return null;}
        }

        public Publication_root getPublication(string stringPublication)
        {


            Publication_root info_publicacion = new Publication_root();
            try 
            {
                info_publicacion = JsonConvert.DeserializeObject<Publication_root>(stringPublication);
            }
            catch
            {
                //HABLAR!!!!
                info_publicacion = null;
                Console.Write("Error de deserializacion del articulo.");
            }
            return info_publicacion;
        }

        //-----------------------------------------
        private string getAbstract(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                string abstractt = objInicial.AbstractsRetrievalResponse.coredata.DcDescription;
                return abstractt;
            }
            else
            {
                string abstractt = null;
                return abstractt;
            }
        }
        //extraccion de pageEnd en el modelo inicial! 
        private string getPageEnd(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                string pageEnd = objInicial.AbstractsRetrievalResponse.coredata.PrismEndingPage;
                return pageEnd;
            }
            else
            {
                string pageEnd = null;
                return pageEnd;
            }
        }
        // extraccion del string de page star en el modelo inicial.
        public string getPageStart(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                string pageStart = objInicial.AbstractsRetrievalResponse.coredata.PrismStartingPage;
                return pageStart;
            }
            else
            {
                string pageStart = null;
                return pageStart;
            }
        }
        public string getTitle(Publication_root objInicial)
        {
            if (objInicial != null & objInicial.AbstractsRetrievalResponse != null)
            {
                string title = objInicial.AbstractsRetrievalResponse.coredata.DcTitle;
                return title;
            }
            else
            {
                string title = null;
                return title;
            }
        }

        public List<Url> getLinks(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                List<Url> url = new List<Url>();
                if (objInicial.AbstractsRetrievalResponse.coredata.link != null)
                {
                    for (int i = 0; i < objInicial.AbstractsRetrievalResponse.coredata.link.Count; i++)
                    {
                        Url link = new Url();
                        if (objInicial.AbstractsRetrievalResponse.coredata.link[i].Rel == "scopus")
                        {
                            link.link = objInicial.AbstractsRetrievalResponse.coredata.link[i].Href;
                            link.description = "scopus"; //this is needed!!!!!! 
                            url.Add(link);
                        }
                    }
                    return url;
                }
                else
                {
                    List<Url> url_null = null;
                    return url_null;
                }
            }
            else
            {
                List<Url> url_null = null;
                return url_null;
            }
        }
        public string getDoi(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                string doi = objInicial.AbstractsRetrievalResponse.coredata.PrismDoi;
                return doi;
            }
            else
            {
                string doi = null;
                return doi;
            }
        }
        public List<KnowledgeArea> getKnowledgeAreas(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                List<KnowledgeArea> knowledgeAreas = new List<KnowledgeArea>();
                if (objInicial.AbstractsRetrievalResponse.SubjectAreas != null)
                {
                    for (int i = 0; i < objInicial.AbstractsRetrievalResponse.SubjectAreas.SubjectArea.Count; i++)
                    {
                        KnowledgeArea area = new KnowledgeArea();
                        area.abbreviation = objInicial.AbstractsRetrievalResponse.SubjectAreas.SubjectArea[i].Abbrev;
                        area.hasCode = objInicial.AbstractsRetrievalResponse.SubjectAreas.SubjectArea[i].Code;
                        area.name = objInicial.AbstractsRetrievalResponse.SubjectAreas.SubjectArea[i].a;
                        knowledgeAreas.Add(area);
                        //TODO ver si esto esta bien insertado en la ontologia porque lo dudo!!! 
                    }
                    return knowledgeAreas;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public Person getAuthorPrincipal(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                if (objInicial.AbstractsRetrievalResponse.coredata.DcCreator != null)
                {
                    Person authorPrincipal = getPerson(objInicial.AbstractsRetrievalResponse.coredata.DcCreator.author[0]);
                    return authorPrincipal;
                }
                else { return null; }
            }
            else
            {
                return null;
            }
        }
        public List<Person> getAuthors(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                List<Person> authores = new List<Person>();
                if (objInicial.AbstractsRetrievalResponse.authors != null)
                {
                    for (int i = 0; i < objInicial.AbstractsRetrievalResponse.authors.author.Count; i++)
                    {
                        Person persona = getPerson(objInicial.AbstractsRetrievalResponse.authors.author[i]);
                        authores.Add(persona);
                    }
                    return authores;
                }
                else { return null; }
            }
            else
            {
                return null;
            }
        }
        public Journal getJournal(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                Journal journal = new Journal();
                journal.name = objInicial.AbstractsRetrievalResponse.coredata.PrismPublicationName;
                journal.issn = objInicial.AbstractsRetrievalResponse.coredata.PrismIssn;
                // TODO mas info sobre la revista
                return journal;
            }
            else
            {
                return null;
            }
        }
        public DateTimeValue getDate(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                DateTimeValue date = new DateTimeValue();
                date.datimeTime = objInicial.AbstractsRetrievalResponse.coredata.PrismCoverDate;
                return date;
            }
            else
            {
                return null;
            }
        }
        public string getLanguage(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                if (objInicial.AbstractsRetrievalResponse.language != null)
                {
                    return objInicial.AbstractsRetrievalResponse.language.XmlLang;
                }
                else { return null; }
            }
            else
            {
                return null;
            }
        }
        public List<String> getFreetextKeyword(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                List<String> freetextKeyword = new List<string>();
                if (objInicial.AbstractsRetrievalResponse.authkeywords != null)
                {
                    for (int i = 0; i < objInicial.AbstractsRetrievalResponse.authkeywords.AuthorKeyword.Count; i++)
                    {
                        freetextKeyword.Add(objInicial.AbstractsRetrievalResponse.authkeywords.AuthorKeyword[i].a);
                    }
                    return freetextKeyword;
                }
                else { return null; }
            }
            else
            {
                return null;
            }
        }
        public PublicationMetric getPublicationMetric(Publication_root objInicial)
        {
            if (objInicial.AbstractsRetrievalResponse != null)
            {
                PublicationMetric publicationMetric = new PublicationMetric();
                publicationMetric.citationCount = objInicial.AbstractsRetrievalResponse.coredata.CitedbyCount;
                publicationMetric.metricName = "Scopus";
                return publicationMetric;
            }
            else
            {
                PublicationMetric publicationMetric = null;
                return publicationMetric;
            }

        }

        public Person getPerson(Author info_person)
        {
            //scopus id ----------------------------------------
            Person author = new Person();
            if (info_person.Auid != "")
            {
                author.identifier = "Scopus-id: " + info_person.Auid;

                // link---------------------------
                List<Url> listAuthorUrl = new List<Url>();
                Url linkScopus = new Url();
                linkScopus.link = "https://www.scopus.com/authid/detail.uri?authorId=" + info_person.Auid;
                linkScopus.description = "Scopus profile";
                listAuthorUrl.Add(linkScopus);
                author.link = listAuthorUrl;
            }
            else { author.link = null; }


            // names-------------
            List<string> names = new List<string>();
            if (info_person.CeIndexedName != null)
            {
                names.Add(info_person.CeIndexedName);
            }
            if (info_person.PreferredName != null)
            {
                author.surname = info_person.PreferredName.CeSurname;
                names.Add(info_person.PreferredName.CeGivenName);
                names.Add(info_person.PreferredName.CeInitials);
            }
            author.name = names;

            return author;

        }

        /// no se deberian necesitar de modificar en otros modelos!!!!!!

        public Publication getGenericPublication(Publication_root objInicial)
        {
            Publication publicacion = new Publication();
            //publicacion.bibliografia = getBibliografia(objInicial);
            //title--------------------------------------------------------------------
            publicacion.title = getTitle(objInicial);
            //links-----------------------------------------------------------------------------
            publicacion.url = getLinks(objInicial);
            //doi-----------------------------------------------------------------------------
            publicacion.doi = getDoi(objInicial);
            //knowledgeAreas--------------------------------------------------------------------
            publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
            //corresponding author-----------------------------------------------------------
            publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
            // lisOfAuthors-------------------------------------------------------------------
            publicacion.seqOfAuthors = getAuthors(objInicial);
            //journal------------------------------------------------------
            publicacion.hasPublicationVenue = getJournal(objInicial);
            //date--------------------------------------------------------
            publicacion.dataIssued = getDate(objInicial);
            //language --------------------------------------------------------
            publicacion.language = getLanguage(objInicial);
            // freetextKeyword ------------------------------------------------------
            publicacion.freetextKeyword = getFreetextKeyword(objInicial);
            //abstract------------------------------------------- 
            publicacion.Abstract = getAbstract(objInicial);
            //page start and page end ------------------------------------------
            publicacion.pageStart = getPageStart(objInicial);
            publicacion.pageEnd = getPageEnd(objInicial);
            // publication metric --------------------------------
            publicacion.hasMetric = getPublicationMetric(objInicial);

            return publicacion;
        }

        public JournalArticle getJournalArticle(Publication_root objInicial)
        {
            JournalArticle publicacion = new JournalArticle();
            // publicacion.bibliografia = getBibliografia(objInicial);

            //title--------------------------------------------------------------------
            publicacion.title = getTitle(objInicial);
            //links-----------------------------------------------------------------------------
            publicacion.url = getLinks(objInicial);
            //doi-----------------------------------------------------------------------------
            publicacion.doi = getDoi(objInicial);
            //knowledgeAreas--------------------------------------------------------------------
            publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
            //corresponding author-----------------------------------------------------------
            publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
            // lisOfAuthors-------------------------------------------------------------------
            publicacion.seqOfAuthors = getAuthors(objInicial);
            //journal------------------------------------------------------
            publicacion.hasPublicationVenue = getJournal(objInicial);
            //date--------------------------------------------------------
            publicacion.dataIssued = getDate(objInicial);
            //language --------------------------------------------------------
            publicacion.language = getLanguage(objInicial);
            // freetextKeyword ------------------------------------------------------
            publicacion.freetextKeyword = getFreetextKeyword(objInicial);
            //abstract------------------------------------------- 
            publicacion.Abstract = getAbstract(objInicial);
            //page start and page end ------------------------------------------
            publicacion.pageStart = getPageStart(objInicial);
            publicacion.pageEnd = getPageEnd(objInicial);
            // publication metric --------------------------------
            publicacion.hasMetric = getPublicationMetric(objInicial);

            return publicacion;

        }

        public ConferencePaper getConferencePaper(Publication_root objInicial)
        {

            ConferencePaper publicacion = new ConferencePaper();
            //publicacion.bibliografia = getBibliografia(objInicial);

            //title--------------------------------------------------------------------
            publicacion.title = getTitle(objInicial);
            //links-----------------------------------------------------------------------------
            publicacion.url = getLinks(objInicial);
            //doi-----------------------------------------------------------------------------
            publicacion.doi = getDoi(objInicial);
            //knowledgeAreas--------------------------------------------------------------------
            publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
            //corresponding author-----------------------------------------------------------
            publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
            // lisOfAuthors-------------------------------------------------------------------
            publicacion.seqOfAuthors = getAuthors(objInicial);
            //journal------------------------------------------------------
            publicacion.hasPublicationVenue = getJournal(objInicial);
            //date--------------------------------------------------------
            publicacion.dataIssued = getDate(objInicial);
            //language --------------------------------------------------------
            publicacion.language = getLanguage(objInicial);
            // freetextKeyword ------------------------------------------------------
            publicacion.freetextKeyword = getFreetextKeyword(objInicial);
            //abstract------------------------------------------- 
            publicacion.Abstract = getAbstract(objInicial);
            //page start and page end ------------------------------------------
            publicacion.pageStart = getPageStart(objInicial);
            publicacion.pageEnd = getPageEnd(objInicial);
            // publication metric --------------------------------
            publicacion.hasMetric = getPublicationMetric(objInicial);

            return publicacion;
        }

    }


}