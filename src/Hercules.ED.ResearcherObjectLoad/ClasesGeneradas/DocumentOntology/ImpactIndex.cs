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
using System.Diagnostics.CodeAnalysis;
using ImpactIndexCategory = ImpactindexcategoryOntology.ImpactIndexCategory;
using ReferenceSource = ReferencesourceOntology.ReferenceSource;

namespace DocumentOntology
{
	[ExcludeFromCodeCoverage]
	public class ImpactIndex : GnossOCBase
	{

		public ImpactIndex() : base() { } 

		public ImpactIndex(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_impactIndexCategory = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/impactIndexCategory");
			if(propRoh_impactIndexCategory != null && propRoh_impactIndexCategory.PropertyValues.Count > 0)
			{
				this.Roh_impactIndexCategory = new ImpactIndexCategory(propRoh_impactIndexCategory.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_impactSource = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/impactSource");
			if(propRoh_impactSource != null && propRoh_impactSource.PropertyValues.Count > 0)
			{
				this.Roh_impactSource = new ReferenceSource(propRoh_impactSource.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_impactIndexInYear = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/impactIndexInYear"));
			this.Roh_impactSourceOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/impactSourceOther"));
			this.Roh_journalNumberInCat = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/journalNumberInCat"));
			this.Roh_publicationPosition = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/publicationPosition"));
			this.Roh_quartile = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/quartile"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/ImpactIndex"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/ImpactIndex"; } }
		public OntologyEntity Entity { get; set; }

		[LABEL(LanguageEnum.es,"Categoría del índice de impacto")]
		[RDFProperty("http://w3id.org/roh/impactIndexCategory")]
		public  ImpactIndexCategory Roh_impactIndexCategory  { get; set;} 
		public string IdRoh_impactIndexCategory  { get; set;} 

		[LABEL(LanguageEnum.es,"Fuente de impacto")]
		[RDFProperty("http://w3id.org/roh/impactSource")]
		public  ReferenceSource Roh_impactSource  { get; set;} 
		public string IdRoh_impactSource  { get; set;} 

		[RDFProperty("http://w3id.org/roh/impactIndexInYear")]
		public  float? Roh_impactIndexInYear { get; set;}

		[RDFProperty("http://w3id.org/roh/impactSourceOther")]
		public  string Roh_impactSourceOther { get; set;}

		[RDFProperty("http://w3id.org/roh/journalNumberInCat")]
		public  int? Roh_journalNumberInCat { get; set;}

		[RDFProperty("http://w3id.org/roh/publicationPosition")]
		public  int? Roh_publicationPosition { get; set;}

		[RDFProperty("http://w3id.org/roh/quartile")]
		public  int? Roh_quartile { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("roh:impactIndexCategory", this.IdRoh_impactIndexCategory));
			propList.Add(new StringOntologyProperty("roh:impactSource", this.IdRoh_impactSource));
			propList.Add(new StringOntologyProperty("roh:impactIndexInYear", this.Roh_impactIndexInYear.ToString()));
			propList.Add(new StringOntologyProperty("roh:impactSourceOther", this.Roh_impactSourceOther));
			propList.Add(new StringOntologyProperty("roh:journalNumberInCat", this.Roh_journalNumberInCat.ToString()));
			propList.Add(new StringOntologyProperty("roh:publicationPosition", this.Roh_publicationPosition.ToString()));
			propList.Add(new StringOntologyProperty("roh:quartile", this.Roh_quartile.ToString()));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
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

		private string GenerarTextoSinSaltoDeLinea(string pTexto)
		{
			return pTexto.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\"", "\\\"");
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