using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.Xrm
{
    public class XrmOrganizationServiceFactory
    {
        public IOrganizationService GetOrganisationService(IXrmConfiguration xrmConfiguration)
        {
            if (!xrmConfiguration.UseXrmToolingConnector)
            {
                return XrmConnection.GetOrgServiceProxy(xrmConfiguration);
            }
            else
            {
                throw new NotSupportedException("Tooling Conenction Not Supported In This Project");
            }
        }
    }
}