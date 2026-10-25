using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Tracking;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.Xml.Linq;
using System.Xml.Linq;
using Row = IVSoftware.Portable.SQLiteMarkdown.Nemesis.Tests.Nemesis_LiveAdmissionTests.CheckedRecord;

namespace IVSoftware.Portable.SQLiteMarkdown.Nemesis.Tests;

/// <summary>
/// Nemesis-generated verification of content-edit continuity and the boundaries of live visibility.
/// </summary>
/// <remarks>
/// - Authorship: Nemesis; uses actual SQLite filtering and the collection's existing awaiter.
/// - Predicates gate live items without consuming protection; a different text filter retires it.
/// </remarks>
[TestClass, DoNotParallelize]
public sealed class Nemesis_LiveEpochTests
{
    [TestMethod]
    public async Task VisibleContentEditBecomesLiveImmediatelyAndKeepsTruthfulQueryMembership()
    {
        var source = Source();
        var edited = new Row { Id = "edited", Description = "apple" };
        var anchor = new Row { Id = "anchor", Description = "apple" };
        var hidden = new Row { Id = "hidden", Description = "pear" };
        source.InitializeFilterOnlyMode(new[] { edited, anchor, hidden });
        int textSettlements = 0;
        source.InputTextSettled += (_, _) => textSettlements++;
        source.InputText = "apple";
        await Settle(source);
        var node = Node(source, edited);

        edited.Description = "orange";
        Assert.IsNotNull(node.Attribute("live"), "Continuity is granted when the visible edit is accepted.");
        Assert.AreEqual(ModelDataExchangeAuthority.NoAuthority, source.ModelDataExchangeAuthority);
        await Settle(source);

        Assert.IsNull(node.Attribute("qmatch"), "Live protection must not disguise a stale SQLite match.");
        CollectionAssert.AreEqual(new[] { edited, anchor }, source.ToArray());
        Assert.AreSame(node, Node(source, edited));
        Assert.AreSame(edited, source.Items[0]);
        Assert.AreEqual("orange", source.FilterQueryDatabase!.Query<Row>("SELECT * FROM items WHERE Id = ?", edited.Id).Single().Description);
        Assert.AreEqual(3, source.Items.Count);
        Assert.AreEqual(1, textSettlements, "Record refresh must not fabricate another user text request.");
    }

    [TestMethod]
    public async Task CheckingALiveEditedItemHidesOnlyThatItem()
    {
        var source = Source();
        var edited = new Row { Id = "edited", Description = "apple" };
        var otherLive = new Row { Id = "other live", Description = "apple" };
        var anchor = new Row { Id = "anchor", Description = "apple" };
        source.InitializeFilterOnlyMode(new[] { edited, otherLive, anchor });
        source.PredicatedImpl.ActivatePredicates(StdPredicate.IsUnchecked);
        source.InputText = "apple";
        await Settle(source);
        edited.Description = "orange";
        otherLive.Description = "pear";
        await Settle(source);

        edited.IsChecked = true;
        Assert.IsNotNull(Node(source, edited).Attribute("live"));
        Assert.IsNotNull(Node(source, otherLive).Attribute("live"));
        await Settle(source);

        CollectionAssert.AreEqual(new[] { otherLive, anchor }, source.ToArray());
        Assert.IsNull(Node(source, edited).Attribute("pmatch"));
        Assert.IsNotNull(Node(source, otherLive).Attribute("live"));
        Assert.AreEqual(1, source.GetTracker(StdPredicate.IsChecked)!.Count);
        Assert.AreEqual(3, source.Items.Count);
    }

    [TestMethod]
    public async Task HiddenAndUnrelatedEditsDoNotAcquireLivePrivilege()
    {
        var source = Source();
        var visible = new Row { Id = "visible", Description = "apple" };
        var hidden = new Row { Id = "hidden", Description = "pear" };
        source.InitializeFilterOnlyMode(new[] { visible, hidden });
        source.InputText = "apple";
        await Settle(source);

        hidden.Description = "orange";
        visible.IsEditing = true;
        await Settle(source);
        Assert.AreEqual(0, source.Model.Descendants().Attributes("live").Count());
        CollectionAssert.AreEqual(new[] { visible }, source.ToArray());

        hidden.Description = "apple pie";
        await Settle(source);
        CollectionAssert.AreEqual(new[] { visible, hidden }, source.ToArray());
        Assert.IsNull(Node(source, hidden).Attribute("live"));
    }

    [TestMethod]
    public async Task DifferentTextAndExplicitClearEndProtectionButSameFilterRefreshDoesNot()
    {
        var source = Source();
        var edited = new Row { Id = "edited", Description = "apple" };
        var anchor = new Row { Id = "anchor", Description = "apple" };
        var pear = new Row { Id = "pear", Description = "pear" };
        source.InitializeFilterOnlyMode(new[] { edited, anchor, pear });
        source.InputText = "apple";
        await Settle(source);
        edited.Description = "orange";
        await Settle(source);
        anchor.Description = "apple pie";
        await Settle(source);
        Assert.IsNotNull(Node(source, edited).Attribute("live"));

        source.InputText = "pear";
        await Settle(source);
        Assert.AreEqual(0, source.Model.Descendants().Attributes("live").Count());
        CollectionAssert.AreEqual(new[] { pear }, source.ToArray());
        pear.Description = "plum";
        await Settle(source);
        Assert.IsNotNull(Node(source, pear).Attribute("live"));
        CollectionAssert.AreEqual(new[] { pear }, source.ToArray(), "Only the protected record survives a zero-match text result.");
        pear.Description = "banana";
        source.InputText = "";
        await Settle(source);
        Assert.AreEqual(0, source.Model.Descendants().Attributes("live").Count());
        Assert.AreEqual(0, source.Model.Descendants().Attributes("qmatch").Count());
        CollectionAssert.AreEqual(new[] { edited, anchor, pear }, source.ToArray());
        Assert.AreEqual("banana", source.FilterQueryDatabase!.Query<Row>("SELECT * FROM items WHERE Id = ?", pear.Id).Single().Description,
            "Clearing the filter must not discard accepted row work waiting for settlement.");
    }

    [TestMethod]
    public async Task QueuedEditsCannotResurrectRemovedOrReplacedBindings()
    {
        var source = Source();
        var removed = new Row { Id = "removed", Description = "apple" };
        var replaced = new Row { Id = "replaced", Description = "apple" };
        var anchor = new Row { Id = "anchor", Description = "apple" };
        source.InitializeFilterOnlyMode(new[] { removed, replaced, anchor });
        source.InputText = "apple";
        await Settle(source);
        removed.Description = "orange";
        replaced.Description = "pear";
        source.RemoveAt(0);
        var replacement = new Row { Id = "replacement", Description = "apple" };
        source[0] = replacement;
        await Settle(source);

        Assert.AreEqual(2, source.Items.Count);
        Assert.IsFalse(source.Items.Any(item => ReferenceEquals(item, removed) || ReferenceEquals(item, replaced)));
        CollectionAssert.AreEquivalent(new[] { "replacement", "anchor" },
            source.FilterQueryDatabase!.Query<Row>("SELECT * FROM items").Select(item => item.Id).ToArray());
        Assert.AreSame(replacement, (Node(source, replacement).Attribute("model") as XBoundAttribute)!.Tag);
    }

    [TestMethod]
    public async Task QueryOnlyRecordRefreshDoesNotPretendToCommitPendingQueryText()
    {
        var source = Source();
        source.QueryFilterConfig = QueryFilterConfig.Query;
        var edited = new Row { Id = "edited", Description = "apple" };
        var other = new Row { Id = "other", Description = "pear" };
        source.LoadCanon(new[] { edited, other });
        int textSettlements = 0;
        source.InputTextSettled += (_, _) => textSettlements++;
        source.InputText = "apple";
        edited.Description = "orange";
        await Settle(source);

        Assert.AreEqual(0, textSettlements);
        Assert.AreEqual("apple", source.InputText);
        Assert.AreEqual(0, source.Model.Descendants().Attributes("live").Count());
        CollectionAssert.AreEqual(new[] { edited, other }, source.ToArray());
        Assert.AreEqual("orange", source.FilterQueryDatabase!.Query<Row>("SELECT * FROM items WHERE Id = ?", edited.Id).Single().Description);
    }

    [TestMethod]
    public async Task LastPredicateVetoSurvivesLaterTextReconciliation()
    {
        var source = Source();
        var edited = new Row { Id = "edited", Description = "apple" };
        var hidden = new Row { Id = "hidden", Description = "apple", IsChecked = true };
        var other = new Row { Id = "other", Description = "pear", IsChecked = true };
        source.InitializeFilterOnlyMode(new[] { edited, hidden, other });
        source.PredicatedImpl.ActivatePredicates(StdPredicate.IsUnchecked);
        source.InputText = "apple";
        await Settle(source);
        edited.Description = "orange";
        await Settle(source);
        edited.IsChecked = true;
        await Settle(source);
        Assert.AreEqual(0, source.Count);

        hidden.Description = "apple pie";
        await Settle(source);
        Assert.AreEqual(0, source.Count, "Text matches cannot erase the host's empty-predicate decision.");
        Assert.IsNotNull(Node(source, edited).Attribute("live"));
        Assert.AreEqual(3, source.Items.Count);
    }

    [TestMethod]
    public async Task CheckboxPredicateRoundTripRestoresTheEditedNonmatchingRecord()
    {
        var source = Source();
        var edited = new Row { Id = "edited", Description = "ABC1" };
        var anchor = new Row { Id = "anchor", Description = "ABC2" };
        var checkedItem = new Row { Id = "checked", Description = "ABC3", IsChecked = true };
        var hidden = new Row { Id = "hidden", Description = "other" };
        source.InitializeFilterOnlyMode(new[] { edited, anchor, checkedItem, hidden });
        source.PredicatedImpl.ActivatePredicates(StdPredicate.IsUnchecked);
        source.InputText = "abc";
        await Settle(source);
        var node = Node(source, edited);
        CollectionAssert.AreEqual(new[] { edited, anchor }, source.ToArray());

        edited.Description = "ABD1";
        Assert.IsNotNull(node.Attribute("live"));
        await Settle(source);
        Assert.IsNull(node.Attribute("qmatch"));
        CollectionAssert.AreEqual(new[] { edited, anchor }, source.ToArray());

        source.PredicatedImpl.ActivatePredicates(StdPredicate.IsChecked);
        CollectionAssert.AreEqual(new[] { checkedItem }, source.ToArray());
        Assert.IsNotNull(node.Attribute("live"), "A predicate shadows protection; it does not spend it.");
        source.PredicatedImpl.ActivatePredicates(StdPredicate.IsUnchecked);
        CollectionAssert.AreEqual(new[] { edited, anchor }, source.ToArray());

        // Changing the record's checked value has the same gate, without a text request.
        edited.IsChecked = true;
        CollectionAssert.AreEqual(new[] { anchor }, source.ToArray());
        await Settle(source);
        Assert.IsNotNull(node.Attribute("live"));
        edited.IsChecked = false;
        await Settle(source);
        CollectionAssert.AreEqual(new[] { edited, anchor }, source.ToArray());
        Assert.AreSame(node, Node(source, edited));
        Assert.AreSame(edited, source.Items[0]);
        Assert.IsNull(node.Attribute("qmatch"));
        Assert.IsNotNull(node.Attribute("live"));
        Assert.AreEqual(4, source.Items.Count);

        source.InputText = "ABD";
        await Settle(source);
        Assert.IsNull(node.Attribute("live"), "Only the changed text request ends this protection.");
        CollectionAssert.AreEqual(new[] { edited }, source.ToArray());
    }

    static ObservableQueryFilterCollection<Row> Source()
    {
        var source = new ObservableQueryFilterCollection<Row>();
        source.ModelTracking |= ModelTrackingFlag.ItemQueries;
        source.InputTextSettlingTime = TimeSpan.FromMilliseconds(20);
        return source;
    }

    static XElement Node(ObservableQueryFilterCollection<Row> source, Row item) =>
        source.Model.Descendants().Single(node => ReferenceEquals((node.Attribute("model") as XBoundAttribute)?.Tag, item));

    static async Task Settle(ObservableQueryFilterCollection<Row> source)
    {
        await AwaitSource().WaitAsync(TimeSpan.FromSeconds(10));
        async Task AwaitSource() => await source;
    }
}
