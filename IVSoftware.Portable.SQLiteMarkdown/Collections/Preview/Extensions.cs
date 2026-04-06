using IVSoftware.Portable.Collections.Common;
using IVSoftware.Portable.Collections.Common.Internal;
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

        private static TolerantDictionary<Type, TolerantDictionary<string, object>> _typeCache = new();
        private static TolerantDictionary<string, object> GetCacheForType(this Type @this)
        {
            if (_typeCache[@this] is not { } cache)
            {
                _typeCache[@this] = cache = new();
            }
            return cache;
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
        /// Resolves and caches the highest-fidelity modeling capability for a type.
        /// </summary>
        /// <remarks>
        /// - Probes known capability surfaces (Id, FullPath, Description, Text)
        ///   in priority order.
        /// - Uses SQLite mapping (PK) when available, falling back to named properties.
        /// - Compiles a fast accessor delegate for the resolved property.
        /// - Result is cached per type to avoid repeated reflection and compilation.
        /// </remarks>
        internal static ModeledFullPathInfo GetModeledPathInfo(this Type @this)
        {
            var mccache = @this.GetCacheForType();

            if (mccache[nameof(StdModelPath)] is ModeledFullPathInfo cached)
            {
                return cached;
            }
            ModeledFullPathInfo info = null!;
            PropertyInfo? fullPathPI = null;
            foreach (StdModelPath aspirant in Enum.GetValues(typeof(StdModelPath)))
            {
                switch (aspirant)
                {
                    case StdModelPath.Id:
                        fullPathPI = @this.GetSQLiteMapping()?.PK?.PropertyInfo;
                        if (fullPathPI is null)
                        {
                            fullPathPI = @this.GetProperty(aspirant.ToString());
                        }
                        break;

                    case StdModelPath.ModelPathAttribute:
                        fullPathPI = @this
                            .GetProperties()
                            .FirstOrDefault(p =>
                                p.GetCustomAttribute<ModelPathAttribute>(inherit: true) is not null);
                        break;
                    case StdModelPath.FullPath:
                    case StdModelPath.Description:
                    case StdModelPath.Text:
                    case StdModelPath.NotFound:
                        fullPathPI = @this.GetProperty(aspirant.ToString());
                        break;

                    default:
                        @this.ThrowHard<NotSupportedException>($"The {aspirant.ToFullKey()} case is not supported.");
                        break;
                }

                if (fullPathPI is not null)
                {
                    info = new ModeledFullPathInfo
                    {
                        StdModelPath = aspirant,
                        GetPath = localCompileDelegate(fullPathPI),
                    };
                    GetPathDlgt localCompileDelegate(PropertyInfo pi)
                    {
                        var instanceParam = Expression.Parameter(typeof(object), "obj");

                        var castInstance = Expression.Convert(instanceParam, pi.DeclaringType!);
                        var propertyAccess = Expression.Property(castInstance, pi);

                        var castResult = Expression.Convert(propertyAccess, typeof(string));

                        var lambda = Expression.Lambda<GetPathDlgt>(castResult, instanceParam);
                        return lambda.Compile();
                    }
                    break;
                }
            }

            if (info is null)
            {
                @this.ThrowHard<InvalidOperationException>("ModelingCapability cannot be resolved.");
                info = new ModeledFullPathInfo
                {
                    StdModelPath = StdModelPath.NotFound,
                    GetPath = null,
                };
            }
            mccache[nameof(StdModelPath)] = info;
            return info;
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
                            xel.Name = nameof(StdModelElement.xitem);
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
