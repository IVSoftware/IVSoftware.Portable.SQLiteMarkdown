using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.StateMachine;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Xml.Linq.XBoundObject;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class ModeledMarkdownContext<T> : IStateRunner, IStateRunnerAsync
    {
        private StateRunnerMMDC StateRunner
        {
            get
            {
                if (_stateRunner is null)
                {
                    _stateRunner = new StateRunnerMMDC(this);
                }
                return _stateRunner;
            }
        }
        StateRunnerMMDC? _stateRunner = null;

        private StateRunnerAsyncMMDC StateRunnerAsync
        {
            get
            {
                if (_stateRunnerAsync is null)
                {
                    _stateRunnerAsync = new StateRunnerAsyncMMDC(this);
                }
                return _stateRunnerAsync;
            }
        }
        StateRunnerAsyncMMDC? _stateRunnerAsync = null;


        protected AuthorityEpochProvider AuthorityEpochProvider => StateRunner.AuthorityProvider;

        /// <summary>
        /// Identifies provenance of INCC.
        /// </summary>
        /// <remarks>
        /// Acts as an authority monitor and circularity guard for DDX between collections.
        /// </remarks>
        public IDisposable BeginCollectionChangeAuthority(CollectionChangeAuthority authority) => AuthorityEpochProvider.GetToken(authority);

        public Enum RunFSM<TFsm>(object? context = null) => StateRunner.RunFSM<TFsm>(context);

        public Enum RunTokenRing<TFsm>(object? context = null) => StateRunner.RunTokenRing<TFsm>(context);

        public Enum ExecState(Enum state, object? context) => StateRunner.ExecState(state, context);

        public Task<Enum> RunFSMAsync<TFsm>(object? context = null) => ((IStateRunnerAsync)StateRunnerAsync).RunFSMAsync<TFsm>(context);

        public Task<Enum> RunTokenRingAsync<TFsm>(object? context = null) => ((IStateRunnerAsync)StateRunnerAsync).RunTokenRingAsync<TFsm>(context);
        Task<Enum> IStateRunnerAsync.ExecStateAsync(Enum state, object? context) => ((IStateRunnerAsync)StateRunnerAsync).ExecStateAsync(state, context);

        class StateRunnerMMDC : StateRunner
        {
            public StateRunnerMMDC(ModeledMarkdownContext<T> mmdc) => MMDC = mmdc;

            ModeledMarkdownContext<T> MMDC { get; }
            XElement Model => MMDC.Model;
            Type ContractType => MMDC.ContractType;

            public override Enum ExecState(Enum state, object? context)
            {
                IEnumerable<object>? canon = context as IEnumerable<object>;
                bool isEmptyProjection = canon?.Any() != true;
#if DEBUG
                switch (state)
                {
                    case NativeClearFSM:
                        break;
                }
#endif
                switch ((StdFSMState)state)
                {
                    case StdFSMState.DetectFastTrack:
                        if (Equals(localDetectFastTrack(), FsmReservedState.FastTrack))
                        {
                            return FsmReservedState.FastTrack;
                        }
                        else
                        {
                            break;
                        }
                    case StdFSMState.ResetOrCanonizeFQBDForEpoch:
                        localResetOrCanonizeFQDBForEpoch();
                        break;
                    case StdFSMState.ResetOrCanonizeModelForEpoch:
                        localResetOrCanonizeModelForEpoch();
                        break;
                    case StdFSMState.UpdateStatesForEpoch:
                        localInitStatesForEpoch();
                        break;
                    case StdFSMState.AddItemToModel:
                        MMDC.AddItemToModel(context);
                        break;
                    case StdFSMState.RemoveItemFromModel:
                        MMDC.RemoveItemFromModel(context);
                        break;
                    case StdFSMState.ModelSettled:
                        localRaiseModelSettled();
                        break;
                    default:
                        Debug.Fail($@"ADVISORY - Unrecognized action.");
                        break;
                }
                return FsmReservedState.Next;

                #region L o c a l F x
                Enum localDetectFastTrack()
                {
                    bool isEmptyProjection =
                        !(MMDC.ObservableNetProjection is IEnumerable projection && projection.Cast<object>().Any());
                    switch (state)
                    {
                        case NativeClearFSM:
                            // If ALL are true.
                            if (MMDC.SearchEntryState == SearchEntryState.Cleared
                                && !Model.HasElements
                                && isEmptyProjection)
                            {
                                return FsmReservedState.FastTrack;
                            }
                            else
                            {
                                break;
                            }
                    }
                    return FsmReservedState.Next;
                }

                Enum localResetOrCanonizeFQDBForEpoch()
                {
                    // Check to see whether we should have a FQDB in the first place.
                    if (MMDC.QueryFilterConfig.HasFlag(QueryFilterConfig.Filter))
                    {
                        try
                        {
                            MMDC.FilterQueryDatabase.RunInTransaction(() =>
                            {
                                // Ensure table exists.
                                MMDC.FilterQueryDatabase.CreateTable(MMDC.ContractType);
                                // Clear any entries from a pre-existing table.
                                MMDC.FilterQueryDatabase.DeleteAll(MMDC.ContractType.GetSQLiteMapping());
                                // [Remember]
                                // - Canonization happens via XML changes as they arrive.
                                // - N O O P
                            });
                        }
                        catch (Exception ex)
                        {
                            this.RethrowHard(ex);
                            return FsmReservedState.Canceled;
                        }
                    }
                    else
                    {   /* G T K - N O O P */
                        // There is no FQDB to maintain in Query-Only mode.
                    }
                    return FsmReservedState.Next;
                }

                void localResetOrCanonizeModelForEpoch()
                {
                    if (canon is not IEnumerable)
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

                        PropertyInfo? pk = MMDC.ContractType.GetSQLiteMapping().PK?.PropertyInfo;
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
                        foreach (var item in canon)
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
                        MMDC.ThrowHard<NullReferenceException>(
                            $"Expecting a non-empty value for PrimaryKey '{pk.Name}'.");
                        return null!;
                    }
                }

                void localInitStatesForEpoch()
                {
                    switch (state)
                    {
                        case NativeClearFSM:
                            MMDC.SearchEntryState = SearchEntryState.Cleared;
                            MMDC.FilteringState = FilteringState.Ineligible;
                            return;
                        default:
                            break;
                    }
                    switch (MMDC.CanonicalCount)
                    {
                        case 0:
                            MMDC.SearchEntryState = SearchEntryState.QueryCompleteNoResults;
                            MMDC.FilteringState = FilteringState.Ineligible;
                            break;
                        case 1:
                            MMDC.SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                            MMDC.FilteringState = FilteringState.Ineligible;
                            break;
                        default:
                            MMDC.SearchEntryState = SearchEntryState.QueryCompleteWithResults;
                            switch (MMDC.QueryFilterConfig)
                            {
                                case QueryFilterConfig.Query:
                                case QueryFilterConfig.Filter:
                                default:
                                    MMDC.FilteringState = FilteringState.Ineligible;
                                    break;
                                case QueryFilterConfig.QueryAndFilter:
                                    MMDC.FilteringState = FilteringState.Armed;
                                    break;
                            }
                            break;
                    }
                }

                void localRaiseModelSettled()
                {
                    var e = context as ModelSettledEventArgs
                        ?? new ModelSettledEventArgs(
                            reason: NotifyCollectionChangeReason.None,
                            action: NotifyCollectionChangedAction.Reset);
                    MMDC.OnModelChanged(e);
                }
                #endregion L o c a l F x
            }
        }

        class StateRunnerAsyncMMDC : StateRunnerAsync
        {
            public StateRunnerAsyncMMDC(ModeledMarkdownContext<T> mmdc) => MMDC = mmdc;

            ModeledMarkdownContext<T> MMDC { get; }
            public override async Task<Enum> ExecStateAsync(Enum state, object? context)
            {
                if(state.GetCustomAttribute<SynchronousStateAttribute>() is null)
                {
                    return await Task.Run(() => MMDC.StateRunner.ExecState(state, context));
                }
                else
                {
                    return MMDC.StateRunner.ExecState(state, context);
                }
            }
        }
    }
}
