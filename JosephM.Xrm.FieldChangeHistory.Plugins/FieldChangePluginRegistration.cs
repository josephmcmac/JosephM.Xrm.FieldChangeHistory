using JosephM.Xrm.FieldChangeHistory.Plugins.Plugins;
using JosephM.Xrm.FieldChangeHistory.Plugins.Xrm;
using Schema;
using System;

namespace JosephM.Xrm.FieldChangeHistory.Plugins
{
    /// <summary>
    /// This is the class for registering plugins in CRM
    /// Each entity plugin type needs to be instantiated in the CreateEntityPlugin method
    /// </summary>
    public class FieldChangePluginRegistration : XrmPluginRegistration
    {
        public override XrmPlugin CreateEntityPlugin(string entityType, bool isRelationship, IServiceProvider serviceProvider)
        {
            switch (entityType)
            {
                case Entities.jmcg_fieldchangeconfiguration: return new FieldChangeConfigurationPlugin();
            }
            return null;
        }
    }
}
