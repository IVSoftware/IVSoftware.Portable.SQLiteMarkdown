using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public abstract class PredicateMarkdownContext 
        : MarkdownContext
        , IPredicateMarkdownContext
    {
        public PredicateMarkdownContext(Type type) : base(type) { }
        public IReadOnlyDictionary<string, Enum> ActiveFilters
        {
            get
            {
                if (_activeFilters is null)
                {
                    _activeFilters = new ReadOnlyDictionary<string, Enum>(ActiveFiltersProtected!);
                }
                return _activeFilters;
            }
        }
        IReadOnlyDictionary<string, Enum>? _activeFilters = null;

        [Careful("Don't draw inferences from changes in the collection itself.")]
        protected TolerantDictionary<string, Enum> ActiveFiltersProtected
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
                            case NotifyCollectionChangingAction.Add:

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
                                Debug.Assert(DateTime.Now.Date == new DateTime(2026, 3, 9).Date, "Don't forget disabled");
#if false
                                IsFiltering = 
                                    MarkdownContext.FilteringState == FilteringState.Active 
                                    || ActiveFilters.Count > 0;
#endif
                                if (IsFiltering)
                                {
                                    MarkdownContext.StartOrRestart();
                                }
                                else
                                {   /* G T K */
                                }
                                _activeFilters = new ReadOnlyDictionary<string, Enum>(_activeFiltersProtected!);
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

        public void ActivateFilters(Enum stdPredicate, params Enum[] more)
        {
            throw new NotImplementedException();
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

        public void DeactivateFilters(Enum stdPredicate, params Enum[] more)
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
        : MarkdownContext
        where T : class, new()
    {
        public PredicateMarkdownContext() : base(typeof(T)) { }
    }
}
