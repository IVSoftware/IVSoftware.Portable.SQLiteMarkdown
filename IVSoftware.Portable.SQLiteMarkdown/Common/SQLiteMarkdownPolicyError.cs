using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    [Policy(typeof(SQLiteMarkdownException))]
    public enum SQLiteMarkdownPolicyViolation
    {
        [Description($"{nameof(SQLiteOperationFailed)} Policy violation: The SQLite operation is expected to succeed.")]
        [PolicyEnforcement(ThrowOrAdvise.ThrowHard)]
        SQLiteOperationFailed,

        [Description($"{nameof(ProxyTableMappingConflict)} Policy violation: Explicit [Table] mappings must not conflict with base classes.")]
        [PolicyEnforcement(ThrowOrAdvise.ThrowHard)]
        ProxyTableMappingConflict,

        [Description($"{nameof(EmptyFilterString)} Policy violation: Semantically empty IME must route to Canon directly.")]
        [PolicyEnforcement(ThrowOrAdvise.ThrowHard)]
        EmptyFilterString,

        [Description($"{nameof(FilterEngineUnavailable)} Policy violation: FilterQueryDatabase cannot be accessed when QueryFilterConfig does not include Filter.")]
        [PolicyEnforcement(ThrowOrAdvise.ThrowHard)]
        FilterEngineUnavailable,

        [Description($"{nameof(ConfigurationModifiedByDatabaseAssignment)} Policy advisory: QueryFilterConfig must be updated for this operation to succeed.")]
        [PolicyEnforcement(ThrowOrAdvise.ThrowSoft)]
        ConfigurationModifiedByDatabaseAssignment,
    }

    public class SQLiteMarkdownException : Exception
    {
        public SQLiteMarkdownException(string message) : base(message) { }
    }
}
