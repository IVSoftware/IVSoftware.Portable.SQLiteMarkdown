using IVSoftware.Portable.Disposable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public class PredicateMarkdownContext 
        : MarkdownContext
        , IPredicateMarkdownContext
    {
        public PredicateMarkdownContext(Type type) : base(type) { }

        public IReadOnlyDictionary<string, Enum> ActiveFilters => throw new NotImplementedException();

        public int UnfilteredCount => throw new NotImplementedException();

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

        public IEnumerable Recordset { set => throw new NotImplementedException(); }
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

        public IDisposable BeginUIAction()
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
    {
        public PredicateMarkdownContext() : base(typeof(T)) { }
    }
}
