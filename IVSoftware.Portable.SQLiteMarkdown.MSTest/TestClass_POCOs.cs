using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.WinOS.MSTest.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                ObservableNetProjection = new ObservableCollection<SelectableQFModel>(),
                QueryFilterConfig = QueryFilterConfig.Filter,
            };
            pmdc.ObservableNetProjection.CollectionChanged += (sender, e) =>
            {
            };

            var items = 
                (IList<SelectableQFModel>)
                pmdc.ObservableNetProjection;


            subtest_TrackAdd();

            #region S U B T E S T S
            void subtest_TrackAdd()
            {
                items.AddDynamic("Brown Dog", "[canine] [color]", false, new() { "loyal", "friend", "furry" });

                actual = pmdc.Model.ToString();
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model autocount=""1"" count=""1"" matches=""1"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" sort=""0"" />
</model>"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting: FILTER MODE => ALWAYS TRACKS."
                );

                items.AddDynamic("Green Apple", "[fruit] [color]", false, new() { "tart", "snack", "healthy" });
                actual = pmdc.Model.ToString();
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model autocount=""2"" count=""2"" matches=""2"">
  <xitem text=""312d1c21-0000-0000-0000-000000000000"" model=""[SelectableQFModel]"" sort=""0"" />
  <xitem text=""312d1c21-0000-0000-0000-000000000001"" model=""[SelectableQFModel]"" sort=""1"" />
</model>"
                ;
            }
            #endregion S U B T E S T S
        }
    }
}
