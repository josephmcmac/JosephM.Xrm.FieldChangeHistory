using System.Collections.Generic;
using JosephM.Xrm.FieldChangeHistory.Plugins.Xrm;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.SharePoint
{
    public class FieldChangeSharepointService : SharePointService
    {
        public FieldChangeSharepointService(XrmService xrmService, ISharePointSettings sharepointSettings)
            : base(sharepointSettings, xrmService)
        {
        }

        public override IEnumerable<SharepointFolderConfig> SharepointFolderConfigs
        {
            get
            {

                return new SharepointFolderConfig[]
                {
                };
            }
        }
    }
}
