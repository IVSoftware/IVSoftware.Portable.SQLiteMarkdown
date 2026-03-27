using IVSoftware.Portable.Common.Attributes;
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
        [TestMethod, Probationary("While I think through what 'Topology' means.")]
        public void Test_Topologies()
        {
            subtest_InheritObservableCollection();
            subtest_InheritMMDC_ObservableOnly();
            subtest_InheritMMDC_AllowDirectChanges();

            #region S U B T E S T S
            void subtest_InheritObservableCollection()
            {
                var oqf = new InheritObservableCollection.OBQFC<SelectableQFModel>();
            }
            void subtest_InheritMMDC_ObservableOnly()
            {
                var oqf = new InheritModeledMarkdownContext.ObservableOnly.OBQFC<SelectableQFModel>();
            }

            void subtest_InheritMMDC_AllowDirectChanges()
            {
                var oqf = new InheritModeledMarkdownContext.AllowDirectUpdates.OBQFC<SelectableQFModel>();
            }
            

            #endregion S U B T E S T S
        }
    }
    namespace InheritObservableCollection
    {
        class OBQFC<T>
            : ObservableCollection<T>
            , INotifyCollectionChanged
            where T : new()
        {
            public OBQFC()
            {
                _mmdc.SetObservableNetProjection(this, NetProjectionTopology.AllowDirectChanges);
            }
            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                base.OnCollectionChanged(e);
            }

            ModeledMarkdownContext<T> _mmdc = new ModeledMarkdownContext<T>();
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
                ProjectionTopology = NetProjectionTopology.AllowDirectChanges;
            }
            public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }
            public event NotifyCollectionChangedEventHandler? CollectionChanged;
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
                ProjectionTopology = NetProjectionTopology.AllowDirectChanges;
            }
            public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }
            public event NotifyCollectionChangedEventHandler? CollectionChanged;
        }
    }
}
