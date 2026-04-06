using IVSoftware.Portable.Collections.Common;
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
#if false
        /// <summary>
        /// Normalizes supported collection change EventArgs into a unified action and payload.
        /// </summary>
        /// <remarks>
        /// - Supports NotifyCollectionChangedEventArgs and internal changing variants.
        /// - Outputs canonical action, items, and indices for downstream application.
        /// - Returns false only after throwing for unsupported inputs.
        /// </remarks>
        internal static bool TryNormalizeTargets(
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
#endif

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

        internal static ModelPreviewDelegate GetDescriptionPreviewDlgt(this Type type)
        {
            if (type.GetProperty("Description") is { } pi)
            {
                return localCompileDelegate(pi);

                ModelPreviewDelegate localCompileDelegate(PropertyInfo pi)
                {
                    var instanceParam = Expression.Parameter(typeof(object), "item");

                    // (T)item
                    var castInstance = Expression.Convert(instanceParam, pi.DeclaringType!);

                    // ((T)item).Description
                    var propertyAccess = Expression.Property(castInstance, pi);

                    // string? -> ensure reference type
                    var description = Expression.Convert(propertyAccess, typeof(string));

                    // description == null
                    var nullCheck = Expression.Equal(description, Expression.Constant(null, typeof(string)));

                    // description.PadRight(10)
                    var padRight = Expression.Call(
                        description,
                        nameof(string.PadRight),
                        Type.EmptyTypes,
                        Expression.Constant(10)
                    );

                    // description.PadRight(10).Substring(0, 10)
                    var substring = Expression.Call(
                        padRight,
                        nameof(string.Substring),
                        Type.EmptyTypes,
                        Expression.Constant(0),
                        Expression.Constant(10)
                    );

                    // null ? "Not Found" : substring
                    var body = Expression.Condition(
                        nullCheck,
                        Expression.Constant("Not Found"),
                        substring
                    );

                    var lambda = Expression.Lambda<ModelPreviewDelegate>(body, instanceParam);
                    return lambda.Compile();
                }
            }
            else
            {
                throw new NotSupportedException($"No delegate is registered for {type.Name}");
            }
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
                model.SetStdAttributeValue(StdModelAttribute.modeling, modeling);
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
