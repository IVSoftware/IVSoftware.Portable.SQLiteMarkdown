using IVSoftware.Portable.Collections;
using System.Collections;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    partial class ObservableQueryFilterSource<T> : IRangeable
    {
        public void AddRange(IEnumerable items)
        => ((IRangeable)CanonicalSupersetProtected).AddRange(items);

        public int AddRangeDistinct(IEnumerable items)
            => ((IRangeable)CanonicalSupersetProtected).AddRangeDistinct(items);

        public void InsertRange(int startingIndex, IEnumerable items)
            => ((IRangeable)CanonicalSupersetProtected).InsertRange(startingIndex, items);

        public int RemoveMultiple(IEnumerable items)
            => ((IRangeable)CanonicalSupersetProtected).RemoveMultiple(items);

        public void RemoveRange(int startingIndex, int endingIndex)
            => ((IRangeable)CanonicalSupersetProtected).RemoveRange(startingIndex, endingIndex);
    }
}
