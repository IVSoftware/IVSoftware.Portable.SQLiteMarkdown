using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.Collections;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    /// <summary>
    /// Test class for Plain Old Collection Objects (POCO).
    /// </summary>
    /// <remarks>
    /// Thanks to Plain Old CLR Objects (POCO) for loaning us their acronym.
    /// </remarks>
    [TestClass]
    public class TestClass_POCOs
    {
        [TestMethod, DoNotParallelize]
        public async Task Test_ItemsSource()
        {
            using var te = this.TestableEpoch();

            string actual, expected;

            // IOC - Construct inline then pull.
            var pmdc = new PredicateMarkdownContext<SelectableQFModel>
            {
                QueryFilterConfig = QueryFilterConfig.Filter,
            };
            pmdc.SetObservableNetProjection(new ObservableCollection<SelectableQFModel>());

            var items = 
                (IList<SelectableQFModel>)
                pmdc.ObservableNetProjection!;


            subtest_TrackAdd();

            #region S U B T E S T S
            void subtest_TrackAdd()
            {
                items.AddDynamic("Brown Dog", "[canine][color]", false, new() { "loyal", "friend", "furry" });

                actual = pmdc.Model.ToString();
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model mdc=""[MDC]"" histo=""[model:1 match:0 qmatch:0 pmatch:0 live:0]"" filters=""[No Active Filters]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" />
</model>"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting: FILTER MODE => ALWAYS TRACKS."
                );

                items.AddDynamic("Green Apple", "[fruit][color]", false, new() { "tart", "snack", "healthy" });
                actual = pmdc.Model.ToString();
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model mdc=""[MDC]"" histo=""[model:2 match:0 qmatch:0 pmatch:0 live:0]"" filters=""[No Active Filters]"">
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" order=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" order=""1"" />
</model>"
                ;
            }
            #endregion S U B T E S T S
        }
    }
}
