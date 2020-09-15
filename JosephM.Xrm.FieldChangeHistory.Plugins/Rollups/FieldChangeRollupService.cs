using Microsoft.Xrm.Sdk;
using JosephM.Xrm.FieldChangeHistory.Plugins.Xrm;
using Schema;
using System;
using System.Collections.Generic;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.Rollups
{
    public class FieldChangeRollupService : RollupService
    {
        public FieldChangeRollupService(XrmService xrmService)
            : base(xrmService)
        {
        }

        private IEnumerable<LookupRollup> _Rollups = new LookupRollup[]
        {
        };

        public override IEnumerable<LookupRollup> AllRollups => _Rollups;
    }
}