using IVSoftware.Portable.SQLiteMarkdown.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_POCOs
    {

        [TestMethod]
        public async Task Test_ItemsSource()
        {
            string actual, expected;

            // IOC - Construct inline then pull.
            var pmdc = new PredicateMarkdownContext<SelectableQFModel>
            {
                ObservableNetProjection = new ObservableCollection<SelectableQFModel>(),
                QueryFilterConfig = QueryFilterConfig.Filter,
            };
            IList items = (IList)pmdc.ObservableNetProjection;


            subtest_TrackAdd();

            #region S U B T E S T S
            void subtest_TrackAdd()
            {

            }
            #endregion S U B T E S T S
        }
    }
}
