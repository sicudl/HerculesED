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
using DegreeType = DegreetypeOntology.DegreeType;
using QualificationType = QualificationtypeOntology.QualificationType;
using OrganizationType = OrganizationtypeOntology.OrganizationType;
using Feature = FeatureOntology.Feature;
using PrizeType = PrizetypeOntology.PrizeType;
using UniversityDegreeType = UniversitydegreetypeOntology.UniversityDegreeType;
using Organization = OrganizationOntology.Organization;

namespace AcademicdegreeOntology
{
	[ExcludeFromCodeCoverage]
	public class AcademicDegree : GnossOCBase
	{

		public AcademicDegree() : base() { } 

		public AcademicDegree(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			SemanticPropertyModel propRoh_foreignDegreeType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/foreignDegreeType");
			if(propRoh_foreignDegreeType != null && propRoh_foreignDegreeType.PropertyValues.Count > 0)
			{
				this.Roh_foreignDegreeType = new DegreeType(propRoh_foreignDegreeType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_mark = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/mark");
			if(propRoh_mark != null && propRoh_mark.PropertyValues.Count > 0)
			{
				this.Roh_mark = new QualificationType(propRoh_mark.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_conductedByType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByType");
			if(propRoh_conductedByType != null && propRoh_conductedByType.PropertyValues.Count > 0)
			{
				this.Roh_conductedByType = new OrganizationType(propRoh_conductedByType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if(propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_prize = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/prize");
			if(propRoh_prize != null && propRoh_prize.PropertyValues.Count > 0)
			{
				this.Roh_prize = new PrizeType(propRoh_prize.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_universityDegreeType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/universityDegreeType");
			if(propRoh_universityDegreeType != null && propRoh_universityDegreeType.PropertyValues.Count > 0)
			{
				this.Roh_universityDegreeType = new UniversityDegreeType(propRoh_universityDegreeType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_degreeType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/degreeType");
			if(propRoh_degreeType != null && propRoh_degreeType.PropertyValues.Count > 0)
			{
				this.Roh_degreeType = new DegreeType(propRoh_degreeType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_conductedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedBy");
			if(propRoh_conductedBy != null && propRoh_conductedBy.PropertyValues.Count > 0)
			{
				this.Roh_conductedBy = new Organization(propRoh_conductedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if(propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_prizeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/prizeOther"));
			this.Roh_conductedByTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByTitle"));
			this.Roh_approvedDate= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/approvedDate"));
			this.Dct_issued= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/terms/issued"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_foreignTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/foreignTitle"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_universityDegreeTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/universityDegreeTypeOther"));
			this.Roh_conductedByTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByTypeOther"));
			this.Roh_approvedDegree= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/approvedDegree"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public AcademicDegree(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_foreignDegreeType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/foreignDegreeType");
			if(propRoh_foreignDegreeType != null && propRoh_foreignDegreeType.PropertyValues.Count > 0)
			{
				this.Roh_foreignDegreeType = new DegreeType(propRoh_foreignDegreeType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_mark = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/mark");
			if(propRoh_mark != null && propRoh_mark.PropertyValues.Count > 0)
			{
				this.Roh_mark = new QualificationType(propRoh_mark.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_conductedByType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByType");
			if(propRoh_conductedByType != null && propRoh_conductedByType.PropertyValues.Count > 0)
			{
				this.Roh_conductedByType = new OrganizationType(propRoh_conductedByType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if(propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_prize = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/prize");
			if(propRoh_prize != null && propRoh_prize.PropertyValues.Count > 0)
			{
				this.Roh_prize = new PrizeType(propRoh_prize.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_universityDegreeType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/universityDegreeType");
			if(propRoh_universityDegreeType != null && propRoh_universityDegreeType.PropertyValues.Count > 0)
			{
				this.Roh_universityDegreeType = new UniversityDegreeType(propRoh_universityDegreeType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_degreeType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/degreeType");
			if(propRoh_degreeType != null && propRoh_degreeType.PropertyValues.Count > 0)
			{
				this.Roh_degreeType = new DegreeType(propRoh_degreeType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_conductedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedBy");
			if(propRoh_conductedBy != null && propRoh_conductedBy.PropertyValues.Count > 0)
			{
				this.Roh_conductedBy = new Organization(propRoh_conductedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if(propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_prizeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/prizeOther"));
			this.Roh_conductedByTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByTitle"));
			this.Roh_approvedDate= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/approvedDate"));
			this.Dct_issued= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/terms/issued"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_foreignTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/foreignTitle"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_universityDegreeTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/universityDegreeTypeOther"));
			this.Roh_conductedByTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByTypeOther"));
			this.Roh_approvedDegree= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/approvedDegree"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public virtual string RdfType { get { return "http://vivoweb.org/ontology/core#AcademicDegree"; } }
		public virtual string RdfsLabel { get { return "http://vivoweb.org/ontology/core#AcademicDegree"; } }
		[RDFProperty("http://w3id.org/roh/foreignDegreeType")]
		public  DegreeType Roh_foreignDegreeType  { get; set;} 
		public string IdRoh_foreignDegreeType  { get; set;} 

		[RDFProperty("http://w3id.org/roh/mark")]
		public  QualificationType Roh_mark  { get; set;} 
		public string IdRoh_mark  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/conductedByType")]
		[RDFProperty("http://w3id.org/roh/conductedByType")]
		public  OrganizationType Roh_conductedByType  { get; set;} 
		public string IdRoh_conductedByType  { get; set;} 

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#hasCountryName")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasCountryName")]
		public  Feature Vcard_hasCountryName  { get; set;} 
		public string IdVcard_hasCountryName  { get; set;} 

		[RDFProperty("http://w3id.org/roh/prize")]
		public  PrizeType Roh_prize  { get; set;} 
		public string IdRoh_prize  { get; set;} 

		[RDFProperty("http://w3id.org/roh/universityDegreeType")]
		public  UniversityDegreeType Roh_universityDegreeType  { get; set;} 
		public string IdRoh_universityDegreeType  { get; set;} 

		[RDFProperty("http://w3id.org/roh/degreeType")]
		public  DegreeType Roh_degreeType  { get; set;} 
		public string IdRoh_degreeType  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/conductedBy")]
		[RDFProperty("http://w3id.org/roh/conductedBy")]
		public  Organization Roh_conductedBy  { get; set;} 
		public string IdRoh_conductedBy  { get; set;} 

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#hasRegion")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasRegion")]
		public  Feature Vcard_hasRegion  { get; set;} 
		public string IdVcard_hasRegion  { get; set;} 

		[RDFProperty("http://w3id.org/roh/prizeOther")]
		public  string Roh_prizeOther { get; set;}

		[RDFProperty("http://w3id.org/roh/conductedByTitle")]
		public  string Roh_conductedByTitle { get; set;}

		[RDFProperty("http://w3id.org/roh/approvedDate")]
		public  DateTime? Roh_approvedDate { get; set;}

		[RDFProperty("http://purl.org/dc/terms/issued")]
		public  DateTime? Dct_issued { get; set;}

		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public  string Roh_crisIdentifier { get; set;}

		[RDFProperty("http://w3id.org/roh/foreignTitle")]
		public  string Roh_foreignTitle { get; set;}

		[RDFProperty("https://www.w3.org/2006/vcard/ns#locality")]
		public  string Vcard_locality { get; set;}

		[RDFProperty("http://w3id.org/roh/universityDegreeTypeOther")]
		public  string Roh_universityDegreeTypeOther { get; set;}

		[RDFProperty("http://w3id.org/roh/conductedByTypeOther")]
		public  string Roh_conductedByTypeOther { get; set;}

		[RDFProperty("http://w3id.org/roh/approvedDegree")]
		public  bool Roh_approvedDegree { get; set;}

		[RDFProperty("http://w3id.org/roh/title")]
		public  string Roh_title { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("roh:foreignDegreeType", this.IdRoh_foreignDegreeType));
			propList.Add(new StringOntologyProperty("roh:mark", this.IdRoh_mark));
			propList.Add(new StringOntologyProperty("roh:conductedByType", this.IdRoh_conductedByType));
			propList.Add(new StringOntologyProperty("vcard:hasCountryName", this.IdVcard_hasCountryName));
			propList.Add(new StringOntologyProperty("roh:prize", this.IdRoh_prize));
			propList.Add(new StringOntologyProperty("roh:universityDegreeType", this.IdRoh_universityDegreeType));
			propList.Add(new StringOntologyProperty("roh:degreeType", this.IdRoh_degreeType));
			propList.Add(new StringOntologyProperty("roh:conductedBy", this.IdRoh_conductedBy));
			propList.Add(new StringOntologyProperty("vcard:hasRegion", this.IdVcard_hasRegion));
			propList.Add(new StringOntologyProperty("roh:prizeOther", this.Roh_prizeOther));
			propList.Add(new StringOntologyProperty("roh:conductedByTitle", this.Roh_conductedByTitle));
			if (this.Roh_approvedDate.HasValue){
				propList.Add(new DateOntologyProperty("roh:approvedDate", this.Roh_approvedDate.Value));
				}
			if (this.Dct_issued.HasValue){
				propList.Add(new DateOntologyProperty("dct:issued", this.Dct_issued.Value));
				}
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			propList.Add(new StringOntologyProperty("roh:foreignTitle", this.Roh_foreignTitle));
			propList.Add(new StringOntologyProperty("vcard:locality", this.Vcard_locality));
			propList.Add(new StringOntologyProperty("roh:universityDegreeTypeOther", this.Roh_universityDegreeTypeOther));
			propList.Add(new StringOntologyProperty("roh:conductedByTypeOther", this.Roh_conductedByTypeOther));
			propList.Add(new BoolOntologyProperty("roh:approvedDegree", this.Roh_approvedDegree));
			propList.Add(new StringOntologyProperty("roh:title", this.Roh_title));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
		} 
		public virtual ComplexOntologyResource ToGnossApiResource(ResourceApi resourceAPI, List<string> listaDeCategorias)
		{
			return ToGnossApiResource(resourceAPI, listaDeCategorias, Guid.Empty, Guid.Empty);
		}

		public virtual ComplexOntologyResource ToGnossApiResource(ResourceApi resourceAPI, List<string> listaDeCategorias, Guid idrecurso, Guid idarticulo)
		{
			ComplexOntologyResource resource = new ComplexOntologyResource();
			Ontology ontology=null;
			GetProperties();
			if(idrecurso.Equals(Guid.Empty) && idarticulo.Equals(Guid.Empty))
			{
				ontology = new Ontology(resourceAPI.GraphsUrl, resourceAPI.OntologyUrl, RdfType, RdfsLabel, prefList, propList, entList);
			}
			else{
				ontology = new Ontology(resourceAPI.GraphsUrl, resourceAPI.OntologyUrl, RdfType, RdfsLabel, prefList, propList, entList,idrecurso,idarticulo);
			}
			resource.Id = GNOSSID;
			resource.Ontology = ontology;
			resource.TextCategories=listaDeCategorias;
			AddResourceTitle(resource);
			AddResourceDescription(resource);
			AddImages(resource);
			AddFiles(resource);
			return resource;
		}

		public override List<string> ToOntologyGnossTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://vivoweb.org/ontology/core#AcademicDegree>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://vivoweb.org/ontology/core#AcademicDegree\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}>", list, " . ");
				if(this.IdRoh_foreignDegreeType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/foreignDegreeType", $"<{this.IdRoh_foreignDegreeType}>", list, " . ");
				}
				if(this.IdRoh_mark != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/mark", $"<{this.IdRoh_mark}>", list, " . ");
				}
				if(this.IdRoh_conductedByType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/conductedByType", $"<{this.IdRoh_conductedByType}>", list, " . ");
				}
				if(this.IdVcard_hasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{this.IdVcard_hasCountryName}>", list, " . ");
				}
				if(this.IdRoh_prize != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/prize", $"<{this.IdRoh_prize}>", list, " . ");
				}
				if(this.IdRoh_universityDegreeType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/universityDegreeType", $"<{this.IdRoh_universityDegreeType}>", list, " . ");
				}
				if(this.IdRoh_degreeType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/degreeType", $"<{this.IdRoh_degreeType}>", list, " . ");
				}
				if(this.IdRoh_conductedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/conductedBy", $"<{this.IdRoh_conductedBy}>", list, " . ");
				}
				if(this.IdVcard_hasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{this.IdVcard_hasRegion}>", list, " . ");
				}
				if(this.Roh_prizeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/prizeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_prizeOther)}\"", list, " . ");
				}
				if(this.Roh_conductedByTitle != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/conductedByTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_conductedByTitle)}\"", list, " . ");
				}
				if(this.Roh_approvedDate != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/approvedDate", $"\"{this.Roh_approvedDate.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Dct_issued != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://purl.org/dc/terms/issued", $"\"{this.Dct_issued.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
				}
				if(this.Roh_foreignTitle != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/foreignTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_foreignTitle)}\"", list, " . ");
				}
				if(this.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality)}\"", list, " . ");
				}
				if(this.Roh_universityDegreeTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/universityDegreeTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_universityDegreeTypeOther)}\"", list, " . ");
				}
				if(this.Roh_conductedByTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/conductedByTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_conductedByTypeOther)}\"", list, " . ");
				}
				if(this.Roh_approvedDegree != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/approvedDegree", $"\"{this.Roh_approvedDegree.ToString()}\"", list, " . ");
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/AcademicDegree_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"academicdegree\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://vivoweb.org/ontology/core#AcademicDegree\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			string search = string.Empty;
				if(this.IdRoh_foreignDegreeType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_foreignDegreeType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/foreignDegreeType", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_mark != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_mark;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/mark", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_conductedByType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_conductedByType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/conductedByType", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdVcard_hasCountryName != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdVcard_hasCountryName;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_prize != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_prize;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/prize", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_universityDegreeType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_universityDegreeType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/universityDegreeType", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_degreeType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_degreeType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/degreeType", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_conductedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_conductedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/conductedBy", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdVcard_hasRegion != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdVcard_hasRegion;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_prizeOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/prizeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_prizeOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_conductedByTitle != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/conductedByTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_conductedByTitle).ToLower()}\"", list, " . ");
				}
				if(this.Roh_approvedDate != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/approvedDate", $"{this.Roh_approvedDate.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Dct_issued != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/dc/terms/issued", $"{this.Dct_issued.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
				}
				if(this.Roh_foreignTitle != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/foreignTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_foreignTitle).ToLower()}\"", list, " . ");
				}
				if(this.Vcard_locality != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality).ToLower()}\"", list, " . ");
				}
				if(this.Roh_universityDegreeTypeOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/universityDegreeTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_universityDegreeTypeOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_conductedByTypeOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/conductedByTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_conductedByTypeOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_approvedDegree != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/approvedDegree", $"\"{this.Roh_approvedDegree.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title).ToLower()}\"", list, " . ");
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
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/search", $"\"{GenerarTextoSinSaltoDeLinea(search.ToLower())}\"", list, " . ");
			}
			return list;
		}

		public override KeyValuePair<Guid, string> ToAcidData(ResourceApi resourceAPI)
		{

			//Insert en la tabla Documento
			string tags = "";
			foreach(string tag in tagList)
			{
				tags += $"{tag}, ";
			}
			if (!string.IsNullOrEmpty(tags))
			{
				tags = tags.Substring(0, tags.LastIndexOf(','));
			}
			string titulo = $"{this.Roh_title.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
			string descripcion = $"{this.Roh_title.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
			string tablaDoc = $"'{titulo}', '{descripcion}', '{resourceAPI.GraphsUrl}', '{tags}'";
			KeyValuePair<Guid, string> valor = new KeyValuePair<Guid, string>(ResourceID, tablaDoc);

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
			return $"{resourceAPI.GraphsUrl}items/AcademicdegreeOntology_{ResourceID}_{ArticleID}";
		}

		private string GenerarTextoSinSaltoDeLinea(string pTexto)
		{
			return pTexto.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\"", "\\\"");
		}

		internal void AddResourceTitle(ComplexOntologyResource resource)
		{
			resource.Title = this.Roh_title;
		}

		internal void AddResourceDescription(ComplexOntologyResource resource)
		{
			resource.Description = this.Roh_title;
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