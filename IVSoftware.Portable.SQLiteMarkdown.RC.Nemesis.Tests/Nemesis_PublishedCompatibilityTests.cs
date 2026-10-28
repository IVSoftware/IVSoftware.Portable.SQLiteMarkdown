using System.Collections;
using IVSoftware.Portable.SQLiteMarkdown;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.Nemesis.Tests;

/// <summary>
/// Nemesis-generated verification of beta compatibility retained during RC extraction.
/// </summary>
/// <remarks>
/// - Authorship: Nemesis; public signatures and executed state transitions, independent of frozen snapshots.
/// </remarks>
[TestClass, DoNotParallelize]
public sealed class Nemesis_PublishedCompatibilityTests
{
    [Table("compatibility")]
    public sealed class Row
    {
        [PrimaryKey] public int Id { get; set; }
        [QueryLikeTerm, FilterLikeTerm] public string Text { get; set; } = "";
    }

    [TestMethod]
    public async Task ConcreteClearRetainsWorkingSetWhileInterfaceClearEmptiesIt()
    {
        var source = new ObservableQueryFilterSource<Row>();
        source.ReplaceItems(new[] { new Row { Id = 1, Text = "apple" }, new Row { Id = 2, Text = "pear" } });
        source.InputText = "apple";
        async Task Settle() => await source;
        await Settle().WaitAsync(TimeSpan.FromSeconds(5));
        Assert.AreEqual(1, source.Count);
        source.Clear();
        await Settle().WaitAsync(TimeSpan.FromSeconds(5));
        Assert.AreEqual(2, source.Count, "The published no-argument call clears search state first.");
        Assert.AreEqual(2, source.UnfilteredItems.Count);
        ((IList)source).Clear();
        Assert.AreEqual(0, source.Count);
        Assert.AreEqual(typeof(void), typeof(ObservableQueryFilterSource<Row>)
            .GetMethod("Clear", new[] { typeof(bool) })!.ReturnType);
        Assert.IsTrue(typeof(ObservableQueryFilterSource<Row>).GetMethod("Clear", new[] { typeof(bool) })!
            .GetParameters()[0].IsOptional);
        Assert.IsNotNull(typeof(MarkdownContext).GetConstructor(new[] { typeof(Type), typeof(bool) }));
        Assert.AreEqual(typeof(CollectionSyncAuthority), typeof(MarkdownContext).GetProperty("SyncAuthority")!.PropertyType);
    }

    [TestMethod]
    public void ParserAllocatesNoFilterStoreAndScopesKeyConstraintsAndLimits()
    {
        var context = new ContextProbe { QueryFilterConfig = QueryFilterConfig.Query };
        string sql = context.ParseSqlMarkdown<Row>("apple | pear");
        Assert.IsNull(context.ReadFilterStore());
        using var db = new SQLiteConnection(":memory:");
        db.CreateTable<Row>();
        db.InsertAll(new[] { new Row { Id = 1, Text = "apple" }, new Row { Id = 2, Text = "pear" } });
        CollectionAssert.AreEqual(new[] { 1, 2 }, db.Query<Row>(sql).Select(row => row.Id).ToArray());

        var source = new ObservableQueryFilterSource<Row>();
        source.ReplaceItems(db.Query<Row>("SELECT * FROM compatibility"));
        source.InputText = "apple | pear";
        source.DisallowedPrimaryKeys.Add(1);
        using (source.BeginLimit(1))
        {
            var matches = db.Query<Row>(source.ParseSqlMarkdown());
            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(2, matches.Single().Id);
        }
        Assert.AreEqual((uint)0, source.Limit);
        // October 28: author permits bounded suffix corrections without a dictionary dependency.
        Assert.AreEqual("SELECT * FROM compatibility WHERE Text LIKE '%apple%'",
            "SELECT * FROM compatibility WHERE Text LIKE '%apples%'".ToFuzzyQuery());
        Assert.AreEqual("json_extract(Properties, '$.Description')", "Properties".JsonExtract("Description"));
    }

    [TestMethod]
    public void LightweightSingularizationKeepsTheEAndHandlesSibilantSuffixes()
    {
        string actual = string.Join("\n", new[] { "apples", "cases", "boxes", "classes", "churches", "brushes", "dogs", "glass" }
            .Select(word => word + " -> " + word.Singularize()));
        const string expected = "apples -> apple\ncases -> case\nboxes -> box\nclasses -> class\nchurches -> church\nbrushes -> brush\ndogs -> dog\nglass -> glass";
        Assert.AreEqual(expected, actual);
    }

    sealed class ContextProbe : MarkdownContext<Row>
    {
        public SQLiteConnection? ReadFilterStore() => FilterQueryDatabase;
    }
}
