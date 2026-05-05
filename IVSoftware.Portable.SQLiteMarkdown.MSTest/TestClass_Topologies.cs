using IVSoftware.Portable.Collections;
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
                var oqf = new InheritMarkdownContext.ObservableOnly.OBQFC<SelectableQFModel>();
            }

            void subtest_InheritMMDC_AllowDirectChanges()
            {
                var oqf = new InheritMarkdownContext.AllowDirectUpdates.OBQFC<SelectableQFModel>();
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
            }
            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                base.OnCollectionChanged(e);
            }

        }
    }

    namespace InheritMarkdownContext.ObservableOnly
    {
        class OBQFC<T>
            : MarkdownContext<T>
            , IEnumerable<T>
            , INotifyCollectionChanged
            where T : new()
        {
            public OBQFC()
            {
                ProjectionTopology = NetProjectionTopology.AllowDirectChanges;
            }

            public NetProjectionTopology ProjectionTopology { get; }

            public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }

            public IEnumerator<T> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public event NotifyCollectionChangedEventHandler? CollectionChanged;
        }
    }

    namespace InheritMarkdownContext.AllowDirectUpdates
    {
        class OBQFC<T>
            : MarkdownContext<T>
            , INotifyCollectionChanged
            where T : new()
        {
            public OBQFC()
            {
                ProjectionTopology = NetProjectionTopology.AllowDirectChanges;
            }
            
            public NetProjectionTopology ProjectionTopology { get; }

            public virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }
            public event NotifyCollectionChangedEventHandler? CollectionChanged;
        }
    }
}
