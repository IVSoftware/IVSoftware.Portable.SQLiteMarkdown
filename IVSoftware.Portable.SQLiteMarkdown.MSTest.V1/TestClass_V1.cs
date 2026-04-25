using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Collections.Exceptions;
using IVSoftware.Portable.Common.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Reflection;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.V1
{
    [TestClass]
    public sealed class TestClass_V1
    {
        /// <summary>
        /// This is more of a reference than a test. We're looking for 
        /// confirmation of what was and wasn't visible in v1.
        /// </summary>
        [Legacy(
            $"This preliminary assessment is not fully formed as" +
            $" {nameof(MSTestExtensions.ToPublicManifest)}."), TestMethod]
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
            { } // <- FIRST TIME ONLY: Adjust the message.
            actual.ToClipboardAssert("Expecting builder content to match.");
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

            // In V1, this *should be*, but *is not* constrained: where T :class, new()
            // Notwithstanding, don't use something like int or object here!
            // WE'RE LOOKING FOR THE EXISTENCE OF THE CONTRACT ONLY.
            var opc = new ObservableQueryFilterSource<SelectableQFModel>();
            Assert.IsTrue(
                opc is IObservableQueryFilterSource<SelectableQFModel>,
                @"Asserting the claim: [Canonical(""Contract published in v1"")]");
        }

        [TestMethod]
        public void Test_ToPublicContract()
        {
            string actual, expected;

            XElement xcontract = 
                typeof(IModeledCollection)
                .Assembly
                .ToPublicContract();

            actual = xcontract.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<assembly name=""IVSoftware.Portable.Xml.Linq.Collections"" version=""1.0.0.0"">
  <type name=""IVSoftware.Portable.Collections.AffinityIncrMode"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Current"" type=""IVSoftware.Portable.Collections.AffinityIncrMode"" />
      <field name=""Postfix"" type=""IVSoftware.Portable.Collections.AffinityIncrMode"" />
      <field name=""Prefix"" type=""IVSoftware.Portable.Collections.AffinityIncrMode"" />
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
  <type name=""IVSoftware.Portable.Collections.AffinityTestableEpoch"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""DefaultIncr"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""GuidReset"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""UtcReset"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
      <method name=""ResetEpoch"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""TestableEpoch"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""WithTestability"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""mode"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""WithTestability"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""incr"" type=""[external]"" />
          <param name=""mode"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.BehaviorMode"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""InsistentNotNull"" type=""IVSoftware.Portable.Collections.BehaviorMode"" />
      <field name=""Normal"" type=""IVSoftware.Portable.Collections.BehaviorMode"" />
      <field name=""TolerantCreateDefaultEntry"" type=""IVSoftware.Portable.Collections.BehaviorMode"" />
      <field name=""TolerantReturnDefault"" type=""IVSoftware.Portable.Collections.BehaviorMode"" />
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
  <type name=""IVSoftware.Portable.Collections.CollectionChangingEventingPolicy"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Coalesce"" type=""IVSoftware.Portable.Collections.CollectionChangingEventingPolicy"" />
      <field name=""Discrete"" type=""IVSoftware.Portable.Collections.CollectionChangingEventingPolicy"" />
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
  <type name=""IVSoftware.Portable.Collections.Dictionaries.DictionaryEntryPreview"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""key"" type=""[external]"" />
          <param name=""value"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Key"" type=""[external]"" canRead=""true"" canWrite=""true"" />
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
  <type name=""IVSoftware.Portable.Collections.DictionaryMode"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Brisk"" type=""IVSoftware.Portable.Collections.DictionaryMode"" />
      <field name=""InsistentNotNull"" type=""IVSoftware.Portable.Collections.DictionaryMode"" />
      <field name=""Normal"" type=""IVSoftware.Portable.Collections.DictionaryMode"" />
      <field name=""TolerantCreateDefaultEntry"" type=""IVSoftware.Portable.Collections.DictionaryMode"" />
      <field name=""TolerantReturnDefault"" type=""IVSoftware.Portable.Collections.DictionaryMode"" />
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
  <type name=""IVSoftware.Portable.Collections.EnumHistogrammer"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""AllowRootChanges"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Authorities"" type=""IVSoftware.Portable.Collections.StdModelAuthority[]"" canRead=""true"" canWrite=""false"" />
      <property name=""Authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" canRead=""true"" canWrite=""false"" />
      <property name=""FormattingDefault"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""IsCancelled"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsDisposing"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Model"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events>
      <event name=""BeginUsing"" type=""[external]"" />
      <event name=""FinalDispose"" type=""[external]"" />
      <event name=""PropertyChanged"" type=""[external]"" />
      <event name=""XModelChanged"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""CancelAuthorityEpoch"" returns=""[external]"">
        <parameters>
          <param name=""throw"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""[external]"">
        <parameters />
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
      <method name=""HasAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""HasRequestedAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""IsZero"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""RequestAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
          <param name=""properties"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""[external]"" />
          <param name=""properties"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""TallyModelToReconcile"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""formatting"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""formatting"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.EnumHistogrammer&lt;T&gt;"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters />
      </ctor>
      <ctor>
        <parameters>
          <param name=""model"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""AllowRootChanges"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Authorities"" type=""IVSoftware.Portable.Collections.StdModelAuthority[]"" canRead=""true"" canWrite=""false"" />
      <property name=""Authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" canRead=""true"" canWrite=""false"" />
      <property name=""Count"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ExplicitFalsePolicy"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" canRead=""true"" canWrite=""true"" />
      <property name=""FormattingDefault"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""HistogramParticipationOption"" type=""IVSoftware.Portable.Collections.HistogramParticipationPolicy"" canRead=""true"" canWrite=""true"" />
      <property name=""IsCancelled"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsDisposing"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Keys"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Model"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Values"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ZeroCountPolicy"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events>
      <event name=""BeginUsing"" type=""[external]"" />
      <event name=""FinalDispose"" type=""[external]"" />
      <event name=""PropertyChanged"" type=""[external]"" />
      <event name=""XModelChanged"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""CancelAuthorityEpoch"" returns=""[external]"">
        <parameters>
          <param name=""throw"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ContainsKey"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""T"" />
        </parameters>
      </method>
      <method name=""Decrement"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""T"" />
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
      <method name=""HasAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""HasRequestedAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""HasRequestedAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Increment"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""T"" />
        </parameters>
      </method>
      <method name=""IsZero"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""RequestAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
          <param name=""properties"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""[external]"" />
          <param name=""properties"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""TallyModelToReconcile"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""formatting"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""formatting"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""TryGetValue"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""T"" />
          <param name=""value"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.Events.EHPropertyChangedEventArgs"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""e"" type=""IVSoftware.Portable.Collections.Events.XModelChangeEventArgs"" />
        </parameters>
      </ctor>
      <ctor>
        <parameters>
          <param name=""key"" type=""[external]"" />
          <param name=""edge"" type=""IVSoftware.Portable.Collections.HistogramEdge"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Changing"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Edge"" type=""IVSoftware.Portable.Collections.HistogramEdge"" canRead=""true"" canWrite=""false"" />
      <property name=""Key"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""KeyPrev"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ObjectChange"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Parent"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""PropertyName"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Value"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ValuePrev"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""XOB"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Collections.Events.ItemPropertyChangedEventArgs"">
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
  <type name=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""eBCL"" type=""[external]"" />
          <param name=""reason"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"" />
          <param name=""scope"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" />
        </parameters>
      </ctor>
      <ctor>
        <parameters>
          <param name=""ePre"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" />
          <param name=""reason"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"" />
          <param name=""scope"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" />
        </parameters>
      </ctor>
      <ctor>
        <parameters>
          <param name=""action"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeAction"" />
          <param name=""reason"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"" />
          <param name=""scope"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" />
          <param name=""newItems"" type=""[external]"" />
          <param name=""oldItems"" type=""[external]"" />
          <param name=""newStartingIndex"" type=""[external]"" />
          <param name=""oldStartingIndex"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Action"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeAction"" canRead=""true"" canWrite=""true"" />
      <property name=""Cancel"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""IsBclCompatible"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsModified"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""NewItems"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""NewStartingIndex"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""OldItems"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""OldStartingIndex"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Reason"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"" canRead=""true"" canWrite=""true"" />
      <property name=""Scope"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""other"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" />
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
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.Events.XModelChangeEventArgs"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""sender"" type=""[external]"" />
          <param name=""objectChange"" type=""[external]"" />
          <param name=""changing"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Changing"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Edge"" type=""IVSoftware.Portable.Collections.HistogramEdge"" canRead=""true"" canWrite=""false"" />
      <property name=""Key"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""KeyPrev"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""ObjectChange"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Parent"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Value"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ValuePrev"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""XOB"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Collections.Exceptions.ModelAccessViolation"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""ModelClearViolation"" type=""IVSoftware.Portable.Collections.Exceptions.ModelAccessViolation"" />
      <field name=""ModelCountViolation"" type=""IVSoftware.Portable.Collections.Exceptions.ModelAccessViolation"" />
      <field name=""ModelDatabaseAccessViolation"" type=""IVSoftware.Portable.Collections.Exceptions.ModelAccessViolation"" />
      <field name=""ModelDecrementViolation"" type=""IVSoftware.Portable.Collections.Exceptions.ModelAccessViolation"" />
      <field name=""ModelIncrementViolation"" type=""IVSoftware.Portable.Collections.Exceptions.ModelAccessViolation"" />
      <field name=""ModelReadOnlyAttributeViolation"" type=""IVSoftware.Portable.Collections.Exceptions.ModelAccessViolation"" />
      <field name=""OperationNoopWarning"" type=""IVSoftware.Portable.Collections.Exceptions.ModelAccessViolation"" />
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
  <type name=""IVSoftware.Portable.Collections.Exceptions.ModelAccessViolationException"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""message"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Data"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""HelpLink"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""HResult"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""InnerException"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Message"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Source"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""StackTrace"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""TargetSite"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetBaseException"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""[external]"">
        <parameters>
          <param name=""info"" type=""[external]"" />
          <param name=""context"" type=""[external]"" />
        </parameters>
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
  <type name=""IVSoftware.Portable.Collections.Exceptions.ModelException"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""message"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Data"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""HelpLink"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""HResult"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""InnerException"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Message"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Source"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""StackTrace"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""TargetSite"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetBaseException"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""[external]"">
        <parameters>
          <param name=""info"" type=""[external]"" />
          <param name=""context"" type=""[external]"" />
        </parameters>
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
  <type name=""IVSoftware.Portable.Collections.Exceptions.ModelPolicyViolation"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""value__"" type=""[external]"" />
      <field name=""XAttributeBooleanToggle"" type=""IVSoftware.Portable.Collections.Exceptions.ModelPolicyViolation"" />
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
  <type name=""IVSoftware.Portable.Collections.ExpandXKeyFormatRequestedEventArgs"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""xkey"" type=""[external]"" />
          <param name=""valueToFormat"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""ValueToFormat"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""XKey"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""All"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" />
      <field name=""None"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" />
      <field name=""Remove"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" />
      <field name=""ThrowHard"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" />
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
  <type name=""IVSoftware.Portable.Collections.Extensions"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields />
    <methods>
      <method name=""AddAttributeFirst"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""attr"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Attribute"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""stdEnum"" type=""[external]"" />
          <param name=""throw"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetCustomAttribute"" returns=""TAttribute"">
        <parameters>
          <param name=""value"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetCustomAttribute"" returns=""[external]"">
        <parameters>
          <param name=""value"" type=""[external]"" />
          <param name=""openGenericType"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""InsertAttributeAfter"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""after"" type=""[external]"" />
          <param name=""attr"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""InsertAttributeAfter"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""after"" type=""[external]"" />
          <param name=""attr"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""InsertPreviewAttributeAfter"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""after"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""InsertPreviewAttributeAfter"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""after"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""MakeXElement"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Move"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""attr"" type=""[external]"" />
          <param name=""index"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""SetStdAttributeValue"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""stdEnum"" type=""[external]"" />
          <param name=""value"" type=""[external]"" />
          <param name=""maxLength"" type=""[external]"" />
          <param name=""padToMaxLength"" type=""[external]"" />
          <param name=""throw"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""XBoundAttribute"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
          <param name=""stdEnum"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
          <param name=""throw"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.FormattingEH"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Current"" type=""IVSoftware.Portable.Collections.FormattingEH"" />
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
  <type name=""IVSoftware.Portable.Collections.FormattingEHM"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Matches"" type=""IVSoftware.Portable.Collections.FormattingEHM"" />
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
  <type name=""IVSoftware.Portable.Collections.FormattingOMC"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Json"" type=""IVSoftware.Portable.Collections.FormattingOMC"" />
      <field name=""Model"" type=""IVSoftware.Portable.Collections.FormattingOMC"" />
      <field name=""ModelWithPreview"" type=""IVSoftware.Portable.Collections.FormattingOMC"" />
      <field name=""StateReport"" type=""IVSoftware.Portable.Collections.FormattingOMC"" />
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
  <type name=""IVSoftware.Portable.Collections.HistogramEdge"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""AffectsSinkCount"" type=""IVSoftware.Portable.Collections.HistogramEdge"" />
      <field name=""Decrement"" type=""IVSoftware.Portable.Collections.HistogramEdge"" />
      <field name=""Hold"" type=""IVSoftware.Portable.Collections.HistogramEdge"" />
      <field name=""Increment"" type=""IVSoftware.Portable.Collections.HistogramEdge"" />
      <field name=""Rebucket"" type=""IVSoftware.Portable.Collections.HistogramEdge"" />
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
  <type name=""IVSoftware.Portable.Collections.HistogramParticipationPolicy"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""OptIn"" type=""IVSoftware.Portable.Collections.HistogramParticipationPolicy"" />
      <field name=""OptOut"" type=""IVSoftware.Portable.Collections.HistogramParticipationPolicy"" />
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
  <type name=""IVSoftware.Portable.Collections.IBriskDictionary"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IInsistent"" />
    </interfaces>
    <constructors />
    <properties>
      <property name=""Item"" type=""IVSoftware.Portable.Collections.IObservableDictionary"" canRead=""true"" canWrite=""false"" />
      <property name=""Model"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events>
      <event name=""ExpandXKeyFormatRequested"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""ContainsKey"" returns=""[external]"">
        <parameters>
          <param name=""key1"" type=""[external]"" />
          <param name=""keysN"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ViewExpandedModel"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IInsistent"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IInsistentDictionary"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IInsistent"" />
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary"" />
    </interfaces>
    <constructors />
    <properties>
      <property name=""ActivationDlgt"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IInsistentDictionary&lt;TKey,TValue&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IInsistent"" />
      <interface name=""IVSoftware.Portable.Collections.IInsistentDictionary"" />
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary&lt;TKey,TValue&gt;"" />
    </interfaces>
    <constructors />
    <properties>
      <property name=""ActivationDlgt"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IModeledCollection"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""AuthorityProviders"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""FilterQueryDatabase"" type=""IVSoftware.Portable.Collections.SQLiteQueryOnlyConnection"" canRead=""true"" canWrite=""false"" />
      <property name=""Histo"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Model"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ModelDataExchangeAuthority"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" canRead=""true"" canWrite=""false"" />
      <property name=""ModelTracking"" type=""IVSoftware.Portable.Collections.ModelTrackingFlag"" canRead=""true"" canWrite=""true"" />
      <property name=""ObservableNetProjection"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""HasAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""SetObservableNetProjection"" returns=""[external]"">
        <parameters>
          <param name=""onp"" type=""IVSoftware.Portable.Collections.INotifyPreviewCollection"" />
          <param name=""topology"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""CollectionChangingEventingPolicy"" type=""IVSoftware.Portable.Collections.CollectionChangingEventingPolicy"" canRead=""true"" canWrite=""false"" />
      <property name=""EventScope"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events>
      <event name=""CollectionChanging"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangingEventHandler"" />
    </events>
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.INotifyPreviewCollection"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
    </interfaces>
    <constructors />
    <properties />
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.Internal.GetFullPathDelegate&lt;T&gt;"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""object"" type=""[external]"" />
          <param name=""method"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Method"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Target"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""BeginInvoke"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
          <param name=""callback"" type=""[external]"" />
          <param name=""object"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Clone"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""DynamicInvoke"" returns=""[external]"">
        <parameters>
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""EndInvoke"" returns=""[external]"">
        <parameters>
          <param name=""result"" type=""[external]"" />
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
      <method name=""GetInvocationList"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""[external]"">
        <parameters>
          <param name=""info"" type=""[external]"" />
          <param name=""context"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Invoke"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.Internal.ModelPreviewDelegate"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""object"" type=""[external]"" />
          <param name=""method"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Method"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Target"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""BeginInvoke"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""[external]"" />
          <param name=""callback"" type=""[external]"" />
          <param name=""object"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Clone"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""DynamicInvoke"" returns=""[external]"">
        <parameters>
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""EndInvoke"" returns=""[external]"">
        <parameters>
          <param name=""result"" type=""[external]"" />
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
      <method name=""GetInvocationList"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""[external]"">
        <parameters>
          <param name=""info"" type=""[external]"" />
          <param name=""context"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Invoke"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IObservableDictionary"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
    </interfaces>
    <constructors />
    <properties>
      <property name=""DHostEphemeralMode"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Mode"" type=""IVSoftware.Portable.Collections.DictionaryMode"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""AddRange"" returns=""[external]"">
        <parameters>
          <param name=""entries"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IObservableDictionary&lt;TKey,TValue&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary"" />
    </interfaces>
    <constructors />
    <properties />
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IPathPlaceable"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""FullPath"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Id"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ParentId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ParentPath"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IPredicated"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IRoutedEnumerable"" />
    </interfaces>
    <constructors />
    <properties>
      <property name=""ActiveFilters"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""ActivatePredicates"" returns=""[external]"">
        <parameters>
          <param name=""stdPredicate"" type=""[external]"" />
          <param name=""more"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""BeginPredicateAtom"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ClearPredicates"" returns=""[external]"">
        <parameters>
          <param name=""clearInputText"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""DeactivatePredicates"" returns=""[external]"">
        <parameters>
          <param name=""stdPredicate"" type=""[external]"" />
          <param name=""more"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IRangeable"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields />
    <methods>
      <method name=""AddRange"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""AddRangeDistinct"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""InsertRange"" returns=""[external]"">
        <parameters>
          <param name=""startingIndex"" type=""[external]"" />
          <param name=""items"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RemoveMultiple"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RemoveRange"" returns=""[external]"">
        <parameters>
          <param name=""startingIndex"" type=""[external]"" />
          <param name=""endingIndex"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IRangeable&lt;T&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IRangeable"" />
    </interfaces>
    <constructors />
    <properties />
    <events />
    <fields />
    <methods>
      <method name=""AddRange"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""AddRangeDistinct"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""InsertRange"" returns=""[external]"">
        <parameters>
          <param name=""startingIndex"" type=""[external]"" />
          <param name=""newItems"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RemoveMultiple"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IRoutedEnumerable"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields />
    <methods>
      <method name=""GetCount"" returns=""[external]"">
        <parameters>
          <param name=""route"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""[external]"">
        <parameters>
          <param name=""route"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IRoutedEnumerable&lt;TItem,TRoute&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IRoutedEnumerable"" />
    </interfaces>
    <constructors />
    <properties />
    <events />
    <fields />
    <methods>
      <method name=""GetCount"" returns=""[external]"">
        <parameters>
          <param name=""route"" type=""TRoute"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""[external]"">
        <parameters>
          <param name=""route"" type=""TRoute"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.ITolerant"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""Item"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.ITolerantDictionary&lt;TKey,TValue&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary&lt;TKey,TValue&gt;"" />
      <interface name=""IVSoftware.Portable.Collections.ITolerant"" />
    </interfaces>
    <constructors />
    <properties>
      <property name=""Item"" type=""TValue"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.IUpgradeableDictionary"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary"" />
    </interfaces>
    <constructors />
    <properties />
    <events />
    <fields />
    <methods>
      <method name=""TransferEvents"" returns=""[external]"">
        <parameters>
          <param name=""to"" type=""IVSoftware.Portable.Collections.IObservableDictionary"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.ModelAuthorityEpochProvider"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""Authorities"" type=""IVSoftware.Portable.Collections.StdModelAuthority[]"" canRead=""true"" canWrite=""false"" />
      <property name=""Authorities"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" canRead=""true"" canWrite=""false"" />
      <property name=""Authority"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Count"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsCancelled"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsDisposing"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsReadOnly"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Keys"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Values"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events>
      <event name=""BeginUsing"" type=""[external]"" />
      <event name=""CountChanged"" type=""[external]"" />
      <event name=""FinalDispose"" type=""[external]"" />
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
          <param name=""key"" type=""[external]"" />
          <param name=""value"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CancelAuthorityEpoch"" returns=""[external]"">
        <parameters>
          <param name=""throw"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CanForwardEvent"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""CanForwardEvent"" returns=""[external]"">
        <parameters>
          <param name=""e"" type=""[external]"" />
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
      <method name=""ContainsKey"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CopyTo"" returns=""[external]"">
        <parameters>
          <param name=""array"" type=""[external]"" />
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
      <method name=""GetEnumerator"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasRequestedAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""HasRequestedAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""IsZero"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Remove"" returns=""[external]"">
        <parameters>
          <param name=""item"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Remove"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
          <param name=""properties"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""[external]"" />
          <param name=""properties"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""TryGetValue"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""[external]"" />
          <param name=""value"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Collection"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" />
      <field name=""CollectionDeferred"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" />
      <field name=""Model"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" />
      <field name=""ModelDeferred"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" />
      <field name=""NoAuthority"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" />
      <field name=""ObservableNetCollection"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" />
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
  <type name=""IVSoftware.Portable.Collections.ModelEpochDisposeEventArgs"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""releasedSenders"" type=""[external]"" />
          <param name=""snapshot"" type=""[external]"" />
          <param name=""batchEventArgs"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" />
          <param name=""finalList"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Digest"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" canRead=""true"" canWrite=""false"" />
      <property name=""FinalList"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""KeyCount"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Keys"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ReleasedSenders"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Values"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""ContainsKey"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""[external]"" />
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
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""TryGetValue"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""[external]"" />
          <param name=""value"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.ModelTrackingFlag"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""ItemPropertyChanges"" type=""IVSoftware.Portable.Collections.ModelTrackingFlag"" />
      <field name=""ItemQueries"" type=""IVSoftware.Portable.Collections.ModelTrackingFlag"" />
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
  <type name=""IVSoftware.Portable.Collections.NetProjectionTopology"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""AllowDirectChanges"" type=""IVSoftware.Portable.Collections.NetProjectionTopology"" />
      <field name=""None"" type=""IVSoftware.Portable.Collections.NetProjectionTopology"" />
      <field name=""ObservableOnly"" type=""IVSoftware.Portable.Collections.NetProjectionTopology"" />
      <field name=""Routed"" type=""IVSoftware.Portable.Collections.NetProjectionTopology"" />
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
  <type name=""IVSoftware.Portable.Collections.NotifyCollectionChangeAction"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Add"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeAction"" />
      <field name=""Digest"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeAction"" />
      <field name=""Move"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeAction"" />
      <field name=""Remove"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeAction"" />
      <field name=""Replace"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeAction"" />
      <field name=""Reset"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeAction"" />
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
  <type name=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""ApplyFilter"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"" />
      <field name=""Cancel"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"" />
      <field name=""Digest"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"" />
      <field name=""Exception"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"" />
      <field name=""None"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"" />
      <field name=""RemoveFilter"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"" />
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
  <type name=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""CancelOnly"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" />
      <field name=""FullControl"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" />
      <field name=""ReadOnly"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" />
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
  <type name=""IVSoftware.Portable.Collections.NotifyCollectionChangingEventHandler"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""object"" type=""[external]"" />
          <param name=""method"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Method"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Target"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""BeginInvoke"" returns=""[external]"">
        <parameters>
          <param name=""sender"" type=""[external]"" />
          <param name=""e"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" />
          <param name=""callback"" type=""[external]"" />
          <param name=""object"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Clone"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""DynamicInvoke"" returns=""[external]"">
        <parameters>
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""EndInvoke"" returns=""[external]"">
        <parameters>
          <param name=""result"" type=""[external]"" />
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
      <method name=""GetInvocationList"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""[external]"">
        <parameters>
          <param name=""info"" type=""[external]"" />
          <param name=""context"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Invoke"" returns=""[external]"">
        <parameters>
          <param name=""sender"" type=""[external]"" />
          <param name=""e"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.ObservableModeledCollection&lt;T&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IModeledCollection"" />
    </interfaces>
    <constructors>
      <ctor>
        <parameters />
      </ctor>
    </constructors>
    <properties>
      <property name=""AuthorityProviders"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Count"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""EventScope"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" canRead=""true"" canWrite=""true"" />
      <property name=""FilterQueryDatabase"" type=""IVSoftware.Portable.Collections.SQLiteQueryOnlyConnection"" canRead=""true"" canWrite=""true"" />
      <property name=""Histo"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""T"" canRead=""true"" canWrite=""true"" />
      <property name=""Model"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ModelDataExchangeAuthority"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" canRead=""true"" canWrite=""false"" />
      <property name=""ModelTracking"" type=""IVSoftware.Portable.Collections.ModelTrackingFlag"" canRead=""true"" canWrite=""true"" />
      <property name=""ObservableNetProjection"" type=""[external]"" canRead=""true"" canWrite=""true"" />
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
      <method name=""CancelModelAuthorityEpoch"" returns=""[external]"">
        <parameters />
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
      <method name=""GetEnumerator"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetEnumerator"" returns=""[external]"">
        <parameters>
          <param name=""route"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""HasAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authorityUnk"" type=""[external]"" />
        </parameters>
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
      <method name=""LoadCanon"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""LoadCanonAsync"" returns=""[external]"">
        <parameters>
          <param name=""items"" type=""[external]"" />
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
      <method name=""RequestAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" />
          <param name=""source"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""SetObservableNetProjection"" returns=""[external]"">
        <parameters>
          <param name=""onp"" type=""IVSoftware.Portable.Collections.INotifyPreviewCollection"" />
          <param name=""topology"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""SortModel"" returns=""[external]"">
        <parameters>
          <param name=""stdAttr"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""formatting"" type=""IVSoftware.Portable.Collections.FormattingEHM"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters>
          <param name=""formatting"" type=""IVSoftware.Portable.Collections.FormattingOMC"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.PathDiscoveryFSM"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Description"" type=""IVSoftware.Portable.Collections.PathDiscoveryFSM"" />
      <field name=""FullPath"" type=""IVSoftware.Portable.Collections.PathDiscoveryFSM"" />
      <field name=""Id"" type=""IVSoftware.Portable.Collections.PathDiscoveryFSM"" />
      <field name=""IPathPlaceable"" type=""IVSoftware.Portable.Collections.PathDiscoveryFSM"" />
      <field name=""ModelPathAttribute"" type=""IVSoftware.Portable.Collections.PathDiscoveryFSM"" />
      <field name=""NotFound"" type=""IVSoftware.Portable.Collections.PathDiscoveryFSM"" />
      <field name=""Text"" type=""IVSoftware.Portable.Collections.PathDiscoveryFSM"" />
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
  <type name=""IVSoftware.Portable.Collections.ReplaceItemsEventingPolicy"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""All"" type=""IVSoftware.Portable.Collections.ReplaceItemsEventingPolicy"" />
      <field name=""ResetOnAnyChange"" type=""IVSoftware.Portable.Collections.ReplaceItemsEventingPolicy"" />
      <field name=""StructuralReplaceEvent"" type=""IVSoftware.Portable.Collections.ReplaceItemsEventingPolicy"" />
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
  <type name=""IVSoftware.Portable.Collections.RequestStdModelAuthorityDlgt"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""object"" type=""[external]"" />
          <param name=""method"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Method"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Target"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""BeginInvoke"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
          <param name=""callback"" type=""[external]"" />
          <param name=""object"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Clone"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""DynamicInvoke"" returns=""[external]"">
        <parameters>
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""EndInvoke"" returns=""[external]"">
        <parameters>
          <param name=""result"" type=""[external]"" />
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
      <method name=""GetInvocationList"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""[external]"">
        <parameters>
          <param name=""info"" type=""[external]"" />
          <param name=""context"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Invoke"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.SemanticContribution"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""ExplicitFalse"" type=""IVSoftware.Portable.Collections.SemanticContribution"" />
      <field name=""ExplicitTrue"" type=""IVSoftware.Portable.Collections.SemanticContribution"" />
      <field name=""NonBoolean"" type=""IVSoftware.Portable.Collections.SemanticContribution"" />
      <field name=""ReservedNull"" type=""IVSoftware.Portable.Collections.SemanticContribution"" />
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
  <type name=""IVSoftware.Portable.Collections.SQLiteAuthority"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""FullControl"" type=""IVSoftware.Portable.Collections.SQLiteAuthority"" />
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
  <type name=""IVSoftware.Portable.Collections.SQLiteConnectionMapper"">
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
      <method name=""GetFullPath"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""IVSoftware.Portable.Collections.IPathPlaceable"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetId"" returns=""[external]"">
        <parameters>
          <param name=""this"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetPK"" returns=""[external]"">
        <parameters>
          <param name=""type"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetSQLiteMapping"" returns=""[external]"">
        <parameters>
          <param name=""type"" type=""[external]"" />
          <param name=""createFlags"" type=""[external]"" />
          <param name=""contractType"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetSQLiteMapping"" returns=""[external]"">
        <parameters>
          <param name=""type"" type=""[external]"" />
          <param name=""pkName"" type=""[external]"" />
          <param name=""pkPropertyName"" type=""[external]"" />
          <param name=""createFlags"" type=""[external]"" />
          <param name=""contractType"" type=""[external]"" />
        </parameters>
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
  <type name=""IVSoftware.Portable.Collections.SQLiteQueryOnlyConnection"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""storeDateTimeAsTicks"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""BusyTimeout"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""DatabasePath"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""DateTimeStringFormat"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Handle"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsInTransaction"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""LibVersionNumber"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""StoreDateTimeAsTicks"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""StoreTimeSpanAsTicks"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""TableMappings"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""TimeExecution"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Trace"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""Tracer"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events>
      <event name=""TableChanged"" type=""[external]"" />
    </events>
    <fields />
    <methods>
      <method name=""Backup"" returns=""[external]"">
        <parameters>
          <param name=""destinationDatabasePath"" type=""[external]"" />
          <param name=""databaseName"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""BeginTransaction"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Close"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Commit"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""CreateCommand"" returns=""[external]"">
        <parameters>
          <param name=""cmdText"" type=""[external]"" />
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CreateCommand"" returns=""[external]"">
        <parameters>
          <param name=""cmdText"" type=""[external]"" />
          <param name=""ps"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CreateIndex"" returns=""[external]"">
        <parameters>
          <param name=""property"" type=""[external]"" />
          <param name=""unique"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CreateIndex"" returns=""[external]"">
        <parameters>
          <param name=""tableName"" type=""[external]"" />
          <param name=""columnName"" type=""[external]"" />
          <param name=""unique"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CreateIndex"" returns=""[external]"">
        <parameters>
          <param name=""tableName"" type=""[external]"" />
          <param name=""columnNames"" type=""[external]"" />
          <param name=""unique"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CreateIndex"" returns=""[external]"">
        <parameters>
          <param name=""indexName"" type=""[external]"" />
          <param name=""tableName"" type=""[external]"" />
          <param name=""columnName"" type=""[external]"" />
          <param name=""unique"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CreateIndex"" returns=""[external]"">
        <parameters>
          <param name=""indexName"" type=""[external]"" />
          <param name=""tableName"" type=""[external]"" />
          <param name=""columnNames"" type=""[external]"" />
          <param name=""unique"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CreateTable"" returns=""[external]"">
        <parameters>
          <param name=""createFlags"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CreateTable"" returns=""[external]"">
        <parameters>
          <param name=""ty"" type=""[external]"" />
          <param name=""createFlags"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CreateTables"" returns=""[external]"">
        <parameters>
          <param name=""createFlags"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""CreateTables"" returns=""[external]"">
        <parameters>
          <param name=""createFlags"" type=""[external]"" />
          <param name=""types"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""DeferredQuery"" returns=""[external]"">
        <parameters>
          <param name=""query"" type=""[external]"" />
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""DeferredQuery"" returns=""[external]"">
        <parameters>
          <param name=""map"" type=""[external]"" />
          <param name=""query"" type=""[external]"" />
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Delete"" returns=""[external]"">
        <parameters>
          <param name=""objectToDelete"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Delete"" returns=""[external]"">
        <parameters>
          <param name=""primaryKey"" type=""[external]"" />
          <param name=""map"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""DeleteAll"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""DeleteAll"" returns=""[external]"">
        <parameters>
          <param name=""map"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Dispose"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""DropTable"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""DropTable"" returns=""[external]"">
        <parameters>
          <param name=""map"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""EnableLoadExtension"" returns=""[external]"">
        <parameters>
          <param name=""enabled"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""EnableWriteAheadLogging"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Equals"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Execute"" returns=""[external]"">
        <parameters>
          <param name=""query"" type=""[external]"" />
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ExecuteScalar"" returns=""[external]"">
        <parameters>
          <param name=""query"" type=""[external]"" />
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Find"" returns=""[external]"">
        <parameters>
          <param name=""predicate"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Find"" returns=""[external]"">
        <parameters>
          <param name=""pk"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Find"" returns=""[external]"">
        <parameters>
          <param name=""pk"" type=""[external]"" />
          <param name=""map"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""FindWithQuery"" returns=""[external]"">
        <parameters>
          <param name=""query"" type=""[external]"" />
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""FindWithQuery"" returns=""[external]"">
        <parameters>
          <param name=""map"" type=""[external]"" />
          <param name=""query"" type=""[external]"" />
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Get"" returns=""[external]"">
        <parameters>
          <param name=""predicate"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Get"" returns=""[external]"">
        <parameters>
          <param name=""pk"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Get"" returns=""[external]"">
        <parameters>
          <param name=""pk"" type=""[external]"" />
          <param name=""map"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""GetMapping"" returns=""[external]"">
        <parameters>
          <param name=""createFlags"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetMapping"" returns=""[external]"">
        <parameters>
          <param name=""type"" type=""[external]"" />
          <param name=""createFlags"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetTableInfo"" returns=""[external]"">
        <parameters>
          <param name=""tableName"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
          <param name=""extra"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
          <param name=""objType"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
          <param name=""extra"" type=""[external]"" />
          <param name=""objType"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""InsertAll"" returns=""[external]"">
        <parameters>
          <param name=""objects"" type=""[external]"" />
          <param name=""runInTransaction"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""InsertAll"" returns=""[external]"">
        <parameters>
          <param name=""objects"" type=""[external]"" />
          <param name=""extra"" type=""[external]"" />
          <param name=""runInTransaction"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""InsertAll"" returns=""[external]"">
        <parameters>
          <param name=""objects"" type=""[external]"" />
          <param name=""objType"" type=""[external]"" />
          <param name=""runInTransaction"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""InsertOrReplace"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""InsertOrReplace"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
          <param name=""objType"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Query"" returns=""[external]"">
        <parameters>
          <param name=""query"" type=""[external]"" />
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Query"" returns=""[external]"">
        <parameters>
          <param name=""map"" type=""[external]"" />
          <param name=""query"" type=""[external]"" />
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""QueryScalars"" returns=""[external]"">
        <parameters>
          <param name=""query"" type=""[external]"" />
          <param name=""args"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ReKey"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""ReKey"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Release"" returns=""[external]"">
        <parameters>
          <param name=""savepoint"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""[external]"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.SQLiteAuthority"" />
        </parameters>
      </method>
      <method name=""Rollback"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""RollbackTo"" returns=""[external]"">
        <parameters>
          <param name=""savepoint"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""RunInTransaction"" returns=""[external]"">
        <parameters>
          <param name=""action"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""SaveTransactionPoint"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Table"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Update"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Update"" returns=""[external]"">
        <parameters>
          <param name=""obj"" type=""[external]"" />
          <param name=""objType"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""UpdateAll"" returns=""[external]"">
        <parameters>
          <param name=""objects"" type=""[external]"" />
          <param name=""runInTransaction"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Collections.StdModelAttribute"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""comparer"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""defer"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""filters"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""histo"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""isreadonly"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""live"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""match"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""mdc"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""model"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""mpath"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""omc"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""order"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""pmatch"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""predicates"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""preview"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""qmatch"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""sort"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
      <field name=""text"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
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
  <type name=""IVSoftware.Portable.Collections.StdModelAuthority"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""AllowSinkChanges"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
      <field name=""Offload"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
      <field name=""Onboard"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
      <field name=""Revert"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
      <field name=""SuspendForwardAll"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
      <field name=""SuspendForwardCollectionChange"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
      <field name=""SuspendForwardPropertyChange"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
      <field name=""SuspendForwardXModel"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
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
  <type name=""IVSoftware.Portable.Collections.StdModelAuthorityKey"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""ModelDataExchangeAuthority"" type=""IVSoftware.Portable.Collections.StdModelAuthorityKey"" />
      <field name=""StdModelAuthority"" type=""IVSoftware.Portable.Collections.StdModelAuthorityKey"" />
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
  <type name=""IVSoftware.Portable.Collections.StdModelElement"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""item"" type=""IVSoftware.Portable.Collections.StdModelElement"" />
      <field name=""model"" type=""IVSoftware.Portable.Collections.StdModelElement"" />
      <field name=""proxy"" type=""IVSoftware.Portable.Collections.StdModelElement"" />
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
  <type name=""IVSoftware.Portable.Collections.StdPreviewPath"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Description"" type=""IVSoftware.Portable.Collections.StdPreviewPath"" />
      <field name=""NotFound"" type=""IVSoftware.Portable.Collections.StdPreviewPath"" />
      <field name=""Preview"" type=""IVSoftware.Portable.Collections.StdPreviewPath"" />
      <field name=""PreviewAttribute"" type=""IVSoftware.Portable.Collections.StdPreviewPath"" />
      <field name=""Text"" type=""IVSoftware.Portable.Collections.StdPreviewPath"" />
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
  <type name=""IVSoftware.Portable.Collections.StdReserved"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""ErrorIndex"" type=""IVSoftware.Portable.Collections.StdReserved"" />
      <field name=""Indeterminate"" type=""IVSoftware.Portable.Collections.StdReserved"" />
      <field name=""InvalidFlags"" type=""IVSoftware.Portable.Collections.StdReserved"" />
      <field name=""MemberNotDefined"" type=""IVSoftware.Portable.Collections.StdReserved"" />
      <field name=""Null"" type=""IVSoftware.Portable.Collections.StdReserved"" />
      <field name=""UndefinedXAttribute"" type=""IVSoftware.Portable.Collections.StdReserved"" />
      <field name=""UndefinedXBoundAttribute"" type=""IVSoftware.Portable.Collections.StdReserved"" />
      <field name=""value__"" type=""[external]"" />
      <field name=""XElementHasNoKey"" type=""IVSoftware.Portable.Collections.StdReserved"" />
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
  <type name=""IVSoftware.Portable.Collections.TolerantValue"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""ExplicitNull"" type=""IVSoftware.Portable.Collections.TolerantValue"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.StdPredicate"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""IsChecked"" type=""IVSoftware.Portable.Collections.Tracking.StdPredicate"" />
      <field name=""IsSelected"" type=""IVSoftware.Portable.Collections.Tracking.StdPredicate"" />
      <field name=""IsUnchecked"" type=""IVSoftware.Portable.Collections.Tracking.StdPredicate"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.TallyContext"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Decrement"" type=""IVSoftware.Portable.Collections.Tracking.TallyContext"" />
      <field name=""Increment"" type=""IVSoftware.Portable.Collections.Tracking.TallyContext"" />
      <field name=""Offload"" type=""IVSoftware.Portable.Collections.Tracking.TallyContext"" />
      <field name=""Onboard"" type=""IVSoftware.Portable.Collections.Tracking.TallyContext"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""mode"" type=""IVSoftware.Portable.Collections.Tracking.TrackedPropertyMode"" />
          <param name=""condition"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Condition"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" canRead=""true"" canWrite=""false"" />
      <property name=""Mode"" type=""IVSoftware.Portable.Collections.Tracking.TrackedPropertyMode"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackAttribute&lt;T&gt;"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""sink"" type=""T"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Sink"" type=""T"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackedPropertyMode"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Multiple"" type=""IVSoftware.Portable.Collections.Tracking.TrackedPropertyMode"" />
      <field name=""None"" type=""IVSoftware.Portable.Collections.Tracking.TrackedPropertyMode"" />
      <field name=""Single"" type=""IVSoftware.Portable.Collections.Tracking.TrackedPropertyMode"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackedState"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Exclusive"" type=""IVSoftware.Portable.Collections.Tracking.TrackedState"" />
      <field name=""Multi"" type=""IVSoftware.Portable.Collections.Tracking.TrackedState"" />
      <field name=""None"" type=""IVSoftware.Portable.Collections.Tracking.TrackedState"" />
      <field name=""Primary"" type=""IVSoftware.Portable.Collections.Tracking.TrackedState"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackedStateEphemeral"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""NotPressed"" type=""IVSoftware.Portable.Collections.Tracking.TrackedStateEphemeral"" />
      <field name=""Pressed"" type=""IVSoftware.Portable.Collections.Tracking.TrackedStateEphemeral"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackedValueDomain"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Binary"" type=""IVSoftware.Portable.Collections.Tracking.TrackedValueDomain"" />
      <field name=""Contextual"" type=""IVSoftware.Portable.Collections.Tracking.TrackedValueDomain"" />
      <field name=""Incompatible"" type=""IVSoftware.Portable.Collections.Tracking.TrackedValueDomain"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackSinkAttribute"">
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
  <type name=""IVSoftware.Portable.Collections.Tracking.VisibilityPredicateAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""visibility"" type=""IVSoftware.Portable.Collections.Tracking.VisibilityPredicateFlag"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""TypeId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Visibility"" type=""IVSoftware.Portable.Collections.Tracking.VisibilityPredicateFlag"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.VisibilityPredicateFlag"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Always"" type=""IVSoftware.Portable.Collections.Tracking.VisibilityPredicateFlag"" />
      <field name=""Empty"" type=""IVSoftware.Portable.Collections.Tracking.VisibilityPredicateFlag"" />
      <field name=""Multiple"" type=""IVSoftware.Portable.Collections.Tracking.VisibilityPredicateFlag"" />
      <field name=""Single"" type=""IVSoftware.Portable.Collections.Tracking.VisibilityPredicateFlag"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.WhereAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""propertyName"" type=""[external]"" />
          <param name=""expr"" type=""[external]"" />
        </parameters>
      </ctor>
      <ctor>
        <parameters>
          <param name=""stdPropertyName"" type=""[external]"" />
          <param name=""expr"" type=""[external]"" />
        </parameters>
      </ctor>
      <ctor>
        <parameters>
          <param name=""binding"" type=""[external]"" />
          <param name=""wherePredicate"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" />
        </parameters>
      </ctor>
      <ctor>
        <parameters>
          <param name=""stdPropertyName"" type=""[external]"" />
          <param name=""wherePredicate"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Binding"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Expr"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Predicate"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.WherePredicate"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""IsFalse"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" />
      <field name=""IsGreaterThanOrEqualToZero"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" />
      <field name=""IsGreaterThanZero"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" />
      <field name=""IsLessThanOrEqualToZero"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" />
      <field name=""IsLessThanZero"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" />
      <field name=""IsNotZero"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" />
      <field name=""IsTrue"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" />
      <field name=""IsZero"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" />
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
  <type name=""IVSoftware.Portable.Collections.Tracking.WherePredicateAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""predicate"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Predicate"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Collections.XList"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""Count"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""EBcl"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""IsExplicitMatchPresent"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsFixedSize"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsReadOnly"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""IsSynchronized"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""[external]"" canRead=""true"" canWrite=""true"" />
      <property name=""SyncRoot"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""Add"" returns=""[external]"">
        <parameters>
          <param name=""value"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""Contains"" returns=""[external]"">
        <parameters>
          <param name=""value"" type=""[external]"" />
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
          <param name=""value"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""[external]"">
        <parameters>
          <param name=""index"" type=""[external]"" />
          <param name=""value"" type=""[external]"" />
        </parameters>
      </method>
      <method name=""Remove"" returns=""[external]"">
        <parameters>
          <param name=""value"" type=""[external]"" />
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
  <type name=""IVSoftware.Portable.Collections.XModelAttribute&lt;T&gt;"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""xattr"" type=""[external]"" />
          <param name=""stdEnum"" type=""T"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""IsOptedIn"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""StdEnum"" type=""T"" canRead=""true"" canWrite=""false"" />
      <property name=""XAttr"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Collections.ZeroCountPolicy"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""AllowNegativeTallies"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" />
      <field name=""DecrementNotAllowed"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" />
      <field name=""Preserve"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" />
      <field name=""Remove"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" />
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.Effect"">
    <interfaces />
    <constructors />
    <properties />
    <events />
    <fields>
      <field name=""Advisory"" type=""IVSoftware.Portable.Xlm.Linq.Attributes.Effect"" />
      <field name=""AdvisorySink"" type=""IVSoftware.Portable.Xlm.Linq.Attributes.Effect"" />
      <field name=""BoundTo"" type=""IVSoftware.Portable.Xlm.Linq.Attributes.Effect"" />
      <field name=""SinkExplicitTrueOr"" type=""IVSoftware.Portable.Xlm.Linq.Attributes.Effect"" />
      <field name=""SourceAdvisory"" type=""IVSoftware.Portable.Xlm.Linq.Attributes.Effect"" />
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogramIgnoreAttribute"">
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogramMemberAttribute"">
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogramMemberOptionAttribute"">
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogrammerAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""memberOption"" type=""IVSoftware.Portable.Collections.HistogramParticipationPolicy"" />
          <param name=""zeroCountOption"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" />
          <param name=""incrementFalseOption"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""ExplicitFalsePolicy"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" canRead=""true"" canWrite=""false"" />
      <property name=""MemberOption"" type=""IVSoftware.Portable.Collections.HistogramParticipationPolicy"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ZeroCountPolicy"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogrammerCommonAttribute"">
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogrammerFormatAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""name"" type=""[external]"" />
          <param name=""moreNames"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Names"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogrammerFormatAttribute&lt;T&gt;"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""key"" type=""T"" />
          <param name=""moreKeys"" type=""T[]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Keys"" type=""T[]"" canRead=""true"" canWrite=""false"" />
      <property name=""Names"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogrammerFormatCurrentAttribute"">
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.ImplicitBooleanAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""value"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""TypeId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Value"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.ImplicitValueAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""value"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""TypeId"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Value"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.IncrementFalsePolicyAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""option"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Option"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.ModelPathAttribute"">
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.UnilateralContractAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""activateAs"" type=""[external]"" />
          <param name=""knownCompatibleTypes"" type=""[external]"" />
        </parameters>
      </ctor>
      <ctor>
        <parameters>
          <param name=""activateAs"" type=""[external]"" />
          <param name=""knownCompatibleTypes"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""ActivateAsType"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""KnownCompatibleTypes"" type=""[external]"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.ZeroCountPolicyAttribute"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""option"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Option"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" canRead=""true"" canWrite=""false"" />
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
  <type name=""IVSoftware.Portable.Xml.Linq.Collections.ModelDataExchangeFinalDisposeEventArgs"">
    <interfaces />
    <constructors>
      <ctor>
        <parameters>
          <param name=""releasedSenders"" type=""[external]"" />
          <param name=""properties"" type=""[external]"" />
          <param name=""digest"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" />
          <param name=""snapshotPre"" type=""[external]"" />
          <param name=""snapshotPost"" type=""[external]"" />
        </parameters>
      </ctor>
    </constructors>
    <properties>
      <property name=""Digest"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""KeyCount"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Keys"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""ReleasedSenders"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""SnapshotPost"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""SnapshotPre"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""Values"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods>
      <method name=""ContainsKey"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""[external]"" />
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
      <method name=""ToString"" returns=""[external]"">
        <parameters />
      </method>
      <method name=""TryGetValue"" returns=""[external]"">
        <parameters>
          <param name=""key"" type=""[external]"" />
          <param name=""value"" type=""[external]"" />
        </parameters>
      </method>
    </methods>
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Xml.Linq.Collections.Tracking.IModelAmbientBindingContext"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""AmbientBindingContext"" type=""[external]"" canRead=""true"" canWrite=""true"" />
    </properties>
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
  <type name=""IVSoftware.Portable.Xml.Linq.Collections.Tracking.ITrackContext"">
    <interfaces />
    <constructors />
    <properties>
      <property name=""Count"" type=""[external]"" canRead=""true"" canWrite=""false"" />
      <property name=""CurrentItems"" type=""[external]"" canRead=""true"" canWrite=""false"" />
    </properties>
    <events />
    <fields />
    <methods />
    <nestedTypes />
  </type>
</assembly>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting V1 contract manifest as shown."
            );

            var idempotent = typeof(IModeledCollection)
                .Assembly
                .ToPublicContract()
                .ToString();
            if(!idempotent.Equals(expected))
            {
                Assert.Fail("Expecting idempotent");
            }

            Assert.IsTrue(idempotent.IsContractValid(idempotent, ManifestTypePolicy.AssemblyOnly));
        }
    }
}
