using IVSoftware.Portable.Common.Collections;
using IVSoftware.Portable.Common.Collections.Internal;
using IVSoftware.Portable.Collections.Modeled;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown;
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
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace IVSoftware.Portable.Collections.Preview
{
    static class Extensions
    {
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
            // ROUTE TO LOCAL FUNCTIONS
            switch (action)
            {
                case NotifyCollectionChangeAction.Add:
                    localCompatibleAddToList();
                    break;
                case NotifyCollectionChangeAction.Remove:
                    localRemoveFromList();
                    break;
                case NotifyCollectionChangeAction.Replace:
                    localReplaceInList();
                    break;
                case NotifyCollectionChangeAction.Move:
                    localMoveInList();
                    break;
                case NotifyCollectionChangeAction.Reset:
                    localResetList();
                    break;
                case NotifyCollectionChangeAction.Digest:
                    localExecutePlaylist();
                    break;
                default:
                    nameof(Extensions)
                        .ThrowFramework<NotSupportedException>(
                        $"The {eUnk.GetType().Name} case is not supported.");
                    break;
            }

            #region L o c a l F x
            void localCompatibleAddToList()
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
                    if (index == list.Count)
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

            void localExecutePlaylist()
            {
                int 
                    removeOffset = 0,
                    removeThreshold = int.MaxValue;
                foreach (var eUnk in newItems ?? Array.Empty<EventArgs>())
                {
                    NotifyCollectionChangingEventArgs eApply; 
                    switch (eUnk)
                    {
                        case NotifyCollectionChangingEventArgs ePre:
                            if(ePre.Scope == NotifyCollectionChangeScope.FullControl)
                            {
                                eApply = ePre;
                            }
                            else
                            {
                                eApply = new NotifyCollectionChangingEventArgs(ePre, scope: NotifyCollectionChangeScope.FullControl);
                            }
                            break;
                        case NotifyCollectionChangedEventArgs ePost:
                            eApply = new NotifyCollectionChangingEventArgs(ePost, scope: NotifyCollectionChangeScope.FullControl);
                            break;
                        default:
                            list.ThrowFramework<NotSupportedException>($"The {eUnk.GetType().Name} case is not supported.");
                            return;
                    }
                    if( removeOffset != 0
                        && eApply.NewStartingIndex != -1 
                        && eApply.NewStartingIndex > removeThreshold)
                    {
                        eApply.NewStartingIndex -= removeOffset;
                        if(eApply.NewStartingIndex < 0)
                        {
                            list.ThrowFramework<IndexOutOfRangeException>($"{nameof(eApply.NewStartingIndex)} cannot be negative.");
                            return;
                        }
                    }
                    if( removeOffset != 0
                        && eApply.OldStartingIndex != -1 
                        && eApply.OldStartingIndex > removeThreshold)
                    {
                        eApply.OldStartingIndex -= removeOffset;
                        if (eApply.OldStartingIndex < 0)
                        {
                            list.ThrowFramework<IndexOutOfRangeException>($"{nameof(eApply.OldStartingIndex)} cannot be negative.");
                            return;
                        }
                    }
                    list.Apply(eApply);
                    if(eApply.Action == NotifyCollectionChangeAction.Remove)
                    {
                        if( eApply.OldStartingIndex != -1
                            && eApply.OldStartingIndex < removeThreshold)
                        {
                            removeThreshold = eApply.OldStartingIndex;
                        }
                        removeOffset++;
                    }
                }
            }
            #endregion L o c a l F x
        }
        /// <summary>
        /// Builds a canonical XElement model and returns its string form.
        /// </summary>
        /// <remarks>
        /// - Enumerates items and places each by full path into the model tree.
        /// - Sets modeling metadata and order; attaches bound model reference.
        /// - Optionally applies preview via resolved delegate.
        /// - Throws if path or placement result is invalid.
        /// </remarks>
        public static string ToString(this IList @this, out XElement model)
        {
            var itemType = @this.GetItemType();
            var previewDlgt = itemType?.GetDescriptionPreviewDlgt();
            var modeling = itemType?.GetModeledPathInfo().StdModelPath;

            model = new XElement(nameof(StdModelElement.model));
            if(modeling is not null)
            {
                model.SetStdAttributeValue(StdModelAttribute.mpath, modeling);
            }
#if DEBUG
            var count = @this.Count;
            var length = @this.Cast<object>().ToArray().Length;
            if(count != length)
            {
                Debug.Fail($@"ADVISORY - Indicates leakage in interface implementation.");
            }
#endif
            var itemCount = 0;
            foreach (var item in @this)
            {
                if (item.GetFullPath() is { } full && !string.IsNullOrWhiteSpace(full))
                {
                    var placerResult = model.Place(full, out var xel);
                    switch (placerResult)
                    {
                        case PlacerResult.Exists:
                            break;
                        case PlacerResult.Created:
                            xel.Name = nameof(StdModelElement.item);
                            xel.SetBoundAttributeValue(
                                tag: item,
                                name: nameof(StdModelAttribute.model));
                            xel.SetAttributeValue(nameof(StdModelAttribute.order), itemCount++);
                            if (previewDlgt?.Invoke(item) is string preview)
                            {
                                xel.SetStdAttributeValue(StdModelAttribute.preview, preview);
                            }
                            break;
                        default:
                            @this.ThrowFramework<NotSupportedException>(
                                $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                            break;
                    }
                }
                else
                {
                    @this.ThrowHard<NullReferenceException>("Expecting object type specifies a [PrimaryKey].");
                }
            }
            return model.ToString();
        }

        public static Type? GetItemType(this IList @this)
        {
            Type listType = @this.GetType();

            if (listType.IsGenericType
                && listType.GetGenericArguments().Length == 1
                && listType.GetGenericArguments()[0] is { } itemType)
            {
                return itemType;
            }
            else return null;
        }
    }
}
