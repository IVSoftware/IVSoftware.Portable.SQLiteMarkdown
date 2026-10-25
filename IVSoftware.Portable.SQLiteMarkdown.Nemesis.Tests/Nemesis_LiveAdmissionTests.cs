using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Tracking;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Nemesis.Tests;

/// <summary>
/// Nemesis-generated verification of ordinary population and later filtered admission.
/// </summary>
/// <remarks>
/// - Authorship: Nemesis; peer checks for the two legacy visible-count failures.
/// - Initial visibility comes from the canonical route; live is a temporary exception.
/// - These scenarios do not redefine the public IsFiltering posture flag.
/// </remarks>
[TestClass, DoNotParallelize]
public sealed class Nemesis_LiveAdmissionTests
{
    [TestMethod]
    public void DefaultModeInitialAddRangeCanBeNarrowedByCheckedPredicate()
    {
        var source = new ObservableQueryFilterCollection<CheckedRecord>();
        var checkedItem = new CheckedRecord { Id = "checked", IsChecked = true };
        var hidden = new CheckedRecord { Id = "unchecked" };
        source.AsInterface<IRangeable<CheckedRecord>>()!.AddRange(new[] { checkedItem, hidden });
        CollectionAssert.AreEqual(new[] { checkedItem, hidden }, source.ToArray());
        Assert.AreEqual(0, LiveCount(source), "Initial population needs no visibility exemption.");

        source.PredicatedImpl.ActivatePredicates(StdPredicate.IsChecked);
        CollectionAssert.AreEqual(new[] { checkedItem }, source.ToArray());
        Assert.AreEqual(2, source.Items.Count);
        Assert.AreSame(hidden, source.Items[1]);
        Assert.AreEqual(1, source.PredicatedImpl.GetTracker(StdPredicate.IsChecked)!.Count);
    }

    [TestMethod]
    public void FilterOnlyInitialPopulationAndLaterLiveAdditionsHaveDifferentMeaning()
    {
        var source = new ObservableQueryFilterCollection<CheckedRecord> { QueryFilterConfig = QueryFilterConfig.Filter };
        var checkedItem = new CheckedRecord { Id = "checked", IsChecked = true };
        var hidden = new CheckedRecord { Id = "unchecked" };
        source.AsInterface<IRangeable<CheckedRecord>>()!.AddRange(new[] { checkedItem, hidden });
        Assert.AreEqual(2, source.Count);
        Assert.AreEqual(0, LiveCount(source));
        source.PredicatedImpl.ActivatePredicates(StdPredicate.IsChecked);
        CollectionAssert.AreEqual(new[] { checkedItem }, source.ToArray());

        // The app supplies creation defaults before admission; Add does not initialize records.
        var first = new CheckedRecord { Id = "first addition", IsChecked = true };
        var second = new CheckedRecord { Id = "second addition", IsChecked = true };
        source.Add(first);
        source.Add(second);
        CollectionAssert.AreEqual(new[] { checkedItem, first, second }, source.ToArray());
        Assert.AreEqual(2, LiveCount(source));
        Assert.AreEqual(4, source.Items.Count);
        Assert.AreEqual(3, source.PredicatedImpl.GetTracker(StdPredicate.IsChecked)!.Count,
            "Visibility exemptions must not fabricate checked membership.");

        var existing = new CheckedRecord { Id = "existing unchecked" };
        source.Add(existing);
        Assert.IsFalse(existing.IsChecked, "Admission must not rewrite existing application data.");
        CollectionAssert.AreEqual(new[] { checkedItem, first, second }, source.ToArray());
        Assert.AreEqual(3, LiveCount(source), "A shadowed live flag is retained even on a nonmatching admission.");
        Assert.AreEqual(5, source.Items.Count);

        source.PredicatedImpl.ClearPredicates();
        source.LoadCanon(new[] { checkedItem, hidden });
        CollectionAssert.AreEqual(new[] { checkedItem, hidden }, source.ToArray());
        Assert.AreEqual(0, LiveCount(source), "A new canonical population ends the prior admission lifetime.");
        source.PredicatedImpl.ActivatePredicates(StdPredicate.IsChecked);
        CollectionAssert.AreEqual(new[] { checkedItem }, source.ToArray());
    }

    [TestMethod]
    public void SelectionPredicateNarrowsAnOrdinaryInitialRange()
    {
        var source = new ObservableQueryFilterCollection<SelectedRecord>();
        var hidden = new SelectedRecord { Id = "unselected" };
        var selected = new SelectedRecord { Id = "selected", IsSelected = true };
        source.AsInterface<IRangeable<SelectedRecord>>()!.AddRange(new[] { hidden, selected });
        Assert.AreEqual(0, LiveCount(source));
        source.PredicatedImpl.ActivatePredicates(SelectionPredicate.Selected);
        CollectionAssert.AreEqual(new[] { selected }, source.ToArray());
        Assert.AreEqual(2, source.Items.Count);
        Assert.AreSame(hidden, source.Items[0]);
    }

    [TestMethod]
    public void ApplicationInitializesSelectionBeforeAdmissionWithoutChangingOtherProperties()
    {
        var source = new ObservableQueryFilterCollection<SelectedRecord>();
        var selected = new SelectedRecord { Id = "selected", IsSelected = true };
        var hidden = new SelectedRecord { Id = "hidden" };
        source.LoadCanon(new[] { selected, hidden });
        source.PredicatedImpl.ActivatePredicates(SelectionPredicate.Selected);

        var created = new SelectedRecord { Id = "new selection", IsSelected = true };
        source.Add(created);
        CollectionAssert.AreEqual(new[] { selected, created }, source.ToArray());
        Assert.IsFalse(created.IsChecked, "Selection defaults are independent of checked state.");
        Assert.AreEqual(2, source.GetTracker(SelectionPredicate.Selected)!.Count);

        var existing = new SelectedRecord { Id = "existing unselected" };
        source.Add(existing);
        Assert.IsFalse(existing.IsSelected, "The collection must not initialize existing records.");
        CollectionAssert.AreEqual(new[] { selected, created }, source.ToArray());
        Assert.AreEqual(4, source.Items.Count);
    }

    [TestMethod]
    public async Task TextFilteredAdditionsStayVisibleWithoutExemptingTheInitialPopulation()
    {
        var source = new ObservableQueryFilterCollection<CheckedRecord>();
        source.ModelTracking |= ModelTrackingFlag.ItemQueries;
        var match = new CheckedRecord { Id = "apple", Description = "apple" };
        var hidden = new CheckedRecord { Id = "pear", Description = "pear" };
        source.InitializeFilterOnlyMode(new[] { match, hidden });
        source.InputTextSettlingTime = TimeSpan.FromMilliseconds(1);
        source.InputText = "apple";
        await Settle().WaitAsync(TimeSpan.FromSeconds(10));
        CollectionAssert.AreEqual(new[] { match }, source.ToArray());
        Assert.AreEqual(0, LiveCount(source));
        var added = new CheckedRecord { Id = "orange", Description = "orange" };
        source.Add(added);
        CollectionAssert.AreEqual(new[] { match, added }, source.ToArray());
        Assert.AreEqual(1, LiveCount(source));
        Assert.AreEqual(3, source.Items.Count);

        async Task Settle() => await source;
    }

    static int LiveCount<T>(ObservableQueryFilterCollection<T> source) =>
        source.Model.Descendants().Attributes("live").Count();

    public sealed class CheckedRecord : SelectableQFModel
    {
        [Track(StdPredicate.IsChecked, scope: TrackingScope.WorkingSet)]
        public new bool IsChecked { get => base.IsChecked; set => base.IsChecked = value; }
    }

    public enum SelectionPredicate
    {
        [Where(nameof(SelectedRecord.IsSelected), WherePredicate.IsTrue)]
        Selected,
    }

    public sealed class SelectedRecord : SelectableQFModel
    {
        [Track(SelectionPredicate.Selected, scope: TrackingScope.WorkingSet)]
        public bool IsSelected
        {
            get => selected;
            set { if (selected != value) { selected = value; OnPropertyChanged(); } }
        }
        bool selected;
    }
}
