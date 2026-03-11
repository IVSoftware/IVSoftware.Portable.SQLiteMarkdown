using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using static SQLite.SQLite3;

namespace IVSoftware.Portable.SQLiteMarkdown
{

    partial class MarkdownContext : IMarkdownContext
    {
        /// <summary>
        /// Returns the singleton, non-replaceable root XElement, created on demand.
        /// </summary>
        /// <remarks>
        /// This represents the canonical ledger.
        /// </remarks>
        public XElement Model
        {
            get
            {
                if (_model is null)
                {
                    _model = new XElement(nameof(StdMarkdownElement.model));
                    _model.Changing += (sender, e) =>
                    {
                        if(sender is XElement xel && e.ObjectChange == XObjectChange.Remove)
                        {
                            _parentsOfRemoved[xel] = xel.Parent;
                        }
                    };
                    _model.Changed += (sender, e) =>
                    {
                        switch (sender)
                        {
                            case XElement xel:
                                XElement pxel;
                                if (e.ObjectChange == XObjectChange.Remove)
                                {
                                    if(!_parentsOfRemoved.TryGetValue(xel, out pxel))
                                    {
                                        _parentsOfRemoved.ThrowSoft<NullReferenceException>(
                                            $"Expecting parent for removed XElement was cached prior." +
                                            $"Unless this throw is escalated, flow will continue with null parent.");
                                    }
                                    _parentsOfRemoved.Remove(xel);
                                }
                                else
                                {
                                    pxel = xel.Parent;
                                }
                                OnXElementChanged(xel, pxel, e);
                                break;
                            case XAttribute xattr:
                                OnXAttributeChanged(xattr, e);
                                break;
                        }
                    };
                }
                return _model;
            }
        }
        XElement? _model = null;

        Dictionary<XElement, XElement> _parentsOfRemoved = new();

        protected virtual void OnXAttributeChanged (XAttribute xattr, XObjectChangeEventArgs e) 
        {
            if (DHostAuthorityEpoch.Authority == CollectionChangeAuthority.None)
            {   /* G T K - N O O P */
            }
            else
            {
                if (xattr is XBoundAttribute xbo && xbo.Tag.GetType() == ContractType)
                {
                    OnBoundItemObjectChange(xbo, e.ObjectChange);
                }
            }
        }

        protected virtual void OnXElementChanged (XElement xel, XElement pxel, XObjectChangeEventArgs e)
        {
            if (DHostAuthorityEpoch.Authority == CollectionChangeAuthority.None)
            {   /* G T K - N O O P */
            }
            else
            {
                if (pxel is null)
                {
                    this.ThrowFramework<NullReferenceException>(
                        $"UNEXPECTED: The '{nameof(pxel)}' argument should be non-null by design.");
                }
                switch (e.ObjectChange)
                {
                    case XObjectChange.Add:
                    case XObjectChange.Remove:
                        var xbo =
                            xel
                            .Attributes()
                            .OfType<XBoundAttribute>()
                            .FirstOrDefault(_ => _.Tag?.GetType() == ContractType);
                        if (xbo is not null)
                        {
                            OnBoundItemObjectChange(xbo, e.ObjectChange);
                        }
                        localCheckAddRemoveCount();
                        break;
                }

                void localCheckAddRemoveCount()
                {
                    XElement? modelRoot = pxel?.AncestorsAndSelf().LastOrDefault();
                    if (modelRoot is null)
                    {
                        this.ThrowFramework<NullReferenceException>(
                            $"UNEXPECTED: The '{nameof(modelRoot)}' argument should be non-null by design.");
                    }
                    else
                    {
                        var autocount = modelRoot.GetAttributeValue<int>(StdMarkdownAttribute.autocount);
                        switch (e.ObjectChange)
                        {
                            case XObjectChange.Add:
                                autocount++;
                                break;
                            case XObjectChange.Remove:
                                if (autocount == 0)
                                {
                                    this.ThrowFramework<InvalidOperationException>(
                                        $"UNEXPECTED: Illegal underflow detected '{nameof(autocount)}'. Count should be >= 0 by design.");
                                }
                                else
                                {
                                    autocount--;
                                }
                                break;
                        }
                        modelRoot.SetAttributeValue(StdMarkdownAttribute.autocount, autocount);
                        // [Careful]
                        // It's too racey here to try and compare counts.
                    }
#if false
                switch (e.ObjectChange)
                {
                    case XObjectChange.Add:
                        count++;
                        root?.SetAttributeValue(StdMarkdownAttribute.count, count);
                        break;
                    case XObjectChange.Remove:
                        if (count == 0)
                        {
                            this.ThrowFramework<InvalidOperationException>(
                                $"UNEXPECTED: Illegal underflow detected '{nameof(count)}'. Count should be >= 0 by design.");
                        }
                        else
                        {
                            count--;
                            {
                                root?.SetAttributeValue(StdMarkdownAttribute.count, count);
                            }
                        }
                        break;
                }
#endif
                }
            }
        }

#if DEBUG
        const bool SQLITE_STRICT = true;
#else
        const bool SQLITE_STRICT = false;
#endif
        protected virtual void OnBoundItemObjectChange(XBoundAttribute xbo, XObjectChange action)
        {
            var item = xbo.Tag;
            switch (action)
            {
                case XObjectChange.Add:
                    localSetModelAuthority(xbo);

                    if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                    {
                        if (SQLITE_STRICT)
                        {
                            if (1 != FilterQueryDatabase.Insert(item))
                            {
                                Debug.Fail($@"ADVISORY - Expecting operation to succeed.");
                            }
                        }
                        else
                        {
                            if (1 != FilterQueryDatabase.InsertOrReplace(item))
                            {

                                Debug.Fail($@"ADVISORY - Expecting operation to succeed.");
                            }
                        }
                    }
                    else
                    {   /* G T K - N O O P */
                        // There is no filter database to maintain.
                    }
                    break;
                case XObjectChange.Remove:
                    FilterQueryDatabase.Delete(item);
                    break;
            }

            void localSetModelAuthority(XBoundAttribute xbo)
            {
                if (xbo.Tag is IAffinityModel modeled)
                {
                    if (xbo.Parent is null)
                    {
                        this.ThrowFramework<NullReferenceException>(
                            "UNEXPECTED: An attribute that is added should have a parent. What was it added *to*?");
                    }
                    else
                    {
                        modeled.Model = xbo.Parent;
                    }
                }
            }
        }

        /// <summary>
        /// Executes a declared FSM sequentially while temporarily asserting any collection-change authority required by the FSM type.
        /// </summary>
        /// <remarks>
        /// If the FSM enum <typeparamref name="TFsm"/> is decorated with
        /// <c>CollectionChangeAuthorityAttribute</c>, an authority token is claimed for the
        /// duration of the FSM execution window. This enables controlled mutation of the
        /// canonical/projection collections during state execution without leaking that
        /// authority outside the run scope.
        ///
        /// The FSM is executed deterministically by iterating the declared enum values
        /// in order and invoking <c>ExecStateAsync</c> for each state. The final state
        /// result is returned to the caller.
        ///
        /// This mechanism allows authority to behave as a dynamic capability rather than
        /// a static property of the context, constraining mutation rights to the precise
        /// interval in which the FSM is running.
        /// </remarks>
        [Probationary("This id a draft implementation that hasn't been thoroughly tested.")]
        protected async Task<Enum> RunFSMAsync<TFsm>(object? context = null) where TFsm : struct, Enum
        {
            Debug.Fail($@"ADVISORY - [Probationary].");
            IDisposable? resetToken = null, authorityToken = null;

            if (typeof(TFsm).GetCustomAttribute<ResetEpochAttribute>() is not null)
            {
                resetToken = BeginResetEpoch();
            }
            if (typeof(TFsm).GetCustomAttribute<CollectionChangeAuthorityAttribute>()?.Authority is CollectionChangeAuthority authority)
            {
                authorityToken = BeginCollectionChangeAuthority(authority);
            }
            using (new TokenDisposer(resetToken, authorityToken))
            {
                Enum result = ReservedFSMState.None;
                // Materialize enumerable context to a stable snapshot so FSM states cannot observe multiple enumerations or deferred side effects.
                // * Reuse an incoming value that is already an object[] to avoid an unnecessary allocation.
                if (context is IEnumerable collection)
                {
                    context = collection is object[] array
                        ? array
                        : collection.Cast<object>().ToArray();
                }

                foreach (Enum state in GetDeclaredValues<TFsm>())
                {
                    // Expecting 'Next' for linear flow.
                    result = await ExecStateAsync(state, context);

                    switch (result)
                    {
                        case ReservedFSMState.Canceled:
                        case ReservedFSMState.FastTrack:
                        case ReservedFSMState.None:
                            return result;
                        case ReservedFSMState.Next:
                            break;
                        default:
                            return await localRunOOB(state, context);
                    }
                }
                return result;
            }

            #region L o c a l F x
            async Task<Enum> localRunOOB(Enum outOfBand, object? context)
            {
                Debug.Fail($@"ADVISORY - First Time.");
                int oobCurrent = 0;
                const int OOB_MAX = 100;
                while (++oobCurrent <= OOB_MAX)
                {
                    outOfBand = ExecState(outOfBand, context);

                    switch (outOfBand)
                    {
                        case ReservedFSMState.Canceled:
                        case ReservedFSMState.FastTrack:
                        case ReservedFSMState.None:
                            return outOfBand;
                        case ReservedFSMState.Next:
                            break;
                    }
                }
                return ReservedFSMState.MaxOOB;
            }
            #endregion L o c a l F x
        }

        /// <summary>
        /// Executes a declared FSM sequentially while temporarily asserting any collection-change authority required by the FSM type.
        /// </summary>
        /// <remarks>
        /// If the FSM enum <typeparamref name="TFsm"/> is decorated with
        /// <c>CollectionChangeAuthorityAttribute</c>, an authority token is claimed for the
        /// duration of the FSM execution window. This enables controlled mutation of the
        /// canonical/projection collections during state execution without leaking that
        /// authority outside the run scope.
        ///
        /// The FSM is executed deterministically by iterating the declared enum values
        /// in order and invoking <c>ExecState</c> for each state. The final state
        /// result is returned to the caller.
        ///
        /// This mechanism allows authority to behave as a dynamic capability rather than
        /// a static property of the context, constraining mutation rights to the precise
        /// interval in which the FSM is running.
        /// </remarks>
        protected Enum RunFSM<TFsm>(object? context = null) where TFsm : struct, Enum
        {
            IDisposable? resetToken = null, authorityToken = null;

            if(typeof(TFsm).GetCustomAttribute<ResetEpochAttribute>() is not null)
            {
                resetToken = BeginResetEpoch();
            }
            if(typeof(TFsm).GetCustomAttribute<CollectionChangeAuthorityAttribute>()?.Authority is CollectionChangeAuthority authority)
            {
                authorityToken = BeginCollectionChangeAuthority(authority);
            }
            using (new TokenDisposer(resetToken, authorityToken))
            {
                Enum result = ReservedFSMState.None;
                // Materialize enumerable context to a stable snapshot so FSM states cannot observe multiple enumerations or deferred side effects.
                // * Reuse an incoming value that is already an object[] to avoid an unnecessary allocation.
                if (context is IEnumerable collection)
                {
                    context = collection is object[] array
                        ? array
                        : collection.Cast<object>().ToArray();
                }

                foreach (Enum state in GetDeclaredValues<TFsm>())
                {
                    // Expecting 'Next' for linear flow.
                    result = ExecState(state, context);

                    switch (result)
                    {
                        case ReservedFSMState.Canceled:
                        case ReservedFSMState.FastTrack:
                        case ReservedFSMState.None:
                            return result;
                        case ReservedFSMState.Next:
                            break;
                        default:
                            return localRunOOB(state, context);
                    }
                }
                return result;
            }

            #region L o c a l F x
            Enum localRunOOB(Enum outOfBand, object? context)
            {
                Debug.Fail($@"ADVISORY - First Time.");
                int oobCurrent = 0;
                const int OOB_MAX = 100;
                while (++oobCurrent <= OOB_MAX)
                {
                    outOfBand = ExecState(outOfBand, context);

                    switch (outOfBand)
                    {
                        case ReservedFSMState.Canceled:
                        case ReservedFSMState.FastTrack:
                        case ReservedFSMState.None:
                            return outOfBand;
                        case ReservedFSMState.Next:
                            break;
                    }
                }
                return ReservedFSMState.MaxOOB;
            }
            #endregion L o c a l F x
        }
        protected virtual async Task<Enum> ExecStateAsync(Enum state, object? context = null)
        {
            return ReservedFSMState.Canceled;
        }

        /// <summary>
        /// Enumerates the values of the enum type <typeparamref name="TFsm"/> in their
        /// declaration order rather than their underlying numeric order. This is used
        /// by the FSM runner to evaluate states exactly in the sequence authored in
        /// the enum definition.
        /// </summary>
        protected IEnumerable<TFsm> GetDeclaredValues<TFsm>() where TFsm : Enum
        {
            return typeof(TFsm)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Select(field => (TFsm)field.GetValue(null)!);
        }

        protected Enum ExecState(Enum state, object? context = null)
        {
            IEnumerable<object>?
                projection = context as IEnumerable<object>;
            bool
                isEmptyProjection = projection?.Any() != true;
#if DEBUG
            switch (state)
            {
                case NativeClearFSM:
                    break;
                case LoadIsFilteringEpochFSM:
                    break;
            }
#endif
            switch ((StdFSMState)state)
            {
                case StdFSMState.DetectFastTrack:
                    if (Equals(localDetectFastTrack(), ReservedFSMState.FastTrack))
                    {
                        return ReservedFSMState.FastTrack;
                    }
                    else
                    {
                        break;
                    }
                case StdFSMState.ResetOrCanonizeFQBDForEpoch:
                    localResetOrCanonizeFQDBForEpoch(context);
                    break;
                case StdFSMState.ResetOrCanonizeModelForEpoch:
                    localResetOrCanonizeModelForEpoch(context);
                    break;
                case StdFSMState.UpdateStatesForEpoch:
                    localInitStatesForEpoch();
                    break;
                case StdFSMState.AddItemToModel:
                    localAddItemToModel(context);
                    break;
                case StdFSMState.RemoveItemFromModel:
                    localRemoveItemFromModel(context);
                    break;
                default:
                    Debug.Fail($@"ADVISORY - Unrecognized action.");
                    break;
            }
            return ReservedFSMState.Next;

            #region L o c a l F x
            Enum localDetectFastTrack()
            {
                bool isEmptyProjection =
                    !(ObservableNetProjection is IEnumerable projection && projection.Cast<object>().Any());
                switch (state)
                {
                    case ClearModelFSM:
                        if (!Model.HasElements)
                        {
                            return ReservedFSMState.FastTrack;
                        }
                        break;
                    case NativeClearFSM:
                        // If ALL are true.
                        if( SearchEntryState == SearchEntryState.Cleared
                            && !Model.HasElements
                            && isEmptyProjection)
                        {
                            return ReservedFSMState.FastTrack;
                        }
                        else
                        {
                            break;
                        }
                }
                return ReservedFSMState.Next;
            }

            Enum localResetOrCanonizeFQDBForEpoch(object? context)
            {
                // Check to see whether we should have a FQDB in the first place.
                if (QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    try
                    {
                        FilterQueryDatabase.RunInTransaction(() =>
                        {
                            // Ensure table exists.
                            FilterQueryDatabase.CreateTable(ContractType);
                            // Clear any entries from a pre-existing table.
                            FilterQueryDatabase.DeleteAll(ContractType.GetSQLiteMapping());
                            // [Remember]
                            // - Canonization happens via XML changes as they arrive.
                            // - N O O P
                        });
                    }
                    catch (Exception ex)
                    {
                        this.RethrowHard(ex);
                        return ReservedFSMState.Canceled;
                    }
                }
                else
                {   /* G T K - N O O P */
                    // There is no FQDB to maintain in Query-Only mode.
                }
                return ReservedFSMState.Next;
            }

            void localResetOrCanonizeModelForEpoch(object? context)
            {
                if (context is not IEnumerable canonical)
                {
                    Model.RemoveNodes(StdMarkdownAttribute.autocount, StdMarkdownAttribute.count, StdMarkdownAttribute.matches);
                    return;
                }
                else
                {
#if DEBUG
                    int nRemoved = 0;
#endif
                    Model.SetAttributeValue(StdMarkdownAttribute.count, null);
                    Model.SetAttributeValue(StdMarkdownAttribute.matches, null);

                    PropertyInfo? pk = ContractType.GetSQLiteMapping().PK?.PropertyInfo;
#if RELEASE
                Model.RemoveNodes();
#else
                    // DEBUG:
                    // Provides clarity on how the XML Changed events work on a bulk RemoveNodes.
                    #region L o c a l F x
                    void localOnXObjectChanged(object? sender, XObjectChangeEventArgs e)
                    {
                        Debug.WriteLine($@"260306.A: Removed {++nRemoved}");
                    }
                    #endregion L o c a l F x
                    using (Model.WithOnDispose(
                        onInit: (sender, e) =>
                        {
                            Model.Changed += localOnXObjectChanged;
                        },
                        onDispose: (sender, e) =>
                        {
                            Model.Changed -= localOnXObjectChanged;
                        }))
                    {
                        if (Model.HasElements)
                        {
                            Model.RemoveNodes();
                        }
                    }
#endif
                    int
                        countDistinct = 0,
                        countDuplicate = 0;

                    if (pk is null)
                    {
                        throw new NotSupportedException($"Type '{ContractType.Name}' has no PK and such types are not (yet) supported.");
                    }
                    foreach (var item in canonical)
                    {
                        // ToDo: Test with item.GetFullPath() extension.
                        var placerResult = Model.Place(path: localGetFullPath(pk, item), out var xel);

                        switch (placerResult)
                        {
                            case PlacerResult.Exists:
                                countDuplicate++;
                                break;
                            case PlacerResult.Created:
                                xel.Name = nameof(StdMarkdownElement.xitem);
                                xel.SetBoundAttributeValue(
                                    tag: item,
                                    name: nameof(StdMarkdownAttribute.model));
                                xel.SetAttributeValue(nameof(StdMarkdownAttribute.sort), countDistinct);
                                countDistinct++;
                                break;
                            default:
                                this.ThrowFramework<NotSupportedException>(
                                    $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                                break;
                        }
                    }

                    Model.SetAttributeValue(StdMarkdownAttribute.count, countDistinct);
                    if (Model.GetAttributeValue<IList?>(StdMarkdownAttribute.predicates) is { } predicates)
                    {
                        Debug.Fail($@"ADVISORY - First Time.");
                        Model.SetAttributeValue(StdMarkdownAttribute.matches, countDistinct); // This will change.
                    }
                    else
                    {
                        Model.SetAttributeValue(StdMarkdownAttribute.matches, countDistinct);
                    }
                    Model.SetAttributeValue(StdMarkdownAttribute.ismatch, null);
                }
            }

            string localGetFullPath(PropertyInfo pk, object unk)
            {
                if (pk.GetValue(unk)?.ToString() is { } id && !string.IsNullOrWhiteSpace(id))
                {
                    return id;
                }
                else
                {
                    this.ThrowHard<NullReferenceException>(
                        $"Expecting a non-empty value for PrimaryKey '{pk.Name}'.");
                    return null!;
                }
            }

            void localInitStatesForEpoch()
            {
                switch (state)
                {
                    case NativeClearFSM:
                        SearchEntryState = SearchEntryState.Cleared;
                        FilteringState = FilteringState.Ineligible;
                        return;
                    default:
                        break;
                }
                switch (CanonicalCount)
                {
                    case 0:
                        SearchEntryState = SearchEntryState.QueryCompleteNoResults;
                        FilteringState = FilteringState.Ineligible;
                        break;
                    case 1:
                        SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                        FilteringState = FilteringState.Ineligible;
                        break;
                    default:
                        SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                        switch (QueryFilterConfig)
                        {
                            case QueryFilterConfig.Query:
                            case QueryFilterConfig.Filter:
                            default:
                                FilteringState = FilteringState.Ineligible;
                                break;
                            case QueryFilterConfig.QueryAndFilter:
                                FilteringState = FilteringState.Armed;
                                break;
                        }
                        break;
                }
            }

            void localAddItemToModel(object? item)
            {
                if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                {
                    int
                        indexForAdd = Model.GetAttributeValue<int>(StdMarkdownAttribute.autocount),
                        countB4 = Model.GetAttributeValue<int>(StdMarkdownAttribute.count, 0),
                        matchesB4 = Model.GetAttributeValue<int>(StdMarkdownAttribute.matches);

                    var placerResult = Model.Place(full, out var xel);
                    switch (placerResult)
                    {
                        case PlacerResult.Exists:
                            break;
                        case PlacerResult.Created:
                            xel.Name = nameof(StdMarkdownElement.xitem);
                            xel.SetBoundAttributeValue(
                                tag: item,
                                name: nameof(StdMarkdownAttribute.model));

                            xel.SetAttributeValue(nameof(StdMarkdownAttribute.sort), indexForAdd);
                            Model.SetAttributeValue(nameof(StdMarkdownAttribute.count), ++countB4);
                            Model.SetAttributeValue(nameof(StdMarkdownAttribute.matches), ++matchesB4);
                            break;
                        default:
                            this.ThrowFramework<NotSupportedException>(
                                $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                            break;
                    }
                }
                else
                {
                    this.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                }
            }

            void localRemoveItemFromModel(object? item)
            {
                if (ContractType.GetPK()?.PropertyInfo is { } pi)
                {

                }
                else this.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
            }
            #endregion L o c a l F x
        }

        /// <summary>
        /// The ephemeral backing store for this collection's contract filtering.
        /// </summary>
        /// <remarks>
        /// - Like any other SQLite database this can be configured with N tables.
        ///   However, the semantic constraints on contract parsing (where ContractType 
        ///   is assumed to be the item type of the collection that subclasses it) will
        ///   provide an advisory stream should this be called upon to service more
        ///   than the implicit single table for the collection.
        /// </remarks>
        protected SQLiteConnection FilterQueryDatabase
        {
            get
            {
                if (!QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    this.ThrowPolicyException(SQLiteMarkdownPolicy.FilterEngineUnavailable);
                    // NOTE:
                    // Handling the Throw creates a benign condition where a DB
                    // that might not really be necessary is instantiated regardless.
                }

                // HYBRID - factory getter.
                if (_filterQueryDatabase is null)
                {
                    _filterQueryDatabase = new SQLiteConnection(":memory:");
                    // ContractType is set at construction and cannot be null.
                    _filterQueryDatabase.CreateTable(ContractType);
                }
                return _filterQueryDatabase;
            }
            set
            {
                if (value is not null && !QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                {
                    // The user must be given the benefit of the doubt if they are explicitly
                    // injecting a connection to be used for internal filter queries. This will
                    // silently upgrade the configuration unless escalated in the Throw handler.
                    this.ThrowPolicyException(SQLiteMarkdownPolicy.ConfigurationModifiedByDatabaseAssignment);
                    QueryFilterConfig |= QueryFilterConfig.Filter;
                }

                if (!Equals(_filterQueryDatabase, value))
                {
                    _filterQueryDatabase = value;
                    if(_filterQueryDatabase is not null)
                    {
                        _filterQueryDatabase.CreateTable(ContractType);
                    }
                    OnPropertyChanged();
                    this.OnAwaited();
                }
            }
        }
        SQLiteConnection? _filterQueryDatabase = default;

#if false
        private bool TryCreateTableForContractType()
        {
            if( _filterQueryDatabase is not null 
                && ContractType?.GetConstructor(Type.EmptyTypes) is not null)
            {
                ContractTypeTableMapping = _filterQueryDatabase.GetMapping(ContractType);
                _filterQueryDatabase.CreateTable(ContractType);
                return true;
            }
            else
            {
                return false;
            }
        }
        public TableMapping ContractTypeTableMapping
        {
            get => _contractTypeTableMapping;
            set
            {
                if (!Equals(_contractTypeTableMapping, value))
                {
                    _contractTypeTableMapping = value;
                    OnPropertyChanged();
                }
            }
        }
        TableMapping _contractTypeTableMapping = default;
#endif

        /// <summary>
        /// Creates a new filter epoch by establishing the provided recordset as the canonical source for subsequent operations.
        /// </summary>
        /// <remarks>
        /// Mental Model: "This is the baseline for filtering, prioritization, and temporal projections."
        /// </remarks>
        public virtual async Task LoadCanonAsync(IEnumerable? recordset)
            => await RunFSMAsync<LoadIsFilteringEpochFSM>(recordset);

        /// <summary>
        /// Established a new canonical model for subsequent operations.
        /// </summary>
        /// <remarks>
        /// Mental Model: "This is the baseline for filtering, prioritization, and temporal projections."
        /// </remarks>
        public virtual void LoadCanon(IEnumerable? recordset)
        {
            recordset ??= Array.Empty<object>();
            using (DHostBusy.GetToken())
            {
                RunFSM<LoadIsFilteringEpochFSM>(recordset);
            }
        }

        #region P R O J E C T I O N
        /// <summary>
        /// Gets or sets the observable projection representing the effective
        /// (net visible) collection after markdown and predicate filtering.
        /// </summary>
        /// <remarks>
        /// Mental Model: "ItemsSource for a CollectionView with both initial query and subsequent filter refinement.
        /// - OBSERVABLE: This is an INCC object that can be tracked.
        /// - NET       : The items in this collection depend on the net result of the recordset and any state-dependent filters.
        /// - PROJECTION: Conveys that this 'filtering' produces a PCL collection, albeit one that is likely to be visible.
        ///
        /// When assigned, this context subscribes to CollectionChanged as a
        /// reconciliation sink. During refinement epochs, structural changes
        /// made against the filtered projection are absorbed into the canonical
        /// backing store so that the canon remains complete and relevant.
        ///
        /// The projection is an interaction surface, not a storage authority.
        /// Its mutations are normalized and merged into the canonical collection
        /// according to the active authority contract.
        ///
        /// Replacing this property detaches the previous projection and attaches the new one.
        ///
        /// This property is infrastructure wiring and is not intended for data binding.
        /// </remarks>
        public INotifyCollectionChanged? ObservableNetProjection
        {
            get => _observableProjection;
            set
            {
                if (!Equals(_observableProjection, value))
                {
                    // Unsubscribe INCC
                    if (_observableProjection is not null)
                    {
                        _observableProjection.CollectionChanged -= OnObservableProjectionCollectionChanged;
                    }

                    _observableProjection = value;

                    // [Careful] This is safest when MDC is first in line.
                    ProjectionTopology = _observableProjection switch
                    {
                        MarkdownContext _ => ProjectionTopology.Inheritance,
                        INotifyCollectionChanged _ and not MarkdownContext
                            => ProjectionTopology.Composition,
                        _ => ProjectionTopology.None,
                    };

                    // Run the handler then subscribe to any subsequent changes.
                    OnObservableProjectionChanged();

                    // Subscribe INCC
                    if (_observableProjection is not null)
                    {
                        _observableProjection.CollectionChanged += OnObservableProjectionCollectionChanged;
                    }
                }
            }
        }
        INotifyCollectionChanged? _observableProjection = null;


        /// <summary>
        /// Raised when the handle to the ObservableNetCollection changes.
        /// </summary>
        /// <remarks>
        /// SYNCHRONOUS - Do *not* mess around. This is information we need *now* and will have to wait for.
        /// MentalMode (Query          config): "Do not track changes on this INCC."
        /// MentalMode (QueryAndFilter config): "The system must be reset to root cause in order to be stable."
        /// MentalMode (Filter         config): "The contents of the new projection must be regarded as a new canon."
        /// </remarks>
        protected virtual void OnObservableProjectionChanged()
        {
            if (ObservableNetProjection is IEnumerable collection && collection.Cast<object>().Any())
            {
                // Treat any non-empty projection as a new canonical recordset.
                LoadCanon(collection);
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Raised when the collection - that is the ObservableNetProjection - is modified in some way.
        /// </summary>
        protected virtual void OnObservableProjectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (DHostAuthorityEpoch.Authority)
            {
                case 0:
                case CollectionChangeAuthority.NetProjection:

                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (e.NewItems?.Count is 1)
                            {
                                RunFSM<TrackUserAddItem>(e.NewItems[0]);
                            }
                            else
                            {
                                LoadCanon(sender as IEnumerable);
                            }
                            break;
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            if (sender is IList list && list.Count == 0)
                            {
                                // #{A665C02F-B1DE-45AE-8DAD-67775114E725}
                                if (Model.HasElements)
                                {
                                    Model.RemoveAll();
                                }
                                if (SearchEntryState != SearchEntryState.Cleared)
                                {
                                    SearchEntryState = SearchEntryState.Cleared;
                                }
                                if (FilteringState != FilteringState.Ineligible)
                                {
                                    FilteringState = FilteringState.Ineligible;
                                }
                            }
                            else
                            {
                                LoadCanon(sender as IEnumerable);
                            }
                            break;
                        default:
                            this.ThrowHard<NotSupportedException>($"The {e.Action.ToFullKey()} case is not supported.");
                            break;
                    }

                    break;
                case CollectionChangeAuthority.None:
                case CollectionChangeAuthority.MarkdownContext:
                default:
                    {   /* G T K - N O O P */
                    }
                    break;
            }
        }


        /// <summary>
        /// Determines whether MDC is allowed to puppeteer the projection directly.
        /// </summary>
        internal NetProjectionOption ProjectionOptions { get; set; } = NetProjectionOption.AllowDirectChanges;


        /// <summary>
        /// Reports on whether this object is inherited or composed.
        /// </summary>
        public ProjectionTopology ProjectionTopology { get; protected set; }
        #endregion P R O J E C T I O N

        [Obsolete("Version 2.0+ uses clearer semantics: CanonicalCount and PredicateMatchCount.")]
        [PublishedContract("1.0")] // Required for backward compatibility. Do not remove this property.
        public int UnfilteredCount 
        {
            get => CanonicalCount;
            protected set => Model.SetAttributeValue(StdMarkdownAttribute.count, value);
        }

        public int CanonicalCount => Model.GetAttributeValue<int>(StdMarkdownAttribute.count);

        public int PredicateMatchCount => Model.GetAttributeValue<int>(StdMarkdownAttribute.matches);

        protected override async Task OnEpochFinalizingAsync(EpochFinalizingAsyncEventArgs e)
        {
            using (BeginCollectionChangeAuthority(CollectionChangeAuthority.MarkdownContext))
            {
                await base.OnEpochFinalizingAsync(e);
                if (!e.Cancel)
                {
                    await OnInputTextSettled(new CancelEventArgs());
                }
            }
        }

        public string[] GetTableNames() => FilterQueryDatabase.GetTableNames();
    }
}
