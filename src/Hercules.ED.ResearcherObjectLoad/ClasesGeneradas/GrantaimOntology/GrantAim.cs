using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using Gnoss.ApiWrapper.Helpers;
using GnossBase;
using Es.Riam.Gnoss.Web.MVC.Models;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections;
using Gnoss.ApiWrapper.Exceptions;

namespace GrantaimOntology
{
	public class GrantAim : GnossOCBase
	{

		public GrantAim() : base() { } 

		public GrantAim(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			this.Dc_identifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/identifier"));
			this.Dc_title = new Dictionary<LanguageEnum,string>();
			this.Dc_title.Add(idiomaUsuario , GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/title")));
			
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/GrantAim"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/GrantAim"; } }
		[LABEL(LanguageEnum.es,"http://purl.org/dc/elements/1.1/identifier")]
		[RDFProperty("http://purl.org/dc/elements/1.1/identifier")]
		public  string Dc_identifier { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/dc/elements/1.1/title")]
		[RDFProperty("http://purl.org/dc/elements/1.1/title")]
		public  Dictionary<LanguageEnum,string> Dc_title { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("dc:identifier", this.Dc_identifier));
			if(this.Dc_title != null)
			{
				foreach (LanguageEnum idioma in this.Dc_title.Keys)
				{
					propList.Add(new StringOntologyProperty("dc:title", this.Dc_title[idioma], idioma.ToString()));
				}
			}
			else
			{
				throw new GnossAPIException($"La propiedad dc:title debe tener al menos un valor en el recurso: {resourceID}");
			}
		}

		internal override void GetEntities()
		{
			base.GetEntities();
		} 
		public virtual SecondaryResource ToGnossApiResource(ResourceApi resourceAPI,string identificador)
		{
			SecondaryResource resource = new SecondaryResource();
			List<SecondaryEntity> listSecondaryEntity = null;
			GetProperties();
			SecondaryOntology ontology = new SecondaryOntology(resourceAPI.GraphsUrl, resourceAPI.OntologyUrl, "http://w3id.org/roh/GrantAim", "http://w3id.org/roh/GrantAim", prefList, propList,identificador,listSecondaryEntity, null);
			resource.SecondaryOntology = ontology;
			AddImages(resource);
			AddFiles(resource);
			return resource;
		}

		public override List<string> ToOntologyGnossTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GrantAim_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/GrantAim>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GrantAim_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/GrantAim\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/GrantAim_{ResourceID}_{ArticleID}>", list, " . ");
				if(this.Dc_identifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GrantAim_{ResourceID}_{ArticleID}",  "http://purl.org/dc/elements/1.1/identifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_identifier)}\"", list, " . ");
				}
				if(this.Dc_title != null)
				{
							foreach (LanguageEnum idioma in this.Dc_title.Keys)
							{
								AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GrantAim_{ResourceID}_{ArticleID}", "http://purl.org/dc/elements/1.1/title",  $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_title[idioma])}\"", list,  $"{idioma} . ");
							}
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			string search = string.Empty;
				if(this.Dc_identifier != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/dc/elements/1.1/identifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_identifier).ToLower()}\"", list, " . ");
				}
				if(this.Dc_title != null)
				{
							foreach (LanguageEnum idioma in this.Dc_title.Keys)
							{
								AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://purl.org/dc/elements/1.1/title",  $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_title[idioma]).ToLower()}\"", list,  $"{idioma} . ");
							}
				}
			if (listaSearch != null && listaSearch.Count > 0)
			{
				foreach(string valorSearch in listaSearch)
				{
					search += $"{valorSearch} ";
				}
			}
			if(!string.IsNullOrEmpty(search))
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/search", $"\"{search.ToLower()}\"", list, " . ");
			}
			return list;
		}

		public override KeyValuePair<Guid, string> ToAcidData(ResourceApi resourceAPI)
		{
			KeyValuePair<Guid, string> valor = new KeyValuePair<Guid, string>();

			return valor;
		}

		protected List<object> ObtenerObjetosDePropiedad(object propiedad)
		{
			List<object> lista = new List<object>();
			if(propiedad is IList)
			{
				foreach (object item in (IList)propiedad)
				{
					lista.Add(item);
				}
			}
			else
			{
				lista.Add(propiedad);
			}
			return lista;
		}
		protected List<string> ObtenerStringDePropiedad(object propiedad)
		{
			List<string> lista = new List<string>();
			if (propiedad is IList)
			{
				foreach (string item in (IList)propiedad)
				{
					lista.Add(item);
				}
			}
			else if (propiedad is IDictionary)
			{
				foreach (object key in ((IDictionary)propiedad).Keys)
				{
					if (((IDictionary)propiedad)[key] is IList)
					{
						List<string> listaValores = (List<string>)((IDictionary)propiedad)[key];
						foreach(string valor in listaValores)
						{
							lista.Add(valor);
						}
					}
					else
					{
					lista.Add((string)((IDictionary)propiedad)[key]);
					}
				}
			}
			else if (propiedad is string)
			{
				lista.Add((string)propiedad);
			}
			return lista;
		}
		public override string GetURI(ResourceApi resourceAPI)
		{
			return $"{resourceAPI.GraphsUrl}items/GrantaimOntology_{ResourceID}_{ArticleID}";
		}

		private string GenerarTextoSinSaltoDeLinea(string pTexto)
		{
			return pTexto.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\"", "\\\"");
		}

		internal void AddResourceTitle(ComplexOntologyResource resource)
		{
		}


		private void AgregarTripleALista(string pSujeto, string pPredicado, string pObjeto, List<string> pLista, string pDatosExtra)
		{
			if(!string.IsNullOrEmpty(pObjeto) && !pObjeto.Equals("\"\"") && !pObjeto.Equals("<>"))
			{
				pLista.Add($"<{pSujeto}> <{pPredicado}> {pObjeto}{pDatosExtra}");
			} 
		} 

		private void AgregarTags(List<string> pListaTriples)
		{
			foreach(string tag in tagList)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://rdfs.org/sioc/types#Tag", tag.ToLower(), pListaTriples, " . ");
			}
		}


	}
}