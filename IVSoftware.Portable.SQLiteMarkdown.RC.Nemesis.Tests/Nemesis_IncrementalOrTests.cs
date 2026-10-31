using IVSoftware.Portable.SQLiteMarkdown;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.Nemesis.Tests;

/// <summary>
/// Nemesis-generated verification of executable OR expressions during incremental typing.
/// </summary>
/// <remarks>
/// - Authorship: Nemesis; peer verification alongside the handwritten suite.
/// - Exercises public parsing and collection paths with synthetic in-memory SQLite data.
/// </remarks>
[TestClass, DoNotParallelize]
public class Nemesis_IncrementalOrTests
{
    static Probe[] Rows() =>
    [
        new() { Id = "1", Text = "Term1" },
        new() { Id = "2", Text = "Term2" },
        new() { Id = "3", Text = "unrelated" },
        new() { Id = "4", Text = "literal|" }
    ];

    [TestMethod]
    public void TrailingOrRetainsCompletePrefixAndExecutableSql()
    {
        using var db = new SQLiteConnection(":memory:");
        db.CreateTable<Probe>();
        db.InsertAll(Rows());
        foreach (var mode in new[] { QueryFilterMode.Query, QueryFilterMode.Filter })
        {
            foreach (var expression in new[] { "Term1|", "Term1 |", "Term1 | ", "Term1||", "Term1 | | ", " Term1  |   " })
            {
                string sql = expression.ParseSqlMarkdown<Probe>(mode);
                CollectionAssert.AreEqual(new[] { "1" }, db.Query<Probe>(sql).Select(row => row.Id).ToArray(), expression);
            }
            foreach (var expression in new[] { "Term1|Term2", "Term1 | Term2", "Term1 || Term2", "Term1 | | Term2" })
            {
                string sql = expression.ParseSqlMarkdown<Probe>(mode);
                CollectionAssert.AreEquivalent(new[] { "1", "2" }, db.Query<Probe>(sql).Select(row => row.Id).ToArray(), expression);
            }
        }
    }

    [TestMethod]
    public void EscapedAndQuotedPipesRemainLiteral()
    {
        using var db = new SQLiteConnection(":memory:");
        db.CreateTable<Probe>();
        db.InsertAll(Rows());
        foreach (var expression in new[] { @"literal\|", "\"literal|\"", "'literal|'" })
        {
            string sql = expression.ParseSqlMarkdown<Probe>();
            CollectionAssert.AreEqual(new[] { "4" }, db.Query<Probe>(sql).Select(row => row.Id).ToArray(), expression);
        }
    }

    [TestMethod]
    public async Task FilterTypingPreservesRawInputAndOrResults()
    {
        var source = new ObservableQueryFilterSource<Probe>();
        source.ReplaceItems(Rows());
        foreach (var text in new[] { "Term1", "Term1 ", "Term1 |", "Term1 | ", "Term1 | T", "Term1 | Term2" })
        {
            source.InputText = text;
            await Settle().WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(text, source.InputText, "Typing must not rewrite the user's expression.");
            if (text.TrimEnd().EndsWith("Term1", StringComparison.Ordinal) || text.TrimEnd().EndsWith('|'))
                CollectionAssert.AreEqual(new[] { "1" }, source.Select(row => row.Id).ToArray(), text);
        }
        CollectionAssert.AreEquivalent(new[] { "1", "2" }, source.Select(row => row.Id).ToArray());

        async Task Settle() => await source;
    }

    [Table("or_probe")]
    public class Probe
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;

        [QueryLikeTerm, FilterLikeTerm]
        public string Text { get; set; } = string.Empty;
    }
}
