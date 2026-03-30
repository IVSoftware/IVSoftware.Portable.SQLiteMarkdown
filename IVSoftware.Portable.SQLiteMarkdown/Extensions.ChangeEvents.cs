using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    partial class Extensions
    {
        /// <summary>
        /// Normalizes supported collection change EventArgs into a unified action and payload.
        /// </summary>
        /// <remarks>
        /// - Supports NotifyCollectionChangedEventArgs and internal changing variants.
        /// - Outputs canonical action, items, and indices for downstream application.
        /// - Returns false only after throwing for unsupported inputs.
        /// </remarks>
        public static bool TryNormalizeTargets(
            this EventArgs eUnk,
            out NotifyCollectionChangeAction action,
            out NotifyCollectionChangeReason reason,
            out IList? newItems,
            out int newStartingIndex,
            out IList? oldItems,
            out int oldStartingIndex
            )
        {
            action = default!;
            reason = NotifyCollectionChangeReason.None;
            newItems = null;
            oldItems = null;
            newStartingIndex = -1;
            oldStartingIndex = -1;
            switch (eUnk)
            {
                case NotifyCollectionChangedEventArgs e:
                    action = (NotifyCollectionChangeAction)e.Action;
                    newItems = e.NewItems;
                    oldItems = e.OldItems;
                    newStartingIndex = e.NewStartingIndex;
                    oldStartingIndex = e.OldStartingIndex;
                    return true;

                case NotifyCollectionChangingEventArgs e:
                    action = e.Action;
                    reason = e.Reason;
                    newItems = e.NewItems;
                    oldItems = e.OldItems;
                    newStartingIndex = e.NewStartingIndex;
                    oldStartingIndex = e.OldStartingIndex;
                    return true;
                default:
                    nameof(Extensions)
                        .ThrowFramework<NotSupportedException>($"The {eUnk.GetType().Name} case is not supported.");
                    return false;
            }
        }

        /// <summary>
        /// Applies a normalized collection change to the XML model.
        /// </summary>
        /// <remarks>
        /// - Delegates structural updates to local handlers based on action.
        /// - Consumes normalized payload (items, indices) without reinterpreting EventArgs.
        /// - Intended as the authoritative model mutation entry point.
        /// </remarks>
        public static void Apply(this XElement model, EventArgs eUnk)
        {
            if (!eUnk.TryNormalizeTargets(
                out var action,
                out var reason,
                out var newItems,
                out var newStartingIndex,
                out var oldItems,
                out var oldStartingIndex))
            {
                eUnk.ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name} case is not supported.");
                return;
            }
            var mdc = model.To<IMarkdownContext>(@throw: true);
            var histo = model.To<EnumHistogrammer<StdMarkdownAttribute>>(@throw: true);
            int itemCount = histo[StdMarkdownAttribute.model];

            switch (action)
            {
                case NotifyCollectionChangeAction.Add: localAddToModel(); break;
                case NotifyCollectionChangeAction.Remove: localRemoveFromModel(); break;
                case NotifyCollectionChangeAction.Replace: localReplaceInModel(); break;
                case NotifyCollectionChangeAction.Move: localMoveInModel(); break;
                case NotifyCollectionChangeAction.Reset: localResetModel(); break;
                default:
                    eUnk.ThrowFramework<NotSupportedException>(
                            $"The {action.ToFullKey()} case is not supported.");
                    break;
            }

            #region L o c a l F x
            void localAddToModel()
            {
                if (newItems is null)
                {
                    eUnk.ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    foreach (var item in newItems)
                    {
                        if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                        {
                            var placerResult = model.Place(full, out var xel);
                            switch (placerResult)
                            {
                                case PlacerResult.Exists:
                                    break;
                                case PlacerResult.Created:
                                    xel.Name = nameof(StdMarkdownElement.xitem);
                                    xel.SetBoundAttributeValue(
                                        tag: item,
                                        name: nameof(StdMarkdownAttribute.model));

                                    xel.SetAttributeValue(nameof(StdMarkdownAttribute.order), itemCount++);
                                    if (mdc.IsFiltering)
                                    {
                                        Debug.Assert(DateTime.Now.Date == new DateTime(2026, 3, 30).Date, "Don't forget disabled");
                                        // Make sure we mark canonical match until next filter op.
                                        // matches++;
                                    }
                                    break;
                                default:
                                    eUnk.ThrowFramework<NotSupportedException>(
                                        $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                                    break;
                            }
                        }
                        else
                        {
                            eUnk.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                        }
                    }
                }
            }

            void localRemoveFromModel()
            {
                Debug.Fail($@"ADVISORY - First Time.");
                if (oldItems is null)
                {
                    eUnk.ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    foreach (var item in oldItems)
                    {
                        if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                        {
                            var placerResult = model.Place(full, out var xel, PlacerMode.FindOrPartial);
                            switch (placerResult)
                            {
                                case PlacerResult.Exists:
                                    if (xel.Parent is not null)
                                    {
                                        xel.Remove();
                                    }
                                    break;
                                default:
                                    eUnk.ThrowFramework<NotSupportedException>(
                                        $"Unexpected result: `{placerResult.ToFullKey()}`. Expected option is {PlacerResult.Exists}");
                                    break;
                            }
                        }
                        else
                        {
                            eUnk.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                        }
                    }
                }
            }

            void localReplaceInModel()
            {
                Debug.Fail($@"ADVISORY - First Time.");
                if (newItems is null || oldItems is null)
                {
                    eUnk.ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    // REMOVE PHASE
                    foreach (var item in oldItems)
                    {
                        if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                        {
                            var placerResult = model.Place(full, out var xel, PlacerMode.FindOrPartial);
                            switch (placerResult)
                            {
                                case PlacerResult.Exists:
                                    if (xel.Parent is not null)
                                    {
                                        xel.Remove();
                                    }
                                    break;
                                default:
                                    eUnk.ThrowFramework<NotSupportedException>(
                                        $"Unexpected result: `{placerResult.ToFullKey()}`. Expected option is {PlacerResult.Exists}");
                                    break;
                            }
                        }
                        else
                        {
                            eUnk.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                        }
                    }

                    // ADD PHASE
                    foreach (var item in newItems)
                    {
                        if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                        {
                            var placerResult = model.Place(full, out var xel);
                            switch (placerResult)
                            {
                                case PlacerResult.Exists:
                                    break;

                                case PlacerResult.Created:
                                    xel.Name = nameof(StdMarkdownElement.xitem);
                                    xel.SetBoundAttributeValue(
                                        tag: item,
                                        name: nameof(StdMarkdownAttribute.model));

                                    xel.SetAttributeValue(nameof(StdMarkdownAttribute.order), itemCount++);
                                    break;

                                default:
                                    eUnk.ThrowFramework<NotSupportedException>(
                                        $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                                    break;
                            }
                        }
                        else
                        {
                            eUnk.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                        }
                    }
                }
            }

            void localMoveInModel()
            {
                Debug.Fail($@"ADVISORY - First Time.");
                if (oldItems is null)
                {
                    eUnk.ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    int targetIndex = newStartingIndex;

                    foreach (var item in oldItems)
                    {
                        if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                        {
                            var placerResult = model.Place(full, out var xel, PlacerMode.FindOrPartial);
                            switch (placerResult)
                            {
                                case PlacerResult.Exists:
                                    xel.SetAttributeValue(nameof(StdMarkdownAttribute.order), targetIndex++);
                                    break;

                                default:
                                    eUnk.ThrowFramework<NotSupportedException>(
                                        $"Unexpected result: `{placerResult.ToFullKey()}`. Expected option is {PlacerResult.Exists}");
                                    break;
                            }
                        }
                        else
                        {
                            eUnk.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                        }
                    }
                }
            }

            void localResetModel()
            {
                if (reason == NotifyCollectionChangeReason.None)
                {
                    model.RemoveNodes();
                    histo.Clear();
                }
                else
                {
                    Debug.Fail($@"ADVISORY - TODO distinguish ReplaceItemsEventingOption.");
                    model.RemoveNodes();
                }
            }
            #endregion L o c a l F x
        }

        /// <summary>
        /// Applies a normalized collection change to a list target.
        /// </summary>
        /// <remarks>
        /// - Mirrors model application semantics for IList-backed projections.
        /// - Uses normalized action and payload for consistent mutation behavior.
        /// - Serves as the projection-side execution counterpart to model updates.
        /// </remarks>
        public static void Apply(this IList list, EventArgs eUnk)
        {
            if (!eUnk.TryNormalizeTargets(
                out var action,
                out var reason,
                out var newItems,
                out var newStartingIndex,
                out var oldItems,
                out var oldStartingIndex))
            {
                nameof(Extensions)
                    .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name} case is not supported.");
                return;
            }

            if (newItems?.Count > 0 && newItems[0] is EventArgs)
            {
                throw new NotSupportedException("Batch Event playlists are not supported (yet)");
            }
            else
            {
                // Apply using standard BCL semantics.
                switch (action)
                {
                    case NotifyCollectionChangeAction.Add: localAddToList(); break;
                    case NotifyCollectionChangeAction.Remove: localRemoveFromList(); break;
                    case NotifyCollectionChangeAction.Replace: localReplaceInList(); break;
                    case NotifyCollectionChangeAction.Move: localMoveInList(); break;
                    case NotifyCollectionChangeAction.Reset: localResetList(); break;
                    default:
                        nameof(Extensions)
                            .ThrowFramework<NotSupportedException>(
                            $"The {eUnk.GetType().Name} case is not supported.");
                        break;
                }
            }

            #region L o c a l F x
            void localAddToList()
            {
                if (newItems is null || newStartingIndex < 0)
                {
                    nameof(Extensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    var index = newStartingIndex;
                    if(index == list.Count)
                    {
                        // Minor optimization avoids shifting cost.
                        foreach (var item in newItems)
                        {
                            list.Add(item);
                        }
                    }
                    else
                    {
                        foreach (var item in newItems)
                        {
                            list.Insert(index++, item);
                        }
                    }
                }
            }

            void localRemoveFromList()
            {
                if (oldItems is null || oldStartingIndex < 0)
                {
                    nameof(Extensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    // Remove at index repeatedly (items shift left)
                    for (int i = 0; i < oldItems.Count; i++)
                    {
                        list.RemoveAt(oldStartingIndex);
                    }
                }
            }

            void localMoveInList()
            {
                if (oldItems is null || oldStartingIndex < 0 || newStartingIndex < 0)
                {
                    nameof(Extensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    // Preserve order of moved block
                    var buffer = new object[oldItems.Count];
                    for (int i = 0; i < oldItems.Count; i++)
                    {
                        buffer[i] = list[oldStartingIndex];
                        list.RemoveAt(oldStartingIndex);
                    }

                    var insertIndex = newStartingIndex;
                    foreach (var item in buffer)
                    {
                        list.Insert(insertIndex++, item);
                    }
                }
            }

            void localReplaceInList()
            {
                if (newItems is null
                    || oldItems is null
                    || newStartingIndex < 0
                    || oldStartingIndex < 0
                    || newStartingIndex != oldStartingIndex
                    || newItems.Count != oldItems.Count
                    || newStartingIndex + newItems.Count > list.Count)
                {
                    nameof(Extensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name}.{action} is improperly provisioned for this action.");
                }
                else
                {
                    // Replace in place
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        list[newStartingIndex + i] = newItems[i];
                    }
                }
            }

            void localResetList()
            {
                list.Clear();
            }
            #endregion L o c a l F x
        }

        /// <summary>
        /// Produces a reconciliation playlist for listBefore -> listAfter.
        /// </summary>
        internal static NotifyCollectionChangingEventArgs Diff(
            this IList listBefore,
            IList listAfter,
            NotifyCollectionChangeScope scope = NotifyCollectionChangeScope.ReadOnly,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            int current = 0;
            EnumHistogrammer<NotifyCollectionChangeAction> histo = new(ZeroCountOption.IncrementOnly);
            List<NotifyCollectionChangingEventArgs> changes = new();

            object? replace, replaceWith;
            int? newStartingIndex = null, oldStartingIndex = null;

            while(current < listBefore.Count && current < listAfter.Count )
            {
                newStartingIndex ??= current;
                oldStartingIndex ??= current;

                replace = listBefore[current];
                replaceWith = listAfter[current];

                changes.Add(new(
                    action: NotifyCollectionChangeAction.Replace,
                    newItems: new[] { listAfter[current] },
                    newStartingIndex: current,
                    oldItems: new[] { listBefore[current] },
                    oldStartingIndex: current));
                histo.Increment(NotifyCollectionChangeAction.Replace);
                current++;
            }

            // Block of contiguous adds.
            while (current < listAfter.Count)
            {
                newStartingIndex ??= current;
                changes.Add(new (
                    action: NotifyCollectionChangeAction.Add,
                    newItems: new []{listAfter[current] },
                    newStartingIndex: current));
                histo.Increment(NotifyCollectionChangeAction.Add);
                current++;
            }

            // Block of contiguous removes.
            while(current < listBefore.Count)
            {
                oldStartingIndex ??= current;
                changes.Add(new(
                    action: NotifyCollectionChangeAction.Remove,
                    oldItems: new[] { listBefore[current] },
                    oldStartingIndex: current));
                histo.Increment(NotifyCollectionChangeAction.Remove);
                current++;
            }
#if DEBUG
            var cMe = histo.ToString(HistogrammerFormat.All);
            { }
#endif
            NotifyCollectionChangingEventArgs result;
            switch (histo.Count())
            {
                case 0:
                    result = new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Reset,
                        scope: scope,
                        reason: reason);
                    break;
                case 1:
                    switch (histo.First())
                    {
                        case NotifyCollectionChangeAction.Add:
                            result = new NotifyCollectionChangingEventArgs(
                                action: NotifyCollectionChangeAction.Add,
                                reason: reason,
                                scope: scope,
                                newStartingIndex: (int)newStartingIndex!,
                                newItems: changes
                                    .Where(_ => _.NewItems is not null)
                                    .SelectMany(_ => _.NewItems!.Cast<object>())
                                    .ToList());
                            break;
                        case NotifyCollectionChangeAction.Remove:
                            result = new NotifyCollectionChangingEventArgs(
                                action: NotifyCollectionChangeAction.Remove,
                                reason: reason,
                                scope: scope,
                                oldStartingIndex: (int)oldStartingIndex!,
                                oldItems: changes
                                    .Where(_ => _.OldItems is not null)
                                    .SelectMany(_ => _.OldItems!.Cast<object>())
                                    .ToList());
                            break;
                        case NotifyCollectionChangeAction.Replace:
                            result = new NotifyCollectionChangingEventArgs(
                                action: NotifyCollectionChangeAction.Replace,
                                reason: reason,
                                scope: scope,
                                oldStartingIndex: (int)oldStartingIndex!,
                                oldItems: changes
                                    .Where(_ => _.OldItems is not null)
                                    .SelectMany(_ => _.OldItems!.Cast<object>())
                                    .ToList());
                            break;
                        default:
                            result = new NotifyCollectionChangingEventArgs(
                                action: NotifyCollectionChangeAction.Reset,
                                scope: scope,
                                reason: reason | NotifyCollectionChangeReason.Exception);
                            break;
                    }
                    break;
                default:
                    throw new NotImplementedException("ToDo");
            }

            // Make sure this got assigned.
            if (result.Reason != reason)
            {
                nameof(Diff).ThrowFramework<NotSupportedException>("Failed to assign reason.");
            }
            return result;
        }

        /// <summary>
        /// Heuristically derives a batch <see cref="NotifyCollectionChangingEventArgs"/> describing
        /// the transition from <paramref name="listBefore"/> to <paramref name="listAfter"/>.
        /// </summary>
        /// <remarks>
        /// This method classifies the change into one of the following shapes:
        /// - <b>Reset</b>: No detectable delta.
        /// - <b>Add</b>: One or more items added, no removals.
        /// - <b>Remove</b>: One or more items removed, no additions.
        /// - <b>Replace (single)</b>: Exactly one item replaced with index fidelity.
        /// - <b>Replace (batch)</b>: Mixed add/remove operations encoded as a sequence of
        ///   micro-operations carried in <see cref="NotifyCollectionChangingEventArgs.NewItems"/>.
        ///
        /// For batch replace:
        /// - Each element is an anonymous payload describing an atomic operation
        ///   (Add, Remove, or Replace) with associated index information.
        /// - Positional intent is preserved where possible via <c>OldIndex</c> and <c>NewIndex</c>.
        ///
        /// This is a heuristic diff:
        /// - Uses set-based comparison (<c>Except</c>) and therefore does not account for duplicates
        ///   or stable ordering beyond index lookup.
        /// - Complex rearrangements may degrade to a batch replace envelope.
        /// - Consumers are expected to interpret batch payloads or fall back to reset semantics,
        ///   optionally using an externally supplied final state.
        ///
        /// All emitted events carry <see cref="NotifyCollectionChangeReason.Batch"/>.
        /// </remarks>
        internal static NotifyCollectionChangingEventArgs DiffOR(
            this IList listBefore,
            IList listAfter,
            NotifyCollectionChangeScope scope = NotifyCollectionChangeScope.ReadOnly,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None)
        {
            int
                current = 0,
                countB4 = listBefore.Count,
                countAfter = listAfter.Count;

            NotifyCollectionChangingEventArgs? result = null;

            if (countAfter == 0)
            {
                if (countB4 == 0)
                {
                    result = new NotifyCollectionChangingEventArgs(
                        NotifyCollectionChangeAction.Reset,
                        reason: reason,
                        scope: scope);                        
                }
                else
                {
                    var ops = new List<object>();
                    for (int i = 0; i < countB4; i++)
                    {
                        ops.Add(new
                        {
                            Action = NotifyCollectionChangeAction.Remove,
                            Item = listBefore[i],
                            OldIndex = i
                        });
                    }

                    result = new NotifyCollectionChangingEventArgs(
                        NotifyCollectionChangeAction.Replace,
                        scope: scope,
                        reason: reason,
                        newItems: ops);
                }
            }
            else
            {
                string path1, path2;

                var replaces = new List<(object OldItem, object NewItem, int Index)>();
                var adds = new List<(object Item, int Index)>();
                var removes = new List<(object Item, int Index)>();

                while (current < countB4 && current < countAfter)
                {
                    var itemB4 = listBefore[current];
                    var itemAfter = listAfter[current];

                    if (localTryGetFullPath(itemB4, out path1) &&
                        localTryGetFullPath(itemAfter, out path2))
                    {
                        if (!string.Equals(path1, path2, StringComparison.Ordinal))
                        {
                            replaces.Add((itemB4, itemAfter, current));
                        }
                    }

                    current++;
                }

                while (current < countB4)
                {
                    removes.Add((listBefore[current], current));
                    current++;
                }

                while (current < countAfter)
                {
                    adds.Add((listAfter[current], current));
                    current++;
                }

                if (replaces.Count == 1 && adds.Count == 0 && removes.Count == 0)
                {
                    var r = replaces[0];

                    result = new NotifyCollectionChangingEventArgs(
                        NotifyCollectionChangeAction.Replace,
                        scope: scope,
                        reason: reason,
                        newItems: new[] { r.NewItem },
                        oldItems: new[] { r.OldItem },
                        newStartingIndex: r.Index,
                        oldStartingIndex: r.Index);
                }
                else if (replaces.Count == 0 && adds.Count > 0 && removes.Count == 0)
                {
                    var items = adds.Select(a => a.Item).ToList();
                    var startIndex = adds[0].Index;

                    result = new NotifyCollectionChangingEventArgs(
                        NotifyCollectionChangeAction.Add,
                        scope: scope,
                        reason: reason,
                        newItems: items,
                        newStartingIndex: startIndex);
                }
                else if (replaces.Count == 0 && removes.Count > 0 && adds.Count == 0)
                {
                    var items = removes.Select(r => r.Item).ToList();
                    var startIndex = removes[0].Index;

                    result = new NotifyCollectionChangingEventArgs(
                        NotifyCollectionChangeAction.Remove,
                        scope: scope,
                        reason: reason,
                        oldItems: items,
                        oldStartingIndex: startIndex);
                }
                else if (replaces.Count == 0 && adds.Count == 0 && removes.Count == 0)
                {
                    result = new NotifyCollectionChangingEventArgs(
                        NotifyCollectionChangeAction.Reset,
                        scope: scope,
                        reason: reason);
                }
                else
                {
                    var ops = new List<object>();

                    foreach (var r in replaces)
                    {
                        ops.Add(new
                        {
                            Action = NotifyCollectionChangeAction.Replace,
                            OldItem = r.OldItem,
                            NewItem = r.NewItem,
                            OldIndex = r.Index,
                            NewIndex = r.Index
                        });
                    }

                    foreach (var r in removes)
                    {
                        ops.Add(new
                        {
                            Action = NotifyCollectionChangeAction.Remove,
                            Item = r.Item,
                            OldIndex = r.Index
                        });
                    }

                    foreach (var a in adds)
                    {
                        ops.Add(new
                        {
                            Action = NotifyCollectionChangeAction.Add,
                            Item = a.Item,
                            NewIndex = a.Index
                        });
                    }

                    result = new NotifyCollectionChangingEventArgs(
                        NotifyCollectionChangeAction.Replace,
                        scope: scope,
                        reason: reason,
                        newItems: ops);
                }


                if (result is null)
                {
                    nameof(Diff).ThrowFramework<NotSupportedException>("Could not detect a valid layout configuration.");
                }
                else if(result.Reason != reason)
                {
                    nameof(Diff).ThrowFramework<NotSupportedException>("Failed to assign reason.");
                }

                static bool localTryGetFullPath(object? item, out string path)
                {
                    if (item is not null && item.GetFullPath() is { } aspirant && !string.IsNullOrWhiteSpace(aspirant))
                    {
                        path = aspirant;
                        return true;
                    }
                    else
                    {
                        item.ThrowHard<InvalidOperationException>("GetFullPath() failed for item.");
                        path = null!;
                        return false;
                    }
                }
            }

            return result!;
        }
    }
}
