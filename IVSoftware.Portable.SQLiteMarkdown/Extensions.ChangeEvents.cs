using IVSoftware.Portable.Collections;
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
using System.Text;
using System.Xml.Linq;

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

                case MutableNotifyCollectionChangingEventArgs e:
                    action = e.Action;
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

            int
                indexForAdd = model.GetAttributeValue<int>(StdMarkdownAttribute.autocount),
                count = model.GetAttributeValue<int>(StdMarkdownAttribute.count, 0),
                matches = model.GetAttributeValue<int>(StdMarkdownAttribute.matches);

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
            model.SetAttributeValue(nameof(StdMarkdownAttribute.count), count);
            model.SetAttributeValue(nameof(StdMarkdownAttribute.matches), matches);

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

                                    xel.SetAttributeValue(nameof(StdMarkdownAttribute.sort), indexForAdd++);
                                    count++;
                                    matches++;
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
                                        count = Math.Max(0, count - 1);
                                        matches = Math.Max(0, matches - 1);
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
                                        count = Math.Max(0, count - 1);
                                        matches = Math.Max(0, matches - 1);
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

                                    xel.SetAttributeValue(nameof(StdMarkdownAttribute.sort), indexForAdd++);
                                    count++;
                                    matches++;
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
                                    xel.SetAttributeValue(nameof(StdMarkdownAttribute.sort), targetIndex++);
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
                }
                else
                {
                    Debug.Fail($@"ADVISORY - TODO distinguish ReplaceItemsEventingOption.");
                    model.RemoveNodes();
                }
                count = 0;
                matches = 0;
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
                    foreach (var item in newItems)
                    {
                        list.Insert(index++, item);
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
                if (newItems is null || oldItems is null || newStartingIndex < 0)
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
        }
    }
}
