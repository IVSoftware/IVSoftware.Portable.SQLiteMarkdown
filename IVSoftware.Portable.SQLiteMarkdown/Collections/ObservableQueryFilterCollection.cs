using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    class ObservableQueryFilterCollection<T>
        : ObservableModeledCollection<T>
        , IObservableQueryFilterSource<T>
    {
        protected MarkdownContext MarkdownContext { get; } = new MarkdownContext<T>();

        public Type ContractType => ((IMarkdownContext)MarkdownContext).ContractType;

        public Type ProxyType => ((IMarkdownContext)MarkdownContext).ProxyType;

        public string InputText { get => ((IMarkdownContext)MarkdownContext).InputText; set => ((IMarkdownContext)MarkdownContext).InputText = value; }

        public bool IsFiltering => ((IMarkdownContext)MarkdownContext).IsFiltering;

        public FilteringState FilteringState => ((IMarkdownContext)MarkdownContext).FilteringState;

        public SearchEntryState SearchEntryState => ((IMarkdownContext)MarkdownContext).SearchEntryState;

        public QueryFilterConfig QueryFilterConfig { get => ((IMarkdownContext)MarkdownContext).QueryFilterConfig; set => ((IMarkdownContext)MarkdownContext).QueryFilterConfig = value; }
        public uint DefaultLimit { get => ((IMarkdownContext)MarkdownContext).DefaultLimit; set => ((IMarkdownContext)MarkdownContext).DefaultLimit = value; }
        public TimeSpan InputTextSettlingTime { get => ((IMarkdownContext)MarkdownContext).InputTextSettlingTime; set => ((IMarkdownContext)MarkdownContext).InputTextSettlingTime = value; }

        public bool Busy => ((IMarkdownContext)MarkdownContext).Busy;

        public int CanonicalCount => ((IMarkdownContext)MarkdownContext).CanonicalCount;

        public int PredicateMatchCount => ((IMarkdownContext)MarkdownContext).PredicateMatchCount;

        public IModelAuthorityContext ModelAuthorityContext => ((IMarkdownContext)MarkdownContext).ModelAuthorityContext;

        public DisposableHost DHostBusy => throw new NotImplementedException();

        public string Placeholder => throw new NotImplementedException();

        public string Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string SQL => throw new NotImplementedException();

        public SQLiteConnection MemoryDatabase { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler? InputTextSettled
        {
            add
            {
                ((IMarkdownContext)MarkdownContext).InputTextSettled += value;
            }

            remove
            {
                ((IMarkdownContext)MarkdownContext).InputTextSettled -= value;
            }
        }

        public event EventHandler<ItemPropertyChangedEventArgs>? ItemPropertyChanged;

        public string ParseSqlMarkdown()
        {
            return ((IMarkdownContext)MarkdownContext).ParseSqlMarkdown();
        }

        public string ParseSqlMarkdown(string expr, Type proxyType, QueryFilterMode qfMode, out XElement xast)
        {
            return ((IMarkdownContext)MarkdownContext).ParseSqlMarkdown(expr, proxyType, qfMode, out xast);
        }

        public string ParseSqlMarkdown<T1>()
        {
            return ((IMarkdownContext)MarkdownContext).ParseSqlMarkdown<T1>();
        }

        public string ParseSqlMarkdown<T1>(string expr, QueryFilterMode qfMode = QueryFilterMode.Query)
        {
            return ((IMarkdownContext)MarkdownContext).ParseSqlMarkdown<T1>(expr, qfMode);
        }

        public void Commit()
        {
            ((IMarkdownContext)MarkdownContext).Commit();
        }

        public FilteringState Clear(bool all)
        {
            return ((IMarkdownContext)MarkdownContext).Clear(all);
        }

        public IDisposable BeginBusy()
        {
            return ((IMarkdownContext)MarkdownContext).BeginBusy();
        }

        public string[] GetTableNames()
        {
            return ((IMarkdownContext)MarkdownContext).GetTableNames();
        }

        public void InitializeFilterOnlyMode(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public void ReplaceItems(IEnumerable<T> items) => LoadCanon(items.ToList());

        public Task ReplaceItemsAsync(IEnumerable<T> items) => LoadCanonAsync(items.ToList());
    }
}
