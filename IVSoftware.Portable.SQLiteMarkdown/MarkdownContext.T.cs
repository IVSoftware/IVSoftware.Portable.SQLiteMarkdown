using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown
{
    public partial class MarkdownContext<T> : MarkdownContext
    {
        public MarkdownContext() : base(typeof(T)) { }
        public IReadOnlyList<T> PredicateMatchSubset { get; } = new List<T>();
        protected List<T> PredicateMatchSubsetProtected { get; } = new List<T>();
        protected override void OnModelSettled(NotifyCollectionChangedEventArgs eBCL)
        {
            base.OnModelSettled(eBCL);
            switch (ProjectionOption)
            {
                case NetProjectionOption.ObservableOnly:
                    break;
                case NetProjectionOption.AllowDirectChanges:
                    localApplyDirectChanges();
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {ProjectionOption.ToFullKey()} case is not supported.");
                    break;
            }
            #region L o c a l F x
            void localApplyDirectChanges()
            {
                if (eBCL is ModelSettledEventArgs eModel)
                {
                    if (ObservableNetProjection is IList<T> projection)
                    {
                        if (eBCL.OldItems is not null) foreach (T item in eBCL.OldItems)
                        {
                            projection.Remove(item);
                        }
                        if (eBCL.NewStartingIndex == -1)
                        {
                            if (eBCL.NewItems is not null) foreach (T item in eBCL.NewItems)
                            {
                                projection.Add(item);
                            }
                        }
                        else
                        {
                            if (eBCL.NewItems is not null) foreach (T item in eBCL.NewItems)
                            {
                                projection.Add(item);
                            }
                        }
                    }
                }
            }
            #endregion L o c a l F x
        }
        protected override void OnXAttributeChanged(XAttribute xattr, XObjectChangeEventArgs e)
        {
            base.OnXAttributeChanged(xattr, e);
        }
    }
}
