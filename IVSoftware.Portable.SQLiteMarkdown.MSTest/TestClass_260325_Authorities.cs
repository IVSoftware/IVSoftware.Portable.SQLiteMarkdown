using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Util;
using IVSoftware.Portable.SQLiteMarkdown.Util;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_260325_Authorities
{
    class TestableMMDC : ModeledMarkdownContext<SelectableQFModel>
    {
        public TestableMMDC(ObservablePreviewCollection<SelectableQFModel> onp, NetProjectionOption option)
        {
            // base.SetObservableNetProjection(onp, option);
        }
    }

    [TestMethod, DoNotParallelize]
    [Claim("00000000-0000-0000-0000-000000000000")]
    public void Test_CSStoModel()
    {
        string actual, expected;
        var builder = new List<string>();
        using var te = this.TestableEpoch();

        #region I T E M    G E N
        IList<SelectableQFModel>? eph = null;
        // CREATE (no side effects)
        var i1 = eph.AddDynamic("Item01");
        var i2 = eph.AddDynamic("Item02");
        var i3 = eph.AddDynamic("Item03");
        #endregion I T E M    G E N

        var itemsSource = new ObservablePreviewCollection<SelectableQFModel>();
        var mmdc = new TestableMMDC(onp: itemsSource, option: NetProjectionOption.AllowDirectChanges);
        var simView = new PlatformCollectionViewSimulator<SelectableQFModel>(itemsSource);

        #region E V E N T S
        // Differentiate between the itemsSource being driven by
        // the simView and the simView being driven by itemsSource.
        simView.CollectionChanged += (sender, e) =>
        {
            var isProjection = ReferenceEquals(sender, itemsSource);
            if (isProjection)
            {
                builder.Add(e.ToString(true));
            }
            else
            {
                builder.Add(e.ToString(false).Replace(
                    "Other        ",
                    "SimView      "));
            }

        };
        #endregion E V E N T S

        #region S U B T E S T S
        #endregion S U B T E S T S
    }
}
