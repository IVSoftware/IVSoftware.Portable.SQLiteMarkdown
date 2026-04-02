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

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
    /// <summary>
    /// Listed in order of preference.
    /// </summary>
    public enum ModeledPathProperty
    {
        /// <summary>
        /// Detected a string property named FullPath.
        /// </summary>
        FullPath,

        /// <summary>
        /// A [PrimaryKey] property or a string property named Id.
        /// </summary>
        Id,

        /// <summary>
        /// A [PrimaryKey] property or a string property named Description.
        /// </summary>
        Description,

        /// <summary>
        /// A [PrimaryKey] property or a string property named Text.
        /// </summary>
        Text,

        /// <summary>
        /// Failed to find a suitable modeling property.
        /// </summary>
        Unavailable,
    }
    public class AdHocModelProvider<T>
    {
        public AdHocModelProvider(IList<T> itemsSource) 
        {
            ItemsSource = itemsSource;
        }
        private IList<T> ItemsSource { get; }
        public XElement CreateModel()
        {
            XElement model = new XElement(nameof(StdMarkdownElement.model));
            model.SetAttributeValue(ModelingCapability);
            int itemCount = 0;
            if (ModelingCapability != ModeledPathProperty.Unavailable)
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
                                xel.Name = nameof(StdMarkdownElement.xitem);
                                xel.SetBoundAttributeValue(
                                    tag: item,
                                    name: nameof(StdMarkdownAttribute.model));

                                xel.SetAttributeValue(nameof(StdMarkdownAttribute.order), itemCount++);
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
        public ModeledPathProperty ModelingCapability
        {
            get
            {
                if (_modelingCapability is null)
                {
                    var type = typeof(T);
                    foreach (ModeledPathProperty capability in Enum.GetValues(typeof(ModeledPathProperty)))
                    {
                        _modelingCapability = capability;
                        switch (capability)
                        {
                            case ModeledPathProperty.Id:
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
                            case ModeledPathProperty.FullPath:
                            case ModeledPathProperty.Description:
                            case ModeledPathProperty.Text:
                            case ModeledPathProperty.Unavailable:
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
                                _modelingCapability = ModeledPathProperty.Unavailable;
                                // If handled, allow loop to continue;
                                break;
                        }
                    }
                }
                breakFromInner:
                return (ModeledPathProperty)_modelingCapability!;
            }
        }
        ModeledPathProperty? _modelingCapability = null;
        PropertyInfo? _fullPathPI = null;

        public GetFullPathDelegate<T>? GetFullPathDlgt
        {
            get
            {
                if (ModelingCapability == ModeledPathProperty.Unavailable)
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
