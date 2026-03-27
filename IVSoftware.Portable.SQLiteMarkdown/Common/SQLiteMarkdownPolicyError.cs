using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    [Policy(typeof(SQLiteMarkdownException))]
    public enum MarkdownContextPolicyViolation
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

        [Description($@"{nameof(ExplicitClearAdvisory)} Policy advisory:
- Inherited MarkdownContext detected, but no parameterless Clear() was found.
- Clear(bool all = false) participates in the MDC filtering state machine and may not
  immediately empty the collection. 
- If your callers expect IList-style behavior, consider implementing Clear() => Clear(true)
  to provide a deterministic terminal clear. You may also expose Clear(bool all) without a 
  default parameter to make the stateful semantics explicit.")]
        [PolicyEnforcement(ThrowOrAdvise.ThrowSoft)]
        ExplicitClearAdvisory,
    }

    public class SQLiteMarkdownException : Exception
    {
        public SQLiteMarkdownException(string message) : base(message) { }
    }
}
