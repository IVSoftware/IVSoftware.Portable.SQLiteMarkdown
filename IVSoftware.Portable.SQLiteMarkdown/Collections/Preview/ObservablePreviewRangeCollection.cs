using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Collections.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class ObservablePreviewRangeCollection<T>
        : ObservableRangeCollection<T>
        , IRangeable
        where T : new()
    {
        public ObservablePreviewRangeCollection() { }

        /// <summary>
        /// PROMOTE the protected BC version for public INotifyPropertyChanging contract.
        /// </summary>
        public new CollectionChangingEventingPolicy CollectionChangingEventingPolicy
        {
            get => base.CollectionChangingEventingPolicy;
            set => base.CollectionChangingEventingPolicy = value;
        }

        protected virtual void OnModelEpochFinalizing(ModelEpochDisposeEventArgs e)
        {
            OnCollectionChanging(e.Digest);
            base.OnModelEpochFinalizing(e);
        }

        /// <summary>
        /// Determine the highest fidelity full path for T.
        /// </summary>
        public ModeledFullPathInfo ModelingCapabilityInfo
        {
            get
            {
                if (_modelingCapability is null)
                {
                    _modelingCapability = typeof(T).GetModeledPathInfo();
                }
                return _modelingCapability!;
            }
        }

        ModeledFullPathInfo? _modelingCapability = null;
        PropertyInfo? _fullPathPI = null;
    }
}
