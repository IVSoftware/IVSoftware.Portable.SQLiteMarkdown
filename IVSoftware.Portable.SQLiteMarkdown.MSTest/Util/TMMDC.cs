using IVSoftware.Portable.SQLiteMarkdown.Common;
using System.Collections.ObjectModel;

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
