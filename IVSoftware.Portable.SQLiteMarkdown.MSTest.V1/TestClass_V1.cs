using IVSoftware.Portable.SQLiteMarkdown.Common;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.V1
{
    [TestClass]
    public sealed class TestClass_V1
    {
        /// <summary>
        /// This is more of a reference than a test. We're looking for 
        /// confirmation of what was and wasn't visible in v1.
        /// </summary>
        [TestMethod]
        public void Test_Capabilities()
        {
            MarkdownContext<SelectableQFModel> mdc = new();
            var cnx = mdc.MemoryDatabase;


            var ct = mdc.ContractType;
        }
    }
}
