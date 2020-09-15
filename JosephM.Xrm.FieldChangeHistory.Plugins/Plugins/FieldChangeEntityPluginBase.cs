using JosephM.Xrm.FieldChangeHistory.Plugins.Services;
using JosephM.Xrm.FieldChangeHistory.Plugins.Rollups;
using JosephM.Xrm.FieldChangeHistory.Plugins.Xrm;
using JosephM.Xrm.FieldChangeHistory.Plugins.SharePoint;
using JosephM.Xrm.FieldChangeHistory.Plugins.Localisation;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.Plugins
{
    /// <summary>
    /// class for shared services or settings objects for plugins
    /// </summary>
    public abstract class FieldChangeEntityPluginBase : XrmEntityPlugin
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

        private FieldChangeRollupService _RollupService;
        public FieldChangeRollupService FieldChangeRollupService
        {
            get
            {
                if (_RollupService == null)
                    _RollupService = new FieldChangeRollupService(XrmService);
                return _RollupService;
            }
        }

        private FieldChangeSharepointService _sharePointService;
        public FieldChangeSharepointService FieldChangeSharepointService
        {
            get
            {
                if (_sharePointService == null)
                    _sharePointService = new FieldChangeSharepointService(XrmService, new FieldChangeSharePointSettings(XrmService));
                return _sharePointService;
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
