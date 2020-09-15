using System;
using JosephM.Xrm.FieldChangeHistory.Plugins.Xrm;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.SharePoint
{
    public class FieldChangeSharePointSettings : ISharePointSettings
    {
        public FieldChangeSharePointSettings(XrmService xrmService)
        {
            XrmService = xrmService;
        }

        private string _username;
        public string UserName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private XrmService XrmService { get; }
    }
}
