using IVSoftware.Portable.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Util
{
    /// <summary>
    /// Testable MarkdownContext for SelectableQFModel
    /// </summary>
    /// <remarks>
    /// For testing, the purpose of this adapter is to expose internal properties and methods as public.
    /// </remarks>
    class TMMDC : MarkdownContext<SelectableQFModel>
    {
        public TMMDC(ObservableCollection<SelectableQFModel> onp, NetProjectionTopology? option = null)
        {
            SetObservableNetProjection(onp, option);
        }

        private void SetObservableNetProjection(ObservableCollection<SelectableQFModel> onp, NetProjectionTopology? option)
        {
            throw new NotImplementedException();
        }
    }
}
