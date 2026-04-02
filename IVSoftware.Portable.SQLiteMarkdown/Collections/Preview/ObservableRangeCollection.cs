using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Collections.Preview;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections;
using System.Xml.Linq;

namespace IVSoftware.Portable.Collections.Preview
{
    internal class ObservableRangeCollection<T>
        : SuppressibleObservableCollection<T>
        , IRangeable
    {

        public static implicit operator XElement(ObservableRangeCollection<T> @this)
        {
            @this.ToString(out XElement model);
            return model;
        }
        public void AddRange(IEnumerable items)
        {
            using (BeginSuppress())
            {
                int newStartingIndex = Count;
                foreach (var item in items)
                {
                    if (item is T itemT)
                    {
                        // [Careful]
                        // Use Insert.
                        // We can't use Add because the collection
                        // doesn't actually change until the end.
                        InsertItem(newStartingIndex++, itemT);
                    }
                    else
                    {
                        item.ThrowHard<InvalidCastException>($"All range items must be {typeof(T).Name}");
                        return;
                    }
                }
            }
        }
        public int AddRangeDistinct(IEnumerable items)
        {
            XElement model = this;

            if (typeof(T).GetModeledPathInfo().GetPath is not GetPathDlgt dlgt)
            {
                return 0;
            }
            else
            {
                int changed = 0;
                using (BeginSuppress())
                {
                    int newStartingIndex = Count;
                    foreach (var item in items)
                    {
                        if (item is T itemT && dlgt(itemT) is string fullPath)
                        {
                            if (string.IsNullOrWhiteSpace(fullPath))
                            {
                                "ObservablePreviewCollection".ThrowHard<ArgumentException>($"The '{nameof(fullPath)}' argument cannot be empty.");
                                CancelSuppress();
                                return 0;
                            }

                            switch (model.Place(fullPath))
                            {
                                case PlacerResult.Created:
                                    InsertItem(newStartingIndex++, itemT);
                                    changed++;
                                    break;
                                default:
                                    /* G T K - N O O P */
                                    // Skipping this item as non-distinct.
                                    break;
                            }
                        }
                        else
                        {
                            item.ThrowHard<InvalidCastException>($"All range items must be {typeof(T).Name}");
                            CancelSuppress();
                            return 0;
                        }
                    }
                    return changed;
                }
            }
        }

        public void InsertRange(int startingIndex, IEnumerable items)
        {
            throw new NotImplementedException();
        }

        public int RemoveMultiple(IEnumerable items)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(int startingIndex, int endingIndex)
        {
            throw new NotImplementedException();
        }
    }
}
