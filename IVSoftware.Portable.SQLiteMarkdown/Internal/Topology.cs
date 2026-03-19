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
                BeginCollectionChangeAuthority = (authority) => mmdc.BeginCollectionChangeAuthority(authority);
                GetAuthority = () => MMDC.Authority;
            }
            else
            {
                this.ThrowHard<ArgumentException>($"The {nameof(model)} argument requires a bound MMDC.");
            }
        }
        public XElement Model { get; }

        #region N U L L    G U A R D E D    I N    C T O R
        IModeledMarkdownContext MMDC { get; } = null!;

        protected Func<CollectionChangeAuthority, IDisposable> BeginCollectionChangeAuthority { get; } = null!;

        protected Func<CollectionChangeAuthority> GetAuthority { get; } = null!;
        #endregion N U L L    G U A R D E D    I N    C T O R

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

        protected IReadOnlyList<T> Read =>
            IsFiltering
            ? PredicateMatchSubset
            : CanonicalSuperset;


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
