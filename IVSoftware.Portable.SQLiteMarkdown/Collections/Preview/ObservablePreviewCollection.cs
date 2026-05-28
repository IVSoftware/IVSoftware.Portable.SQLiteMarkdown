using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Collections.Internal;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.Collections
{
    /// <summary>
    /// Suppressible collection with Preview semantics (but no Range semantics).
    /// </summary>
    [Careful(
        "The CollectionChanging is protected; " +
        "This SAF object *does not* expose INotifyCollectionChanging")]
    internal partial class ObservablePreviewCollection<T> 
        : ObservableModeledCollection<T>
        , INotifyCollectionChanging
    {
        public CollectionChangingEventingPolicy CollectionChangingEventingPolicy
        { 
            get => NotifyCollectionChangingImpl.CollectionChangingEventingPolicy; 
            set => NotifyCollectionChangingImpl.CollectionChangingEventingPolicy = value;
        }

        public event NotifyCollectionChangingEventHandler? CollectionChanging
        {
            add => NotifyCollectionChangingImpl.CollectionChanging += value;
            remove => NotifyCollectionChangingImpl.CollectionChanging -= value;
        }
    }
}
