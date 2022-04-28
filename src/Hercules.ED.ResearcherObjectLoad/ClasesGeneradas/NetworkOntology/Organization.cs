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
using Organization = OrganizationOntology.Organization;
using OrganizationType = OrganizationtypeOntology.OrganizationType;

namespace NetworkOntology
{
	[ExcludeFromCodeCoverage]
	public class Organization : GnossOCBase
	{

		public Organization() : base() { } 

		public Organization(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_organization = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/organization");
			if(propRoh_organization != null && propRoh_organization.PropertyValues.Count > 0)
			{
				this.Roh_organization = new Organization(propRoh_organization.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_organizationType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/organizationType");
			if(propRoh_organizationType != null && propRoh_organizationType.PropertyValues.Count > 0)
			{
				this.Roh_organizationType = new OrganizationType(propRoh_organizationType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_organizationTypeOther = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/organizationTypeOther");
			this.Roh_organizationTypeOther = new List<string>();
			if (propRoh_organizationTypeOther != null && propRoh_organizationTypeOther.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_organizationTypeOther.PropertyValues)
				{
					this.Roh_organizationTypeOther.Add(propValue.Value);
				}
			}
			this.Roh_organizationTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/organizationTitle"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/Organization"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/Organization"; } }
		public OntologyEntity Entity { get; set; }

		[RDFProperty("http://w3id.org/roh/organization")]
		public  Organization Roh_organization  { get; set;} 
		public string IdRoh_organization  { get; set;} 

		[RDFProperty("http://w3id.org/roh/organizationType")]
		public  OrganizationType Roh_organizationType  { get; set;} 
		public string IdRoh_organizationType  { get; set;} 

		[RDFProperty("http://w3id.org/roh/organizationTypeOther")]
		public  List<string> Roh_organizationTypeOther { get; set;}

		[RDFProperty("http://w3id.org/roh/organizationTitle")]
		public  string Roh_organizationTitle { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("roh:organization", this.IdRoh_organization));
			propList.Add(new StringOntologyProperty("roh:organizationType", this.IdRoh_organizationType));
			propList.Add(new ListStringOntologyProperty("roh:organizationTypeOther", this.Roh_organizationTypeOther));
			propList.Add(new StringOntologyProperty("roh:organizationTitle", this.Roh_organizationTitle));
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
