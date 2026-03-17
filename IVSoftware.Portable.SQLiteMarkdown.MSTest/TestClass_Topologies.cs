using IVSoftware.Portable.SQLiteMarkdown.Common;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_Topologies
    {
        /// <summary>
        /// QueryFilter Router that inherits MMDC
        /// </summary>
        [TestMethod]
        public void Test_InheritObservableCollection()
        {
            var oqf = new InheritObservableCollection.OBQFC<SelectableQFModel>();
        }
        /// <summary>
        /// QueryFilter Router that inherits MMDC
        /// </summary>
        [TestMethod]
        public void Test_InheritMMDC_ObservableOnly()
        {
            var oqf = new InheritModeledMarkdownContext.ObservableOnly.OBQFC<SelectableQFModel>();
        }
        /// <summary>
        /// QueryFilter Router that inherits MMDC
        /// </summary>
        [TestMethod]
        public void Test_InheritMMDC_AllowDirectChanges()
        {
            var oqf = new InheritModeledMarkdownContext.AllowDirectUpdates.OBQFC<SelectableQFModel>();
        }
    }
    namespace InheritObservableCollection
    {
        class OBQFC<T>
            : ObservableCollection<T>
            , INotifyCollectionChanged
            where T : new()
        {
            public OBQFC() { }
            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                base.OnCollectionChanged(e);
            }
        }
    }

    namespace InheritModeledMarkdownContext.ObservableOnly
    {
        class OBQFC<T>
            : ModeledMarkdownContext<T>
            , IEnumerable<T>
            , INotifyCollectionChanged
            where T : new()
        {
            public OBQFC()
            {
            }
            public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }
            public event NotifyCollectionChangedEventHandler? CollectionChanged;

            ModeledMarkdownContext<T> _mmdc = new ModeledMarkdownContext<T>();
        }
    }

    namespace InheritModeledMarkdownContext.AllowDirectUpdates
    {
        class OBQFC<T>
            : ModeledMarkdownContext<T>
            , INotifyCollectionChanged
            where T : new()
        {
            public OBQFC()
            { 
            }
            public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }
            public event NotifyCollectionChangedEventHandler? CollectionChanged;
        }
    }
}
