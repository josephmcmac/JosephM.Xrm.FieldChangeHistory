using JosephM.Xrm.FieldChangeHistory.Plugins.Core;
using JosephM.Xrm.FieldChangeHistory.Plugins.Services;
using JosephM.Xrm.FieldChangeHistory.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Schema;
using System;
using System.Linq;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.Plugins
{
    public class ProcessFieldChangeHistoryPlugin : FieldChangeEntityPluginBase
    {
        public ProcessFieldChangeHistoryPlugin(FieldChangeConfig config)
        {
            Config = config;
        }

        public FieldChangeConfig Config { get; }

        public override void GoExtention()
        {
            UpdateFieldChangeHistory();
        }

        private void UpdateFieldChangeHistory()
        {
            if (IsMessage(PluginMessage.Create, PluginMessage.Update, PluginMessage.Delete) && IsStage(PluginStage.PostEvent) && IsMode(PluginMode.Synchronous))
            {
                var field = Config.FieldChangeEntity.GetStringField(Fields.jmcg_fieldchangeconfiguration_.jmcg_field);
                var meetsFilterPre =
                    !IsMessage(PluginMessage.Create)
                    && (Config.FilterExpression == null
                    || XrmEntity.MeetsFilter(GetFieldFromPreImage, Config.FilterExpression));
                var meetsFilterNow =
                    !IsMessage(PluginMessage.Delete)
                    && (Config.FilterExpression == null
                    || XrmEntity.MeetsFilter(GetField, Config.FilterExpression));
                var utcNow = DateTime.UtcNow;
                var timeToSetOnOwnerHistory = utcNow.AddMilliseconds(-1 * utcNow.Millisecond);

                var closeOld = meetsFilterPre && (FieldChanging(field) || !meetsFilterNow) && GetFieldFromPreImage(field) != null;
                var createNew = meetsFilterNow && (FieldChanging(field) || !meetsFilterPre) && GetField(field) != null;
                if (closeOld)
                {
                    var existingHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
                    {
                        new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedfieldname, ConditionOperator.Equal, field),
                        new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedtypename, ConditionOperator.Equal, TargetType),
                        new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, TargetId.ToString()),
                        new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_endtime, ConditionOperator.Null),
                    });
                    if (existingHistory.Any())
                    {
                        foreach(var existing in existingHistory)
                        {
                            existing.SetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime, timeToSetOnOwnerHistory);
                            XrmService.Update(existing, Fields.jmcg_fieldchangehistory_.jmcg_endtime);
                        }
                    }
                }
                if (createNew)
                {
                    var newOwnerHistory = new Entity(Entities.jmcg_fieldchangehistory);
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_starttime, timeToSetOnOwnerHistory);
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_changedtypename, TargetType);
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_changedtypedisplayname, Config.TypeDisplayName);
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, TargetId.ToString());
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_changedfieldname, field);
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_changedfielddisplayname, Config.FieldDisplayName);
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_changedby, GetField("modifiedby"));
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_previousvalue, XrmService.GetFieldAsDisplayString(TargetType, field, GetFieldFromPreImage(field), LocalisationService).Left(Config.MaxPreviousValueLength));
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_previousinternalvalue, XrmService.GetFieldAsInternalString(TargetType, field, GetFieldFromPreImage(field)).Left(Config.MaxPreviousInternalValueLength));
                    if (GetFieldFromPreImage(field) is EntityReference erPrevious)
                    {
                        newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_previousinternalvaluelookuptype, erPrevious.LogicalName);
                    }
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_value, XrmService.GetFieldAsDisplayString(TargetType, field, GetField(field), LocalisationService).Left(Config.MaxValueLength));
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_internalvalue, XrmService.GetFieldAsInternalString(TargetType, field, GetField(field)).Left(Config.MaxInternalValueLength));
                    if (GetField(field) is EntityReference er)
                    {
                        newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_internalvaluelookuptype, er.LogicalName);
                    }
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordname, GetStringField(Config.TypePrimaryField).Left(Config.MaxNameLength));
                    newOwnerHistory.SetField(Fields.jmcg_fieldchangehistory_.jmcg_name, $"{newOwnerHistory.GetField(Fields.jmcg_fieldchangehistory_.jmcg_changedtypedisplayname)} {newOwnerHistory.GetField(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordname)} changed {newOwnerHistory.GetField(Fields.jmcg_fieldchangehistory_.jmcg_changedfielddisplayname)} to {newOwnerHistory.GetField(Fields.jmcg_fieldchangehistory_.jmcg_value)}".Left(Config.MaxHistoryNameLength));

                    var fieldForTarget = Config.FieldChangeEntity.GetStringField(Fields.jmcg_fieldchangeconfiguration_.jmcg_changedrecordlookupfield);
                    if (!string.IsNullOrWhiteSpace(fieldForTarget))
                    {
                        newOwnerHistory.SetLookupField(fieldForTarget, TargetEntity);
                    }

                    var fieldForLookup = Config.FieldChangeEntity.GetStringField(Fields.jmcg_fieldchangeconfiguration_.jmcg_lookupfieldfield);
                    if (!string.IsNullOrWhiteSpace(fieldForLookup))
                    {
                        newOwnerHistory.SetField(fieldForLookup, GetField(field));
                    }

                    XrmService.Create(newOwnerHistory);
                }
            }
        }
    }
}