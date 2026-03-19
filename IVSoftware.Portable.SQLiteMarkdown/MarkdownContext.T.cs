using IVSoftware.Portable.SQLiteMarkdown.Internal;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public class MarkdownContext<T> : MarkdownContext
    {
        /// <summary>
        /// Creates a typed context whose base infrastructure is initialized
        /// with the element type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// The base <see cref="MarkdownContext"/> constructs its internal
        /// collections using the supplied element type. As a result, the
        /// predicate-match subset is guaranteed to be backed by a
        /// <c>List&lt;T&gt;</c> (exposed as <see cref="IList"/> in the base
        /// contract). The generic context therefore only projects a typed
        /// read-only view and does not need to enforce the type at runtime.
        /// </remarks>
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
