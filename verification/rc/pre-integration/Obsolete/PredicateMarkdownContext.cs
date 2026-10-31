using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using IVSoftware.Portable.Collections.Tracking;
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject;

namespace IVSoftware.Portable.SQLiteMarkdown.Obsolete
{
    public class PredicateMarkdownContext<T> 
        : MarkdownContext<T>
        , IPredicateMarkdownContext
        where T : new()
    {

        [Careful("Don't draw inferences from change events in the collection itself.")]
        TolerantDictionaryInternal<string, Enum> ActivePredicatesProtected
        {
            get
            {
                if (_activePredicatesProtected is null)
                {
                    _activePredicatesProtected = new TolerantDictionaryInternal<string, Enum>();
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
                                Debug.Assert(DateTime.Now.Date == new DateTime(2026, 3, 27).Date, "Don't forget disabled");
#if false
                                IsFiltering = 
                                    MarkdownContext.FilteringState == FilteringState.Active 
                                    || ActivePredicates.Count > 0;
#endif
                                if (IsFiltering)
                                {
                                    StartOrRestart();
                                }
                                else
                                {   /* G T K */
                                }
                                Predicates =
                                    ActivePredicates.Values
                                    .Select(_ => _.GetCustomAttribute<WhereAttribute>()?.Expr)
                                    .Where(_ => !string.IsNullOrWhiteSpace(_))
                                    .Select(_ => $"({_})")
                                    .ToArray();// Add parentheses out of an abundance of paranoia.
                                OnPropertyChanged(nameof(ActivePredicates));
                                break;
                        }
                    };
                }
                return _activePredicatesProtected;
            }
        }
        TolerantDictionaryInternal<string, Enum>? _activePredicatesProtected = null;

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

        public void ActivatePredicates(Enum filter, params Enum[] moreFilters)
        {
            foreach (var member in new[] { filter }.Concat(moreFilters))
            {
                if (filter.GetCustomAttribute<WhereAttribute>()?.Binding is { } propertyName && !string.IsNullOrWhiteSpace(propertyName))
                {
                    ActivePredicatesProtected[propertyName] = filter;
                }
            }
        }
        public void DeactivatePredicates(Enum filter, params Enum[] moreFilters)
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

        public IDisposable BeginPredicateAtom() => DHostAtomic.GetToken();

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

        public IReadOnlyDictionary<string, Enum> ActivePredicates => throw new NotImplementedException();
        
#if true || MIGRATING
        public object Model 
        {
            get => throw new NotImplementedException("ToDo");
            internal set => throw new NotImplementedException("ToDo");
        }
#else
#endif

        public event NotifyCollectionChangedEventHandler? ModelChanged;

        private DisposableHost? _dhostAtomic = null;

        public void ClearPredicates(bool clearInputText = true)
        {
            throw new NotImplementedException();
        }
    }
}
