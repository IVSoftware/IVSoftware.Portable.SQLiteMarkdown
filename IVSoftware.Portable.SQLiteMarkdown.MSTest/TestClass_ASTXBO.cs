using static IVSoftware.Portable.Threading.Extensions;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System.Diagnostics;
using SQLite;
using Newtonsoft.Json;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.QFTemplates;

namespace IVSoftware.Portable.SQLiteMarkdown.XBO
{
    [TestClass]
    public sealed class TestClass_ASTXBO
    {
        static Dictionary<Type, Dictionary<QueryFilterMode, Dictionary<string, string>>> LimitTable { get; } = new()
        {            
            // PUBLISHED: ReadMe version verify claims.
            [typeof(PetProfile)] = new()
            {
                [QueryFilterMode.Query] = new()
                {
                    // Implicit AND
                    ["cat dog"] = "SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')",
                    // Explicit AND
                    ["cat & dog"] = @"SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')",
                    // Redundant AND syntax normalized 
                    ["cat &&& dog"] = @"SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')",
                    // OR Operator
                    ["cat | dog"] = @"SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%')",
                    // Redundant OR syntax normalized
                    ["cat || dog"] = @"SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%')",
                    // AND with NOT
                    ["cat !dog"] = @"SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%') AND (NOT (Name LIKE '%dog%' OR Species LIKE '%dog%'))",
                    // Escaped NOT — treated as literal
                    ["\\!cat"] = @"SELECT * FROM pets WHERE (Name LIKE '%!cat%' OR Species LIKE '%!cat%')",
                    // Single NOT
                    ["!cat"] = @"SELECT * FROM pets WHERE (NOT (Name LIKE '%cat%' OR Species LIKE '%cat%'))",
                    // Negated group
                    ["!(cat | dog)"] = @"SELECT * FROM pets WHERE (NOT ((Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%')))",
                    // Exact match using single quotes 
                    ["'exact phrase'"] = @"SELECT * FROM pets WHERE (Name LIKE '%exact phrase%' OR Species LIKE '%exact phrase%')",
                    // Exact match using double quotes 
                    ["\"exact phrase\""] = @"SELECT * FROM pets WHERE (Name LIKE '%exact phrase%' OR Species LIKE '%exact phrase%')",
                    // Escaped quotes
                    ["\"\"Hello\"\""] = @"SELECT * FROM pets WHERE (Name LIKE '%""Hello""%' OR Species LIKE '%""Hello""%')",
                    // Escaped quotes
                    ["\\\"Hello\\\""] = @"SELECT * FROM pets WHERE (Name LIKE '%""Hello""%' OR Species LIKE '%""Hello""%')",
                }
            },

            // Split Contract Template over PetProfileSC
            [typeof(PetProfileN)] = new()
            {
                [QueryFilterMode.Query] = new()
                {
                    ["cat"] = "SELECT * FROM pets WHERE (Name LIKE '%cat%')",
                    ["!cat"] = "SELECT * FROM pets WHERE (NOT (Name LIKE '%cat%'))",
                    ["cat dog"] = "SELECT * FROM pets WHERE (Name LIKE '%cat%') AND (Name LIKE '%dog%')",
                    ["!cat dog"] = "SELECT * FROM pets WHERE (NOT (Name LIKE '%cat%')) AND (Name LIKE '%dog%')",
                    ["!(cat|dog)"] = "SELECT * FROM pets WHERE (NOT ((Name LIKE '%cat%') OR (Name LIKE '%dog%')))",
                    ["pet!(cat|dog)"] = "SELECT * FROM pets WHERE (Name LIKE '%pet%') AND (NOT ((Name LIKE '%cat%') OR (Name LIKE '%dog%')))",
                }
            },

            // Split Contract Template over PetProfileSC
            [typeof(PetProfileNS)] = new()
            {
                [QueryFilterMode.Query] = new()
                {
                    ["cat"] = "SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%')",
                    ["!cat"] = "SELECT * FROM pets WHERE (NOT (Name LIKE '%cat%' OR Species LIKE '%cat%'))",
                    ["cat dog"] = "SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')",
                    ["!cat dog"] = "SELECT * FROM pets WHERE (NOT (Name LIKE '%cat%' OR Species LIKE '%cat%')) AND (Name LIKE '%dog%' OR Species LIKE '%dog%')",
                    ["pet!(cat|dog)"] = "SELECT * FROM pets WHERE (Name LIKE '%pet%' OR Species LIKE '%pet%') AND (NOT ((Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%')))",
                },
            },

            // Split Contract Template over PetProfileSC
            [typeof(PetProfileNS_N)] = new()
            {
                [QueryFilterMode.Query] = new()
                {
                    ["cat"] = "SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%')",
                    ["!cat"] = "SELECT * FROM pets WHERE (NOT (Name LIKE '%cat%' OR Species LIKE '%cat%'))",
                    ["cat dog"] = "SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')",
                    ["!cat dog"] = "SELECT * FROM pets WHERE (NOT (Name LIKE '%cat%' OR Species LIKE '%cat%')) AND (Name LIKE '%dog%' OR Species LIKE '%dog%')",
                    ["pet!(cat|dog)"] = "SELECT * FROM pets WHERE (Name LIKE '%pet%' OR Species LIKE '%pet%') AND (NOT ((Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%')))"
                },
                [QueryFilterMode.Filter] = new()
                {
                    ["cat"] = "SELECT * FROM pets WHERE (Name LIKE '%cat%')",
                }
            },

            // Split Contract Template over PetProfileSC
            [typeof(PetProfileN_NT_T)] = new()
            {
                [QueryFilterMode.Query] = new()
                {
                },
                [QueryFilterMode.Filter] = new()
                {
                }
            }
        };


#if false
        [TestMethod]
        public void Test_Template()
        {
            try
            {
                Awaited += localOnAwaited;
                @"cat".ParseSqlMarkdown<PetProfileN>();    
            }
            finally
            {
                Awaited -= localOnAwaited;
            }

            void localOnAwaited(object? sender, AwaitedEventArgs e)
            {
                if (sender is MarkdownContext mc)
                {
                    switch (e.Caller)
                    {
                        case "localEscape":
                            break;
                        case "localLint":
                            break;
                        case "localTransliterate":
                            break;
                        case "localBuildAST":
                            break;
                        default:
                            break;
                    }
                }
            }  
        }
#endif
        [TestMethod]
        public void Test_TransformWithEscapedCharacters()
        {
            // See also: {7E8A9B6F-5AED-48CB-9EB8-EF72D22B9970}
            var id1 = Thread.CurrentThread.ManagedThreadId;
            string actual, expected;

            try
            {
                Awaited += localOnAwaited;
                @"\& \| \! \( \) \[ \] \' \"" \\".ParseSqlMarkdown<PetProfileN>();
            }
            finally
            {
                Awaited -= localOnAwaited;
            }

            void localOnAwaited(object? sender, AwaitedEventArgs e)
            {
                var id2 = Thread.CurrentThread.ManagedThreadId;
                if (id1 == id2)
                {
                    if (sender is MarkdownContext mc)
                    {
                        switch (e.Caller)
                        {
                            case "localEscape":
                                actual = mc.Transform;
                                expected = @" 
$FEFE0000$ $FEFE0001$ $FEFE0002$ $FEFE0003$ $FEFE0004$ $FEFE0005$ $FEFE0006$ $FEFE0007$ $FEFE0008$ $FEFE0009$";

                                Assert.AreEqual(
                                    expected.NormalizeResult(),
                                    actual.NormalizeResult(),
                                    "Expecting escaped but not linted"
                                );
                                break;

                            case "localLint":
                                actual = mc.Transform;
                                expected = @" 
$FEFE0000$&$FEFE0001$&$FEFE0002$&$FEFE0003$&$FEFE0004$&$FEFE0005$&$FEFE0006$&$FEFE0007$&$FEFE0008$&$FEFE0009$"
                                ;

                                Assert.AreEqual(
                                    expected.NormalizeResult(),
                                    actual.NormalizeResult(),
                                    "Expecting lint to make substitutions for single spaces."
                                );
                                break;
#if false
                            case "localTransliterate":
                                actual = mc.TermLogic;

                                actual.ToClipboardExpected();

                                actual.ToClipboardExpected();
                                { }
                                expected = @" 
$FEFF_property$ LIKE '%$FEFE0000$%'
AND
$FEFF_property$ LIKE '%$FEFE0001$%'
AND
$FEFF_property$ LIKE '%$FEFE0002$%'
AND
$FEFF_property$ LIKE '%$FEFE0003$%'
AND
$FEFF_property$ LIKE '%$FEFE0004$%'
AND
$FEFF_property$ LIKE '%$FEFE0005$%'
AND
$FEFF_property$ LIKE '%$FEFE0006$%'
AND
$FEFF_property$ LIKE '%$FEFE0007$%'
AND
$FEFF_property$ LIKE '%$FEFE0008$%'
AND
$FEFF_property$ LIKE '%$FEFE0009$%'"
                                ;

                                Assert.AreEqual(
                                    expected.NormalizeResult(),
                                    actual.NormalizeResult(),
                                    "Expecting templated core logic term"
                                );
                                break;
#endif

                            case "localBuildAST":
#if false
                                actual = mc.TermLogic;

                                actual.ToClipboardExpected();
                                expected = @" 
$FEFF_property$ LIKE '%$FEFE0000$%'
AND
$FEFF_property$ LIKE '%$FEFE0001$%'
AND
$FEFF_property$ LIKE '%$FEFE0002$%'
AND
$FEFF_property$ LIKE '%$FEFE0003$%'
AND
$FEFF_property$ LIKE '%$FEFE0004$%'
AND
$FEFF_property$ LIKE '%$FEFE0005$%'
AND
$FEFF_property$ LIKE '%$FEFE0006$%'
AND
$FEFF_property$ LIKE '%$FEFE0007$%'
AND
$FEFF_property$ LIKE '%$FEFE0008$%'
AND
$FEFF_property$ LIKE '%$FEFE0009$%'"
                                ;

                                Assert.AreEqual(
                                    expected.NormalizeResult(),
                                    actual.NormalizeResult(),
                                    "Expecting sql with placeholders for property and escaped characters."
                                );
#endif
                                break;

                            case "localUnescapeTermLogic":
#if false
                                actual = mc.TermLogic;
                                actual.ToClipboardExpected();
                                { }
                                expected = @" 
$FEFF_property$ LIKE '%&%'
AND
$FEFF_property$ LIKE '%|%'
AND
$FEFF_property$ LIKE '%!%'
AND
$FEFF_property$ LIKE '%(%'
AND
$FEFF_property$ LIKE '%)%'
AND
$FEFF_property$ LIKE '%[%'
AND
$FEFF_property$ LIKE '%]%'
AND
$FEFF_property$ LIKE '%'%'
AND
$FEFF_property$ LIKE '%""%'
AND
$FEFF_property$ LIKE '%\%'"
                                ;

                                Assert.AreEqual(
                                    expected.NormalizeResult(),
                                    actual.NormalizeResult(),
                                    "Expecting sql with placeholders for property and rehydrated characters."
                                );
#endif
                                break;

                            case "localBuildExpression":
                                actual = mc.Query;

                                actual.ToClipboardExpected();
                                { }
                                expected = @" 
SELECT * FROM pets WHERE 
(Name LIKE '%&%') AND (Name LIKE '%|%') AND (Name LIKE '%!%') AND (Name LIKE '%(%') AND (Name LIKE '%)%') AND (Name LIKE '%[%') AND (Name LIKE '%]%') AND (Name LIKE '%''%') AND (Name LIKE '%""%') AND (Name LIKE '%\%')"
                                ;

                                Assert.AreEqual(
                                    expected.NormalizeResult(),
                                    actual.NormalizeResult(),
                                    "Expecting valid fully escaped query"
                                );
                                break;
                            default:
                                throw new NotImplementedException($"Bad case: {e.Caller}");
                        }
                    }
                }
                else
                { }
            }
        }

        [TestMethod]
        public void Test_LimitTable()
        {
            string actual;
            XElement xast;
            foreach (var (type, modeDict) in LimitTable)
            {
                foreach (var (qfMode, exprDict) in modeDict)
                {
                    foreach (var (input, expected) in exprDict)
                    {
                        try
                        {
                            Debug.WriteLine("============================================");
                            Debug.WriteLine($"🧪 Type: {type.Name}");
                            Debug.WriteLine($"🧪 Mode: {qfMode}");
                            Debug.WriteLine($"🧪 Input: {input}");

                            actual = input.ParseSqlMarkdown(type, qfMode, out xast);

                            Debug.WriteLine("✅ Actual SQL:");
                            Debug.WriteLine(actual);
                            Debug.WriteLine("✅ Expected SQL:");
                            Debug.WriteLine(expected);

                            Assert.AreEqual(
                                expected.NormalizeResult(),
                                actual.NormalizeResult(),
                                $"❌ MISMATCH for [{type.Name}] [{qfMode}]\nInput: \"{input}\""
                            );

                            Debug.WriteLine("✅ PASS");
                            Debug.WriteLine(xast.ToString());
                        }
                        catch (Exception ex)
                        {
                            var msg = $"💥 EXCEPTION for [{type.Name}] [{qfMode}]\nInput: \"{input}\"\n{ex}";
                            Debug.WriteLine(msg);
                            Assert.Fail(msg);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Test_SingleTermVariants()
        {
            string actual, expected;
            #region S U B T E S T S
            XElement xast;
            subtestPetProfileN();
            subtestPetProfileNS();

            void subtestPetProfileN()
            {
                actual = "cat".ParseSqlMarkdown<PetProfileN>();
                expected = @" 
SELECT * FROM pets WHERE (Name LIKE '%cat%') "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting single term LIKE expression"
                );

                actual = "!cat".ParseSqlMarkdown<PetProfileN>();

                actual.ToClipboardExpected();
                expected = @" 
SELECT * FROM pets WHERE (NOT (Name LIKE '%cat%'))"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting single term NOT LIKE expression"
                );
            }
            void subtestPetProfileNS()
            {
                actual = "cat".ParseSqlMarkdown<PetProfileNS>();

                actual.ToClipboardExpected();
                expected = @" 
SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%') "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting single term LIKE expression"
                );

                actual = "!cat".ParseSqlMarkdown<PetProfileNS>();
                actual.ToClipboardExpected();
                { }
                expected = @" 
SELECT * FROM pets WHERE (NOT (Name LIKE '%cat%' OR Species LIKE '%cat%'))"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting single term NOT LIKE expression"
                );
            }
            #endregion S U B T E S T S
        }

        /// <summary>
        /// Transitory test representing a single GZ in progress.
        /// </summary>
        [TestMethod]
        public void Test_GZ()
        {
            string actual, expected;

            actual = "\\\"Hello\\\"".ParseSqlMarkdown<PetProfileN>();
            actual.ToClipboardExpected();
            { }
            expected = @" 
SELECT * FROM pets WHERE 
(Name LIKE '%""Hello""%')"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult()
            );
        }

        [TestMethod]
        public void Test_TwoTermVariants()
        {
            string actual, expected;
            #region S U B T E S T S

            subtestPetProfileN();
            subtestPetProfileNS();

            void subtestPetProfileN()
            {
                actual = "cat dog".ParseSqlMarkdown<PetProfileN>();
                expected = @" 
SELECT * FROM pets WHERE (Name LIKE '%cat%') AND (Name LIKE '%dog%')"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting equivalent expression."
                );

                actual = "!cat dog".ParseSqlMarkdown<PetProfileN>();
                expected = @" 
SELECT * FROM pets WHERE (NOT (Name LIKE '%cat%')) AND (Name LIKE '%dog%')"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting equivalent expression."
                );

                actual = "!(cat|dog)".ParseSqlMarkdown<PetProfileN>();
                expected = @" 
SELECT * FROM pets WHERE (NOT ((Name LIKE '%cat%') OR (Name LIKE '%dog%')))"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting equivalent expression."
                );

                actual = "pet!(cat|dog)".ParseSqlMarkdown<PetProfileN>();
                expected = @" 
SELECT * FROM pets WHERE (Name LIKE '%pet%') AND (NOT ((Name LIKE '%cat%') OR (Name LIKE '%dog%')))"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting equivalent expression."
                );
            }

            void subtestPetProfileNS()
            {
                actual = "cat dog".ParseSqlMarkdown<PetProfileNS>();
                expected = @" 
SELECT * FROM pets WHERE (Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting equivalent expression."
                );

                actual = "!cat dog".ParseSqlMarkdown<PetProfileNS>();
                actual.ToClipboardExpected();
                expected = @" 
SELECT * FROM pets WHERE (NOT (Name LIKE '%cat%' OR Species LIKE '%cat%')) AND (Name LIKE '%dog%' OR Species LIKE '%dog%')"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting equivalent expression."
                );
            }
            #endregion S U B T E S T S
        }

        [TestMethod]
        public void Test_AtomicQuotes()
        {
            string actual, expected;
            subtestDQuote();
            subtestSQuote();
            subtestApostropheInsideDoubleQuote();

            #region S U B T E S T S
            void subtestDQuote()
            {
                actual = @"""atomic quote""".ParseSqlMarkdown<PetProfileNS>();
                expected = @" 
    SELECT * FROM pets WHERE (Name LIKE '%atomic quote%' OR Species LIKE '%atomic quote%')";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting space is preserved inside atomic quote."
                );
            }
            // To be clear:
            // - This produces an IDENTICAL result to the DQuote version.
            // - Yes, the test limit is the same by design.
            void subtestSQuote()
            {
                actual = @"'atomic quote'".ParseSqlMarkdown<PetProfileNS>();
                expected = @" 
    SELECT * FROM pets WHERE (Name LIKE '%atomic quote%' OR Species LIKE '%atomic quote%')";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting space is preserved inside atomic quote."
                );
            }
            void subtestApostropheInsideDoubleQuote()
            {
                actual = @"""asperger's""".ParseSqlMarkdown<PetProfileNS>();
                expected = @" 
SELECT * FROM pets WHERE 
(Name LIKE '%asperger's%' OR Species LIKE '%asperger's%')"
                ; 
                expected = @" 
SELECT * FROM pets WHERE (Name LIKE '%asperger''s%' OR Species LIKE '%asperger''s%')"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting apostrophe is preserved inside atomic quote."
                );
            }
            #endregion S U B T E S T S
        }

        [TestMethod]
        public void Test_TagVariants()
        {
            string actual, expected;

            actual = @"match".ParseSqlMarkdown<PetProfileN_N_T>();

            actual.ToClipboardExpected();
            { }
            expected = @" 
SELECT * FROM pets WHERE 
(Name LIKE '%match%')"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting absence of Tags LIKE because the term is not a tag and mode is strict."
            );

            actual = @"[match]".ParseSqlMarkdown<PetProfileN_N_T>();
            expected = @" 
SELECT * FROM pets WHERE (Name LIKE '%[match]%' OR Tags LIKE '%[match]%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting Tags LIKE because the token identifies as an explicit tag."
            );

            actual = @"match".ParseSqlMarkdown<PetProfileN_NT_T>();
            expected = @" 
SELECT * FROM pets WHERE (Name LIKE '%match%' OR Tags LIKE '%match%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting Tags LIKE with relaxed match."
            );
        }

        [TestMethod]
        public void Test_SplitContract()
        {
            string actual, expected;
            List<PetProfileSC> recordset;
            int autoIncrement = 0;
            using (var cnx = new SQLiteConnection(":memory:"))
            {
                cnx.CreateTable<PetProfileSC>(); cnx.InsertAll(new PetProfileSC[]
                {
                    new() { Id = $"{autoIncrement++:D4}", Name = "Whiskers", Species = "Cat", Tags = "[indoor],[pet]" },
                    new() { Id = $"{autoIncrement++:D4}", Name = "Rover", Species = "Dog", Tags = "[outdoor],[pet]" },
                    new() { Id = $"{autoIncrement++:D4}", Name = "Mr. Fluff", Species = "Rabbit", Tags = "[pet]" },
                    new() { Id = $"{autoIncrement++:D4}", Name = "Shadow", Species = "Cat", Tags = "[indoor],[shy]" },
                    new() { Id = $"{autoIncrement++:D4}", Name = "Fido", Species = "Dog", Tags = "[loyal]" },
                    new() { Id = $"{autoIncrement++:D4}", Name = "Mystery", Species = "Unknown", Tags = "[feral],[alert]" },
                });

                // POC
                var sql = "Whiskers".ParseSqlMarkdown<PetProfileN_NST_T>();

                actual = sql;
                expected = @" 
SELECT * FROM pets WHERE (Name LIKE '%Whiskers%' OR Species LIKE '%Whiskers%' OR Tags LIKE '%Whiskers%')";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting full SQL expr, not just a clause."
                );

                recordset = cnx.Query<PetProfileSC>("Whiskers".ParseSqlMarkdown<PetProfileN_NST_T>());
                actual = string.Join(Environment.NewLine, recordset.Select(_ => JsonConvert.SerializeObject(_, Formatting.Indented)));
                expected = @" 
{
  ""Id"": ""0000"",
  ""Name"": ""Whiskers"",
  ""Species"": ""Cat"",
  ""Tags"": ""[indoor],[pet]""
}";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting recordset to match."
                );

                // Species match via split contract
                recordset = cnx.Query<PetProfileSC>("cat".ParseSqlMarkdown<PetProfileNS>());
                actual = string.Join(Environment.NewLine, recordset.Select(r => r.Id));
                expected = string.Join(Environment.NewLine, new[] { "0000", "0003" });
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting match for: cat"
                );

                // Exact name match
                recordset = cnx.Query<PetProfileSC>("Rover".ParseSqlMarkdown<PetProfileN_NST_T>());
                actual = string.Join(Environment.NewLine, recordset.Select(r => r.Id));
                expected = string.Join(Environment.NewLine, new[] { "0001" });
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting match for: Rover"
                );

                // Strict tag match
                recordset = cnx.Query<PetProfileSC>("[pet]".ParseSqlMarkdown<PetProfileN_NST_T>());
                actual = string.Join(Environment.NewLine, recordset.Select(r => r.Id));
                expected = string.Join(Environment.NewLine, new[] { "0000", "0001", "0002" });
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting match for: [pet]"
                );

                // Soft tag match
                recordset = cnx.Query<PetProfileSC>("pet".ParseSqlMarkdown<PetProfileN_NST_T>());
                actual = string.Join(Environment.NewLine, recordset.Select(r => r.Id));
                expected = string.Join(Environment.NewLine, new[] { "0000", "0001", "0002" });
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting match for: pet"
                );

                // Combined name and tag match
                recordset = cnx.Query<PetProfileSC>("whiskers [pet]".ParseSqlMarkdown<PetProfileN_NST_T>());
                actual = string.Join(Environment.NewLine, recordset.Select(r => r.Id));
                expected = string.Join(Environment.NewLine, new[] { "0000" });
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting match for: whiskers [pet]"
                );

                // Negated tag
                recordset = cnx.Query<PetProfileSC>("![shy]".ParseSqlMarkdown<PetProfileN_NST_T>());
                actual = string.Join(Environment.NewLine, recordset.Select(r => r.Id));
                expected = string.Join(Environment.NewLine, new[] { "0000", "0001", "0002", "0004", "0005" });
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting match for: ![shy]"
                );

                // OR logic
                recordset = cnx.Query<PetProfileSC>("rabbit | [indoor]".ParseSqlMarkdown<PetProfileN_NST_T>());
                actual = string.Join(Environment.NewLine, recordset.Select(r => r.Id));
                expected = string.Join(Environment.NewLine, new[] { "0000", "0002", "0003" });
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting match for: rabbit | [indoor]"
                );

                // Name match
                recordset = cnx.Query<PetProfileSC>("mystery".ParseSqlMarkdown<PetProfileN_NST_T>());
                actual = string.Join(Environment.NewLine, recordset.Select(r => r.Id));
                expected = string.Join(Environment.NewLine, new[] { "0005" });
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting match for: mystery"
                );
            }
        }
    }
}
