using System;
using System.ComponentModel;
using System.Reflection;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    // [Policy(typeof(AffinityException)]
    public enum AffinityPolicy
    {
        [Description("Materialized Path Policy violation: Path must end with Id.")]
        // [PolicyEnforcement(ThrowOrAdvise.ThrowHard)]
        MaterializedPathMustEndWithId,

        [Description("ChildAffinityMode cannot be specified for a Fixed AffinityItem.")]
        InvalidChildModeForFixedItem,
    }

    public class AffinityException : Exception
    {
        public AffinityPolicy Policy { get; }

        public AffinityException(AffinityPolicy policy)
            : base(GetPolicyDescription(policy))
        {
            Policy = policy;
        }

        public AffinityException(AffinityPolicy policy, Exception innerException)
            : base(GetPolicyDescription(policy), innerException)
        {
            Policy = policy;
        }

        private static string GetPolicyDescription(AffinityPolicy policy)
        {
            var member = typeof(AffinityPolicy).GetMember(policy.ToString());
            if (member.Length > 0)
            {
                var attribute = member[0].GetCustomAttribute<DescriptionAttribute>();
                if (attribute is not null)
                {
                    return attribute.Description;
                }
            }
            return $"Affinity policy violation: {policy}.";
        }
    }
}
