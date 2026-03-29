using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Util
{
    /// <summary>
    /// Testable ModeledMarkdownContext for SelectableQFModel
    /// </summary>
    /// <remarks>
    /// For testing, the purpose of this adapter is to expose internal properties and methods as public.
    /// </remarks>
    class TMMDC : ModeledMarkdownContext<SelectableQFModel>
    {
        public TMMDC(ObservableCollection<SelectableQFModel> onp, NetProjectionTopology? option = null)
        {
            SetObservableNetProjection(onp, option);
        }
    }
}
