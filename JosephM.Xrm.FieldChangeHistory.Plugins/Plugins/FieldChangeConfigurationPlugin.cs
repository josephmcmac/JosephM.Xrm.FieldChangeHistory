using JosephM.Xrm.FieldChangeHistory.Plugins.Xrm;
using Schema;
using System.Linq;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.Plugins
{
    public class FieldChangeConfigurationPlugin : FieldChangeEntityPluginBase
    {
        public override void GoExtention()
        {
            RefreshPluginRegistration();
        }

        private void RefreshPluginRegistration()
        {
            if (IsMessage(PluginMessage.Create, PluginMessage.Update, PluginMessage.Delete) && IsStage(PluginStage.PostEvent) && IsMode(PluginMode.Synchronous))
            {
                var refreshSdkMessageProcessingSteps = IsMessage(PluginMessage.Delete)
                    || ConfigFieldChanging();
                if (refreshSdkMessageProcessingSteps)
                {
                    var isActive = IsMessage(PluginMessage.Create)
                        || GetOptionSet(Fields.jmcg_fieldchangeconfiguration_.statecode) == OptionSets.FieldChangeConfiguration.Status.Active;
                    FieldChangeService.RefreshPluginRegistrations(TargetId, isActive);
                }
            }
        }

        private bool ConfigFieldChanging()
        {
            return TargetEntity
                .Attributes
                .Keys
                .Where(k => k == Fields.jmcg_fieldchangeconfiguration_.statecode
                            || k.StartsWith("jmcg"))
                .Any(FieldChanging);
        }
    }
}