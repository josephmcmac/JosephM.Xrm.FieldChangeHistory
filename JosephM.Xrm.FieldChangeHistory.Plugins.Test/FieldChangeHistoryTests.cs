using JosephM.Xrm.FieldChangeHistory.Plugins.Xrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.Test
{
    [TestClass]
    public class FieldChangeHistoryTests : FieldChangeXrmTest
    {
        [TestMethod]
        public void FieldChangeHistoryIgnoreNullsTest()
        {
            DeleteAll(Entities.jmcg_fieldchangeconfiguration);
            DeleteAll(Entities.jmcg_fieldchangehistory);
            DeleteAll(Entities.jmcg_testentity);

            var filterXml = "<filter type=\"and\"><condition attribute=\"statecode\" operator=\"eq\" value=\"0\" /><condition attribute=\"jmcg_boolean\" operator=\"eq\" value=\"1\" /></filter>";

            var fieldChangeConfig = CreateTestRecord(Entities.jmcg_fieldchangeconfiguration, new Dictionary<string, object>
                {
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_name, Fields.jmcg_testentity_.jmcg_user },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_entitytype, Entities.jmcg_testentity },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_field, Fields.jmcg_testentity_.jmcg_user },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_lookupfieldfield, Fields.jmcg_testentity_.ownerid },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_historyfilter, filterXml },
                });

            var testEntity = CreateTestRecord(Entities.jmcg_testentity, new Dictionary<string, object>
            {
                { Fields.jmcg_testentity_.jmcg_boolean, true },
            });

            var fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 0);

            testEntity.SetLookupField(Fields.jmcg_testentity_.jmcg_user, OtherAdmin);
            testEntity = UpdateFieldsAndRetreive(testEntity, Fields.jmcg_testentity_.jmcg_user);

            fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 1);
            Assert.IsNull(fieldChangeHistory.First().GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime));

            testEntity.SetField(Fields.jmcg_testentity_.jmcg_user, null);
            testEntity = UpdateFieldsAndRetreive(testEntity, Fields.jmcg_testentity_.jmcg_user);

            fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 1);
            Assert.IsNotNull(fieldChangeHistory.First().GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime));

            testEntity.SetLookupField(Fields.jmcg_testentity_.jmcg_user, OtherAdmin);
            testEntity = UpdateFieldsAndRetreive(testEntity, Fields.jmcg_testentity_.jmcg_user);

            fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 2);
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) != null));
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) == null));
        }

        [TestMethod]
        public void FieldChangeHistoryFilterTest()
        {
            DeleteAll(Entities.jmcg_fieldchangeconfiguration);
            DeleteAll(Entities.jmcg_fieldchangehistory);
            DeleteAll(Entities.jmcg_testentity);

            var filterXml = "<filter type=\"and\"><condition attribute=\"statecode\" operator=\"eq\" value=\"0\" /><condition attribute=\"jmcg_boolean\" operator=\"eq\" value=\"1\" /></filter>";

            var fieldChangeConfig = CreateTestRecord(Entities.jmcg_fieldchangeconfiguration, new Dictionary<string, object>
                {
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_name, Fields.jmcg_testentity_.jmcg_user },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_entitytype, Entities.jmcg_testentity },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_field, Fields.jmcg_testentity_.jmcg_user },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_lookupfieldfield, Fields.jmcg_testentity_.ownerid },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_historyfilter, filterXml },
                });

            var testEntity = CreateTestRecord(Entities.jmcg_testentity, new Dictionary<string, object>
            {
                { Fields.jmcg_testentity_.jmcg_boolean, true },
                { Fields.jmcg_testentity_.jmcg_user, new EntityReference(Entities.systemuser, CurrentUserId) }
            });
            var fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 1);
            Assert.AreEqual(CurrentUserId, fieldChangeHistory.First().GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid));
            Assert.IsNull(fieldChangeHistory.First().GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime));

            testEntity.SetLookupField(Fields.jmcg_testentity_.jmcg_user, OtherAdmin);
            testEntity.SetField(Fields.jmcg_testentity_.jmcg_boolean, false);
            testEntity = UpdateFieldsAndRetreive(testEntity, Fields.jmcg_testentity_.jmcg_user, Fields.jmcg_testentity_.jmcg_boolean);

            fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 1);
            Assert.IsNotNull(fieldChangeHistory.First().GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime));

            testEntity.SetField(Fields.jmcg_testentity_.jmcg_boolean, true);
            testEntity = UpdateFieldsAndRetreive(testEntity, Fields.jmcg_testentity_.jmcg_boolean);

            fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 2);
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == CurrentUserId));
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == OtherAdmin.Id && h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) == null));

            XrmService.SetState(testEntity.LogicalName, testEntity.Id, OptionSets.TestEntity.Status.Inactive);
            fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 2);
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == CurrentUserId));
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == OtherAdmin.Id && h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) != null));

            XrmService.SetState(testEntity.LogicalName, testEntity.Id, OptionSets.TestEntity.Status.Active);
            fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 3);
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == CurrentUserId));
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == OtherAdmin.Id && h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) == null));
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == OtherAdmin.Id && h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) != null));


            testEntity.SetLookupField(Fields.jmcg_testentity_.jmcg_user, CurrentUserId, Entities.systemuser);
            testEntity = UpdateFieldsAndRetreive(testEntity, Fields.jmcg_testentity_.jmcg_user);

            fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 4);
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == CurrentUserId && h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) == null));
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == CurrentUserId && h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) != null));
            Assert.AreEqual(2, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == OtherAdmin.Id && h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) != null));

            XrmService.Delete(testEntity);

            fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 4);
            Assert.AreEqual(2, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == CurrentUserId && h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) != null));
            Assert.AreEqual(2, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid) == OtherAdmin.Id && h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) != null));
        }

        [TestMethod]
        public void FieldChangeHistoryRunPluginAsTest()
        {
            Assert.IsNotNull(TestTeam);
            Assert.IsNotNull(OtherAdmin);

            DeleteAll(Entities.jmcg_fieldchangeconfiguration);
            DeleteAll(Entities.jmcg_fieldchangehistory);

            var fieldChangeConfig = CreateTestRecord(Entities.jmcg_fieldchangeconfiguration, new Dictionary<string, object>
                {
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_name, Fields.team_.description },{ Fields.jmcg_fieldchangeconfiguration_.jmcg_entitytype, Entities.team },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_field, Fields.team_.description },{ Fields.jmcg_fieldchangeconfiguration_.jmcg_changedrecordlookupfield, Fields.jmcg_fieldchangehistory_.ownerid },
                  { Fields.jmcg_fieldchangeconfiguration_.jmcg_runpluginas, OtherAdmin.ToEntityReference() },
                });

            TestTeam.SetField(Fields.team_.description, DateTime.UtcNow.ToString("yyyy-MM-dd hh:ss:mm"));
            TestTeam = UpdateFieldsAndRetreive(TestTeam, Fields.team_.description);

            var fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, TestTeam.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 1);
            Assert.AreEqual(OtherAdmin.Id, fieldChangeHistory.First().GetLookupGuid(Fields.jmcg_fieldchangehistory_.createdby));

            fieldChangeConfig.SetLookupField(Fields.jmcg_fieldchangeconfiguration_.jmcg_runpluginas, CurrentUserId, Entities.systemuser);
            fieldChangeConfig = UpdateFieldsAndRetreive(fieldChangeConfig, Fields.jmcg_fieldchangeconfiguration_.jmcg_runpluginas);

            Thread.Sleep(1000);

            TestTeam.SetField(Fields.team_.description, DateTime.UtcNow.ToString("yyyy-MM-dd hh:ss:mm"));
            TestTeam = UpdateFieldsAndRetreive(TestTeam, Fields.team_.description);

            fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, TestTeam.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 2);
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.createdby) == OtherAdmin.Id));
            Assert.AreEqual(1, fieldChangeHistory.Count(h => h.GetLookupGuid(Fields.jmcg_fieldchangehistory_.createdby) == CurrentUserId));
        }

        [TestMethod]
        public void FieldChangeHistorySetLookupValueTest()
        {
            Assert.IsNotNull(TestTeam);
            Assert.IsNotNull(OtherAdmin);

            DeleteAll(Entities.jmcg_fieldchangeconfiguration);
            DeleteAll(Entities.jmcg_fieldchangehistory);

            if(TestTeam.GetLookupGuid(Fields.team_.administratorid) != CurrentUserId)
            {
                TestTeam.SetLookupField(Fields.team_.administratorid, CurrentUserId, Entities.systemuser);
                TestTeam = UpdateFieldsAndRetreive(TestTeam, Fields.team_.administratorid);
            }

            var fieldChangeConfig = CreateTestRecord(Entities.jmcg_fieldchangeconfiguration, new Dictionary<string, object>
                {
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_name, Fields.team_.administratorid },{ Fields.jmcg_fieldchangeconfiguration_.jmcg_entitytype, Entities.team },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_field, Fields.team_.administratorid },{ Fields.jmcg_fieldchangeconfiguration_.jmcg_changedrecordlookupfield, Fields.jmcg_fieldchangehistory_.ownerid },
                });

            TestTeam.SetLookupField(Fields.team_.administratorid, OtherAdmin);
            TestTeam = UpdateFieldsAndRetreive(TestTeam, Fields.team_.administratorid);

            var fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, TestTeam.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 1);
            Assert.AreEqual(TestTeam.Id, fieldChangeHistory.First().GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid));
        }

        [TestMethod]
        public void FieldChangeHistorySetTargetTest()
        {
            Assert.IsNotNull(TestTeam);
            Assert.IsNotNull(OtherAdmin);

            DeleteAll(Entities.jmcg_fieldchangeconfiguration);
            DeleteAll(Entities.jmcg_fieldchangehistory);

            var fieldChangeConfig = CreateTestRecord(Entities.jmcg_fieldchangeconfiguration, new Dictionary<string, object>
                {
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_name, Fields.team_.description },{ Fields.jmcg_fieldchangeconfiguration_.jmcg_entitytype, Entities.team },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_field, Fields.team_.description },{ Fields.jmcg_fieldchangeconfiguration_.jmcg_changedrecordlookupfield, Fields.jmcg_fieldchangehistory_.ownerid },
                });

            TestTeam.SetField(Fields.team_.description, DateTime.UtcNow.ToString("yyyy-MM-dd hh:ss:mm"));
            TestTeam = UpdateFieldsAndRetreive(TestTeam, Fields.team_.description);

            var fieldChangeHistory = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
{
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, TestTeam.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistory.Count() == 1);
            Assert.AreEqual(TestTeam.Id, fieldChangeHistory.First().GetLookupGuid(Fields.jmcg_fieldchangehistory_.ownerid));
        }

        [TestMethod]
        public void FieldChangeHistoryRefreshRegistrationsTest()
        {
            DeleteAll(Entities.jmcg_fieldchangeconfiguration);
            DeleteAll(Entities.jmcg_fieldchangehistory);
            DeleteAll(Entities.jmcg_testentity);
            DeleteAll(Entities.jmcg_testentitytwo);

            var testConfigs = new[]
{
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_account, TestContactAccount.ToEntityReference(), TestAccount2.ToEntityReference()),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_boolean, true, false),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_date, LocalisationService.ConvertTargetToUtc(new  DateTime(2020,1,1,0,0,0, DateTimeKind.Unspecified)), LocalisationService.ConvertTargetToUtc(new DateTime(2020,2,1,0,0,0, DateTimeKind.Unspecified))),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_decimal, (decimal)1111.1, (decimal)2222.2),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_float, (double)1111.11, (double)22222.22),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_integer, 1, 2),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_money, new Money((decimal)1111.11), new Money((decimal)2222.22)),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_name, "Name 1", "Name 2"),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_picklist, new OptionSetValue(OptionSets.TestEntity.Picklist.Option1), new OptionSetValue(OptionSets.TestEntity.Picklist.Option2)),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_string, "String 1", "String 2"),
                new TestConfiguration(Entities.jmcg_testentitytwo, Fields.jmcg_testentitytwo_.jmcg_name, "Name 1", "Name 2"),
            };

            var configRecords = testConfigs.Select(config =>
            {
                return CreateTestRecord(Entities.jmcg_fieldchangeconfiguration, new Dictionary<string, object>
                {
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_name, config.Field },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_entitytype, config.EntityType },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_field, config.Field },
                });
            }).ToArray();

            var pluginEvents = FieldChangeService.GetFieldChangeHistoryEvents();
            Assert.AreEqual(testConfigs.Count() * 3, pluginEvents.Count());

            var testEntity1 = CreateTestRecord(Entities.jmcg_testentity);
            var testEntity1b = CreateTestRecord(Entities.jmcg_testentity);
            var testEntity2 = CreateTestRecord(Entities.jmcg_testentitytwo);

            var fieldChangeHistories1 = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity1.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistories1.Count() > 0);

            var fieldChangeHistories1b = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity1b.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistories1b.Count() > 0);

            var fieldChangeHistories2 = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
{
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity2.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistories2.Count() > 0);

            XrmService.Delete(configRecords.Last());

            pluginEvents = FieldChangeService.GetFieldChangeHistoryEvents();
            Assert.AreEqual((testConfigs.Count() - 1) * 3, pluginEvents.Count());

            XrmService.SetState(configRecords.First().LogicalName, configRecords.First().Id, OptionSets.FieldChangeConfiguration.Status.Inactive);

            pluginEvents = FieldChangeService.GetFieldChangeHistoryEvents();
            Assert.AreEqual((testConfigs.Count() - 2) * 3, pluginEvents.Count());

            XrmService.SetState(configRecords.First().LogicalName, configRecords.First().Id, OptionSets.FieldChangeConfiguration.Status.Active);

            pluginEvents = FieldChangeService.GetFieldChangeHistoryEvents();
            Assert.AreEqual((testConfigs.Count() - 1) * 3, pluginEvents.Count());
        }

        [TestMethod]
        public void FieldChangeHistoryTest()
        {
            //todo add tests for history generation
            DeleteAll(Entities.jmcg_fieldchangeconfiguration);
            DeleteAll(Entities.jmcg_fieldchangehistory);
            DeleteAll(Entities.jmcg_testentity);

            var testConfigs = new[]
            {
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_account, TestContactAccount.ToEntityReference(), TestAccount2.ToEntityReference()),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_boolean, true, false),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_date, LocalisationService.ConvertTargetToUtc(new  DateTime(2020,1,1,0,0,0, DateTimeKind.Unspecified)), LocalisationService.ConvertTargetToUtc(new DateTime(2020,2,1,0,0,0, DateTimeKind.Unspecified))),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_decimal, (decimal)1111.1, (decimal)2222.2),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_float, (double)1111.11, (double)22222.22),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_integer, 1, 2),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_money, new Money((decimal)1111.11), new Money((decimal)2222.22)),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_name, "Name 1", "Name 2"),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_picklist, new OptionSetValue(OptionSets.TestEntity.Picklist.Option1), new OptionSetValue(OptionSets.TestEntity.Picklist.Option2)),
                new TestConfiguration(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_string, "String 1", "String 2")
            };

            foreach(var config in testConfigs)
            {
                var fieldChangeConfig = CreateTestRecord(Entities.jmcg_fieldchangeconfiguration, new Dictionary<string, object>
                {
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_name, config.Field },{ Fields.jmcg_fieldchangeconfiguration_.jmcg_entitytype, config.EntityType },
                    { Fields.jmcg_fieldchangeconfiguration_.jmcg_field, config.Field },
                });
            }

            var testEntity = new Entity(Entities.jmcg_testentity);
            foreach (var config in testConfigs)
            {
                testEntity.SetField(config.Field, config.Value1);
            }
            testEntity = CreateAndRetrieve(testEntity);

            var fieldChangeHistories = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.AreEqual(testConfigs.Count(), fieldChangeHistories.Count());

            foreach (var config in testConfigs)
            {
                var history = fieldChangeHistories.First(fc => fc.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_changedfieldname) == config.Field);
                Assert.IsNotNull(history.GetField(Fields.jmcg_fieldchangehistory_.jmcg_starttime));
                Assert.IsNull(history.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime));
                Assert.AreEqual(XrmService.GetFieldAsInternalString(config.EntityType, config.Field, config.Value1), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_internalvalue));
                Assert.AreEqual(XrmService.GetFieldAsDisplayString(config.EntityType, config.Field, config.Value1, LocalisationService), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_value));
                if(config.Value1 is EntityReference er)
                {
                    Assert.AreEqual(er.LogicalName, history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_internalvaluelookuptype));
                }
                Assert.AreEqual(testEntity.GetStringField(Fields.jmcg_testentity_.jmcg_name), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordname));
                Assert.AreEqual(CurrentUserId, history.GetLookupGuid(Fields.jmcg_fieldchangehistory_.jmcg_changedby));
                Assert.AreEqual(XrmService.GetEntityDisplayName(config.EntityType), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_changedtypedisplayname));
                Assert.AreEqual(XrmService.GetFieldLabel(config.Field, config.EntityType), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_changedfielddisplayname));
                Assert.AreEqual(config.EntityType, history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_changedtypename));
            }

            foreach (var config in testConfigs)
            {
                testEntity.SetField(config.Field, config.Value2);
            }
            testEntity = UpdateFieldsAndRetreive(testEntity, testConfigs.Select(c => c.Field).ToArray());

            fieldChangeHistories = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.AreEqual(testConfigs.Count() * 2, fieldChangeHistories.Count());

            foreach (var config in testConfigs)
            {
                var previousHistory = fieldChangeHistories.First(fc => fc.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_changedfieldname) == config.Field
                    && fc.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) != null);
                var history = fieldChangeHistories.First(fc => fc.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_changedfieldname) == config.Field
                    && fc.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) == null);

                Assert.IsNotNull(history.GetField(Fields.jmcg_fieldchangehistory_.jmcg_starttime));
                Assert.IsNull(history.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime));
                Assert.AreEqual(XrmService.GetFieldAsInternalString(config.EntityType, config.Field, config.Value1), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_previousinternalvalue));
                Assert.AreEqual(XrmService.GetFieldAsDisplayString(config.EntityType, config.Field, config.Value1, LocalisationService), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_previousvalue));
                if (config.Value1 is EntityReference er)
                {
                    Assert.AreEqual(er.LogicalName, history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_previousinternalvaluelookuptype));
                }
                Assert.AreEqual(XrmService.GetFieldAsInternalString(config.EntityType, config.Field, config.Value2), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_internalvalue));
                Assert.AreEqual(XrmService.GetFieldAsDisplayString(config.EntityType, config.Field, config.Value2, LocalisationService), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_value));
                if (config.Value2 is EntityReference er2)
                {
                    Assert.AreEqual(er2.LogicalName, history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_internalvaluelookuptype));
                }
                Assert.AreEqual(testEntity.GetStringField(Fields.jmcg_testentity_.jmcg_name), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordname));
                Assert.AreEqual(CurrentUserId, history.GetLookupGuid(Fields.jmcg_fieldchangehistory_.jmcg_changedby));
                Assert.AreEqual(XrmService.GetEntityDisplayName(config.EntityType), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_changedtypedisplayname));
                Assert.AreEqual(XrmService.GetFieldLabel(config.Field, config.EntityType), history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_changedfielddisplayname));
                Assert.AreEqual(config.EntityType, history.GetStringField(Fields.jmcg_fieldchangehistory_.jmcg_changedtypename));
            }

            XrmService.Delete(testEntity);
            fieldChangeHistories = XrmService.RetrieveAllAndConditions(Entities.jmcg_fieldchangehistory, new[]
            {
                new ConditionExpression(Fields.jmcg_fieldchangehistory_.jmcg_changedrecordid, ConditionOperator.Equal, testEntity.Id.ToString())
            });
            Assert.IsTrue(fieldChangeHistories.All(h => h.GetField(Fields.jmcg_fieldchangehistory_.jmcg_endtime) != null));
        }

        private void DeleteAll(string entityType)
        {
            var entities = XrmService.RetrieveAllEntityType(entityType);
            foreach (var entity in entities)
            {
                XrmService.Delete(entity);
            }
        }

        public class TestConfiguration
        {
            public TestConfiguration(string entityType, string field, object value1, object value2)
            {
                EntityType = entityType;
                Field = field;
                Value1 = value1;
                Value2 = value2;
            }

            public string EntityType { get; }
            public string Field { get; }
            public object Value1 { get; }
            public object Value2 { get; }
        }
    }
}