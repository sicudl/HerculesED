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
using Accreditation = AccreditationOntology.Accreditation;

namespace CurriculumvitaeOntology
{
	public class RelatedOtherDistinction : GnossOCBase
	{

		public RelatedOtherDistinction() : base() { } 

		public RelatedOtherDistinction(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_relatedOtherDistinctionCV = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relatedOtherDistinctionCV");
			if(propRoh_relatedOtherDistinctionCV != null && propRoh_relatedOtherDistinctionCV.PropertyValues.Count > 0)
			{
				this.Roh_relatedOtherDistinctionCV = new RelatedOtherDistinctionCV(propRoh_relatedOtherDistinctionCV.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVivo_relatedBy = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#relatedBy");
			if(propVivo_relatedBy != null && propVivo_relatedBy.PropertyValues.Count > 0)
			{
				this.Vivo_relatedBy = new Accreditation(propVivo_relatedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_isPublic= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isPublic"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/RelatedOtherDistinction"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/RelatedOtherDistinction"; } }
		public OntologyEntity Entity { get; set; }

		[RDFProperty("http://w3id.org/roh/relatedOtherDistinctionCV")]
		public  RelatedOtherDistinctionCV Roh_relatedOtherDistinctionCV { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#relatedBy")]
		[RDFProperty("http://vivoweb.org/ontology/core#relatedBy")]
		[Required]
		public  Accreditation Vivo_relatedBy  { get; set;} 
		public string IdVivo_relatedBy  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/isPublic")]
		[RDFProperty("http://w3id.org/roh/isPublic")]
		public  bool Roh_isPublic { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("vivo:relatedBy", this.IdVivo_relatedBy));
			propList.Add(new BoolOntologyProperty("roh:isPublic", this.Roh_isPublic));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if(Roh_relatedOtherDistinctionCV!=null){
				Roh_relatedOtherDistinctionCV.GetProperties();
				Roh_relatedOtherDistinctionCV.GetEntities();
				OntologyEntity entityRoh_relatedOtherDistinctionCV = new OntologyEntity("http://w3id.org/roh/RelatedOtherDistinctionCV", "http://w3id.org/roh/RelatedOtherDistinctionCV", "roh:relatedOtherDistinctionCV", Roh_relatedOtherDistinctionCV.propList, Roh_relatedOtherDistinctionCV.entList);
				entList.Add(entityRoh_relatedOtherDistinctionCV);
			}
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
