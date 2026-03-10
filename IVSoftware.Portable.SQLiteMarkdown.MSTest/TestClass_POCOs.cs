using IVSoftware.Portable.SQLiteMarkdown.Common;
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
        [TestMethod, Ignore]
        public async Task Test_ItemsSource()
        {
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
                { }
                actual = pmdc.Model.ToString();
                actual.ToClipboardExpected();
                { }
                expected = @"not-empty";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting: FILTER MODE => ALWAYS TRACKS."
                );
            }
            #endregion S U B T E S T S
        }
    }
}
