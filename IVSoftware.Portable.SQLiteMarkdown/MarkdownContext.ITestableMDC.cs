using IVSoftware.Portable.Collections;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class MarkdownContext
    {
        internal ITestableMDC TestableMDCImpl
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
        _TestableMDCImpl_? _testableMDCImpl = null;

        private class _TestableMDCImpl_ : ITestableMDC
        {
            public _TestableMDCImpl_(MarkdownContext @this)
            {
                this.@this = @this;
            }
            MarkdownContext @this;
            public bool HasFQDB => @this._filterQueryDatabase is SQLiteConnection;
        }
    }
}
