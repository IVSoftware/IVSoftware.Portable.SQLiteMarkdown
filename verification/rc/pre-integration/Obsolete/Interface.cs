using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Common.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Obsolete
{
    /// <summary>
    /// Extends MarkdownContext with a predicate AND clause that is property-based.
    /// </summary>
    /// <remarks>
    /// Query and filter modes are equally affected.
    /// EXAMPLE:
    /// Activating this StdPredicate value will hide checked items in queries 
    /// and filters, and checking the item in the UI will filter that item out.
    /// <c>
    /// [Where("IsChecked", WherePredicate.IsFalse)]
    /// IsUnchecked,
    /// </c>
    /// </remarks>
    [Probationary("Maintain as Internal until stable.")]
    [PublishedContract("2.0.0-alpha31", typeof(IPredicateMarkdownContext))]
    [Careful("This interface is not allowed to implement INotifyCollectionChanged.")]
    public interface IPredicateMarkdownContext
    {
        /// <summary>
        /// Obtain a token that suspends updates.
        /// </summary>
        /// <remarks>
        /// Avoids intermediate transitions when multiple predicates change state 
        /// simultaneously (e.g., ShowChecked v ShowUnchecked radio buttons).
        /// </remarks>
        IDisposable BeginPredicateAtom();

        IReadOnlyDictionary<string, Enum> ActivePredicates { get; }

        void ActivatePredicates(Enum stdPredicate, params Enum[] more);

        void DeactivatePredicates(Enum stdPredicate, params Enum[] more);

        void ClearPredicates(bool clearInputText = true);
    }
}
