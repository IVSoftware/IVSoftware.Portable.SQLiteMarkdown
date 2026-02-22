using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.WinOS.MSTest.Extensions;
using Newtonsoft.Json;
using SQLite;
using System.Configuration;
using System.Security.Cryptography;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_V2
{
#if false
    /// <summary>
    /// Exercise the 2.0.0 parameterless CTor which we describe as Anonymous.
    /// </summary>
    /// <remarks>
    /// When constructed in this manner, the ContractType is null and will
    /// throw hard if accessed. But this is not a breaking change because
    /// previous versions simply did not allow instantiation in this condition.
    /// </remarks>
    [TestMethod]
    public void Test_Anonymous()
    {
        string actual, expected;

        #region L o c a l F x
        var builderThrow = new List<string>();
        void localOnBeginThrowOrAdvise(object? sender, Throw e)
        {
            builderThrow.Add(e.Message);
            e.Handled = true;
        }
        #endregion L o c a l F x
        using var local = this.WithOnDispose(
            onInit: (sender, e) =>
            {
                Throw.BeginThrowOrAdvise += localOnBeginThrowOrAdvise;
            },
            onDispose: (sender, e) =>
            {
                Throw.BeginThrowOrAdvise -= localOnBeginThrowOrAdvise;
            });

        MarkdownContext mdc = new();
        Assert.IsNull(mdc.ContractType);

        actual = JsonConvert.SerializeObject(builderThrow, Formatting.Indented);
        expected = @" 
[
  ""ContractType cannot be null.""
]";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting ContractType cannot be null."
        );
    }
#endif

    [TestMethod]
    public void Test_Contract()
    {
        string actual, expected;

        MarkdownContext mdc = new(typeof(SelectableQFModel));
        Assert.IsNotNull(mdc.ContractType);
    }

    [TestMethod]
    public void Test_ContractT()
    {
        string actual, expected;

        MarkdownContext<SelectableQFModel> mdc = new();
        Assert.IsNotNull(mdc.ContractType);
    }

    /// <summary>
    /// Streamlined checker for table inheritance.
    /// </summary>
    /// <remarks>
    /// #{B593ED5F-684A-4EF1-AA45-66E3766C7277}
    /// </remarks>
    [TestMethod]
    public void Test_StreamlinedTableAttributeInheritance()
    {
        string actual, expected;
        Queue<SenderEventPair> eventQueue = new();
        SenderEventPair sep;

        #region L o c a l F x 
        using var awaited = this.WithOnDispose(
            onInit: (sender, e) => Threading.Extensions.Awaited += localOnAwaited,
            onDispose: (sender, e) => Threading.Extensions.Awaited -= localOnAwaited);
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            switch (e.Caller)
            {
                case "FilterQueryDatabase":
                    eventQueue.Enqueue(new(sender, e));
                    break;
            }
        }
        void localOnEvent(object? sender, Throw e)
        {
            eventQueue.Enqueue((sender, e));
        }
        #endregion L o c a l F x

        using var local = this.WithOnDispose(
            onInit: (sender, e) =>
            {
                Throw.BeginThrowOrAdvise += localOnEvent;
            },
            onDispose: (sender, e) =>
            {
                Throw.BeginThrowOrAdvise += localOnEvent;
            });

        TableMapping mapping;
        using SQLiteConnection cnx = new(":memory:");

        subtest_SelectableQFModel();
        subtest_ExplicitAttributeOnSubclass();
        subtest_ImplicitGotcha();

        #region S U B T E S T S
        void subtest_SelectableQFModel()
        {
            // Uncontroversial explicit mapping
            mapping = cnx.GetMapping<SelectableQFModel>();
            Assert.AreEqual("items", mapping.TableName, @"Expecting [Table(""items""]");
            Assert.AreEqual("Id", mapping.PK.PropertyName);
        }

        void subtest_ExplicitAttributeOnSubclass()
        {
            // Uncontroversial explicit mapping
            mapping = cnx.GetMapping<SelectableQFModelSubclassA>();
            Assert.AreEqual("itemsA", mapping.TableName, @"Expecting [Table(""itemsA""]");
            Assert.AreEqual("Id", mapping.PK.PropertyName);
        }

        void subtest_ImplicitGotcha()
        {
            // Illustrative, and problematic implicit mapping
            mapping = cnx.GetMapping<SelectableQFModelSubclassG>();

            // B O O
            Assert.AreEqual(
                "SelectableQFModelSubclassG",
                mapping.TableName,
                @"Expecting inherited attribute goes unused.");
            Assert.AreEqual("Id", mapping.PK.PropertyName);
        }
        void subtest_Case4()
        {
            actual = "green".ParseSqlMarkdown<SelectableQFModel>();

            actual.ToClipboardExpected();
            { } // <- FIRST TIME ONLY: Adjust the message.
            actual.ToClipboardAssert("Expecting result to match.");
            { }
        }
        #endregion S U B T E S T S
    }

    [TestMethod]
    public void Test_SelfIndexingIllegalChars()
    {
        string actual, expected;

        subtest_SafeCharsOnly();
        subtest_ExclamationPoint();

        #region S U B T E S T S 
        void subtest_SafeCharsOnly()
        {
            var model = new SelectableQFModel
            {
                Description = "Hello World",
                Keywords = "standard greeting",
                Tags = "intro, 101",
            };
            actual = model.QueryTerm;
            actual.ToClipboardExpected();
            { }
            expected = @" 
hello~world~standard~greeting~[intro][101]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting resolved controversial term generation."
            );

            actual = model.FilterTerm;
            actual.ToClipboardExpected();
            { }
            expected = @" 
hello~world~standard~greeting~[intro][101]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting resolved controversial term generation."
            );

            actual = model.TagMatchTerm;
            actual.ToClipboardExpected();
            { }
            expected = @" 
[intro][101]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting resolved controversial term generation."
            );
        }
        void subtest_ExclamationPoint()
        {
            var model = new SelectableQFModel
            {
                Description = "Hello World!",
                Keywords = "standard greeting",
                Tags = "intro, 101",
            };
            actual = model.QueryTerm;
            actual.ToClipboardExpected();
            { }
            expected = @" 
hello~world!~standard~greeting~[intro][101]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting resolved controversial term generation."
            );

            actual = model.FilterTerm;
            actual.ToClipboardExpected();
            { }
            expected = @" 
hello~world!~standard~greeting~[intro][101]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting resolved controversial term generation."
            );

            actual = model.TagMatchTerm;
            actual.ToClipboardExpected();
            { }
            expected = @" 
[intro][101]"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting uncontroversial term generation."
            );
        }
        #endregion S U B T E S T S
    }

    /// <summary>
    /// Tests transient e.g. 'filter time out' queries with trailing operators.
    /// </summary>
    [TestMethod]
    public async Task Test_TrailingOperator()
    {
        string actual, expected, sql;

        var mdc = new MarkdownContext<SelectableQFModel>();

        using (var cnx = new SQLiteConnection(":memory:"))
        {
            cnx.CreateTable<SelectableQFModel>();

            await subtest_TrailingBackslash();
            await subtest_TrailingAnd();
            await subtest_TrailingOr();
            await subtest_TrailingNot();

            #region S U B T E S T S
            async Task subtest_TrailingBackslash()
            {
                mdc.InputText = @"animal\";
                await mdc;
                sql = mdc.ParseSqlMarkdown();
                actual = sql;

                actual.ToClipboardExpected();
                { }
                expected = @" 
SELECT * FROM items WHERE 
(QueryTerm LIKE '%animal%')"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting trailing operator uncertainty."
                );
                // Ensure no exceptions on actual query.
                _ = cnx.Query<SelectableQFModel>(sql);
            }
            async Task subtest_TrailingAnd()
            {
                mdc.InputText = @"animal&";
                await mdc;
                sql = mdc.ParseSqlMarkdown();

                actual = sql;
                expected = @" 
SELECT * FROM items WHERE 
(QueryTerm LIKE '%animal%')"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting trailing operator uncertainty."
                );
                // Ensure no exceptions on actual query.
                _ = cnx.Query<SelectableQFModel>(sql);
            }
            async Task subtest_TrailingOr()
            {
                mdc.InputText = @"animal|";
                await mdc;
                sql = mdc.ParseSqlMarkdown();

                actual = sql;
                expected = @" 
SELECT * FROM items WHERE 
(QueryTerm LIKE '%animal%')"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting trailing operator uncertainty."
                );
                // Ensure no exceptions on actual query.
                _ = cnx.Query<SelectableQFModel>(sql);
            }
            async Task subtest_TrailingNot()
            {
                mdc.InputText = @"animal!";
                await mdc;
                sql = mdc.ParseSqlMarkdown();

                actual = sql;
                expected = @" 
SELECT * FROM items WHERE 
(QueryTerm LIKE '%animal%')"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting trailing operator uncertainty."
                );
                // Ensure no exceptions on actual query.
                _ = cnx.Query<SelectableQFModel>(sql);
            }
            #endregion S U B T E S T S
        }
    }

    /// <summary>
    /// The MDC will raise OnAwaited if/when the FilterQueryDatabase is instantiated. 
    /// Success in this test relies on the absence of any such events.
    /// </summary>
    [TestMethod]
    public void Test_NoSpuriousFQD()
    {
        Queue<SenderEventPair> eventQueue = new();

        #region L o c a l F x 
        using var awaited = this.WithOnDispose(
            onInit: (sender, e) => Threading.Extensions.Awaited += localOnAwaited,
            onDispose: (sender, e) => Threading.Extensions.Awaited -= localOnAwaited);
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            switch (e.Caller)
            {
                case "FilterQueryDatabase":
                    eventQueue.Enqueue(new(sender, e));
                    break;
            }
        }
        void localOnEvent(object? sender, Throw e)
        {
            eventQueue.Enqueue((sender, e));
        }
        #endregion L o c a l F x

        using var local = this.WithOnDispose(
            onInit: (sender, e) =>
            {
                Throw.BeginThrowOrAdvise += localOnEvent;
            },
            onDispose: (sender, e) =>
            {
                Throw.BeginThrowOrAdvise -= localOnEvent;
            });

        MarkdownContext<SelectableQFModel> mdc;

        subtest_AssertCtorNoFQD();

        subtest_StringExtensionNoFQD();

        #region S U B T E S T S 
        // Captures 'absence of' OnAwaited event.
        void subtest_AssertCtorNoFQD()
        {
            mdc = new();
            Assert.AreEqual(0, eventQueue.Count(), "Expecting *no* database creation.");
        }

        // Captures 'absence of' OnAwaited event.
        void subtest_StringExtensionNoFQD()
        {
            "carrot".ParseSqlMarkdown<SelectableQFModel>();
            Assert.AreEqual(0, eventQueue.Count(), "Expecting *no* database creation.");
        }
        #endregion S U B T E S T S
    }


    [TestMethod]
    public void Test_UtcEpoch()
    {
        string actual, expected;
        List<string> builder = new();

        SelectableQFPrimeModel prime, utcParent;

        subtest_CreateUtcParentInstance();
        subtest_Asap();

        subtest_UtcEpochClock();

        #region S U B T E S T S
        void subtest_CreateUtcParentInstance()
        {
            utcParent = new SelectableQFPrimeModel();
            Assert.IsNull(
                utcParent.AffinityMode,
                "Expecting null because nothing is set.");

            // This will pull Position into UtcStart.
            utcParent.AffinityMode = AffinityMode.Fixed;
            Assert.AreEqual(
                AffinityMode.Fixed,
                utcParent.AffinityMode,
                "Expecting FIXED because UTC Start is set.");

            utcParent.Duration = TimeSpan.FromMinutes(5);
            Assert.AreEqual(
                TimeSpan.FromMinutes(5),
                utcParent.Remaining,
                "Expecting FIXED even though DURATION is set because UTC Start is set and FIXED is higher priority that ASAP.");
        }

        void subtest_Asap()
        {
            prime = new SelectableQFPrimeModel();
            Assert.IsNull(
                prime.AffinityMode, 
                "Expecting null because nothing is set.");

            Assert.IsNull(
                prime.Duration, 
                "Expecting null by default.");

            Assert.IsNull(
                prime.Remaining,
                "Expecting null by default.");

            prime.Duration = TimeSpan.FromMinutes(5);
            Assert.AreEqual(
                TimeSpan.FromMinutes(5),
                prime.Remaining,
                "Expecting hard reset on Remaining.");

            Assert.AreEqual(
                AffinityMode.Asap,
                prime.AffinityMode,
                "Expecting ASAP because Duration is set and UtcStart is NULL.");
        }

        void subtest_UtcEpochClock()
        {
            UtcEpochClock.System.AffinityEpochTime = StdIvsEpoch.January1.ToDateTimeOffset();
            UtcEpochClock.System.AffinityEpochTime += TimeSpan.FromMinutes(1);
        }
        #endregion S U B T E S T S
    }

    [TestMethod]
    public async Task Test_ModularQuery()
    {
        string actual, expected;
        List<string> builder = new();


        var mdc = new MarkdownContext<SelectableQFPrimeModel>();


        await subtest_ModularQuery1();
        await subtest_ModularQuery2();
        await subtest_ModularQuery3();
        await subtest_ModularQuery4();
        await subtest_ModularQuery5();
        await subtest_ModularQuery6();
        await subtest_ModularQuery7();
        await subtest_ModularQuery8();
        await subtest_ModularQuery9();
        await subtest_ModularQuery10();

        #region S U B T E S T S
        async Task subtest_ModularQuery1()
        {
        }
        async Task subtest_ModularQuery2()
        {
        }
        async Task subtest_ModularQuery3()
        {
        }
        async Task subtest_ModularQuery4()
        {
        }
        async Task subtest_ModularQuery5()
        {
        }
        async Task subtest_ModularQuery6()
        {
        }
        async Task subtest_ModularQuery7()
        {
        }
        async Task subtest_ModularQuery8()
        {
        }
        async Task subtest_ModularQuery9()
        {
        }
        async Task subtest_ModularQuery10()
        {
        }
        #endregion S U B T E S T S
    }
}
