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
    public class PredicateMarkdownContext<T> 
        : ModeledMarkdownContext<T>
        , IPredicateMarkdownContext
        where T : new()
    {
        public IReadOnlyDictionary<string, Enum> ActiveFilters => ActivePredicatesProtected.AsReadOnly;

        [Careful("Don't draw inferences from changes in the collection itself.")]
        TolerantDictionary<string, Enum> ActivePredicatesProtected
        {
            get
            {
                if (_activePredicatesProtected is null)
                {
                    _activePredicatesProtected = new TolerantDictionary<string, Enum>();
                    _activePredicatesProtected.CollectionChanging += (sender, e) =>
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
                    _activePredicatesProtected.CollectionChanged += (sender, e) =>
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            case NotifyCollectionChangedAction.Remove:
                            case NotifyCollectionChangedAction.Reset:
                                Debug.Assert(DateTime.Now.Date == new DateTime(2026, 3, 14).Date, "Don't forget disabled");
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
                return _activePredicatesProtected;
            }
        }
        TolerantDictionary<string, Enum>? _activePredicatesProtected = null;

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
                    ActivePredicatesProtected[propertyName] = filter;
                }
            }
        }
        public void DeactivateFilters(Enum filter, params Enum[] moreFilters)
        {
            string binding, predicate;
            if (filter.TryGetWhereAttribute(out binding, out predicate, @throw: true))
            {
                // Retrieve the current property-bound predicate...
                if (ActivePredicatesProtected[binding] is { } found)
                {
                    // ... but don't remove it unless it's a MATCH for the remove request.
                    if (Equals(found, filter))
                    {
                        ActivePredicatesProtected.Remove(binding);
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
                    if (ActivePredicatesProtected[binding] is { } found && Equals(found, more))
                    {
                        ActivePredicatesProtected.Remove(binding);
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

        private DisposableHost? _dhostAtomic = null;

        public void ClearFilters(bool clearInputText = true)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginPredicateAtom() => DHostAtomic.GetToken();

        public void ActivatePredicates(Enum stdPredicate, params Enum[] more)
        {
        }

        public void DeactivatePredicates(Enum stdPredicate, params Enum[] more)
        {
        }

        public void ClearPredicates(bool clearInputText = true) => ActivePredicatesProtected.Clear();
    }
}
