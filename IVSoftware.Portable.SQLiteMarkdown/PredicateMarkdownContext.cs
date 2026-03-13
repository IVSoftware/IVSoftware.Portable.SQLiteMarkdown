using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public abstract class PredicateMarkdownContext 
        : MarkdownContext
        , IPredicateMarkdownContext
    {
        public PredicateMarkdownContext(Type type) : base(type) { }
        public IReadOnlyDictionary<string, Enum> ActiveFilters => ActiveFiltersProtected.AsReadOnly;

        [Careful("Don't draw inferences from changes in the collection itself.")]
        TolerantDictionary<string, Enum> ActiveFiltersProtected
        {
            get
            {
                if (_activeFiltersProtected is null)
                {
                    _activeFiltersProtected = new TolerantDictionary<string, Enum>();
                    _activeFiltersProtected.CollectionChanging += (sender, e) =>
                    {
                        switch (e.Action)
                        {
                            case CollectionChangingAction.Add:

#if false
                                IsFiltering = true;
#endif
                                break;
                        }
                    };
                    _activeFiltersProtected.CollectionChanged += (sender, e) =>
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            case NotifyCollectionChangedAction.Remove:
                            case NotifyCollectionChangedAction.Reset:
                                Debug.Assert(DateTime.Now.Date == new DateTime(2026, 3, 11).Date, "Don't forget disabled");
#if false
                                IsFiltering = 
                                    MarkdownContext.FilteringState == FilteringState.Active 
                                    || ActiveFilters.Count > 0;
#endif
                                if (IsFiltering)
                                {
                                    StartOrRestart();
                                }
                                else
                                {   /* G T K */
                                }
                                Predicates =
                                    ActiveFilters.Values
                                    .Select(_ => _.GetCustomAttribute<WhereAttribute>()?.Expr)
                                    .Where(_ => !string.IsNullOrWhiteSpace(_))
                                    .Select(_ => $"({_})")
                                    .ToArray();// Add parentheses out of an abundance of paranoia.
                                OnPropertyChanged(nameof(ActiveFilters));
                                break;
                        }
                    };
                }
                return _activeFiltersProtected;
            }
        }
        TolerantDictionary<string, Enum>? _activeFiltersProtected = null;

        [Obsolete]
        public string[] Predicates
        {
            get => _predicates;
            set
            {
                value ??= [];
                if (!Equals(_predicates, value))
                {
                    _predicates = value;
                    OnPropertyChanged();
                }
            }
        }
        string[] _predicates = [];

        public void ActivateFilters(Enum filter, params Enum[] moreFilters)
        {
            foreach (var member in new[] { filter }.Concat(moreFilters))
            {
                if (filter.GetCustomAttribute<WhereAttribute>()?.Binding is { } propertyName && !string.IsNullOrWhiteSpace(propertyName))
                {
                    ActiveFiltersProtected[propertyName] = filter;
                }
            }
        }
        public void DeactivateFilters(Enum filter, params Enum[] moreFilters)
        {
            string binding, predicate;
            if (filter.TryGetWhereAttribute(out binding, out predicate, @throw: true))
            {
                // Retrieve the current property-bound predicate...
                if (ActiveFiltersProtected[binding] is { } found)
                {
                    // ... but don't remove it unless it's a MATCH for the remove request.
                    if (Equals(found, filter))
                    {
                        ActiveFiltersProtected.Remove(binding);
                    }
                    else
                    {   /* G T K */
                        // This was a BUGIRL. Fixed now.
                    }
                }
            }
            foreach (var more in moreFilters)
            {
                if (more.TryGetWhereAttribute(out binding, out predicate, @throw: true))
                {
                    if (ActiveFiltersProtected[binding] is { } found && Equals(found, more))
                    {
                        ActiveFiltersProtected.Remove(binding);
                    }
                }
            }
        }

        public IDisposable BeginFilterAtom() => DHostAtomic.GetToken();

        protected DisposableHost DHostAtomic
        {
            get
            {
                if (_dhostAtomic is null)
                {
                    _dhostAtomic = new DisposableHost();
                }
                return _dhostAtomic;
            }
        }
        public INotifyCollectionChanged ItemsSource { set => throw new NotImplementedException(); }

        private DisposableHost? _dhostAtomic = null;

        public void ClearFilters(bool clearInputText = true)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginPredicateAtom()
        {
            throw new NotImplementedException();
        }

        public void ActivatePredicates(Enum stdPredicate, params Enum[] more)
        {
            throw new NotImplementedException();
        }

        public void DeactivatePredicates(Enum stdPredicate, params Enum[] more)
        {
            throw new NotImplementedException();
        }

        public void ClearPredicates(bool clearInputText = true)
        {
            throw new NotImplementedException();
        }
    }
    public class PredicateMarkdownContext<T> 
        : MarkdownContext<T>
        where T : class, new()
    {
        public PredicateMarkdownContext() { }
    }
}
