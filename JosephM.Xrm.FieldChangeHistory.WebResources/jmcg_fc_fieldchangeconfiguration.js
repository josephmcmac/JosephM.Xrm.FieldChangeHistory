/// <reference path="jmcg_fc_pageutility.js" />
/// <reference path="jmcg_fc_webserviceutility.js" />

ChangeConfigJs = new Object();
ChangeConfigJs.options = {
};


ChangeConfigJs.RunOnLoad = function () {
    fieldChangePageUtility.CommonForm(ChangeConfigJs.RunOnChange, ChangeConfigJs.RunOnSave);

    ChangeConfigJs.PopulateTypeLists(["jmcg_entitytypeselectionfield"]);
    ChangeConfigJs.AddFieldSelectionPicklist("jmcg_entitytype", "jmcg_fieldselectionfield", "jmcg_field", null);
    ChangeConfigJs.AddFieldSelectionPicklistStaticType("jmcg_fieldchangehistory", "jmcg_changedrecordlookupfieldselectionfield", "jmcg_changedrecordlookupfield", ["Lookup"]);
    ChangeConfigJs.AddFieldSelectionPicklistStaticType("jmcg_fieldchangehistory", "jmcg_lookupfieldfieldselectionfield", "jmcg_lookupfieldfield", ["Lookup", "Customer", "Owner"]);
};

ChangeConfigJs.RunOnChange = function (fieldName) {
    switch (fieldName) {
        case "jmcg_entitytypeselectionfield":
            ChangeConfigJs.SetEntitySelection("jmcg_entitytypeselectionfield", "jmcg_entitytype");
            break;
    }
};

ChangeConfigJs.RunOnSave = function () {
};

ChangeConfigJs.FieldLists = new Array();
ChangeConfigJs.AddFieldSelectionPicklist = function (entityField, fieldSelectionField, targetField, validTypes) {
    var selectionConfig = {
        entityField: entityField,
        fieldSelectionField: fieldSelectionField,
        targetField: targetField,
        validTypes: validTypes,
        loadSelectionsFunction: function () {
            var entityType = fieldChangePageUtility.GetFieldValue(entityField);
            var processResults = function (results) {
                var newArray = new Array();
                var ignoreFields = [];
                for (var j = 0; j < results.length; j++) {
                    if (validTypes == null || fieldChangePageUtility.ArrayContains(validTypes, results[j].FieldType)
                        && !fieldChangePageUtility.ArrayContains(ignoreFields, results[j].LogicalName)
                        && results[j].Createable == true) {
                        newArray.push(results[j]);
                    }
                }
                ChangeConfigJs.FieldLists[fieldSelectionField] = newArray;
                var fieldOptions = new Array();
                fieldOptions.push(new fieldChangePageUtility.PicklistOption(0, "Select to change the field below"));
                for (var i = 1; i <= ChangeConfigJs.FieldLists[fieldSelectionField].length; i++) {
                    fieldOptions.push(new fieldChangePageUtility.PicklistOption(i, ChangeConfigJs.FieldLists[fieldSelectionField][i - 1]["DisplayName"]));
                }
                fieldChangePageUtility.SetPicklistOptions(fieldSelectionField, fieldOptions);
                fieldChangePageUtility.SetFieldValue(fieldSelectionField, 0);
            };
            if (entityType != null) {
                fieldChangeServiceUtility.GetFieldMetadata(entityType, processResults);
            }
        },
        applySelected: function () {
            var selectedoption = Xrm.Page.getAttribute(fieldSelectionField).getSelectedOption();
            if (selectedoption != null && parseInt(selectedoption.value) != 0) {
                var value = selectedoption.value;
                var selectedField = ChangeConfigJs.FieldLists[fieldSelectionField][parseInt(value) - 1];
                var selectedFieldName = selectedField["LogicalName"];
                fieldChangePageUtility.SetFieldValue(targetField, selectedFieldName);
                fieldChangePageUtility.SetFieldValue(fieldSelectionField, 0);
            }
        },
        setFieldEnabled: function () {
            var entityFieldPopulated = fieldChangePageUtility.GetFieldValue(entityField) != null
                && fieldChangePageUtility.GetFieldValue(entityField) != "";
            fieldChangePageUtility.SetFieldDisabled(fieldSelectionField, !entityFieldPopulated);
        }
    };

    selectionConfig.setFieldEnabled();
    selectionConfig.loadSelectionsFunction();
    fieldChangePageUtility.AddOnChange(entityField, selectionConfig.setFieldEnabled);
    fieldChangePageUtility.AddOnChange(entityField, selectionConfig.loadSelectionsFunction);
    fieldChangePageUtility.AddOnChange(fieldSelectionField, selectionConfig.applySelected);
};
ChangeConfigJs.AddFieldSelectionPicklistStaticType = function (entityType, fieldSelectionField, targetField, validTypes) {
    var selectionConfig = {
        entityType: entityType,
        fieldSelectionField: fieldSelectionField,
        targetField: targetField,
        validTypes: validTypes,
        loadSelectionsFunction: function () {
            var processResults = function (results) {
                var newArray = new Array();
                var ignoreFields = [];
                for (var j = 0; j < results.length; j++) {
                    if (validTypes == null || fieldChangePageUtility.ArrayContains(validTypes, results[j].FieldType)
                        && !fieldChangePageUtility.ArrayContains(ignoreFields, results[j].LogicalName)
                        && results[j].Createable == true) {
                        newArray.push(results[j]);
                    }
                }
                ChangeConfigJs.FieldLists[fieldSelectionField] = newArray;
                var fieldOptions = new Array();
                fieldOptions.push(new fieldChangePageUtility.PicklistOption(0, "Select to change the field below"));
                for (var i = 1; i <= ChangeConfigJs.FieldLists[fieldSelectionField].length; i++) {
                    fieldOptions.push(new fieldChangePageUtility.PicklistOption(i, ChangeConfigJs.FieldLists[fieldSelectionField][i - 1]["DisplayName"]));
                }
                fieldChangePageUtility.SetPicklistOptions(fieldSelectionField, fieldOptions);
                fieldChangePageUtility.SetFieldValue(fieldSelectionField, 0);
            };
            if (entityType != null) {
                fieldChangeServiceUtility.GetFieldMetadata(entityType, processResults);
            }
        },
        applySelected: function () {
            var selectedoption = Xrm.Page.getAttribute(fieldSelectionField).getSelectedOption();
            if (selectedoption != null && parseInt(selectedoption.value) != 0) {
                var value = selectedoption.value;
                var selectedField = ChangeConfigJs.FieldLists[fieldSelectionField][parseInt(value) - 1];
                var selectedFieldName = selectedField["LogicalName"];
                fieldChangePageUtility.SetFieldValue(targetField, selectedFieldName);
                fieldChangePageUtility.SetFieldValue(fieldSelectionField, 0);
            }
        }
    };

    selectionConfig.loadSelectionsFunction();
    fieldChangePageUtility.AddOnChange(fieldSelectionField, selectionConfig.applySelected);
};

ChangeConfigJs.EntityTypes = null;
ChangeConfigJs.PopulateTypeLists = function (fields) {
    var compare = function (a, b) {
        if (a.DisplayName < b.DisplayName)
            return -1;
        if (a.DisplayName > b.DisplayName)
            return 1;
        return 0;
    };

    var processResults = function (results) {
        results.sort(compare);
        ChangeConfigJs.EntityTypes = results;
        var entityOptions = new Array();
        entityOptions.push(new fieldChangePageUtility.PicklistOption(0, "Select to change the selected entity type"));
        for (var i = 1; i <= results.length; i++) {
            entityOptions.push(new fieldChangePageUtility.PicklistOption(i, results[i - 1]["DisplayName"]));
        }
        for (var j = 0; j <= fields.length; j++) {
            fieldChangePageUtility.SetPicklistOptions(fields[j], entityOptions);
            fieldChangePageUtility.SetFieldValue(fields[j], 0);
        }
    };
    fieldChangeServiceUtility.GetAllEntityMetadata(processResults);
};

ChangeConfigJs.SetEntitySelection = function (selectionField, entityField) {
    var selectedoption = Xrm.Page.getAttribute(selectionField).getSelectedOption();
    if (selectedoption != null && parseInt(selectedoption.value) != 0) {
        var value = selectedoption.value;
        var selectedEntity = ChangeConfigJs.EntityTypes[parseInt(value) - 1];
        var selectedEntityName = selectedEntity["LogicalName"];
        fieldChangePageUtility.SetFieldValue(entityField, selectedEntityName);
        fieldChangePageUtility.SetFieldValue(selectionField, 0);
    }
};