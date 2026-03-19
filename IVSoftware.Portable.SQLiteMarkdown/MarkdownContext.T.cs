using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public class MarkdownContext<T> : MarkdownContext
    {
        /// <summary>
        /// Creates a typed context whose base infrastructure is initialized with the element type <typeparamref name="T"/>.
        /// </summary>
        public MarkdownContext() : base(typeof(T)) { }

        /// <summary>
        /// True when InputText is empty regardless of IsFiltering.
        /// </summary>
        /// <remarks>
        /// Mental Model:
        /// "If the input text is empty, just swap the handle instead of recalculating."
        /// Functional Behavior:
        /// - External predicate filters must still run even if IME doesn't contribute.
        /// - This is the purview of the subclass. Override for full control.
        /// </remarks>
        public virtual bool RouteToFullRecordset
        {
            get
            {
                if (InputText.Trim().Length == 0)
                {
                    return true;
                }
                int
                    autocount = Model.GetAttributeValue<int>(StdMarkdownAttribute.autocount, 0),
                    matches = Model.GetAttributeValue<int>(StdMarkdownAttribute.matches, 0);
                return autocount == matches;
            }
        }

        bool _routeToFullRecordset = true;
    }
}
