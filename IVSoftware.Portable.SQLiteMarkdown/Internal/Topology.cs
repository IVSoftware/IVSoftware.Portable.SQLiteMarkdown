using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Common.Exceptions;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    class Topology<T>
    {
        public Topology(XElement model, ObservableCollection<T>? projection = null)
        {
            Model = model;
            if(Model.To<IModeledMarkdownContext>() is { } mmdc)
            {
                MMDC = mmdc;
                mmdc.PropertyChanged += (sender, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(IsFiltering):
                            IsFiltering = mmdc.IsFiltering;
                            break;
                    }
                };
            }
            else
            {
                this.ThrowHard<ArgumentException>($"The {nameof(model)} argument requires a bound MMDC.");
            }
        }
        IModeledMarkdownContext MMDC { get; } = null!; // Guarded in CTor
        public bool IsFiltering { get; set; }

        [JsonIgnore]
        public IReadOnlyList<T> Read =>
            IsFiltering
            ? PredicateMatchSubset
            : CanonicalSuperset;

        [JsonIgnore]
        public IList Write 
            => CanonicalSupersetProtected;


        /// <summary>
        /// If present, this external IList must be kept in sync with the net filtered result.
        /// </summary>
        /// <remarks>
        /// To eliminate churn, user may inherit from <see cref="AuthoritativeObservableCollection{T}AuthoritativeObservableCollection"/>
        /// </remarks>
        public ObservableCollection<T>? ObservableNetCollection { get; }

        public IReadOnlyList<T> CanonicalSuperset
        {
            get
            {
                if (_canonicalSuperset is null)
                {
                    _canonicalSuperset = new ReadOnlyCollection<T>(CanonicalSupersetProtected);
                }
                return _canonicalSuperset;
            }
        }
        IReadOnlyList<T> _canonicalSuperset = null!;

        protected AuthoritativeObservableCollection<T> CanonicalSupersetProtected
        {
            get
            {
                if (_canonicalSupersetProtected is null)
                {
                    _canonicalSupersetProtected = new AuthoritativeObservableCollection<T>(MMDC);
                }
                return _canonicalSupersetProtected;
            }
        }
        AuthoritativeObservableCollection<T> _canonicalSupersetProtected = null!;

        public IReadOnlyList<T> PredicateMatchSubset
        {
            get
            {
                if (_predicateMatchSubset is null)
                {
                    _predicateMatchSubset = new ReadOnlyCollection<T>(PredicateMatchSubsetProtected);
                }
                return _predicateMatchSubset;
            }
        }
        IReadOnlyList<T> _predicateMatchSubset = null!;
        protected List<T> PredicateMatchSubsetProtected { get; } = new();

        public bool Contains(T value)
        {
            if (value?.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
            {
                return PlacerResult.Exists == Model.Place(full, PlacerMode.FindOrPartial);
            }
            else
            {
                return false;
            }
        }

        public int IndexOf(T value)
            => Read is IList list ? list.IndexOf(value) : -1;

        public object GetAt(int index)
            => ((IList)Read)[index];

        public XElement Model { get; }

        #region P O L I C Y    A R B I T R A T I O N
        public int Count => Read.Count;

        [JsonIgnore]
        public bool IsSynchronized => Write.IsSynchronized;

        [JsonIgnore]
        public object SyncRoot => Write.SyncRoot;

        [JsonIgnore]
        public bool IsFixedSize { get; internal set; }

        [JsonIgnore]
        public bool IsReadOnly { get; internal set; }

        public void CopyTo(T[] array, int index)
        {
            for (int i = 0; i < Read.Count; i++)
            {
                array[index + i] = Read[i];
            }
        }
        #endregion P O L I C Y    A R B I T R A T I O N
    }
}
