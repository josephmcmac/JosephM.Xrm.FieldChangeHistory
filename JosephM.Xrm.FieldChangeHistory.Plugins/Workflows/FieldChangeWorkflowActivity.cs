using JosephM.Xrm.FieldChangeHistory.Plugins.Localisation;
using JosephM.Xrm.FieldChangeHistory.Plugins.Services;
using JosephM.Xrm.FieldChangeHistory.Plugins.Xrm;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.Workflows
{
    /// <summary>
    /// class for shared services or settings objects for workflow activities
    /// </summary>
    public abstract class FieldChangeWorkflowActivity<T> : XrmWorkflowActivityInstance<T>
        where T : XrmWorkflowActivityRegistration
    {
        private FieldChangeSettings _settings;
        public FieldChangeSettings FieldChangeSettings
        {
            get
            {
                if (_settings == null)
                    _settings = new FieldChangeSettings(XrmService);
                return _settings;
            }
        }

        private FieldChangeService _service;
        public FieldChangeService FieldChangeService
        {
            get
            {
                if (_service == null)
                    _service = new FieldChangeService(XrmService, FieldChangeSettings, LocalisationService);
                return _service;
            }
        }

        private LocalisationService _localisationService;
        public LocalisationService LocalisationService
        {
            get
            {
                if (_localisationService == null)
                    _localisationService = new LocalisationService(new LocalisationSettings(XrmService));
                return _localisationService;
            }
        }
    }
}
