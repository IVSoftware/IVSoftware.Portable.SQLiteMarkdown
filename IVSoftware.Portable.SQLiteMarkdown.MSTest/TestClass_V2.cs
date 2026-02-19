using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.WinOS.MSTest.Extensions;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_V2
{
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

    [TestMethod]
    public async Task Test_IsFilterExecutionEnabled()
    {
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
                case nameof(MarkdownContext.FilterQueryDatabase):
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

        MarkdownContext<SelectableQFModel> mdc;

        subtest_MDCExpectingFilterDatabase();

        subtest_MDCSuppressingDatabase();

        subtest_StringExtensionAutoSuppress();

        #region S U B T E S T S 
        // Captures OnAwaited event when FilterDatabase is created in parameterless (original) CTor.
        void subtest_MDCExpectingFilterDatabase()
        {
            mdc = new();

            sep = eventQueue.DequeueSingle();
            Assert.IsNotNull(sep, "Expecting database creation.");
        }

        // Captures 'absence of' OnAwaited event when FilterDatabase is suppressed in CTor.
        void subtest_MDCSuppressingDatabase()
        {
            mdc = new MarkdownContext<SelectableQFModel>(
                isFilterExecutionEnabled: false
            );
            Assert.AreEqual(0, eventQueue.Count(), "Expecting *no* database creation.");
        }

        void subtest_StringExtensionAutoSuppress()
        {
            "carrot".ParseSqlMarkdown<SelectableQFModel>();
            Assert.AreEqual(0, eventQueue.Count(), "Expecting *no* database creation.");
        }
        #endregion S U B T E S T S
    }
}
