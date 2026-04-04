using IVSoftware.Portable.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.Collections.Modeled
{
    /// <summary>
    /// Marks a property as the authoritative modeled full path.
    /// </summary>
    /// <remarks>
    /// - Value must be a non-empty string.
    /// - May represent hierarchical segments delimited by '\'.
    /// - Expected to be unique within the modeled collection.
    /// </remarks>
    [Canonical, AttributeUsage(AttributeTargets.Property)]
    public sealed class ModelPathAttribute : Attribute { }


}
