using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    [Policy(typeof(AffinityException))]
    public enum SQLiteMarkdownPolicy
    {
        [Description("ProxyTableMapping Policy violation: Explicit [Table] mappings must not conflict with base classes.")]
        [PolicyEnforcement(ThrowOrAdvise.ThrowHard)]
        ProxyTableMapping,

        [Description("EmptyFilterString Policy violation: Semantically empty IME must route to Canon directly.")]
        [PolicyEnforcement(ThrowOrAdvise.ThrowHard)]
        EmptyFilterString,
    }
    public class SQLiteMarkdownException : Exception
    {
        public SQLiteMarkdownException(string message) : base(message) { }
    }
}
