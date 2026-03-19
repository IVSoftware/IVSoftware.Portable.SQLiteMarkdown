using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    partial class Topology<T>
    {
        public Topology(XElement model, ObservableCollection<T>? projection = null)
        {
            Model = model;
            if (Model.To<IModeledMarkdownContext>() is { } mmdc)
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
        public XElement Model { get; }

        public bool IsFiltering
        {
            get => _isFiltering;
            set
            {
                if (!Equals(_isFiltering, value))
                {
                    if (value)
                    {
                        if (_authorityToken is null)
                        {
                            _isFiltering = true;
                            _authorityToken = MMDC.BeginCollectionChangeAuthority(CollectionChangeAuthority.Model);
                        }
                        else
                        {
                            this.ThrowHard<InvalidOperationException>(
                                "Filtering state invariant violated: token already present on enter.");
                        }
                    }
                    else
                    {
                        if (_authorityToken is null)
                        {
                            this.ThrowHard<InvalidOperationException>(
                                "Filtering state invariant violated: token missing on exit.");
                        }
                        else
                        {
                            var tmp = _authorityToken;
                            _authorityToken = null;
                            tmp.Dispose();
                            _isFiltering = false;
                        }
                    }
                }
            }
        }
        bool _isFiltering = false;
        IDisposable? _authorityToken = null;


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
                    _canonicalSupersetProtected = new AuthoritativeObservableCollection<T>(()=>MMDC.Authority);
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
    }
}
