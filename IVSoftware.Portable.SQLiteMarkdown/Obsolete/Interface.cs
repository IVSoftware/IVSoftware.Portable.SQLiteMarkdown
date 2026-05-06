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

#if false
    public interface IModeledMarkdownContext
        : IMarkdownContext
        , ITopology
    {
        #region M O D E L
        /// <summary>
        /// Maintains the canonical recordset as a hierarchy.
        /// </summary>
        /// <remarks>
        /// - The string value of the property designated as the PK is 
        ///   used as the address in the item.
        /// - For item types that support a FullPath property, the
        ///   model can also represent depth.
        /// </remarks>
        XElement Model { get; }

        /// <summary>
        /// Requests that an external host raise a collection change notification.
        /// </summary>
        /// <remarks>
        /// <see cref="MarkdownContext"/> itself does not implement <see cref="INotifyCollectionChanged"/>.
        /// Instead, canonical mutations are surfaced through this request so that an owning
        /// surface—often a derived type or UI adapter that *does* implement
        /// <see cref="INotifyCollectionChanged"/>—may relay the corresponding
        /// <see cref="NotifyCollectionChangedEventArgs"/> to observers.
        ///
        /// Mental Model: "Filtering model has been reconfigured. Ask the host to raise INCC."
        /// </remarks>
        event EventHandler ModelSettled;
        #endregion M O D E L

        #region P R O J E C T I O N

        /// <summary>
        /// Creates a new filter epoch by establishing the provided recordset as the canonical source for subsequent operations.
        /// </summary>
        /// <remarks>
        /// Mental Model: "This is the baseline for filtering, prioritization, and temporal projections."
        /// </remarks>
        void LoadCanon(IEnumerable? recordset);

        /// <summary>
        /// Creates a new filter epoch by establishing the provided recordset as the canonical source for subsequent operations.
        /// </summary>
        /// <remarks>
        /// Mental Model: "This is the baseline for filtering, prioritization, and temporal projections."
        /// </remarks>
        [Probationary]
        Task LoadCanonAsync(IEnumerable? recordset);
        #endregion P R O J E C T I O N

        #region D I S P O S A B L E
        /// <summary>
        /// Guards receptivity of the unfiltered items collection.
        /// </summary> 
        /// <remarks>
        /// Intended use: EpochFinalizing should be wrapped with this reference counter.
        /// </remarks>
        IDisposable BeginCollectionChangeAuthority(CollectionChangeAuthority authority);

        /// <summary>
        /// Returns the current collection DDX authority.
        /// </summary>
        CollectionChangeAuthority Authority { get; }

        #endregion D I S P O S A B L E
    }
    public interface IModeledMarkdownContext<T> : IModeledMarkdownContext
    {
        /// <summary>
        /// Represents a bindable and observable collection representing 'net visible' filtered items.
        /// </summary>
        new ObservableCollection<T>? ObservableNetProjection { get; }
        void SetObservableNetProjection(
            ObservableCollection<T>? onp,
            NetProjectionTopology? option = null);

        new IReadOnlyList<T> CanonicalSuperset { get; }

        new IReadOnlyList<T> PredicateMatchSubset { get; }
    }
#endif
}
