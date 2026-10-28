// Handwritten verification view; source provenance is in the adjacent manifest.
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using IgnoreAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
[TestClass]
public partial class TestClass_TestClass_SQLiteMarkdown
{
private class DatabaseDisposableHost : DisposableHost
        {
            public DatabaseDisposableHost()
            {
                while (File.Exists(DisposableDatabasePathForTest))
                {
                    try
                    {
                        File.Delete(DisposableDatabasePathForTest);
                    }
                    catch (IOException)
                    {
                        switch (
                            MessageBox.Show(
                                "Make sure database is not open in DB Browser, then click OK",
                                "Deletion failed",
                                MessageBoxButtons.OKCancel))
                        {
                            case DialogResult.OK:
                                try
                                {
                                    File.Delete(DisposableDatabasePathForTest);
                                }
                                catch (IOException)
                                {
                                    Debug.WriteLine($"ADVISORY: Disposable database is still open in DB Browser for SQLite");
                                }
                                break;
                            default:
                            case DialogResult.Cancel:
                                goto breakFromInner;
                        }
                    }
                }
                breakFromInner:

                // NuGet Disposable TODO:
                // - ADD CTOR for default BeginUsing and FinalDispose.
                // - ADD this[Enum] indexer.
                // - ADD GetValue<T>(key, default); 

                BeginUsing += (sender, e) =>
                {
                    _onDisk = new SQLiteConnection(DisposableDatabasePathForTest);
                    _inMemory = new SQLiteConnection(":memory:");
                };
                FinalDispose += (sender, e) =>
                {
                    InMemory.Dispose();
                    OnDisk.Dispose();
                    if (File.Exists(DisposableDatabasePathForTest))
                    {
                        try
                        {
                            File.Delete(DisposableDatabasePathForTest);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"ADVISORY: Disposable database is still open in DB Browser for SQLite");
                        }
                    }
                };
            }
            public SQLiteConnection InMemory => _inMemory ?? throw new NullReferenceException();
            private SQLiteConnection? _inMemory = null;
            public SQLiteConnection OnDisk => _onDisk ?? throw new NullReferenceException();
            private SQLiteConnection? _onDisk = null;
            public static string DisposableDatabasePathForTest
            {
                get
                {
                    if (_databasePathForTest is null)
                    {
                        _databasePathForTest = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "IVSoftware",
                            "Portable",
                            "SQLiteMarkdown.MSTest",
                            "disposable.db"
                        );
                    }
                    if (_databasePathForTest is null) throw new NullReferenceException();
                    Directory.CreateDirectory(Path.GetDirectoryName(_databasePathForTest)!);
                    _databasePathForTest.ToClipboard();
                    return _databasePathForTest;
                }
            }
            static string? _databasePathForTest = default;
        }
DatabaseDisposableHost DHostDatabase
        {
            get
            {
                if (_dhostDatabase is null)
                {
                    _dhostDatabase = new DatabaseDisposableHost();
                }
                return _dhostDatabase;
            }
        }
DatabaseDisposableHost? _dhostDatabase = default;
class MSTestTraceListener : TraceListener
        {
            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message)
            {
            }
        }
static MSTestTraceListener trace
        {
            get
            {
                if (_trace is null)
                {
                    _trace = new MSTestTraceListener();
                }
                return _trace;
            }
        }
static MSTestTraceListener? _trace = default;
[ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Trace.Listeners.Add(new MSTestTraceListener());
        }
[ClassCleanup]
        public static void ClassCleanup()
        {
            if (_trace is not null)
            {
                Trace.Listeners.Remove(_trace);
                _trace.Dispose();
                _trace = null;
            }
        }
enum ReviewStatus
        {
            /// <summary>Indicates that the result is proposed and has not been reviewed yet.</summary>
            Proposed,
            /// <summary>Indicates that the current result is correct.</summary>
            ReviewPassed,
            /// <summary>Indicates that the current result is incorrect.</summary>
            CurrentLimitIsWrong,
        }
private class ExpressionTestEntry
        {
            /// <summary>
            /// Implicitly converts a string to an ExpressionTestEntry by assigning the string to Expr.
            /// </summary>
            /// <param name="expr">The expression string to convert.</param>
            public static implicit operator ExpressionTestEntry(string expr) =>
                new ExpressionTestEntry { Expr = expr };

            /// <summary>Description of the test case.</summary>
            public string? Description { get; set; }
            /// <summary>The SQL expression being tested.</summary>
            public string? Expr { get; set; }
            /// <summary>The expected result of the expression's evaluation.</summary>
            public string? Expected { get; set; }
            /// <summary>The expected validation state of the expression.</summary>
            public ValidationState ExpectedValidationState { get; set; }
            /// <summary>The current review status of this test entry.</summary>
            public ReviewStatus ReviewStatus { get; set; }

            internal string Report(MarkdownContextOR context, ValidationState validationState)
            {
                var builder = new List<string>
                {
                    $"{Expr}: {Description}"
                };
                string actual = context;
                if (string.IsNullOrWhiteSpace(actual))
                {
                    builder.Add($"{validationState}");
                }
                else
                {
                    builder.Add(actual);
                }
                builder.Add($"{ReviewStatus.GetType().Name}.{ReviewStatus}");
                builder.Add(string.Empty);
                return string.Join(Environment.NewLine, builder);
            }
        }
Dictionary<string, ExpressionTestEntry> ExpressionTable
        {
            get
            {
                bool asEmpty = true;

                // Check if the table is null and initialize it if needed
                if (_expressionTable is null)
                {
                    _expressionTable = JsonConvert.DeserializeObject<Dictionary<string, ExpressionTestEntry>>
                    (
                        asEmpty ?
                            // Initialize an empty serialized dictionary if `asEmpty` is true
                            JsonConvert.SerializeObject(new Dictionary<string, ExpressionTestEntry>()) :
                           @" 
{
  "":Empty"": {
    ""Description"": ""Empty"",
    ""Expr"": """",
    ""Expected"": """",
    ""ExpectedValidationState"": 0,
    ""ReviewStatus"": 0
  },
  ""a:Initial single character 'a' - expected invalid state"": {
    ""Description"": ""Initial single character 'a' - expected invalid state"",
    ""Expr"": ""a"",
    ""Expected"": """",
    ""ExpectedValidationState"": 1,
    ""ReviewStatus"": 0
  },
  ""an:Intermediate two-character expression 'an' - expected invalid state"": {
    ""Description"": ""Intermediate two-character expression 'an' - expected invalid state"",
    ""Expr"": ""an"",
    ""Expected"": """",
    ""ExpectedValidationState"": 1,
    ""ReviewStatus"": 0
  },
  ""ani:Valid expression 'ani' - expected valid state"": {
    ""Description"": ""Valid single term expression 'ani' - expected valid state"",
    ""Expr"": ""ani"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%ani%' OR ContainsTerm LIKE '%ani%' OR TagMatchTerm LIKE '%[ani]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""ani color:Basic AND condition between two terms"": {
    ""Description"": ""Basic AND condition between two terms"",
    ""Expr"": ""ani color"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%ani%' OR ContainsTerm LIKE '%ani%' OR TagMatchTerm LIKE '%[ani]%') AND (LikeTerm LIKE '%color%' OR ContainsTerm LIKE '%color%' OR TagMatchTerm LIKE '%[color]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""ani & color:Alternative AND condition with &"": {
    ""Description"": ""Alternative AND condition with &"",
    ""Expr"": ""ani & color"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%ani%' OR ContainsTerm LIKE '%ani%' OR TagMatchTerm LIKE '%[ani]%') AND (LikeTerm LIKE '%color%' OR ContainsTerm LIKE '%color%' OR TagMatchTerm LIKE '%[color]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  "" ani color :Expression with leading and trailing spaces"": {
    ""Description"": ""Expression with leading and trailing spaces"",
    ""Expr"": "" ani color "",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%ani%' OR ContainsTerm LIKE '%ani%' OR TagMatchTerm LIKE '%[ani]%') AND (LikeTerm LIKE '%color%' OR ContainsTerm LIKE '%color%' OR TagMatchTerm LIKE '%[color]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""ani && color:Another AND condition with double ampersands"": {
    ""Description"": ""Another AND condition with double ampersands"",
    ""Expr"": ""ani && color"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%ani%' OR ContainsTerm LIKE '%ani%' OR TagMatchTerm LIKE '%[ani]%') AND (LikeTerm LIKE '%color%' OR ContainsTerm LIKE '%color%' OR TagMatchTerm LIKE '%[color]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""!fruit:Basic NOT condition"": {
    ""Description"": ""Basic NOT condition"",
    ""Expr"": ""!fruit"",
    ""Expected"": ""SELECT * FROM items WHERE\r\nNOT ((LikeTerm LIKE '%fruit%' OR ContainsTerm LIKE '%fruit%' OR TagMatchTerm LIKE '%[fruit]%'))"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""Tom:Search for the name 'Tom'"": {
    ""Description"": ""Search for the name 'Tom'"",
    ""Expr"": ""Tom"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%Tom%' OR ContainsTerm LIKE '%Tom%' OR TagMatchTerm LIKE '%[Tom]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""brown:Search for single term 'brown'"": {
    ""Description"": ""Search for single term 'brown'"",
    ""Expr"": ""brown"",
    ""Expected"": ""SELECT * FROM items WHERE\r\n(LikeTerm LIKE '%brown%' OR ContainsTerm LIKE '%brown%' OR TagMatchTerm LIKE '%[brown]%')"",
    ""ExpectedValidationState"": 2,
    ""ReviewStatus"": 0
  },
  ""!!!:Invalid expression with special characters"": {
    ""Description"": ""Invalid expression with special characters"",
    ""Expr"": ""!!!"",
    ""Expected"": """",
    ""ExpectedValidationState"": 1,
    ""ReviewStatus"": 0
  }
}"

                    );

                    /// <summary>
                    /// Populate the ExpressionTable with specific entries if they are missing.
                    /// This ensures that essential test cases are included for validation.
                    /// </summary>
                    foreach (var exprEx in new[]
                    {
                        ":Empty",
                        "a:Initial single character 'a' - expected invalid state",
                        "an:Intermediate two-character expression 'an' - expected invalid state",
                        "ani:Valid expression 'ani' - expected valid state",
                        "ani color:Basic AND condition between two terms",
                        "ani & color:Alternative AND condition with &",
                        " ani color :Expression with leading and trailing spaces",
                        "ani && color:Another AND condition with double ampersands",
                        "!fruit:Basic NOT condition",
                        "Tom:Search for the name 'Tom'",
                        "brown:Search for single term 'brown'",
                        "!!!:Invalid expression with special characters"
                    })
                    {
                        if (!ExpressionTable.ContainsKey(exprEx))
                        {
                            ValidationState state = ValidationState.Empty;
                            var split = exprEx.Split(':');
                            ExpressionTable[exprEx] = new ExpressionTestEntry
                            {
                                Description = split[1],
                                Expr = split[0],
                                Expected = IVSoftware.Portable.SQLiteMarkdown.Extensions.ParseSqlMarkdown<SelfIndexedItem>(split[0], ref state),
                                ExpectedValidationState = state,
                            };
                        }
                    }
                }
                if (_expressionTable is null) throw new NullReferenceException();
                return _expressionTable;
            }
        }
Dictionary<string, ExpressionTestEntry>? _expressionTable = default;
private class TestableMarkdownContext<T> : MarkdownContext<T>
        {
            public new SQLiteConnection FilterQueryDatabase
            {
                get => base.FilterQueryDatabase;
                set
                {
                    base.FilterQueryDatabase = value;
                    base.FilterQueryDatabase?.CreateTable<T>();
                }
            }
        }
}
}
