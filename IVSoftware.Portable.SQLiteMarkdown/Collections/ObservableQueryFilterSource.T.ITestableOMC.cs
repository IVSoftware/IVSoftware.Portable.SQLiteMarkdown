using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Disposable;
using SQLite;
using System;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    partial class ObservableQueryFilterSource<T>
    {
        internal ITestableOMC TestableOMCImpl
        {
            get
            {
                if (_testableMDCImpl is null)
                {
                    _testableMDCImpl = new(this);
                }
                return _testableMDCImpl;
            }
        }
        _TestableOMCImpl_? _testableMDCImpl = null;

        private class _TestableOMCImpl_ : ITestableOMC
        {
            public _TestableOMCImpl_(ObservableQueryFilterSource<T> @this)
            {
                this.@this = @this;
            }
            ObservableQueryFilterSource<T> @this;
            public bool HasFQDB => @this.FilterQueryDatabase is SQLiteConnection;

            public IAuthorityEpochProvider ModelDataExchangeAuthorityProvider => 
                @this
                .CanonicalSupersetProtected
                .AsInterface<ITestableOMC>()!
                .ModelDataExchangeAuthorityProvider;
        }

        /// <summary>
        /// Returns a 'hidden' interface implementation if available.
        /// </summary>
        /// <remarks>
        /// Intended for a call site that employs pattern matching.
        /// </remarks>
        public new TInterface? AsInterface<TInterface>() where TInterface : class =>
            typeof(TInterface) switch
            {
                Type t when t == typeof(ITestableOMC) => (TInterface)TestableOMCImpl,
                _ => base.AsInterface<TInterface>(),
            };
    }
}
