using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public static class Static
    {
        public static Predicate<string> DefaultValidationPredicate { get; set; } = (expr) => true;
    }
}
