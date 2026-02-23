using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_AffinityField
    {

        [TestMethod]
        public void Test_TestableEpoch()
        {
            string actual, expected;

            using var local = this.TestableEpoch();

            SelectableQFModel item;

            item = new();
            { }
            Assert.AreEqual(AffinityTestableEpoch.GuidReset.ToString(), item.Id);
        }


        [TestMethod]
        public void Test_AffinityFieldModel101()
        {
            string actual, expected;
        }
    }
}
