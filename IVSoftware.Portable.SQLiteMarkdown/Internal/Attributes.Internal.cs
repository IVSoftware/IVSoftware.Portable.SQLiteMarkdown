using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Internal
{
    /// <summary>
    /// Indicates that an enum must be treated as a discrete value set rather than a bit-field.
    /// </summary>
    /// <remarks>
    /// This attribute explicitly marks an enum as incompatible with flag semantics.
    /// It is intended for validation scenarios where APIs accept enums that are
    /// sometimes used with bitwise combinations.
    ///
    /// When applied, helper methods such as <c>HasFlags</c> or similar flag-inspection
    /// utilities should treat usage as invalid and may report an advisory or throw
    /// an exception depending on the configured error policy.
    ///
    /// The attribute exists to guard against accidental misuse where a caller
    /// attempts to apply flag logic to enums that were designed to represent
    /// mutually exclusive states rather than combinable capabilities.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Enum)]
    internal class NotFlagsAttribute : Attribute
    {

    }
}
