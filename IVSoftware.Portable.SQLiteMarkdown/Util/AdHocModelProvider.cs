using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using IVSoftware.Portable.Collections.Common;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    public class AdHocModelProvider<T>
    {
        public AdHocModelProvider(IList<T> itemsSource) 
        {
            ItemsSource = itemsSource;
        }
        private IList<T> ItemsSource { get; }
        public XElement CreateModel()
        {
            XElement model = new XElement(nameof(StdModelElement.model));
            model.SetAttributeValue(ModelingCapability);
            int itemCount = 0;
            if (ModelingCapability != StdModelPath.NotFound)
            {
                foreach (var item in ItemsSource)
                {
                    if (GetFullPathDlgt?.Invoke(item) is { } fullPath)
                    {
                        if (string.IsNullOrWhiteSpace(fullPath))
                        {
                            "ObservablePreviewCollection".ThrowHard<ArgumentException>($"The '{nameof(fullPath)}' argument cannot be empty.");
                            continue;
                        }
                        var placerResult = model.Place(fullPath, out var xel);
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
                                break;
                            default:
                                "ObservablePreviewCollection".ThrowFramework<NotSupportedException>(
                                    $"Unexpected result: `{placerResult.ToFullKey()}`. Expected options are {PlacerResult.Created} or {PlacerResult.Exists}");
                                break;
                        }
                    }
                }
            }
            return model;
        }
        /// <summary>
        /// Determine the highest fidelity full path for T.
        /// </summary>
        public StdModelPath ModelingCapability
        {
            get
            {
                if (_modelingCapability is null)
                {
                    var type = typeof(T);
                    foreach (StdModelPath capability in Enum.GetValues(typeof(StdModelPath)))
                    {
                        _modelingCapability = capability;
                        switch (capability)
                        {
                            case StdModelPath.Id:
                                _fullPathPI = type.GetSQLiteMapping()?.PK?.PropertyInfo;
                                if (_fullPathPI is null)
                                {
                                    _fullPathPI = type.GetProperty(capability.ToString());
                                }
                                if (_fullPathPI is null) // Still...
                                {
                                    break;
                                }
                                else
                                {
                                    goto breakFromInner;
                                }
                            case StdModelPath.FullPath:
                            case StdModelPath.Description:
                            case StdModelPath.Text:
                            case StdModelPath.NotFound:
                                _fullPathPI = type.GetProperty(capability.ToString());
                                if (_fullPathPI is null)
                                {
                                    break;
                                }
                                else
                                {
                                    goto breakFromInner;
                                }
                            default:
                                this.ThrowHard<NotSupportedException>($"The {capability.ToFullKey()} case is not supported.");
                                _modelingCapability = StdModelPath.NotFound;
                                // If handled, allow loop to continue;
                                break;
                        }
                    }
                }
                breakFromInner:
                return (StdModelPath)_modelingCapability!;
            }
        }
        StdModelPath? _modelingCapability = null;
        PropertyInfo? _fullPathPI = null;

        public GetFullPathDelegate<T>? GetFullPathDlgt
        {
            get
            {
                if (ModelingCapability == StdModelPath.NotFound)
                {
                    return null;
                }
                else
                {
                    if (_getFullPath is null)
                    {
                        var instance = Expression.Parameter(typeof(T), "item");
                        var property = Expression.Property(instance, _fullPathPI);

                        Expression body =
                            property.Type == typeof(string)
                            ? property
                            : Expression.Call(property, nameof(object.ToString), Type.EmptyTypes);

#if DEBUG
                        Debug.WriteLine($"260331.A {Expression.Lambda<GetFullPathDelegate<T>>(body, instance)}");
                        { }
#endif

                        _getFullPath =
                            Expression.Lambda<GetFullPathDelegate<T>>(body, instance)
                            .Compile();
                    }
                    return _getFullPath;
                }
            }
        }
        GetFullPathDelegate<T>? _getFullPath;
    }
}
