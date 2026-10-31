using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Events;
using IVSoftware.Portable.Collections.Internal;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace IVSoftware.Portable.Collections
{
    internal static class ExtensionsInternal
    {
        /// <summary>
        /// Builds a canonical XElement model and returns its string form.
        /// </summary>
        /// <remarks>
        /// - Enumerates items and places each by full path into the model tree.
        /// - Sets modeling metadata and order; attaches bound model reference.
        /// - Optionally applies preview via resolved delegate.
        /// - Throws if path or placement result is invalid.
        /// </remarks>
        public static string ToString(this IList @this, out XElement model, int previewLength = 10)
        {
            var itemType = @this.GetItemType();
            var previewDlgt = itemType?.GetDescriptionPreviewDlgt();
            var mpath = itemType?.GetModeledPathInfo().StdModelPath;

            model = new XElement(nameof(StdModelElement.model));
            if(mpath is not null)
            {
                model.SetStdAttributeValue(StdModelAttribute.mpath, mpath);
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
                            xel.SetAttributeValue(nameof(StdModelAttribute.index), itemCount++);
                            if (previewLength > 0 && previewDlgt?.Invoke(item, previewLength) is string preview)
                            {
                                xel.InsertPreviewAttributeAfter(StdModelAttribute.model);
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
