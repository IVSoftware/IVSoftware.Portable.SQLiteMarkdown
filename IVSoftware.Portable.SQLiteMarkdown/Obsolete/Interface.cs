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

        IReadOnlyDictionary<string, Enum> ActiveFilters { get; }

        void ActivatePredicates(Enum stdPredicate, params Enum[] more);

        void DeactivatePredicates(Enum stdPredicate, params Enum[] more);

        void ClearPredicates(bool clearInputText = true);
    }
    public interface ITopology
    {
        #region C O N F I G U R A T I O N    P R O P E R T I E S
        /// <summary>
        /// OPT-IN that allows MarkdownContext to modify the ObservableNetCollection directly.
        /// </summary>
        NetProjectionTopology ProjectionTopology { get; }

        /// <summary>
        /// Determines whether filter update events are provided as structural changes
        /// with old-new item semantics, alternatively as a bulk reset, or both.
        /// </summary>
        /// <remarks>
        /// Some UI platforms respond more efficiently to a raw reset.
        /// </remarks>

        [Careful("Not to be conflated with CollectionChangingEventingPolicy")]
        ReplaceItemsEventingPolicy ReplaceItemsEventingPolicy { get; set; }
        #endregion C O N F I G U R A T I O N    P R O P E R T I E S

        #region P R O J E C T I O N
        /// <summary>
        /// Represents a bindable and observable collection representing 'net visible' filtered items.
        /// </summary>
        IList? ObservableNetProjection { get; }

        public IList CanonicalSuperset { get; }

        public IList PredicateMatchSubset { get; }
        #endregion  P R O J E C T I O N

        public int Count { get; }
    }
}
