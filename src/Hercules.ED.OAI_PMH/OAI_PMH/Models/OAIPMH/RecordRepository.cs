﻿using OAI_PMH.Controllers;
using OAI_PMH.Models.SGI;
using OAI_PMH.Models.SGI.ActividadDocente;
using OAI_PMH.Models.SGI.Autorizacion;
using OAI_PMH.Models.SGI.FormacionAcademica;
using OAI_PMH.Models.SGI.Organization;
using OAI_PMH.Models.SGI.PersonalData;
using OAI_PMH.Models.SGI.ProduccionCientifica;
using OAI_PMH.Models.SGI.Project;
using OAI_PMH.Services;
using OaiPmhNet;
using OaiPmhNet.Converters;
using OaiPmhNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OAI_PMH.Models.OAIPMH
{
    public class RecordRepository : IRecordRepository
    {
        private readonly IDateConverter _dateConverter;
        private readonly ConfigService _Config;

        public RecordRepository(ConfigService pConfig)
        {
            _dateConverter = new DateConverter();
            _Config = pConfig;
        }

        public RecordContainer GetIdentifiers(ArgumentContainer arguments, IResumptionToken resumptionToken = null)
        {
            return GetRecords(arguments, resumptionToken);
        }

        public Record GetRecord(string identifier, string metadataPrefix)
        {
            Record record = new();
            List<Record> listaRecords = new List<Record>();
            string set = identifier.Split('_')[0];
            DateTime date = DateTime.UtcNow;

            switch (set)
            {
                case "Persona":
                    Persona persona = PersonalData.GetPersona(identifier, _Config);
                    record = ToRecord(persona, set, identifier, date, metadataPrefix);
                    break;
                case "Proyecto":
                    Proyecto proyecto = Project.GetProyecto(identifier, _Config);
                    record = ToRecord(proyecto, set, identifier, date, metadataPrefix);
                    break;
                case "Organizacion":
                    Empresa organizacion = Organization.GetEmpresa(identifier, _Config);
                    record = ToRecord(organizacion, set, identifier, date, metadataPrefix);
                    break;
                case "Autorizacion":
                    Autorizacion autorizacion = Autorizaciones.GetAutorizacion(identifier, _Config);
                    record = ToRecord(autorizacion, set, identifier, date, metadataPrefix);
                    break;
                default:
                    break;
            }
            return record;
        }

        private static Record ToRecord(SGI_Base pObject, string pSet, string pId, DateTime pDate, string pMetadataPrefix)
        {
            Record record = new()
            {
                Header = new RecordHeader()
                {
                    Identifier = pId,
                    SetSpecs = new List<string>() { pSet },
                    Datestamp = pDate
                }
            };

            switch (pMetadataPrefix)
            {
                case "EDMA":
                    try
                    {
                        record.Metadata = new RecordMetadata()
                        {
                            Content = XElement.Parse(pObject.ToXML())
                        };
                    }
                    catch (Exception error)
                    {
                        return null;
                    }
                    break;
            }
            return record;
        }

        public RecordContainer GetRecords(ArgumentContainer arguments, IResumptionToken resumptionToken = null)
        {
            RecordContainer container = new RecordContainer();
            DateTime startDate = DateTime.MinValue;
            if (_dateConverter.TryDecode(arguments.From, out DateTime from))
            {
                startDate = from;
            }

            List<XML> listxml = new();

            if (arguments.Verb == OaiVerb.ListIdentifiers.ToString())
            {
                switch (arguments.Set)
                {
                    case "Persona":
                        Dictionary<string, DateTime> modifiedPeopleIds = PersonalData.GetModifiedPeople(arguments.From, _Config);
                        List<Record> personRecordList = new();
                        foreach (string personId in modifiedPeopleIds.Keys)
                        {
                            personRecordList.Add(ToIdentifiersRecord("Persona", personId, modifiedPeopleIds[personId]));
                        }
                        container.Records = personRecordList;
                        break;
                    case "Organizacion":
                        Dictionary<string, DateTime> modifiedOrganizationsIds = Organization.GetModifiedOrganizations(arguments.From, _Config);
                        List<Record> organizationRecordList = new();
                        foreach (string organizationId in modifiedOrganizationsIds.Keys)
                        {
                            organizationRecordList.Add(ToIdentifiersRecord("Organizacion", organizationId, modifiedOrganizationsIds[organizationId]));
                        }
                        container.Records = organizationRecordList;
                        break;
                    case "Proyecto":
                        Dictionary<string, DateTime> modifiedProjectsIds = Project.GetModifiedProjects(arguments.From, _Config);
                        List<Record> projectRecordList = new();
                        foreach (string projectId in modifiedProjectsIds.Keys)
                        {
                            projectRecordList.Add(ToIdentifiersRecord("Proyecto", projectId, modifiedProjectsIds[projectId]));
                        }
                        container.Records = projectRecordList;
                        break;
                    case "PRC":
                        Dictionary<string, DateTime> modifiedPRC = PRC.GetModifiedPRC(arguments.From, _Config);
                        List<Record> prcRecordList = new();
                        foreach (string prcId in modifiedPRC.Keys)
                        {
                            prcRecordList.Add(ToIdentifiersRecord("PRC", prcId, modifiedPRC[prcId]));
                        }
                        container.Records = prcRecordList;
                        break;
                    case "Autorizacion":
                        Dictionary<string, DateTime> modifiedAutorizaciones = Autorizaciones.GetModifiedAutorizaciones(arguments.From, _Config);
                        List<Record> autorizacionRecordList = new();
                        foreach (string autorizacionId in modifiedAutorizaciones.Keys)
                        {
                            autorizacionRecordList.Add(ToIdentifiersRecord("Autorizacion", autorizacionId, modifiedAutorizaciones[autorizacionId]));
                        }
                        container.Records = autorizacionRecordList;
                        break;
                }
            }
            else
            {
                switch (arguments.Set)
                {
                    case "Persona":
                        Dictionary<string, DateTime> modifiedPeopleIds = PersonalData.GetModifiedPeople(arguments.From, _Config);
                        List<Persona> peopleList = new();
                        foreach (string personId in modifiedPeopleIds.Keys)
                        {
                            peopleList.Add(PersonalData.GetPersona(personId, _Config));
                        }
                        List<Record> personRecordList = new();
                        foreach (Persona persona in peopleList)
                        {
                            personRecordList.Add(ToRecord(persona, arguments.Set, persona.Id, startDate, arguments.MetadataPrefix));
                        }
                        container.Records = personRecordList;
                        break;
                    case "Organizacion":
                        Dictionary<string, DateTime> modifiedOrganizationsIds = Organization.GetModifiedOrganizations(arguments.From, _Config);
                        List<Empresa> organizationsList = new();
                        foreach (string organizationId in modifiedOrganizationsIds.Keys)
                        {
                            organizationsList.Add(Organization.GetEmpresa(organizationId, _Config));
                        }
                        List<Record> organizationRecordList = new();
                        foreach (Empresa empresa in organizationsList)
                        {
                            organizationRecordList.Add(ToRecord(empresa, arguments.Set, empresa.Id, startDate, arguments.MetadataPrefix));
                        }
                        container.Records = organizationRecordList;
                        break;
                    case "Proyecto":
                        Dictionary<string, DateTime> modifiedProjectsIds = Project.GetModifiedProjects(arguments.From, _Config);
                        List<Proyecto> projectsList = new();
                        foreach (string projectId in modifiedProjectsIds.Keys)
                        {
                            projectsList.Add(Project.GetProyecto(projectId, _Config));
                        }
                        List<Record> projectRecordList = new();
                        foreach (Proyecto proyecto in projectsList)
                        {
                            projectRecordList.Add(ToRecord(proyecto, arguments.Set, proyecto.Id, startDate, arguments.MetadataPrefix));
                        }
                        container.Records = projectRecordList;
                        break;
                    case "PRC":
                        List<ProduccionCientificaEstado> prcList = PRC.GetPRC(arguments.From, _Config);
                        List<Record> prcRecordList = new();
                        foreach (ProduccionCientificaEstado prc in prcList)
                        {
                            prcRecordList.Add(ToRecord(prc, arguments.Set, prc.IdRef, startDate, arguments.MetadataPrefix));
                        }
                        container.Records = prcRecordList;
                        break;
                    case "Autorizaciob":
                        Dictionary<string, DateTime> modifiedAutorizacionIds = Autorizaciones.GetModifiedAutorizaciones(arguments.From, _Config);
                        List<Autorizacion> autorizacionList = new();
                        foreach (string autorizacionId in modifiedAutorizacionIds.Keys)
                        {
                            autorizacionList.Add(Autorizaciones.GetAutorizacion(autorizacionId, _Config));
                        }
                        List<Record> autorizacionRecordList = new();
                        foreach (Autorizacion autorizacion in autorizacionList)
                        {
                            autorizacionRecordList.Add(ToRecord(autorizacion, arguments.Set, autorizacion.id.ToString(), startDate, arguments.MetadataPrefix));
                        }
                        container.Records = autorizacionRecordList;
                        break;
                }
            }
            return container;
        }

        private static Record ToIdentifiersRecord(string pSet, string pId, DateTime pDate)
        {
            Record record = new()
            {
                Header = new RecordHeader()
                {
                    Identifier = pId,
                    SetSpecs = new List<string>() { pSet },
                    Datestamp = pDate
                }
            };
            return record;
        }
    }
}
