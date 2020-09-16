using JosephM.Xrm.FieldChangeHistory.Plugins.Localisation;
using JosephM.Xrm.FieldChangeHistory.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.Services
{
    /// <summary>
    /// A service class for performing logic
    /// </summary>
    public class FieldChangeService
    {
        private XrmService XrmService { get; set; }
        private FieldChangeSettings FieldChangeSettings { get; set; }
        public LocalisationService LocalisationService { get; }

        public FieldChangeService(XrmService xrmService, FieldChangeSettings settings, LocalisationService localisationService)
        {
            XrmService = xrmService;
            FieldChangeSettings = settings;
            LocalisationService = localisationService;
        }

        public void RefreshPluginRegistrations(Guid targetId, bool isActive)
        {
            var fieldChangeConfiguration = isActive
                ? XrmService.Retrieve(Entities.jmcg_fieldchangeconfiguration, targetId)
                : null;

            var fieldChangeHistoryEvents = GetFieldChangeHistoryEvents(targetId);

            var requiredPostOperationMessages = new[]
            {
                PluginMessage.Create,
                PluginMessage.Update,
                PluginMessage.Delete
            };

            foreach (var message in requiredPostOperationMessages)
            {
                var matchingMessages = fieldChangeHistoryEvents
                    .Where(m => m.GetStringField("MESSAGE." + Fields.sdkmessage_.name) == message);

                if(!isActive)
                {
                    foreach(var matchingMessage in matchingMessages)
                    {
                        XrmService.Delete(matchingMessage);
                    }
                }
                else
                {
                    if(matchingMessages.Count() > 1)
                    {
                        foreach(var extraMessage in matchingMessages.Skip(1))
                        {
                            XrmService.Delete(extraMessage);
                        }
                    }

                    var sdkMessage = matchingMessages.Any()
                    ? matchingMessages.First()
                    : new Entity(Entities.sdkmessageprocessingstep);

                    var targetTypeForChange = fieldChangeConfiguration.GetStringField(Fields.jmcg_fieldchangeconfiguration_.jmcg_entitytype);
                    var targetFieldForChange = fieldChangeConfiguration.GetStringField(Fields.jmcg_fieldchangeconfiguration_.jmcg_field);
                    var runAs = fieldChangeConfiguration.GetField(Fields.jmcg_fieldchangeconfiguration_.jmcg_runpluginas) as EntityReference;

                    var serialised = SerialiseToString(fieldChangeConfiguration);
                    if (sdkMessage.Id == Guid.Empty)
                    {
                        sdkMessage.SetField(Fields.sdkmessageprocessingstep_.configuration, serialised);
                        sdkMessage.SetField(Fields.sdkmessageprocessingstep_.description, fieldChangeConfiguration.Id.ToString() + " !! DO NOT REMOVE THE GUID AT THE START OF THIS DESCRIPTION !!");
                        sdkMessage.SetLookupField(Fields.sdkmessageprocessingstep_.plugintypeid, GetPluginType());
                        sdkMessage.SetOptionSetField(Fields.sdkmessageprocessingstep_.stage, PluginStage.PostEvent);
                        sdkMessage.SetLookupField(Fields.sdkmessageprocessingstep_.sdkmessagefilterid, GetPluginFilter(targetTypeForChange, message));
                        sdkMessage.SetLookupField(Fields.sdkmessageprocessingstep_.sdkmessageid, GetPluginMessage(message));
                        sdkMessage.SetField(Fields.sdkmessageprocessingstep_.rank, 1);
                        sdkMessage.SetField(Fields.sdkmessageprocessingstep_.name, $"Change History For {targetTypeForChange}.{targetFieldForChange} on {message}");
                        if (runAs != null)
                        {
                            sdkMessage.SetField(Fields.sdkmessageprocessingstep_.impersonatinguserid, runAs);
                        }
                        sdkMessage.Id = XrmService.Create(sdkMessage);

                        if (message == PluginMessage.Update || message == PluginMessage.Delete)
                        {
                            var imageRecord = new Entity(Entities.sdkmessageprocessingstepimage);
                            imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.name, "PreImage");
                            imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.entityalias, "PreImage");
                            imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.messagepropertyname, "Target");
                            imageRecord.SetLookupField(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, sdkMessage);
                            imageRecord.SetOptionSetField(Fields.sdkmessageprocessingstepimage_.imagetype, OptionSets.SdkMessageProcessingStepImage.ImageType.PreImage);
                            XrmService.Create(imageRecord);
                        }
                    }
                    else
                    {
                        var updateEntity = new Entity(Entities.sdkmessageprocessingstep)
                        {
                            Id = sdkMessage.Id
                        };

                        updateEntity.SetField(Fields.sdkmessageprocessingstep_.configuration, serialised);
                        if (!XrmEntity.FieldsEqual(runAs, sdkMessage.GetField(Fields.sdkmessageprocessingstep_.impersonatinguserid)))
                        {
                            updateEntity.SetField(Fields.sdkmessageprocessingstep_.impersonatinguserid, runAs);
                        }
                        XrmService.Update(updateEntity);
                    }
                }
            }
        }

        public Entity GetPluginType()
        {
            var entity = XrmService.GetFirst(Entities.plugintype, Fields.plugintype_.typename, PluginQualifiedName);
            if (entity == null)
                throw new NullReferenceException(string.Format("No {0} Exists With {1} = {2}",
                    XrmService.GetEntityDisplayName(Entities.plugintype), XrmService.GetFieldLabel(Fields.plugintype_.typename, Entities.plugintype),
                    PluginQualifiedName));
            return entity;
        }

        public Entity GetPluginFilter(string entityType, string message)

        {
            var pluginFilters = XrmService.RetrieveAllAndConditions(Entities.sdkmessagefilter, new[]
            {
                new ConditionExpression(Fields.sdkmessagefilter_.primaryobjecttypecode, ConditionOperator.Equal,
                    XrmService.GetEntityMetadata(entityType).ObjectTypeCode),
                new ConditionExpression(Fields.sdkmessagefilter_.sdkmessageid, ConditionOperator.Equal, GetPluginMessage(message).Id)
            });

            if (pluginFilters.Count() != 1)
                throw new InvalidPluginExecutionException(string.Format(
                    "Error Getting {0} for {1} {2} and type {3}",
                    XrmService.GetEntityDisplayName(Entities.sdkmessagefilter), XrmService.GetEntityDisplayName(Entities.sdkmessage), message,
                    XrmService.GetEntityDisplayName(entityType)));
            return pluginFilters.First();
        }

        public Entity GetPluginMessage(string message)
        {
            return XrmService.GetFirst(Entities.sdkmessage, Fields.sdkmessage_.name, message);
        }

        private string SerialiseToString(Entity entity)
        {
            var serialise = new SerialisedEntity();
            serialise.Id = entity.Id;
            serialise.LogicalName = entity.LogicalName;
            serialise.Attributes = entity.Attributes.ToDictionary(kv => kv.Key,
                kv =>
                {
                    if (kv.Value == null)
                        return null;
                    if (KnownSerialisationTypes.Any(t => t == kv.Value.GetType()))
                        return kv.Value;
                    return kv.Value.ToString();
                });

            var serializer = new DataContractJsonSerializer(typeof(SerialisedEntity), KnownSerialisationTypes);
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, serialise);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        public Entity DeserialiseEntity(string serialised)
        {
            object theObject;
            var serializer = new DataContractJsonSerializer(typeof(SerialisedEntity), KnownSerialisationTypes);
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(serialised)))
            {
                theObject = serializer.ReadObject(stream);
            }
            var deserialised = (SerialisedEntity)theObject;

            var entity = new Entity(deserialised.LogicalName);
            entity.Id = deserialised.Id;
            foreach (var keyValue in deserialised.Attributes)
            {
                entity[keyValue.Key] = keyValue.Value;
            }
            return entity;
        }

        private IEnumerable<Type> KnownSerialisationTypes
        {
            get
            {
                return new[]
                {
                    typeof(EntityReference),
                    typeof(OptionSetValue),
                    typeof(Money),
                    typeof(DateTime),
                    typeof(int),
                    typeof(decimal),
                    typeof(Guid),
                    typeof(bool),
                };
            }
        }

        public FieldChangeConfig LoadCalculatedFieldConfig(Entity fieldChangeEntity)
        {
            var config = new FieldChangeConfig
            {
                FieldChangeEntity = fieldChangeEntity,
            };
            var type = config.FieldChangeEntity.GetStringField(Fields.jmcg_fieldchangeconfiguration_.jmcg_entitytype);
            var field = config.FieldChangeEntity.GetStringField(Fields.jmcg_fieldchangeconfiguration_.jmcg_field);
            config.TypeDisplayName = XrmService.GetEntityDisplayName(type);
            config.FieldDisplayName = XrmService.GetFieldLabel(field, type);
            config.TypePrimaryField = XrmService.GetPrimaryField(type);
            config.MaxNameLength = XrmService.GetMaxLength(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordname, Entities.jmcg_fieldchangehistory);
            config.MaxPreviousInternalValueLength = XrmService.GetMaxLength(Fields.jmcg_fieldchangehistory_.jmcg_previousinternalvalue, Entities.jmcg_fieldchangehistory);
            config.MaxPreviousValueLength = XrmService.GetMaxLength(Fields.jmcg_fieldchangehistory_.jmcg_previousvalue, Entities.jmcg_fieldchangehistory);
            config.MaxInternalValueLength = XrmService.GetMaxLength(Fields.jmcg_fieldchangehistory_.jmcg_internalvalue, Entities.jmcg_fieldchangehistory);
            config.MaxValueLength = XrmService.GetMaxLength(Fields.jmcg_fieldchangehistory_.jmcg_value, Entities.jmcg_fieldchangehistory);
            config.MaxHistoryNameLength = XrmService.GetMaxLength(Fields.jmcg_fieldchangehistory_.jmcg_name, Entities.jmcg_fieldchangehistory);

            var filterXml = fieldChangeEntity.GetStringField(Fields.jmcg_fieldchangeconfiguration_.jmcg_historyfilter);
            if (!string.IsNullOrWhiteSpace(filterXml))
            {

                var fetchXml = "<fetch distinct=\"true\" no-lock=\"false\" mapping=\"logical\"><entity name=\"" + type + "\">" + filterXml + "</entity></fetch>";
                var response = (Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionResponse)XrmService.Execute(new Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionRequest
                {
                    FetchXml = fetchXml
                });

                config.FilterExpression = response.Query.Criteria;
            }

            return config;
        }

        public IEnumerable<Entity> GetFieldChangeHistoryEvents(Guid targetId)
        {
            var sdkMessageProcessingStepsQuery = XrmService.BuildQuery(Entities.sdkmessageprocessingstep, null, new[]
            {
                new ConditionExpression(Fields.sdkmessageprocessingstep_.description, ConditionOperator.BeginsWith, targetId.ToString())
            });
            var pluginTypeJoin = sdkMessageProcessingStepsQuery.AddLink(Entities.plugintype, Fields.sdkmessageprocessingstep_.plugintypeid, Fields.plugintype_.plugintypeid);
            pluginTypeJoin.LinkCriteria.AddCondition(new ConditionExpression(Fields.plugintype_.typename, ConditionOperator.Equal, PluginQualifiedName));
            var pluginFilterJoin = sdkMessageProcessingStepsQuery.AddLink(Entities.sdkmessagefilter, Fields.sdkmessageprocessingstep_.sdkmessagefilterid, Fields.sdkmessagefilter_.sdkmessagefilterid);
            pluginFilterJoin.EntityAlias = "FILTER";
            pluginFilterJoin.Columns = new ColumnSet(Fields.sdkmessagefilter_.primaryobjecttypecode);
            var pluginFilterMessage = pluginFilterJoin.AddLink(Entities.sdkmessage, Fields.sdkmessagefilter_.sdkmessageid, Fields.sdkmessage_.sdkmessageid);
            pluginFilterMessage.EntityAlias = "MESSAGE";
            pluginFilterMessage.Columns = new ColumnSet(Fields.sdkmessage_.name);
            var sdkMessageProcessingSteps = XrmService.RetrieveAll(sdkMessageProcessingStepsQuery);
            return sdkMessageProcessingSteps;
        }

        public IEnumerable<Entity> GetFieldChangeHistoryEvents()
        {
            var sdkMessageProcessingStepsQuery = XrmService.BuildQuery(Entities.sdkmessageprocessingstep, null, null);
            var pluginTypeJoin = sdkMessageProcessingStepsQuery.AddLink(Entities.plugintype, Fields.sdkmessageprocessingstep_.plugintypeid, Fields.plugintype_.plugintypeid);
            pluginTypeJoin.LinkCriteria.AddCondition(new ConditionExpression(Fields.plugintype_.typename, ConditionOperator.Equal, PluginQualifiedName));
            var pluginFilterJoin = sdkMessageProcessingStepsQuery.AddLink(Entities.sdkmessagefilter, Fields.sdkmessageprocessingstep_.sdkmessagefilterid, Fields.sdkmessagefilter_.sdkmessagefilterid);
            pluginFilterJoin.EntityAlias = "FILTER";
            pluginFilterJoin.Columns = new ColumnSet(Fields.sdkmessagefilter_.primaryobjecttypecode);
            var pluginFilterMessage = pluginFilterJoin.AddLink(Entities.sdkmessage, Fields.sdkmessagefilter_.sdkmessageid, Fields.sdkmessage_.sdkmessageid);
            pluginFilterMessage.EntityAlias = "MESSAGE";
            pluginFilterMessage.Columns = new ColumnSet(Fields.sdkmessage_.name);
            var sdkMessageProcessingSteps = XrmService.RetrieveAll(sdkMessageProcessingStepsQuery);
            return sdkMessageProcessingSteps;
        }

        private string PluginQualifiedName
        {
            get { return "JosephM.Xrm.FieldChangeHistory.Plugins.ProcessFieldChangeHistoryPluginRegistration"; }
        }

        public class SerialisedEntity
        {
            public string LogicalName { get; set; }
            public Guid Id { get; set; }
            public Dictionary<string, object> Attributes { get; set; }
        }
    }
}
