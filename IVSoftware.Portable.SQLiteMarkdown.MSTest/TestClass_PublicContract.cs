using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.WinOS.MSTest.Extensions;
using SQLite;
using IVSoftware.Portable.Common;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using Microsoft.ApplicationInsights.Metrics.Extensibility;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public partial class TestClass_SQLiteMarkdown
{
    [TestMethod]
    public void Test_PolicyException()
    {
        string actual, expected;

        #region L o c a l F x
        var builderThrow = new List<string>();
        void localOnBeginThrowOrAdvise(object? sender, Throw e)
        {
            builderThrow.Add(e.ToString());
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

        this.ThrowPolicyException(AffinityPolicy.MaterializedPathMustEndWithId);
        actual = string.Join(Environment.NewLine, builderThrow);
        expected = @" 
Id: AffinityPolicy.MaterializedPathMustEndWithId
Materialized Path Policy violation: Path must end with Id."
        ;

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting builder content to match."
        );
    }

    [TestMethod]
    public void Test_GetBreakingChanges()
    {
        string actual, expected;

        subtest_InstantiateMCC();

        subtest_PublicContract();
        void subtest_PublicContract()
        {
            // Future us:
            // - This subtest is intentionally green with one remaining diff.
            // - The surviving break is the removal of the old declared
            //   ObservableQueryFilterSource<T>.Clear(bool) surface.
            // - V2 keeps the parameterless "no surprises" clear, but does not
            //   preserve the older member that made a list-like type silently
            //   participate in MDC regression semantics.
            string 
                contractV1 =
                    "IVSoftware.Portable.SQLiteMarkdown.MSTest.Contracts.Version=1.0.1.xml"
                    .ReadManifestResourceFile<TestClass_SQLiteMarkdown>(ThrowOrAdvise.ThrowSoft),
                contractCurrent =
                    typeof(MarkdownContext)
                    .Assembly
                    .ToPublicContract()
                    .ToString();
            if(!contractV1.IsContractValid(contractCurrent, ManifestTypePolicy.IVSoftwareAssembliesOnly))
            {
                var diff = 
                    string.Join(
                        Environment.NewLine,
                        contractV1.GetBreakingChanges(contractCurrent, ManifestTypePolicy.AssemblyOnly));
                { }

#if DEBUG
                var mdc = new MarkdownContext(typeof(SelectableQFModel));
                var mdcT = new MarkdownContext<SelectableQFModel>();
                { }
                _ = mdc.ContractType;
                _ = mdcT.ContractType;
                _ = mdcT.RouteToFullRecordset;
                _ = mdc.ProxyType;
#endif

                actual = diff;
                actual.ToClipboardExpected();
                { }
                expected = @" 
<breakingChanges policy=""AssemblyOnly"">
    <namespace name=""IVSoftware.Portable.SQLiteMarkdown.Collections"">
        <type name=""ObservableQueryFilterSource&lt;T&gt;"">
            <method name=""Clear"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource&lt;T&gt;|Clear([external])-&gt;[external]"" />
        </type>
    </namespace>
</breakingChanges>"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting ONE deliberate breaking change that replaces a silent killer."
                );

                actual = string.Join(
                    Environment.NewLine,
                    typeof(IObservableQueryFilterSource<object>).GetInterfaces().Select(_ => _.Name));
                actual.ToClipboardExpected();
                { }
                expected = @" 
IObservableQueryFilterSource
IList
ICollection
IEnumerable
INotifyCollectionChanged
INotifyPropertyChanged";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting result to match."
                );
#if false && ABSTRACT
               // This was the original breaking change profile
               // when  we started analyzing V2 against V1.

                expected = @" 
<breakingChanges policy=""IVSoftwareAssembliesOnly"">
  <namespace name=""IVSoftware.Portable.SQLiteMarkdown"">
    <type name=""Extensions"">
      <method name=""ParseSqlMarkdown"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Extensions|ParseSqlMarkdown(IVSoftware.Portable.SQLiteMarkdown.MarkdownContext,[external],[external],IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode,[external])-&gt;[external]"" />
    </type>
    <type name=""IEditableQueryFilterItem"">
      <typeRemoved />
    </type>
    <type name=""MarkdownContext"">
      <property name=""ContractType"" type=""[external]"" canRead=""true"" canWrite=""true"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.MarkdownContext|ContractType|[external]|true|true"" />
      <property name=""InputTextSettleInterval"" type=""[external]"" canRead=""true"" canWrite=""true"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.MarkdownContext|InputTextSettleInterval|[external]|true|true"" />
      <property name=""Query"" type=""[external]"" canRead=""true"" canWrite=""true"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.MarkdownContext|Query|[external]|true|true"" />
      <property name=""RouteToFullRecordset"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.MarkdownContext|RouteToFullRecordset|[external]|true|false"" />
    </type>
    <type name=""MarkdownContext&lt;T&gt;"">
      <property name=""ContractType"" type=""[external]"" canRead=""true"" canWrite=""true"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.MarkdownContext&lt;T&gt;|ContractType|[external]|true|true"" />
      <property name=""InputTextSettleInterval"" type=""[external]"" canRead=""true"" canWrite=""true"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.MarkdownContext&lt;T&gt;|InputTextSettleInterval|[external]|true|true"" />
      <property name=""ProxyType"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.MarkdownContext&lt;T&gt;|ProxyType|[external]|true|false"" />
      <property name=""RouteToFullRecordset"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.MarkdownContext&lt;T&gt;|RouteToFullRecordset|[external]|true|false"" />
    </type>
  </namespace>
  <namespace name=""IVSoftware.Portable.SQLiteMarkdown.Collections"">
    <type name=""NotifyQueryFilterCollectionChangedAction"">
      <field name=""Add"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" signature=""F:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|Add|IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""ApplyFilter"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" signature=""F:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|ApplyFilter|IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""Move"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" signature=""F:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|Move|IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""QueryResult"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" signature=""F:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|QueryResult|IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""Remove"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" signature=""F:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|Remove|IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""RemoveFilter"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" signature=""F:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|RemoveFilter|IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""Replace"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" signature=""F:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|Replace|IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""Reset"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" signature=""F:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|Reset|IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""value__"" type=""[external]"" signature=""F:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|value__|[external]"" />
      <method name=""CompareTo"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|CompareTo([external])-&gt;[external]"" />
      <method name=""Equals"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|Equals([external])-&gt;[external]"" />
      <method name=""GetHashCode"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|GetHashCode()-&gt;[external]"" />
      <method name=""GetType"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|GetType()-&gt;[external]"" />
      <method name=""GetTypeCode"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|GetTypeCode()-&gt;[external]"" />
      <method name=""HasFlag"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|HasFlag([external])-&gt;[external]"" />
      <method name=""ToString"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|ToString()-&gt;[external]"" />
      <method name=""ToString"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|ToString([external],[external])-&gt;[external]"" />
      <method name=""ToString"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|ToString([external])-&gt;[external]"" />
      <typeRemoved />
    </type>
    <type name=""NotifyQueryFilterCollectionChangedEventArgs"">
      <constructor signature=""C:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs(IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction,[external])"" />
      <method name=""Equals"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs|Equals([external])-&gt;[external]"" />
      <method name=""GetHashCode"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs|GetHashCode()-&gt;[external]"" />
      <method name=""GetType"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs|GetType()-&gt;[external]"" />
      <method name=""ToString"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs|ToString()-&gt;[external]"" />
      <property name=""Action"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs|Action|[external]|true|false"" />
      <property name=""Action"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs|Action|IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction|true|false"" />
      <property name=""NewItems"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs|NewItems|[external]|true|false"" />
      <property name=""NewStartingIndex"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs|NewStartingIndex|[external]|true|false"" />
      <property name=""OldItems"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs|OldItems|[external]|true|false"" />
      <property name=""OldStartingIndex"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs|OldStartingIndex|[external]|true|false"" />
      <typeRemoved />
    </type>
    <type name=""ObservableQueryFilterSource"">
      <method name=""Equals"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource|Equals([external])-&gt;[external]"" />
      <method name=""GetHashCode"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource|GetHashCode()-&gt;[external]"" />
      <method name=""GetType"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource|GetType()-&gt;[external]"" />
      <method name=""ToString"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource|ToString()-&gt;[external]"" />
      <typeRemoved />
    </type>
    <type name=""ObservableQueryFilterSource&lt;T&gt;"">
      <method name=""Clear"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource&lt;T&gt;|Clear([external])-&gt;[external]"" />
      <property name=""ContractType"" type=""[external]"" canRead=""true"" canWrite=""true"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource&lt;T&gt;|ContractType|[external]|true|true"" />
      <property name=""InputTextSettleInterval"" type=""[external]"" canRead=""true"" canWrite=""true"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource&lt;T&gt;|InputTextSettleInterval|[external]|true|true"" />
      <property name=""Item"" type=""T"" canRead=""true"" canWrite=""true"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource&lt;T&gt;|Item|T|true|true"" />
      <property name=""MarkdownContextOR"" type=""IVSoftware.Portable.SQLiteMarkdown.MarkdownContextOR"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource&lt;T&gt;|MarkdownContextOR|IVSoftware.Portable.SQLiteMarkdown.MarkdownContextOR|true|false"" />
      <property name=""Placeholder"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource&lt;T&gt;|Placeholder|[external]|true|false"" />
      <property name=""ProxyType"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource&lt;T&gt;|ProxyType|[external]|true|false"" />
      <property name=""RouteToFullRecordset"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource&lt;T&gt;|RouteToFullRecordset|[external]|true|false"" />
      <property name=""UnfilteredItems"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource&lt;T&gt;|UnfilteredItems|[external]|true|false"" />
    </type>
  </namespace>
  <namespace name=""IVSoftware.Portable.SQLiteMarkdown.Events"">
    <type name=""ItemPropertyChangedEventArgs"">
      <constructor signature=""C:IVSoftware.Portable.SQLiteMarkdown.Events.ItemPropertyChangedEventArgs([external],[external])"" />
      <method name=""Equals"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Events.ItemPropertyChangedEventArgs|Equals([external])-&gt;[external]"" />
      <method name=""GetHashCode"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Events.ItemPropertyChangedEventArgs|GetHashCode()-&gt;[external]"" />
      <method name=""GetType"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Events.ItemPropertyChangedEventArgs|GetType()-&gt;[external]"" />
      <method name=""ToString"" signature=""M:IVSoftware.Portable.SQLiteMarkdown.Events.ItemPropertyChangedEventArgs|ToString()-&gt;[external]"" />
      <property name=""Item"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Events.ItemPropertyChangedEventArgs|Item|[external]|true|false"" />
      <property name=""PropertyName"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.SQLiteMarkdown.Events.ItemPropertyChangedEventArgs|PropertyName|[external]|true|false"" />
      <typeRemoved />
    </type>
  </namespace>
</breakingChanges>";
#endif
            }
        }

        #region S U B T E S T S
        void subtest_InstantiateMCC()
        {
            // Instantiate MDC
            MarkdownContext mdc = new(typeof(SelectableQFModel));
            Assert.IsNotNull(mdc.ContractType);
        }
        #endregion S U B T E S T S
    }

    [TestMethod]
    public void Test_ContractT()
    {
        string actual, expected;

        subtest_InstantiateMCC();

        #region S U B T E S T S
        void subtest_InstantiateMCC()
        {
            // Instantiate MMDC
            MarkdownContext mdc = new(typeof(SelectableQFModel));
            Assert.IsNotNull(mdc.ContractType);
        }
        #endregion S U B T E S T S
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
hello~world~standard~greeting~[intro]~[101]"
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
hello~world~standard~greeting~[intro]~[101]"
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
[intro] [101]"
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
hello~world!~standard~greeting~[intro]~[101]"
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
hello~world!~standard~greeting~[intro]~[101]"
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
[intro] [101]"
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
    [TestMethod, DoNotParallelize]
    public void Test_NoSpuriousFilterQueryDatabaseInstantiation()
    {
        string actual, expected;

        var builder = new List<string>();
        #region L o c a l F x 
        using var local = this.WithOnDispose(
            onInit: (sender, e) => IVSoftware.Portable.Threading.Extensions.Awaited += localOnAwaited,
            onDispose: (sender, e) => IVSoftware.Portable.Threading.Extensions.Awaited -= localOnAwaited);
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            switch (e.Caller)
            {
                case nameof(IModeledCollection.FilterQueryDatabase):
                    builder.Add(e[nameof(SQLiteConnection)]?.ToString() ?? "Missing");
                    break;
                default:
                    break;
            }
        }
        #endregion L o c a l F x
        subtest_AssertCtorNoFQD();
        subtest_OQFS();
        subtest_StringExtensionNoFQD();

        #region S U B T E S T S 
        // Captures 'absence of' OnAwaited event.
        void subtest_AssertCtorNoFQD()
        {
            builder.Clear();
            MarkdownContext<SelectableQFModel> mdc = new();
            Assert.HasCount(0, builder, $"Expecting no pings on FilterQueryDatabase Awaited.");
        }

        void subtest_OQFS()
        {
            // Test the translation from QueryFilterConfig to OMC.ModelTracking
            ObservableQueryFilterSource<SelectableQFModel> oqfs = new()
            {
                QueryFilterConfig = QueryFilterConfig.Query,
            };

            // It should not be possible to pull FQDB from anywhere.
            // NOTE: The idea of a "hybrid factory" is no more.
            if(oqfs.AsInterface<ITestableMDC>() is { } tmdc)
            {
                builder.Clear();

                // Tug on the factory getter down in the MDC.
                Assert.IsFalse(tmdc.HasFQDB);

                actual = string.Join(Environment.NewLine, builder); builder.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
get.IModeledCollection.Null";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting descriptor indicates Null with expected path."
                );

                // QueryFilterConfig must track ModelTracking.
                Assert.IsFalse(oqfs.ModelTracking.HasFlag(ModelTrackingFlag.ItemQueries));
                oqfs.QueryFilterConfig = QueryFilterConfig.QueryAndFilter;
                Assert.IsTrue(oqfs.ModelTracking.HasFlag(ModelTrackingFlag.ItemQueries));

                // Tug on the factory getter down in the MDC.
                Assert.IsTrue(tmdc.HasFQDB);

                actual = string.Join(Environment.NewLine, builder); builder.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
get.IModeledCollection.Assigned"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting descriptor indicates Assigned from IModeledCollection."
                );

                // ModelTracking must track QueryFilterConfig.
                Assert.IsTrue(oqfs.QueryFilterConfig.HasFlag(QueryFilterConfig.Filter));
                oqfs.ModelTracking &= ~ModelTrackingFlag.ItemQueries;
                Assert.IsFalse(oqfs.QueryFilterConfig.HasFlag(QueryFilterConfig.Filter));
            }
        }

        // Captures 'absence of' OnAwaited event.
        void subtest_StringExtensionNoFQD()
        {
            builder.Clear();
            "carrot".ParseSqlMarkdown<SelectableQFModel>();
            Assert.HasCount(0, builder, $"Expecting no pings on FilterQueryDatabase Awaited.");
        }
        #endregion S U B T E S T S
    }

    /// <summary>
    /// Test detection of "true proxy" mode.
    /// </summary>
    [TestMethod]
    public void Test_BUGIRL_ProxyInheritance()
    {
        string actual, expected, sql;

        var mapping = typeof(ItemCardModel).GetSQLiteMapping();
        Assert.AreEqual(nameof(ItemCardModel), mapping.TableName);

        subtest_IsNotTrueProxy();
        subtest_IsWeakProxy();
        subtest_IsTrueProxy();
        subtest_IsTrueProxyWithExtendSchema();
        subtest_NonIncoherentProxy();

        #region S U B T E S T S
        void subtest_IsNotTrueProxy()
        {
            sql = "green".ParseSqlMarkdown<ItemCardModel>();

            actual = sql;
            actual.ToClipboardExpected();
            { }
            expected = @" 
    SELECT * FROM ItemCardModel WHERE
    (QueryTerm LIKE '%green%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting table name to be ItemCardModel."
            );
        }
        void subtest_IsWeakProxy()
        {
            // PREREQUISITE FOR ANY PROXY
            var mdc = new MarkdownContext<SelectableQFModel>();

            // Because ItemCardModel is a subclass of SelectableQFModel
            // it *is* a weak proxy by inheritance. But it's still a
            // proxy, so we need to use the ContractType for table.

            sql = mdc.ParseSqlMarkdown<ItemCardModel>("green");

            actual = sql;
            actual.ToClipboardExpected();
            { }
            expected = @" 
    SELECT * FROM items WHERE
    (QueryTerm LIKE '%green%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting table name to be ItemCardModel."
            );
        }

        void subtest_IsTrueProxy()
        {
            // PREREQUISITE FOR ANY PROXY
            var mdc = new MarkdownContext<SelectableQFModel>();

            // Because ItemCardModel is a subclass of SelectableQFModel
            // it *is* a weak proxy by inheritance. But it's still a
            // proxy, so we need to use the ContractType for table.

            sql = mdc.ParseSqlMarkdown<TrueProxy>("green");

            actual = sql;
            actual.ToClipboardExpected();
            { }
            expected = @" 
    SELECT * FROM items WHERE
    (QueryTerm LIKE '%green%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting table name to be ItemCardModel."
            );
        }

        void subtest_IsTrueProxyWithExtendSchema()
        {
            // PREREQUISITE FOR ANY PROXY
            var mdc = new MarkdownContext<SelectableQFModel>();

            // Because ItemCardModel is a subclass of SelectableQFModel
            // it *is* a weak proxy by inheritance. But it's still a
            // proxy, so we need to use the ContractType for table.

            sql = mdc.ParseSqlMarkdown<TrueProxyWithExtendSchema>("green");

            actual = sql;
            actual.ToClipboardExpected();
            { }
            expected = @" 
    SELECT * FROM items WHERE
    (QueryTerm LIKE '%green%')";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting table name to be ItemCardModel."
            );
        }

        void subtest_NonIncoherentProxy()
        {
            #region L o c a l F x
            var builderThrow = new List<string>();
            void localOnBeginThrowOrAdvise(object? sender, Throw e)
            {
                builderThrow.Add(e.Message);
                e.Handled = true;
            }
            using var local = this.WithOnDispose(
                onInit: (sender, e) =>
                {
                    Throw.BeginThrowOrAdvise += localOnBeginThrowOrAdvise;
                },
                onDispose: (sender, e) =>
                {
                    Throw.BeginThrowOrAdvise -= localOnBeginThrowOrAdvise;
                });
            #endregion L o c a l F x

            // PREREQUISITE FOR ANY PROXY
            var mdc = new MarkdownContext<SelectableQFModel>();

            // Because ItemCardModel is a subclass of SelectableQFModel
            // it *is* a weak proxy by inheritance. But it's still a
            // proxy, so we need to use the ContractType for table.

            sql = mdc.ParseSqlMarkdown<NonCoherentProxy>("green");

            Assert.AreEqual(string.Empty, sql, "Expecting empty expression for failed parse.");

            actual = string.Join(Environment.NewLine, builderThrow);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Proxy type cannot resolve to the contract table.";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting throw event (handled locally)."
            );
        }
        #endregion S U B T E S T S
    }
}
