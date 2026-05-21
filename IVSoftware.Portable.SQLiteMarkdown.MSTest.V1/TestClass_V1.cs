using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Common;
using IVSoftware.Portable.Common.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Reflection;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.V1
{
    [TestClass]
    public sealed class TestClass_V1
    {
        [TestMethod]
        public void Test_V1Capabilities()
        {
            string actual, expected;

            var asmFullName = typeof(MarkdownContext<SelectableQFModel>).Assembly.FullName!;

            actual = asmFullName;
            actual.ToClipboardExpected();
            { }
            expected = @" 
IVSoftware.Portable.SQLiteMarkdown, Version=1.0.1.0, Culture=neutral, PublicKeyToken=becf53b24b0b41eb";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting Version=1.0.1.0."
            );

            MarkdownContext<SelectableQFModel> mdc = new();
            // In V1, this *should be*, but *is not* constrained: where T :class, new()
            // Notwithstanding, don't use something like int or object here!
            // WE'RE LOOKING FOR THE EXISTENCE OF THE CONTRACT ONLY.
            var oqfs = new ObservableQueryFilterSource<SelectableQFModel>();
            Assert.IsTrue(
                oqfs is IObservableQueryFilterSource<SelectableQFModel>,
                @"Asserting the claim: [Canonical(""Contract published in v1"")]");

            // published
            _ = mdc.ProxyType;
            _ = oqfs.ProxyType;
            
            var cnx = mdc.MemoryDatabase;

            var ct = mdc.ContractType;

#if false && AVAILABLE
            var tn = mdc.TableName;
#endif            
            var builder = new List<string>();
            Type[] types;
            PropertyInfo[] pis;

            types = [typeof(IObservableQueryFilterSource), typeof(IObservableQueryFilterSource<object>)];
            pis =
                 types
                .SelectMany(t => t.GetProperties())
                .DistinctBy(p => p.Name)
                .OrderBy(p => p.Name)
                .ToArray();

            foreach (var pi in pis)
            {
                builder.Add($"{pi.Name}: {pi.PropertyType.Name}");
            }

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Busy: Boolean
DHostBusy: DisposableHost
FilteringState: FilteringState
InputText: String
IsFiltering: Boolean
MemoryDatabase: SQLiteConnection
Placeholder: String
QueryFilterConfig: QueryFilterConfig
SearchEntryState: SearchEntryState
SQL: String
Title: String"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting Version=1.0.1.0 contract only."
            );

            pis = typeof(MarkdownContext<object>).GetProperties();
            builder.Clear();
            foreach (var pi in pis)
            {
                builder.Add($"{pi.Name}: {pi.PropertyType.Name}");
            }

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Raw: String
ContractType: Type
ProxyType: Type
Preamble: String
Transform: String
XAST: XElement
Atomics: Dictionary`2
Query: String
NamedQuery: String
NamedArgs: Dictionary`2
PositionalQuery: String
PositionalArgs: Object[]
ValidationPredicate: Predicate`1
QueryFilterConfig: QueryFilterConfig
DHostSelfIndexing: DisposableHost
MemoryDatabase: SQLiteConnection
RouteToFullRecordset: Boolean
FilteringState: FilteringState
FilteringStateForTest: FilteringState
IsFiltering: Boolean
InputText: String
SearchEntryState: SearchEntryState
DHostBusy: DisposableHost
Busy: Boolean
InputTextSettleInterval: TimeSpan
QueryTerm: String
FilterTerm: String
TagMatchTerm: String"
            ;

            var mdcAsm =
                AppDomain
                .CurrentDomain
                .GetAssemblies()
                 .Where(_ => _.GetName().Name == "IVSoftware.Portable.SQLiteMarkdown");
            builder.Clear();
            foreach (var asm in mdcAsm)
            {
                var name = asm.GetName();
                builder.Add(
                    $"{name.Name} | Version={name.Version}");
            }

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { } 
            expected = @" 
IVSoftware.Portable.SQLiteMarkdown | Version=1.0.1.0"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting Version=1.0.1.0"
            );

            actual = string.Join(
                Environment.NewLine,
                typeof(IObservableQueryFilterSource<object>).GetInterfaces().Select(_=>_.Name));
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

            mdc.Clear(); // Illegal in V2! See "no surprises" semantics.

            // NOPE
            // oqfs.SetObservableNetProjection();

            // NOPE
            //var requestEventContract =
            //        typeof(IVSoftware.Portable.SQLiteMarkdown.Events.RecordsetRequestEventArgs)
            //        .Assembly
            //        .ToPublicContract()
            //        .ToString();
        }

        [TestMethod]
        public void Test_ToPublicContract()
        {
            string actual, expected;
            Type target = typeof(MarkdownContext);

            string 
                contractOrig = 
                    typeof(MarkdownContext)
                    .Assembly
                    .ToPublicContract()
                    .ToString(),
                contractRedux =
                    typeof(MarkdownContext)
                    .Assembly
                    .ToPublicContract()
                    .ToString();

            // Compare raw XML string.
            Assert.AreEqual(
                contractOrig,
                contractRedux,
                "Expecting idempotent.");

            // Compare standard normalized result.
            Assert.AreEqual(
                contractOrig.NormalizeResult(),
                contractRedux.NormalizeResult(),
                "Expecting idempotent.");

#if false || SAVE
            // EmbeddedResource
            File.WriteAllText(@"Version=1.0.1.xml", contractOrig);
#else

            actual = contractOrig.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<assembly name=""IVSoftware.Portable.SQLiteMarkdown"" version=""1.0.1.0"">
  <type name=""IVSoftware.Portable.SQLiteMarkdown.ASTNode"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""type"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeType"" />
          <param name=""value"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""ASTType"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeType"" canRead=""true"" canWrite=""true"" />
      <property name=""Children"" type=""System.Collections.Generic.List&lt;IVSoftware.Portable.SQLiteMarkdown.ASTNode&gt;"" canRead=""true"" canWrite=""true"" />
      <property name=""Value"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyCollectionResetEventArgs"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""oldItems"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Action"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""NewItems"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""NewStartingIndex"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""OldItems"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""OldStartingIndex"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Add"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""ApplyFilter"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""Move"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""QueryResult"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""Remove"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""RemoveFilter"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""Replace"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""Reset"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedEventArgs"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""action"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" />
          <param name=""changedItems"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Action"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Action"" type=""IVSoftware.Portable.SQLiteMarkdown.Collections.NotifyQueryFilterCollectionChangedAction"" canRead=""true"" canWrite=""false"" />
      <property name=""NewItems"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""NewStartingIndex"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""OldItems"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""OldStartingIndex"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableHashSet"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""Count"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events>
      <event name=""CollectionChanged"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""Add"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Add"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Contains"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CopyTo"" returns=""[external]"">
        <parameters>
          <param name=""array"" type=""[external]"" />
          <param name=""index"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IndexOf"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
          <param name=""item"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
          <param name=""item"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Move"" returns=""[external]"">
        <parameters>
          <param name=""oldIndex"" type=""[external]"" />
          <param name=""newIndex"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Remove"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RemoveAt"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableHashSet&lt;T&gt;"">
    <interfaces>
      <interface name=""System.Collections.Generic.ICollection&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IList&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IReadOnlyCollection&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IReadOnlyList&lt;T&gt;"" />
    </interfaces>
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""Count"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""T"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events>
      <event name=""CollectionChanged"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""Add"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Add"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Contains"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""CopyTo"" returns=""[external]"">
        <parameters>
          <param name=""array"" type=""T[]"" />
          <param name=""index"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.Generic.IEnumerator&lt;T&gt;"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IndexOf"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Move"" returns=""[external]"">
        <parameters>
          <param name=""oldIndex"" type=""[external]"" />
          <param name=""newIndex"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Remove"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""RemoveAt"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableQueryFilterSource&lt;T&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.SQLiteMarkdown.IObservableQueryFilterSource"" />
      <interface name=""IVSoftware.Portable.SQLiteMarkdown.IObservableQueryFilterSource&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.ICollection&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IList&lt;T&gt;"" />
    </interfaces>
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""Atomics"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Busy"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ContractType"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Count"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""DHostBusy"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""DHostSelfIndexing"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""FilteringState"" type=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"" canRead=""true"" canWrite=""false"" />
      <property name=""FilteringStateForTest"" type=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"" canRead=""true"" canWrite=""true"" />
      <property name=""FilterTerm"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""InputText"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""InputTextSettleInterval"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""IsFiltering"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsFixedSize"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsReadOnly"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""T"" canRead=""true"" canWrite=""true"" />
      <property name=""MarkdownContextOR"" type=""IVSoftware.Portable.SQLiteMarkdown.MarkdownContextOR"" canRead=""true"" canWrite=""false"" />
      <property name=""MemoryDatabase"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""NamedArgs"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""NamedQuery"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Placeholder"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""PositionalArgs"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""PositionalQuery"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Preamble"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ProxyType"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Query"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""QueryFilterConfig"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterConfig"" canRead=""true"" canWrite=""true"" />
      <property name=""QueryTerm"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Raw"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""RouteToFullRecordset"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""SearchEntryState"" type=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState"" canRead=""true"" canWrite=""false"" />
      <property name=""SQL"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""TagMatchTerm"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Title"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Transform"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""UnfilteredItems"" type=""System.Collections.Generic.IReadOnlyList&lt;T&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""ValidationPredicate"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""XAST"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events>
      <event name=""CollectionChanged"" type=""[external]"" />
      <event name=""InputTextSettled"" type=""[external]"" />
      <event name=""ItemPropertyChanged"" type=""System.EventHandler&lt;IVSoftware.Portable.SQLiteMarkdown.Events.ItemPropertyChangedEventArgs&gt;"" />
      <event name=""PropertyChanged"" type=""[external]"" />
    </events>
    <fields>
      <field name=""_lock"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""Add"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"">
        <parameters>
          <param name=""all"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""[external]"">
        <parameters>
          <param name=""all"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Commit"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Contains"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""CopyTo"" returns=""[external]"">
        <parameters>
          <param name=""array"" type=""T[]"" />
          <param name=""arrayIndex"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetAwaiter"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.Generic.IEnumerator&lt;T&gt;"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IndexOf"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""InitializeFilterOnlyMode"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
          <param name=""qfMode"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
        </parameters>
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
          <param name=""proxyType"" type=""[external]"" />
          <param name=""qfMode"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
          <param name=""xast"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Rehydrate"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Remove"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""RemoveAt"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ReplaceItems"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
        </parameters>
      </method>
      <method name=""ReplaceItemsAsync"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Collections.ObservableSelectionHashSet&lt;T&gt;"">
    <interfaces>
      <interface name=""System.Collections.Generic.ICollection&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IList&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IReadOnlyCollection&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IReadOnlyList&lt;T&gt;"" />
    </interfaces>
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""CanMultiselect"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Count"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""T"" canRead=""true"" canWrite=""true"" />
      <property name=""SelectionMode"" type=""IVSoftware.Portable.SQLiteMarkdown.SelectionMode"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events>
      <event name=""CollectionChanged"" type=""[external]"" />
      <event name=""PropertyChanged"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""Add"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Add"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Contains"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""CopyTo"" returns=""[external]"">
        <parameters>
          <param name=""array"" type=""T[]"" />
          <param name=""index"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.Generic.IEnumerator&lt;T&gt;"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IndexOf"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Move"" returns=""[external]"">
        <parameters>
          <param name=""oldIndex"" type=""[external]"" />
          <param name=""newIndex"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Remove"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""RemoveAt"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Common.SelectableQFModel"">
    <interfaces>
      <interface name=""IVSoftware.Portable.SQLiteMarkdown.ISelectable"" />
      <interface name=""IVSoftware.Portable.SQLiteMarkdown.ISelfIndexedMarkdown"" />
    </interfaces>
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""Description"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""FilterTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Id"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""IsChecked"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""IsEditing"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Keywords"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""KeywordsDisplay"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""PrimaryKey"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Properties"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""QueryTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Selection"" type=""IVSoftware.Portable.SQLiteMarkdown.ItemSelection"" canRead=""true"" canWrite=""true"" />
      <property name=""TagMatchTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Tags"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""TagsDisplay"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events>
      <event name=""PropertyChanged"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Report"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Common.StringWrapper"">
    <interfaces>
      <interface name=""IVSoftware.Portable.SQLiteMarkdown.ISelectable"" />
    </interfaces>
    <constructors>
      <ctor>
        <parameters />
      </ctor>
      <ctor>
        <parameters>
          <param name=""value"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Id"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""IsEditing"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Selection"" type=""IVSoftware.Portable.SQLiteMarkdown.ItemSelection"" canRead=""true"" canWrite=""true"" />
      <property name=""Value"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events>
      <event name=""PropertyChanged"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Events.ItemPropertyChangedEventArgs"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""propertyName"" type=""[external]"" />
          <param name=""item"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Item"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""PropertyName"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Extensions"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields />
    <methods>
      <method name=""CanParseAsJson"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetFilterTermAttribute"" returns=""IVSoftware.Portable.SQLiteMarkdown.MarkdownTermAttribute"">
        <parameters>
          <param name=""pi"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetQueryTermAttribute"" returns=""IVSoftware.Portable.SQLiteMarkdown.MarkdownTermAttribute"">
        <parameters>
          <param name=""pi"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IsFilterTermAttribute"" returns=""[external]"">
        <parameters>
          <param name=""mta"" type=""IVSoftware.Portable.SQLiteMarkdown.MarkdownTermAttribute"" />
        </parameters>
      </method>
      <method name=""IsQueryTermAttribute"" returns=""[external]"">
        <parameters>
          <param name=""mta"" type=""IVSoftware.Portable.SQLiteMarkdown.MarkdownTermAttribute"" />
        </parameters>
      </method>
      <method name=""MakeTags"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""termDelimiter"" type=""IVSoftware.Portable.SQLiteMarkdown.TermDelimiter"" />
          <param name=""stringCasing"" type=""IVSoftware.Portable.SQLiteMarkdown.StringCasing"" />
        </parameters>
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""qfMode"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
        </parameters>
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""qfMode"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
          <param name=""xast"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""minInputLength"" type=""[external]"" />
          <param name=""qfMode"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
        </parameters>
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""type"" type=""[external]"" />
          <param name=""qfMode"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
          <param name=""xast"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""IVSoftware.Portable.SQLiteMarkdown.MarkdownContext"" />
          <param name=""expr"" type=""[external]"" />
          <param name=""type"" type=""[external]"" />
          <param name=""qfMode"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
          <param name=""xast"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Singularize"" returns=""[external]"">
        <parameters>
          <param name=""word"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToFuzzyQuery"" returns=""[external]"">
        <parameters>
          <param name=""sql"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToStringFromEventType"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.ExtensionsOR"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""PromptEachStep"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events>
      <event name=""StackPrompt"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GenerateTerm"" returns=""[external]"">
        <parameters>
          <param name=""astNodes"" type=""IVSoftware.Portable.SQLiteMarkdown.ASTNode[]"" />
          <param name=""nodeTypes"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeTypeFlags"" />
          <param name=""casing"" type=""IVSoftware.Portable.SQLiteMarkdown.StringCasing"" />
          <param name=""termStyle"" type=""IVSoftware.Portable.SQLiteMarkdown.TermDelimiter"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Lint"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
          <param name=""trim"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""LintProbationary"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
          <param name=""trim"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ParseSqlMarkdown"" returns=""IVSoftware.Portable.SQLiteMarkdown.MarkdownContextOR"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
          <param name=""searchEntryState"" type=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState&amp;"" />
        </parameters>
      </method>
      <method name=""ParseSqlMarkdown"" returns=""IVSoftware.Portable.SQLiteMarkdown.MarkdownContextOR"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
          <param name=""validationState"" type=""IVSoftware.Portable.SQLiteMarkdown.ValidationState&amp;"" />
          <param name=""validationPredicate"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""TryTokenizeOR"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
          <param name=""result"" type=""IVSoftware.Portable.SQLiteMarkdown.ASTNode[]&amp;"" />
          <param name=""minInputLength"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.FilterContainsTermAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""TypeId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Match"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Active"" type=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"" />
      <field name=""Armed"" type=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"" />
      <field name=""Ineligible"" type=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.FilterLikeTermAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""TypeId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Match"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.IEditableQueryFilterItem"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.IndexingMode"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""All"" type=""IVSoftware.Portable.SQLiteMarkdown.IndexingMode"" />
      <field name=""FilterLikeTerm"" type=""IVSoftware.Portable.SQLiteMarkdown.IndexingMode"" />
      <field name=""QueryLikeTerm"" type=""IVSoftware.Portable.SQLiteMarkdown.IndexingMode"" />
      <field name=""QueryOrFilter"" type=""IVSoftware.Portable.SQLiteMarkdown.IndexingMode"" />
      <field name=""TagMatchTerm"" type=""IVSoftware.Portable.SQLiteMarkdown.IndexingMode"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.IObservableQueryFilterSource"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""Busy"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""FilteringState"" type=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"" canRead=""true"" canWrite=""false"" />
      <property name=""InputText"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""IsFiltering"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""MemoryDatabase"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Placeholder"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""QueryFilterConfig"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterConfig"" canRead=""true"" canWrite=""true"" />
      <property name=""SearchEntryState"" type=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState"" canRead=""true"" canWrite=""false"" />
      <property name=""SQL"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Title"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events>
      <event name=""InputTextSettled"" type=""[external]"" />
      <event name=""ItemPropertyChanged"" type=""System.EventHandler&lt;IVSoftware.Portable.SQLiteMarkdown.Events.ItemPropertyChangedEventArgs&gt;"" />
    </events>
    <fields />
    <methods>
      <method name=""Clear"" returns=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"">
        <parameters>
          <param name=""all"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Commit"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.IObservableQueryFilterSource&lt;T&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.SQLiteMarkdown.IObservableQueryFilterSource"" />
    </interfaces>
    <constructors />
    <properties>
      <property name=""DHostBusy"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""InitializeFilterOnlyMode"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
        </parameters>
      </method>
      <method name=""ReplaceItems"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
        </parameters>
      </method>
      <method name=""ReplaceItemsAsync"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.ISelectable"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""IsEditing"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Selection"" type=""IVSoftware.Portable.SQLiteMarkdown.ItemSelection"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.ISelfIndexedMarkdown"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""FilterTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""PrimaryKey"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Properties"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""QueryTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""TagMatchTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.ItemSelection"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Exclusive"" type=""IVSoftware.Portable.SQLiteMarkdown.ItemSelection"" />
      <field name=""Multi"" type=""IVSoftware.Portable.SQLiteMarkdown.ItemSelection"" />
      <field name=""None"" type=""IVSoftware.Portable.SQLiteMarkdown.ItemSelection"" />
      <field name=""Primary"" type=""IVSoftware.Portable.SQLiteMarkdown.ItemSelection"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.MarkdownContext"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""type"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Atomics"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Busy"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ContractType"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""DHostBusy"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""DHostSelfIndexing"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""FilteringState"" type=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"" canRead=""true"" canWrite=""false"" />
      <property name=""FilteringStateForTest"" type=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"" canRead=""true"" canWrite=""true"" />
      <property name=""FilterTerm"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""InputText"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""InputTextSettleInterval"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""IsFiltering"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""MemoryDatabase"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""NamedArgs"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""NamedQuery"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""PositionalArgs"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""PositionalQuery"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Preamble"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ProxyType"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Query"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""QueryFilterConfig"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterConfig"" canRead=""true"" canWrite=""true"" />
      <property name=""QueryTerm"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Raw"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""RouteToFullRecordset"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""SearchEntryState"" type=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState"" canRead=""true"" canWrite=""false"" />
      <property name=""TagMatchTerm"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Transform"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ValidationPredicate"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""XAST"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events>
      <event name=""InputTextSettled"" type=""[external]"" />
      <event name=""PropertyChanged"" type=""[external]"" />
    </events>
    <fields>
      <field name=""_lock"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""Clear"" returns=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"">
        <parameters>
          <param name=""all"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetAwaiter"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
          <param name=""qfMode"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
        </parameters>
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
          <param name=""proxyType"" type=""[external]"" />
          <param name=""qfMode"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
          <param name=""xast"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Rehydrate"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.MarkdownContext&lt;T&gt;"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""Atomics"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Busy"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ContractType"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""DHostBusy"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""DHostSelfIndexing"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""FilteringState"" type=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"" canRead=""true"" canWrite=""false"" />
      <property name=""FilteringStateForTest"" type=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"" canRead=""true"" canWrite=""true"" />
      <property name=""FilterTerm"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""InputText"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""InputTextSettleInterval"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""IsFiltering"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""MemoryDatabase"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""NamedArgs"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""NamedQuery"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""PositionalArgs"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""PositionalQuery"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Preamble"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ProxyType"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Query"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""QueryFilterConfig"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterConfig"" canRead=""true"" canWrite=""true"" />
      <property name=""QueryTerm"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Raw"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""RouteToFullRecordset"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""SearchEntryState"" type=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState"" canRead=""true"" canWrite=""false"" />
      <property name=""TagMatchTerm"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Transform"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ValidationPredicate"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""XAST"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events>
      <event name=""InputTextSettled"" type=""[external]"" />
      <event name=""PropertyChanged"" type=""[external]"" />
    </events>
    <fields>
      <field name=""_lock"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""Clear"" returns=""IVSoftware.Portable.SQLiteMarkdown.FilteringState"">
        <parameters>
          <param name=""all"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetAwaiter"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
          <param name=""qfMode"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
        </parameters>
      </method>
      <method name=""ParseSqlMarkdown"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
          <param name=""proxyType"" type=""[external]"" />
          <param name=""qfMode"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
          <param name=""xast"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Rehydrate"" returns=""[external]"">
        <parameters>
          <param name=""expr"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.MarkdownContextOR"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""query"" type=""[external]"" />
          <param name=""args"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Args"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""PositionalArgs"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""PositionalQuery"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Query"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields>
      <field name=""Atomics"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetToken"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.MarkdownTermAttribute"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""TypeId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Match"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.NodeType"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""And"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeType"" />
      <field name=""Not"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeType"" />
      <field name=""Or"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeType"" />
      <field name=""Parenthesis"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeType"" />
      <field name=""Tag"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeType"" />
      <field name=""Term"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeType"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.NodeTypeFlags"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""All"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeTypeFlags"" />
      <field name=""Tag"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeTypeFlags"" />
      <field name=""Term"" type=""IVSoftware.Portable.SQLiteMarkdown.NodeTypeFlags"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.PersistenceMode"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Json"" type=""IVSoftware.Portable.SQLiteMarkdown.PersistenceMode"" />
      <field name=""None"" type=""IVSoftware.Portable.SQLiteMarkdown.PersistenceMode"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterConfig"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Filter"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterConfig"" />
      <field name=""Query"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterConfig"" />
      <field name=""QueryAndFilter"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterConfig"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Filter"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
      <field name=""Query"" type=""IVSoftware.Portable.SQLiteMarkdown.QueryFilterMode"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.QueryLikeTermAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""TypeId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Match"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Cleared"" type=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState"" />
      <field name=""QueryCompleteNoResults"" type=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState"" />
      <field name=""QueryCompleteWithResults"" type=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState"" />
      <field name=""QueryEmpty"" type=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState"" />
      <field name=""QueryEN"" type=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState"" />
      <field name=""QueryENB"" type=""IVSoftware.Portable.SQLiteMarkdown.SearchEntryState"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.SelectionMode"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Multiple"" type=""IVSoftware.Portable.SQLiteMarkdown.SelectionMode"" />
      <field name=""None"" type=""IVSoftware.Portable.SQLiteMarkdown.SelectionMode"" />
      <field name=""Single"" type=""IVSoftware.Portable.SQLiteMarkdown.SelectionMode"" />
      <field name=""SingleWithModifiers"" type=""IVSoftware.Portable.SQLiteMarkdown.SelectionMode"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.SelfIndexed"">
    <interfaces>
      <interface name=""IVSoftware.Portable.SQLiteMarkdown.ISelfIndexedMarkdown"" />
    </interfaces>
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""FilterTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Id"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""PrimaryKey"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Properties"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""QueryTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""TagMatchTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events>
      <event name=""PropertyChanged"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.SelfIndexedAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""indexingMode"" type=""IVSoftware.Portable.SQLiteMarkdown.IndexingMode"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""IndexingMode"" type=""IVSoftware.Portable.SQLiteMarkdown.IndexingMode"" canRead=""true"" canWrite=""false"" />
      <property name=""PersistenceMode"" type=""IVSoftware.Portable.SQLiteMarkdown.PersistenceMode"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Match"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.SelfIndexedOR"">
    <interfaces>
      <interface name=""IVSoftware.Portable.SQLiteMarkdown.ISelfIndexedMarkdown"" />
    </interfaces>
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""FilterTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Id"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""PrimaryKey"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Properties"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""QueryTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""TagMatchTerm"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events>
      <event name=""PropertyChanged"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.SqlLikeTermAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""TypeId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Match"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Static"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""DefaultValidationPredicate"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.StdAstAttr"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""args"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstAttr"" />
      <field name=""clauseE"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstAttr"" />
      <field name=""clauseN"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstAttr"" />
      <field name=""ismatched"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstAttr"" />
      <field name=""key"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstAttr"" />
      <field name=""run"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstAttr"" />
      <field name=""value"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstAttr"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.StdAstNode"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""and"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstNode"" />
      <field name=""ast"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstNode"" />
      <field name=""expr"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstNode"" />
      <field name=""memo"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstNode"" />
      <field name=""not"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstNode"" />
      <field name=""or"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstNode"" />
      <field name=""sub"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstNode"" />
      <field name=""term"" type=""IVSoftware.Portable.SQLiteMarkdown.StdAstNode"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.StringCasing"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Lower"" type=""IVSoftware.Portable.SQLiteMarkdown.StringCasing"" />
      <field name=""Original"" type=""IVSoftware.Portable.SQLiteMarkdown.StringCasing"" />
      <field name=""Upper"" type=""IVSoftware.Portable.SQLiteMarkdown.StringCasing"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.TagMatchTermAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""TypeId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Match"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.TermDelimiter"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Comma"" type=""IVSoftware.Portable.SQLiteMarkdown.TermDelimiter"" />
      <field name=""Semicolon"" type=""IVSoftware.Portable.SQLiteMarkdown.TermDelimiter"" />
      <field name=""Tilde"" type=""IVSoftware.Portable.SQLiteMarkdown.TermDelimiter"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.ValidationState"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""DisableMinLength"" type=""IVSoftware.Portable.SQLiteMarkdown.ValidationState"" />
      <field name=""Empty"" type=""IVSoftware.Portable.SQLiteMarkdown.ValidationState"" />
      <field name=""Invalid"" type=""IVSoftware.Portable.SQLiteMarkdown.ValidationState"" />
      <field name=""Valid"" type=""IVSoftware.Portable.SQLiteMarkdown.ValidationState"" />
      <field name=""value__"" type=""[external]"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.SQLiteMarkdown.Win32Message"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""value__"" type=""[external]"" />
      <field name=""WM_CONTEXTMENU"" type=""IVSoftware.Portable.SQLiteMarkdown.Win32Message"" />
      <field name=""WM_LBUTTONDOWN"" type=""IVSoftware.Portable.SQLiteMarkdown.Win32Message"" />
      <field name=""WM_LBUTTONUP"" type=""IVSoftware.Portable.SQLiteMarkdown.Win32Message"" />
      <field name=""WM_MOUSEHOVER"" type=""IVSoftware.Portable.SQLiteMarkdown.Win32Message"" />
      <field name=""WM_MOUSELEAVE"" type=""IVSoftware.Portable.SQLiteMarkdown.Win32Message"" />
      <field name=""WM_MOUSEMOVE"" type=""IVSoftware.Portable.SQLiteMarkdown.Win32Message"" />
      <field name=""WM_NCMOUSEMOVE"" type=""IVSoftware.Portable.SQLiteMarkdown.Win32Message"" />
      <field name=""WM_RBUTTONDOWN"" type=""IVSoftware.Portable.SQLiteMarkdown.Win32Message"" />
      <field name=""WM_RBUTTONUP"" type=""IVSoftware.Portable.SQLiteMarkdown.Win32Message"" />
    </fields>
    <methods>
      <method name=""CompareTo"" returns=""[external]"">
        <parameters>
          <param name=""target"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""[external]"">
        <parameters>
          <param name=""flag"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""format"" type=""[external]"" />
          <param name=""provider"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
</assembly>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting canonical V1 contract."
            );
#endif
        }
    }
}
