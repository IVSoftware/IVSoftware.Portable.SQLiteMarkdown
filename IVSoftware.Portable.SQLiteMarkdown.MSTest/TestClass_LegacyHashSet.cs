using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_LegacyHashSet
{
    [TestMethod]
    public void Test_GenericIndexedContract()
    {
        var items = new ObservableHashSet<string>();
        Assert.IsTrue(items.Add("a"));
        Assert.IsTrue(items.Add("c"));
        Assert.IsTrue(items.Insert(1, "b"));
        Assert.IsFalse(items.Insert(0, "a"));
        Assert.AreEqual(1, items.IndexOf("b"));

        var events = new List<NotifyCollectionChangedEventArgs>();
        items.CollectionChanged += (_, e) => events.Add(e);
        items.Move(2, 0);
        CollectionAssert.AreEqual(new[] { "c", "a", "b" }, items.ToArray());
        Assert.AreEqual(NotifyCollectionChangedAction.Move, events.Single().Action);
        Assert.AreEqual(2, events.Single().OldStartingIndex);
        Assert.AreEqual(0, events.Single().NewStartingIndex);

        events.Clear();
        items[1] = "d";
        Assert.IsFalse(items.Contains("a"));
        Assert.IsTrue(items.Contains("d"));
        Assert.AreEqual(NotifyCollectionChangedAction.Replace, events.Single().Action);
        items[1] = "b";
        Assert.AreEqual("d", items[1], "Replacing with a duplicate must leave the set unchanged.");
        Assert.AreEqual(1, events.Count);

        items.RemoveAt(0);
        Assert.IsFalse(items.Contains("c"));
        ICollection<string> collection = items;
        collection.Add("b");
        IList list = items;
        list.Insert(0, "b");
        Assert.AreEqual(2, items.Count, "Interface mutation paths must also reject duplicates.");

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => items.Insert(99, "uninserted"));
        Assert.IsFalse(items.Contains("uninserted"));
        Assert.IsTrue(items.Add("uninserted"), "A failed insertion must not poison hash membership.");

        events.Clear();
        var properties = new List<string?>();
        ((INotifyPropertyChanged)items).PropertyChanged += (_, e) => properties.Add(e.PropertyName);
        items.Clear();
        var reset = (NotifyCollectionResetEventArgs)events.Single();
        CollectionAssert.AreEqual(new[] { "d", "b", "uninserted" }, reset.OldItems.Cast<string>().ToArray());
        CollectionAssert.AreEqual(new[] { "Count", "Item[]" }, properties.ToArray());
        Assert.AreEqual(0, items.Count);
        Assert.IsTrue(items.Add("d"));
    }

    [TestMethod]
    public void Test_NonGenericIndexedContract()
    {
        var items = new ObservableHashSet();
        Assert.IsTrue(items.Add("a"));
        Assert.IsTrue(items.Insert(0, "b"));
        Assert.IsFalse(items.Add("b"));
        Assert.AreEqual(1, items.IndexOf("a"));
        items.Move(0, 1);
        Assert.AreEqual("a", items[0]);
        items[0] = "c";
        Assert.IsFalse(items.Contains("a"));
        Assert.IsTrue(items.Contains("c"));
        items[0] = "b";
        Assert.AreEqual("c", items[0]);
        items.RemoveAt(1);
        Assert.IsFalse(items.Contains("b"));

        IList list = items;
        list.Add("c");
        list.Insert(0, "c");
        Assert.AreEqual(1, items.Count);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => items.Insert(-1, "uninserted"));
        Assert.IsFalse(items.Contains("uninserted"));

        var events = new List<NotifyCollectionChangedEventArgs>();
        items.CollectionChanged += (_, e) => events.Add(e);
        items.Clear();
        var reset = (NotifyCollectionResetEventArgs)events.Single();
        CollectionAssert.AreEqual(new[] { "c" }, reset.OldItems.Cast<string>().ToArray());
        Assert.IsTrue(items.Add("c"));
    }

    [TestMethod]
    public void Test_SelectionIndexedCleanup()
    {
        var items = new ObservableSelectionHashSet<SelectableQFModel>
        {
            SelectionMode = IVSoftware.Portable.SQLiteMarkdown.SelectionMode.Multiple,
        };
        var first = new SelectableQFModel { Selection = ItemSelection.Exclusive };
        var second = new SelectableQFModel { Selection = ItemSelection.Exclusive };
        var replacement = new SelectableQFModel { Selection = ItemSelection.Exclusive };
        Assert.IsTrue(items.Add(first));
        Assert.IsTrue(items.Insert(1, second));
        items[0] = replacement;
        first.Selection = ItemSelection.None;
        first.Selection = ItemSelection.Exclusive;
        Assert.IsFalse(items.Contains(first), "Replaced items must no longer drive selection.");

        items.RemoveAt(1);
        second.Selection = ItemSelection.None;
        second.Selection = ItemSelection.Exclusive;
        Assert.IsFalse(items.Contains(second), "Removed items must no longer drive selection.");

        var properties = new List<string?>();
        items.PropertyChanged += (_, e) => properties.Add(e.PropertyName);
        items.SelectionMode = IVSoftware.Portable.SQLiteMarkdown.SelectionMode.Single;
        Assert.AreEqual(nameof(items.SelectionMode), properties.Single());
        properties.Clear();
        ((IList)items).Clear();
        Assert.AreEqual(ItemSelection.None, replacement.Selection);
        CollectionAssert.AreEqual(new[] { "Count", "Item[]" }, properties.ToArray());
        replacement.Selection = ItemSelection.Exclusive;
        Assert.AreEqual(0, items.Count, "Cleared items must no longer drive selection.");
    }
}
