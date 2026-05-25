using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_260525_OQFC_OQFS
{
    [TestMethod, DoNotParallelize]
    public void Test_Revert()
    {
        string actual, expected;
        using var te = this.TestableEpoch();

        IModeledCollection<SelectableQFModel>[] omcs =
        [
            new ObservableQueryFilterCollection<SelectableQFModel>(),
            // new ObservableQueryFilterSource<SelectableQFModel>(),
        ];
        foreach (var omc in omcs)
        {
            localRevert(omc);
        }

        void localRevert(IModeledCollection<SelectableQFModel> omc)
        {
            IObservableQueryFilterSource<SelectableQFModel>
                oqfs = (IObservableQueryFilterSource<SelectableQFModel>)omc;
            Assert.AreEqual(
                QueryFilterConfig.QueryAndFilter,
                oqfs.QueryFilterConfig,
                "Expecting valid cast to IOQFS and default config.");
            omc.PopulateForDemo(10);
        }
    }
}
