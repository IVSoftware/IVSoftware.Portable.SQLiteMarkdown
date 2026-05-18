using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    class ObservableQueryFilterCollection<T>
        : ObservableModeledCollection<T>
        , IObservableQueryFilterSource<T>
        , IModelAuthorityContext
    {
        public ObservableQueryFilterCollection()
        {
            Model.SetBoundAttributeValue(
                MarkdownContext = new MarkdownContext<T>(),
                StdModelAttribute.mdc,
                "[MDC]");
            MarkdownContext.ModelAuthorityContext = this;
            Model.SortAttributes<StdModelAttribute>();
        }

        protected MarkdownContext<T> MarkdownContext { get; }

        public Type ContractType
            => MarkdownContext.ContractType;

        public Type ProxyType 
            => MarkdownContext.ProxyType;

        public string InputText 
        { 
            get => MarkdownContext.InputText;
            set => MarkdownContext.InputText = value;
        }

        public bool IsFiltering 
            => MarkdownContext.IsFiltering;

        public FilteringState FilteringState
            => MarkdownContext.FilteringState;

        public SearchEntryState SearchEntryState
            => MarkdownContext.SearchEntryState;

        public QueryFilterConfig QueryFilterConfig
        { 
            get => MarkdownContext.QueryFilterConfig;
            set => MarkdownContext.QueryFilterConfig = value;
        }
        public uint DefaultLimit
        {
            get => MarkdownContext.DefaultLimit; 
            set => MarkdownContext.DefaultLimit = value; 
        }

        public TimeSpan InputTextSettlingTime
        {
            get => MarkdownContext.InputTextSettlingTime; 
            set => MarkdownContext.InputTextSettlingTime = value;
        }

        public bool Busy => MarkdownContext.Busy;

        public int CanonicalCount => MarkdownContext.CanonicalCount;

        public int PredicateMatchCount => MarkdownContext.PredicateMatchCount;

        public IModelAuthorityContext? ModelAuthorityContext => MarkdownContext.ModelAuthorityContext;

        public DisposableHost DHostBusy => MarkdownContext.DHostBusy;

        public string Placeholder
        {
            get => _placeholder;
            set
            {
                if (!Equals(_placeholder, value))
                {
                    _placeholder = value;
                    OnPropertyChanged();
                }
            }
        }
        string _placeholder = string.Empty;

        public string Title
        {
            get => _title;
            set
            {
                if (!Equals(_title, value))
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }
        string _title = string.Empty;

        public string SQL => MarkdownContext.Query;

        public SQLiteConnection MemoryDatabase 
        { 
            get => MarkdownContext.MemoryDatabase;
            set => MarkdownContext.MemoryDatabase = value; 
        }

        public event EventHandler? InputTextSettled
        {
            add => MarkdownContext.InputTextSettled += value;

            remove => MarkdownContext.InputTextSettled -= value;
        }

        public event EventHandler<ItemPropertyChangedEventArgs>? ItemPropertyChanged;

        /// <summary>
        /// No Surprises IList.Clear
        /// </summary>
        public new void Clear()
        {
            Clear(all: true);
            using (RequestAuthority(StdModelAuthority.SuspendForwardPropertyChange))
            {
                InputText = string.Empty;
            }
        }
        void IList.Clear() => Clear();

        public string ParseSqlMarkdown()
        {
            return MarkdownContext.ParseSqlMarkdown();
        }

        public string ParseSqlMarkdown(string expr, Type proxyType, QueryFilterMode qfMode, out XElement xast)
        {
            return MarkdownContext.ParseSqlMarkdown(expr, proxyType, qfMode, out xast);
        }

        public string ParseSqlMarkdown<T1>()
        {
            return MarkdownContext.ParseSqlMarkdown<T1>();
        }

        public string ParseSqlMarkdown<T1>(string expr, QueryFilterMode qfMode = QueryFilterMode.Query)
        {
            return MarkdownContext.ParseSqlMarkdown<T1>(expr, qfMode);
        }

        public void Commit()
        {
            MarkdownContext.Commit();
        }

        public FilteringState Clear(bool all)
        {
            return MarkdownContext.Clear(all);
        }

        public IDisposable BeginBusy()
        {
            return MarkdownContext.BeginBusy();
        }

        public string[] GetTableNames()
        {
            return MarkdownContext.GetTableNames();
        }

        public void InitializeFilterOnlyMode(IEnumerable<T> items)
        {
            QueryFilterConfig = QueryFilterConfig.Filter;
            LoadCanon(items.ToList());
        }

        public void ReplaceItems(IEnumerable<T> items) => LoadCanon(items.ToList());

        public Task ReplaceItemsAsync(IEnumerable<T> items) => LoadCanonAsync(items.ToList());

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        public TaskAwaiter<TaskStatus> GetAwaiter() => MarkdownContext.GetAwaiter();
    }
}
