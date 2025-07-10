using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
	public class ObservableHashSet : ObservableCollection<object>
	{
		protected virtual HashSet<object> HashSet { get; } = new HashSet<object>();

		protected override void InsertItem(int index, object item)
		{
			if (HashSet.Add(item))
				base.InsertItem(index, item);
		}

		protected override void SetItem(int index, object item)
		{
			var oldItem = this[index];
			if (Equals(oldItem, item)) return;

			if (HashSet.Contains(item)) return; // no dupes

			HashSet.Remove(oldItem);
			HashSet.Add(item);
			base.SetItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			var item = this[index];
			HashSet.Remove(item);
			base.RemoveItem(index);
		}

		protected override void ClearItems()
		{
			HashSet.Clear();
			base.ClearItems();
		}
	}
	public class ObservableHashSet<T> : ObservableCollection<T>
	{
		protected virtual HashSet<T> HashSet { get; } = new HashSet<T>();

		protected override void InsertItem(int index, T item)
		{
			if (HashSet.Add(item))
				base.InsertItem(index, item);
		}

		protected override void SetItem(int index, T item)
		{
			var oldItem = this[index];
			if (Equals(oldItem, item)) return;

			if (HashSet.Contains(item)) return; // no dupes

			HashSet.Remove(oldItem);
			HashSet.Add(item);
			base.SetItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			var item = this[index];
			HashSet.Remove(item);
			base.RemoveItem(index);
		}

		protected override void ClearItems()
		{
			HashSet.Clear();
			base.ClearItems();
		}
	}
}
