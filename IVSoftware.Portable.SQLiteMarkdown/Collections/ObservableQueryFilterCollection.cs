using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
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
        [Canonical("The parameterless CTor is the only CTor.")]
        public ObservableQueryFilterCollection()
        {
            Model.SetBoundAttributeValue(
                MarkdownContext,
                StdModelAttribute.mdc,
                "[MDC]");

            Model.SortAttributes<StdModelAttribute>();
        }

        private void MDCPropertyChangedForwarder(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        protected override void OnModelTrackingChanged()
        {
            base.OnModelTrackingChanged();
            MarkdownContext.FilterQueryDatabase = FilterQueryDatabase!;
        }

        /// <summary>
        /// Protected class with public properties.
        /// </summary>
        protected class MarkdownContextProtected : MarkdownContext<T>
        {
            ObservableQueryFilterCollection<T> @this => (ObservableQueryFilterCollection<T>)ModelAuthorityContext!;
            /// <summary>
            /// Used by LoadCanon to attribute recordset count.
            /// </summary>
            public new SearchEntryState SearchEntryState
            {
                get => base.SearchEntryState;
                set => base.SearchEntryState = value;
            }
            protected override void OnClear(bool all)
            {
                base.OnClear(all);
                if(all)
                {
                    // The one-and-only clear authority,
                    @this.ClearItems();
                }
            }
            public new SQLiteConnection FilterQueryDatabase
            {
                get => base.FilterQueryDatabase;
                set => base.FilterQueryDatabase = value;
            }
        }
        protected override void OnMDEXFinalizing(FinalDisposeEventArgs eUnk)
        {
            if(HasAuthority(StdModelAuthority.TerminalClear))
            {
                // #TNT
            }
            base.OnMDEXFinalizing(eUnk);
        }
        protected MarkdownContextProtected MarkdownContext
        {
            get
            {
                if (_markdownContext is null)
                {
                    _markdownContext = new()
                    {
                        ModelAuthorityContext = this
                    };
                    // Property changed forwarder
                    MarkdownContext.PropertyChanged += MDCPropertyChangedForwarder;
                }
                return _markdownContext;
            }
        }
        MarkdownContextProtected? _markdownContext = null;

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
        public new void Clear() => Clear(true);

        public FilteringState Clear(bool all)
        {
            if (!HasAuthority(ModelDataExchangeAuthority.Collection))
            {
                using (RequestAuthority(StdModelAuthority.TerminalClear))
                using (RequestAuthority(ModelDataExchangeAuthority.Collection))
                {
                    if (!HasAuthority(ModelDataExchangeAuthority.Model))
                    {
                        return MarkdownContext.Clear(all);
                    }
                }
            }
            return FilteringState;
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

        public TaskAwaiter<TaskStatus> GetAwaiter() => MarkdownContext.GetAwaiter();
        public new void LoadCanon(IList<T> items)
        {
            base.LoadCanon(items);
            MarkdownContext.SearchEntryState =
                Count == 0
                ? SearchEntryState.QueryCompleteNoResults
                : SearchEntryState.QueryCompleteWithResults;
        }

        public new async Task LoadCanonAsync(IList<T> items)
        {
            this.ThrowHard<NotSupportedException>();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);            
        }
    }
}
