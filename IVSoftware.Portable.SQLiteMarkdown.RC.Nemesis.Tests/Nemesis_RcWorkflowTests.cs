using System.Collections;
using System.Xml.Linq;
using IVSoftware.Portable.SQLiteMarkdown;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.Nemesis.Tests;

/// <summary>
/// Nemesis-generated verification of the RC's portable parser and list workflows.
/// </summary>
/// <remarks>
/// - Authorship: Nemesis; peer verification alongside unchanged handwritten checks.
/// - Uses public APIs and synthetic in-memory records; no modeled-collection test substitute.
/// </remarks>
[TestClass, DoNotParallelize]
public class Nemesis_RcWorkflowTests
{
    static Nemesis_IncrementalOrTests.Probe[] Rows() =>
    [
        new() { Id = "1", Text = "Term1" },
        new() { Id = "2", Text = "Term2" },
        new() { Id = "3", Text = "other" }
    ];

    [TestMethod]
    public void LibraryDoesNotReferenceExtractedEngines()
    {
        var assembly = typeof(MarkdownContext).Assembly;
        Assert.IsFalse(assembly.GetReferencedAssemblies().Any(reference =>
            reference.Name!.Contains("Collections", StringComparison.OrdinalIgnoreCase) ||
            reference.Name.Contains("Affinity", StringComparison.OrdinalIgnoreCase) ||
            reference.Name.Contains("OnePage", StringComparison.OrdinalIgnoreCase)));
        Assert.IsFalse(assembly.GetTypes().Any(type => type.Name.Contains("Affinity") || type.Name.Contains("ModeledCollection")));
    }

    [TestMethod]
    public async Task FilteringReturnsCanonicalRecordsAndClearsToSameSet()
    {
        var rows = Rows();
        var source = new ObservableQueryFilterSource<Nemesis_IncrementalOrTests.Probe>();
        source.ReplaceItems(rows);
        source.InputText = "Term1";
        await Settle().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(1, source.Count);
        Assert.AreSame(rows[0], source[0], "Filtering must not substitute a newly materialized row for the consumer's record.");
        source.InputText = "";
        await Settle().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(3, source.Count);
        Assert.AreSame(rows[0], source[0]);
        Assert.AreSame(rows[1], source[1]);
        Assert.AreEqual(FilteringState.Armed, source.FilteringState);
        async Task Settle() => await source;
    }

    [TestMethod]
    public async Task FilterOnlyRetainsDatasetWhenSearchIsCleared()
    {
        var source = new ObservableQueryFilterSource<Nemesis_IncrementalOrTests.Probe>();
        source.InitializeFilterOnlyMode(Rows());
        source.InputText = "Term2";
        await Settle().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual("2", source.Single().Id);
        source.Clear(false);
        await Settle().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(3, source.Count);
        Assert.AreEqual(QueryFilterConfig.Filter, source.QueryFilterConfig);
        async Task Settle() => await source;
    }

    [TestMethod]
    public void TerminalCollectionClearAlwaysEmptiesTheDataset()
    {
        var source = new ObservableQueryFilterSource<Nemesis_IncrementalOrTests.Probe>();
        source.ReplaceItems(Rows());
        ((IList)source).Clear();
        Assert.AreEqual(0, source.Count);
        Assert.AreEqual(0, source.UnfilteredItems.Count);
        source.ReplaceItems(Rows());
        source.Clear();
        Assert.AreEqual(0, source.Count);
    }

    [TestMethod]
    public void ExternalRecordsetHandoffPreservesItsRecords()
    {
        var rows = Rows();
        var source = new ObservableQueryFilterSource<Nemesis_IncrementalOrTests.Probe>();
        int requested = 0;
        source.RecordsetRequest += (sender, e) =>
        {
            requested++;
            StringAssert.Contains(e.SQL, "Term1");
            Assert.AreSame(source, sender);
            e.Recordset = rows;
        };
        source.InputText = "Term1";
        source.Commit();
        Assert.AreEqual(1, requested);
        Assert.AreEqual(3, source.Count);
        Assert.AreSame(rows[0], source[0]);
        Assert.AreEqual(SearchEntryState.QueryCompleteWithResults, source.SearchEntryState);
    }

    [TestMethod]
    public void QueryOnlyAcquiresRowsWithoutEnteringFilterMode()
    {
        using var db = new SQLiteConnection(":memory:");
        db.CreateTable<Nemesis_IncrementalOrTests.Probe>();
        db.InsertAll(Rows());
        var source = new ObservableQueryFilterSource<Nemesis_IncrementalOrTests.Probe>
        {
            QueryFilterConfig = QueryFilterConfig.Query,
            MemoryDatabase = db,
            InputText = "Term1"
        };
        source.Commit();
        Assert.AreEqual("1", source.Single().Id);
        Assert.IsFalse(source.IsFiltering);
        Assert.AreEqual(FilteringState.Ineligible, source.FilteringState);
    }

    [TestMethod]
    public async Task ZeroFilterResultsCanRecoverWithoutLosingCanonicalRows()
    {
        var source = new ObservableQueryFilterSource<Nemesis_IncrementalOrTests.Probe>();
        source.ReplaceItems(Rows());
        source.InputText = "absent";
        await Settle().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(0, source.Count);
        Assert.AreEqual(3, source.UnfilteredItems.Count);
        source.InputText = "Term2";
        await Settle().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual("2", source.Single().Id);
        async Task Settle() => await source;
    }

    [TestMethod]
    public void PrimaryKeyConstraintsComposeWithTheExpression()
    {
        using var db = new SQLiteConnection(":memory:");
        db.CreateTable<Nemesis_IncrementalOrTests.Probe>();
        db.InsertAll(Rows());
        var source = new ObservableQueryFilterSource<Nemesis_IncrementalOrTests.Probe>();
        source.ReplaceItems(Rows());
        source.InputText = "Term1 | Term2";
        source.AllowedPrimaryKeys.Add("2");
        string sql = source.ParseSqlMarkdown();
        CollectionAssert.AreEqual(new[] { "2" }, db.Query<Nemesis_IncrementalOrTests.Probe>(sql).Select(row => row.Id).ToArray());
    }

    [TestMethod]
    public void QuotedAndTaggedIndexingSurvivesRepeatedEdits()
    {
        var item = new SelectableQFModel { Description = "Blue Bird", Tags = "[bird]", Keywords = "[]" };
        StringAssert.Contains(item.QueryTerm, "blue");
        StringAssert.Contains(item.TagMatchTerm, "bird");
        item.Description = "Green Apple";
        StringAssert.Contains(item.QueryTerm, "green");
        Assert.IsFalse(item.QueryTerm.Contains("blue", StringComparison.Ordinal));
    }
}
