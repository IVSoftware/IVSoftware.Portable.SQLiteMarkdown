using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.SQLiteMarkdown.Events;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    [Careful("Overload OnCommit in Modeled subclass, not here.")]
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
    }
}
