using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    [Policy(typeof(AffinityException))]
    public enum AffinityPolicy
    {
        [Description("Materialized Path Policy violation: Path must end with Id.")]
        [PolicyEnforcement(ThrowOrAdvise.ThrowHard)]
        MaterializedPathMustEndWithId,

        [Description("Identity invariant violation: Id must be a valid Guid.")]
        [PolicyEnforcement(ThrowOrAdvise.ThrowHard)]
        IdMustBeGuid,
    }
    public class AffinityException : Exception 
    {
        public AffinityException(string message) : base(message) { }
    }
}
