using JosephM.Xrm.FieldChangeHistory.Plugins.Core;
using JosephM.Xrm.FieldChangeHistory.Plugins.Localisation;
using JosephM.Xrm.FieldChangeHistory.Plugins.Plugins;
using JosephM.Xrm.FieldChangeHistory.Plugins.Services;
using JosephM.Xrm.FieldChangeHistory.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace JosephM.Xrm.FieldChangeHistory.Plugins
{
    /// <summary>
    /// This is the class for registering plugins in CRM
    /// Each entity plugin type needs to be instantiated in the CreateEntityPlugin method
    /// </summary>
    public class ProcessFieldChangeHistoryPluginRegistration : XrmPluginRegistration
    {
        private readonly string _unsecureConfiguration;
        public ProcessFieldChangeHistoryPluginRegistration(string unsecure)
        {
            _unsecureConfiguration = unsecure;
        }
        public FieldChangeConfig Configs { get; private set; }

        private bool _loadedConfig;

        private object _lockObject = new object();

        public override XrmPlugin CreateEntityPlugin(string entityType, bool isRelationship, IServiceProvider serviceProvider)
        {
            lock (_lockObject)
            {
                if (!_loadedConfig)
                {
                    var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                    var xrmService = new XrmService(factory.CreateOrganizationService(context.UserId), new LogController());
                    var calculatedService = new FieldChangeService(xrmService, new FieldChangeSettings(xrmService), new LocalisationService(new LocalisationSettings(xrmService)));

                    var loadedToConfigs = calculatedService.LoadCalculatedFieldConfig(calculatedService.DeserialiseEntity(_unsecureConfiguration));

                    Configs = loadedToConfigs;

                    _loadedConfig = true;
                }
            }
            return new ProcessFieldChangeHistoryPlugin(Configs);
        }
    }
}
