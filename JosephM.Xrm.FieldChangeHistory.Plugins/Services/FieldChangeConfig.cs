using Microsoft.Xrm.Sdk;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.Services
{
    public class FieldChangeConfig
    {
        public Entity FieldChangeEntity { get; set; }
        public string TypeDisplayName { get; set; }
        public string FieldDisplayName { get; set; }
        public string TypePrimaryField { get; set; }
        public int MaxNameLength { get; set; }
        public int MaxPreviousInternalValueLength { get; set; }
        public int MaxPreviousValueLength { get; set; }
        public int MaxValueLength { get; set; }
        public int MaxInternalValueLength { get; set; }
        public int MaxHistoryNameLength { get; set; }
    }
}
