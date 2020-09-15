using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Xrm.FieldChangeHistory.Plugins.Test
{
    //this class just for general debug purposes
    [TestClass]
    public class DebugTests : FieldChangeXrmTest
    {
        [TestMethod]
        public void Debug()
        {
            var me = XrmService.WhoAmI();
        }
    }
}