using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Exceptions;
using System;
using System.Collections;

namespace IVSoftware.Portable.Collections.Preview
{
    internal class ObservableRangeCollection<T>
        : SuppressibleObservableCollection<T>
        , IRangeable
    {
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
            throw new NotImplementedException();
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

#if false
        public int AddRangeDistinct(IEnumerable items)
        {
            XElement model = this;

            if (ModelingCapabilityInfo.GetFullPath is not GetFullPathDlgt dlgt)
            {
                return 0;
            }
            else
            {
                int changed = 0;
                using (BeginSuppressNotify())
                {
                    int newStartingIndex = Count;
                    foreach (var item in items)
                    {
                        if (item is T itemT && dlgt(itemT) is string fullPath)
                        {
                            if (string.IsNullOrWhiteSpace(fullPath))
                            {
                                "ObservablePreviewCollection".ThrowHard<ArgumentException>($"The '{nameof(fullPath)}' argument cannot be empty.");
                                CancelSuppressNotify();
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
                            CancelSuppressNotify();
                            return 0;
                        }
                    }
                    return changed;
                }
            }
        }

        public void InsertRange(int startingIndex, IEnumerable items)
        {
            using (BeginSuppressNotify())
            {

            }
        }

        public int RemoveMultiple(IEnumerable items)
        {
            using (BeginSuppressNotify())
            {

            }
            return 0;
        }

        public void RemoveRange(int startingIndex, int endingIndex)
        {
            using (BeginSuppressNotify())
            {

            }
        }
#endif
    }
}
