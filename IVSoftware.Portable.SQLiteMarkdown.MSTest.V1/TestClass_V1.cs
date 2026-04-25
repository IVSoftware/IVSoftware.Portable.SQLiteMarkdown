using IVSoftware.Portable.Collections;
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

            XElement xpc = typeof(IModeledCollection).Assembly.ToPublicContract();

            actual = xpc.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<assembly name=""IVSoftware.Portable.Xml.Linq.Collections"" version=""1.0.0.0"">
  <type name=""IVSoftware.Portable.Collections.AffinityIncrMode"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.AffinityTestableEpoch"">
    <interfaces />
    <properties>
      <property name=""DefaultIncr"" type=""System.TimeSpan"" canRead=""true"" canWrite=""true"" />
      <property name=""GuidReset"" type=""System.Guid"" canRead=""true"" canWrite=""false"" />
      <property name=""UtcReset"" type=""System.DateTimeOffset"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ResetEpoch"" returns=""System.Void"">
        <parameters>
          <param name=""this"" type=""System.IDisposable"" />
        </parameters>
      </method>
      <method name=""TestableEpoch"" returns=""System.IDisposable"">
        <parameters>
          <param name=""this"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""WithTestability"" returns=""System.Guid"">
        <parameters>
          <param name=""this"" type=""System.Guid"" />
          <param name=""mode"" type=""System.Nullable&lt;IVSoftware.Portable.Collections.AffinityIncrMode&gt;"" />
        </parameters>
      </method>
      <method name=""WithTestability"" returns=""System.DateTimeOffset"">
        <parameters>
          <param name=""this"" type=""System.DateTimeOffset"" />
          <param name=""incr"" type=""System.Nullable&lt;System.TimeSpan&gt;"" />
          <param name=""mode"" type=""System.Nullable&lt;IVSoftware.Portable.Collections.AffinityIncrMode&gt;"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.BehaviorMode"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.CollectionChangingEventingPolicy"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Dictionaries.DictionaryEntryPreview"">
    <interfaces />
    <properties>
      <property name=""Key"" type=""System.Object"" canRead=""true"" canWrite=""true"" />
      <property name=""Value"" type=""System.Object"" canRead=""true"" canWrite=""true"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.DictionaryMode"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.EnumHistogrammer"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Disposable.IAuthorityEpochProvider"" />
      <interface name=""System.ComponentModel.INotifyPropertyChanged"" />
    </interfaces>
    <properties>
      <property name=""AllowRootChanges"" type=""System.Boolean"" canRead=""true"" canWrite=""true"" />
      <property name=""Authorities"" type=""IVSoftware.Portable.Collections.StdModelAuthority[]"" canRead=""true"" canWrite=""false"" />
      <property name=""Authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" canRead=""true"" canWrite=""false"" />
      <property name=""FormattingDefault"" type=""System.Enum"" canRead=""true"" canWrite=""true"" />
      <property name=""IsCancelled"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""IsDisposing"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""Model"" type=""System.Xml.Linq.XElement"" canRead=""true"" canWrite=""true"" />
    </properties>
    <methods>
      <method name=""CancelAuthorityEpoch"" returns=""System.Void"">
        <parameters>
          <param name=""throw"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""HasAuthority"" returns=""System.Boolean"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""HasRequestedAuthority"" returns=""System.Boolean"">
        <parameters>
          <param name=""authority"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""IsZero"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
          <param name=""properties"" type=""System.Collections.Generic.Dictionary&lt;System.String,System.Object&gt;"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""System.Enum"" />
          <param name=""properties"" type=""System.Collections.Generic.Dictionary&lt;System.String,System.Object&gt;"" />
        </parameters>
      </method>
      <method name=""TallyModelToReconcile"" returns=""System.Nullable&lt;System.Boolean&gt;"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""formatting"" type=""Newtonsoft.Json.Formatting"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""formatting"" type=""System.Enum"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.EnumHistogrammer&lt;T&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Disposable.IAuthorityEpochProvider"" />
      <interface name=""IVSoftware.Portable.Disposable.IAuthorityEpochProvider&lt;IVSoftware.Portable.Collections.StdModelAuthority&gt;"" />
      <interface name=""System.Collections.Generic.IEnumerable&lt;System.Collections.Generic.KeyValuePair&lt;T,System.Int32&gt;&gt;"" />
      <interface name=""System.Collections.Generic.IReadOnlyCollection&lt;System.Collections.Generic.KeyValuePair&lt;T,System.Int32&gt;&gt;"" />
      <interface name=""System.Collections.Generic.IReadOnlyDictionary&lt;T,System.Int32&gt;"" />
      <interface name=""System.Collections.IEnumerable"" />
      <interface name=""System.ComponentModel.INotifyPropertyChanged"" />
    </interfaces>
    <properties>
      <property name=""AllowRootChanges"" type=""System.Boolean"" canRead=""true"" canWrite=""true"" />
      <property name=""Authorities"" type=""IVSoftware.Portable.Collections.StdModelAuthority[]"" canRead=""true"" canWrite=""false"" />
      <property name=""Authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" canRead=""true"" canWrite=""false"" />
      <property name=""Count"" type=""System.Int32"" canRead=""true"" canWrite=""false"" />
      <property name=""ExplicitFalsePolicy"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" canRead=""true"" canWrite=""true"" />
      <property name=""FormattingDefault"" type=""System.Enum"" canRead=""true"" canWrite=""true"" />
      <property name=""HistogramParticipationOption"" type=""IVSoftware.Portable.Collections.HistogramParticipationPolicy"" canRead=""true"" canWrite=""true"" />
      <property name=""IsCancelled"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""IsDisposing"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""System.Int32"" canRead=""true"" canWrite=""false"" />
      <property name=""Keys"" type=""System.Collections.Generic.IEnumerable&lt;T&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""Model"" type=""System.Xml.Linq.XElement"" canRead=""true"" canWrite=""true"" />
      <property name=""Values"" type=""System.Collections.Generic.IEnumerable&lt;System.Int32&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""ZeroCountPolicy"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" canRead=""true"" canWrite=""true"" />
    </properties>
    <methods>
      <method name=""CancelAuthorityEpoch"" returns=""System.Void"">
        <parameters>
          <param name=""throw"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""ContainsKey"" returns=""System.Boolean"">
        <parameters>
          <param name=""key"" type=""T"" />
        </parameters>
      </method>
      <method name=""Decrement"" returns=""System.Int32"">
        <parameters>
          <param name=""key"" type=""T"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.Generic.IEnumerator&lt;T&gt;"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""HasAuthority"" returns=""System.Boolean"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""HasRequestedAuthority"" returns=""System.Boolean"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""HasRequestedAuthority"" returns=""System.Boolean"">
        <parameters>
          <param name=""authority"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""Increment"" returns=""System.Int32"">
        <parameters>
          <param name=""key"" type=""T"" />
        </parameters>
      </method>
      <method name=""IsZero"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
          <param name=""properties"" type=""System.Collections.Generic.Dictionary&lt;System.String,System.Object&gt;"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""System.Enum"" />
          <param name=""properties"" type=""System.Collections.Generic.Dictionary&lt;System.String,System.Object&gt;"" />
        </parameters>
      </method>
      <method name=""TallyModelToReconcile"" returns=""System.Nullable&lt;System.Boolean&gt;"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""formatting"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""formatting"" type=""Newtonsoft.Json.Formatting"" />
        </parameters>
      </method>
      <method name=""TryGetValue"" returns=""System.Boolean"">
        <parameters>
          <param name=""key"" type=""T"" />
          <param name=""value"" type=""System.Int32&amp;"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Events.EHPropertyChangedEventArgs"">
    <interfaces />
    <properties>
      <property name=""Changing"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""Edge"" type=""IVSoftware.Portable.Collections.HistogramEdge"" canRead=""true"" canWrite=""false"" />
      <property name=""Key"" type=""System.Enum"" canRead=""true"" canWrite=""false"" />
      <property name=""KeyPrev"" type=""System.Enum"" canRead=""true"" canWrite=""false"" />
      <property name=""ObjectChange"" type=""System.Nullable&lt;System.Xml.Linq.XObjectChange&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""Parent"" type=""System.Xml.Linq.XElement"" canRead=""true"" canWrite=""false"" />
      <property name=""PropertyName"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""Value"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""ValuePrev"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""XOB"" type=""System.Xml.Linq.XObject"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Events.ItemPropertyChangedEventArgs"">
    <interfaces />
    <properties>
      <property name=""Item"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
      <property name=""PropertyName"" type=""System.String"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"">
    <interfaces>
      <interface name=""System.IEquatable&lt;IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs&gt;"" />
    </interfaces>
    <properties>
      <property name=""Action"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeAction"" canRead=""true"" canWrite=""true"" />
      <property name=""Cancel"" type=""System.Boolean"" canRead=""true"" canWrite=""true"" />
      <property name=""IsBclCompatible"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""IsModified"" type=""System.Boolean"" canRead=""true"" canWrite=""true"" />
      <property name=""NewItems"" type=""System.Collections.IList"" canRead=""true"" canWrite=""false"" />
      <property name=""NewStartingIndex"" type=""System.Int32"" canRead=""true"" canWrite=""true"" />
      <property name=""OldItems"" type=""System.Collections.IList"" canRead=""true"" canWrite=""false"" />
      <property name=""OldStartingIndex"" type=""System.Int32"" canRead=""true"" canWrite=""true"" />
      <property name=""Reason"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"" canRead=""true"" canWrite=""true"" />
      <property name=""Scope"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""other"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Events.XModelChangeEventArgs"">
    <interfaces />
    <properties>
      <property name=""Changing"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""Edge"" type=""IVSoftware.Portable.Collections.HistogramEdge"" canRead=""true"" canWrite=""false"" />
      <property name=""Key"" type=""System.Enum"" canRead=""true"" canWrite=""true"" />
      <property name=""KeyPrev"" type=""System.Enum"" canRead=""true"" canWrite=""true"" />
      <property name=""ObjectChange"" type=""System.Xml.Linq.XObjectChange"" canRead=""true"" canWrite=""false"" />
      <property name=""Parent"" type=""System.Xml.Linq.XElement"" canRead=""true"" canWrite=""true"" />
      <property name=""Value"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""ValuePrev"" type=""System.String"" canRead=""true"" canWrite=""true"" />
      <property name=""XOB"" type=""System.Xml.Linq.XObject"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Exceptions.ModelAccessViolation"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Exceptions.ModelAccessViolationException"">
    <interfaces>
      <interface name=""System.Runtime.Serialization.ISerializable"" />
    </interfaces>
    <properties>
      <property name=""Data"" type=""System.Collections.IDictionary"" canRead=""true"" canWrite=""false"" />
      <property name=""HelpLink"" type=""System.String"" canRead=""true"" canWrite=""true"" />
      <property name=""HResult"" type=""System.Int32"" canRead=""true"" canWrite=""true"" />
      <property name=""InnerException"" type=""System.Exception"" canRead=""true"" canWrite=""false"" />
      <property name=""Message"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""Source"" type=""System.String"" canRead=""true"" canWrite=""true"" />
      <property name=""StackTrace"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""TargetSite"" type=""System.Reflection.MethodBase"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetBaseException"" returns=""System.Exception"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""System.Void"">
        <parameters>
          <param name=""info"" type=""System.Runtime.Serialization.SerializationInfo"" />
          <param name=""context"" type=""System.Runtime.Serialization.StreamingContext"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Exceptions.ModelException"">
    <interfaces>
      <interface name=""System.Runtime.Serialization.ISerializable"" />
    </interfaces>
    <properties>
      <property name=""Data"" type=""System.Collections.IDictionary"" canRead=""true"" canWrite=""false"" />
      <property name=""HelpLink"" type=""System.String"" canRead=""true"" canWrite=""true"" />
      <property name=""HResult"" type=""System.Int32"" canRead=""true"" canWrite=""true"" />
      <property name=""InnerException"" type=""System.Exception"" canRead=""true"" canWrite=""false"" />
      <property name=""Message"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""Source"" type=""System.String"" canRead=""true"" canWrite=""true"" />
      <property name=""StackTrace"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""TargetSite"" type=""System.Reflection.MethodBase"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetBaseException"" returns=""System.Exception"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""System.Void"">
        <parameters>
          <param name=""info"" type=""System.Runtime.Serialization.SerializationInfo"" />
          <param name=""context"" type=""System.Runtime.Serialization.StreamingContext"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Exceptions.ModelPolicyViolation"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.ExpandXKeyFormatRequestedEventArgs"">
    <interfaces />
    <properties>
      <property name=""ValueToFormat"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
      <property name=""XKey"" type=""System.Xml.Linq.XElement"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Extensions"">
    <interfaces />
    <properties />
    <methods>
      <method name=""AddAttributeFirst"" returns=""System.Xml.Linq.XElement"">
        <parameters>
          <param name=""this"" type=""System.Xml.Linq.XElement"" />
          <param name=""attr"" type=""System.Xml.Linq.XAttribute"" />
        </parameters>
      </method>
      <method name=""Attribute"" returns=""System.Xml.Linq.XAttribute"">
        <parameters>
          <param name=""this"" type=""System.Xml.Linq.XElement"" />
          <param name=""stdEnum"" type=""System.Enum"" />
          <param name=""throw"" type=""System.Nullable&lt;IVSoftware.Portable.Common.Exceptions.ThrowOrAdvise&gt;"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetCustomAttribute"" returns=""TAttribute"">
        <parameters>
          <param name=""value"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""GetCustomAttribute"" returns=""System.Attribute"">
        <parameters>
          <param name=""value"" type=""System.Enum"" />
          <param name=""openGenericType"" type=""System.Type"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""InsertAttributeAfter"" returns=""System.Xml.Linq.XElement"">
        <parameters>
          <param name=""this"" type=""System.Xml.Linq.XElement"" />
          <param name=""after"" type=""System.Enum"" />
          <param name=""attr"" type=""System.Xml.Linq.XAttribute"" />
        </parameters>
      </method>
      <method name=""InsertAttributeAfter"" returns=""System.Xml.Linq.XElement"">
        <parameters>
          <param name=""this"" type=""System.Xml.Linq.XElement"" />
          <param name=""after"" type=""System.String"" />
          <param name=""attr"" type=""System.Xml.Linq.XAttribute"" />
        </parameters>
      </method>
      <method name=""InsertPreviewAttributeAfter"" returns=""System.Xml.Linq.XElement"">
        <parameters>
          <param name=""this"" type=""System.Xml.Linq.XElement"" />
          <param name=""after"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""InsertPreviewAttributeAfter"" returns=""System.Xml.Linq.XElement"">
        <parameters>
          <param name=""this"" type=""System.Xml.Linq.XElement"" />
          <param name=""after"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""MakeXElement"" returns=""System.Xml.Linq.XElement"">
        <parameters>
          <param name=""this"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""Move"" returns=""System.Xml.Linq.XElement"">
        <parameters>
          <param name=""this"" type=""System.Xml.Linq.XElement"" />
          <param name=""attr"" type=""System.Xml.Linq.XAttribute"" />
          <param name=""index"" type=""System.Int32"" />
        </parameters>
      </method>
      <method name=""SetStdAttributeValue"" returns=""System.Xml.Linq.XElement"">
        <parameters>
          <param name=""this"" type=""System.Xml.Linq.XElement"" />
          <param name=""stdEnum"" type=""System.Enum"" />
          <param name=""value"" type=""System.Object"" />
          <param name=""maxLength"" type=""System.Byte"" />
          <param name=""padToMaxLength"" type=""System.Boolean"" />
          <param name=""throw"" type=""System.Nullable&lt;IVSoftware.Portable.Common.Exceptions.ThrowOrAdvise&gt;"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""XBoundAttribute"" returns=""IVSoftware.Portable.Xml.Linq.XBoundAttribute"">
        <parameters>
          <param name=""this"" type=""System.Xml.Linq.XElement"" />
          <param name=""stdEnum"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
          <param name=""throw"" type=""System.Nullable&lt;IVSoftware.Portable.Common.Exceptions.ThrowOrAdvise&gt;"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.FormattingEH"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.FormattingEHM"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.FormattingOMC"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.HistogramEdge"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.HistogramParticipationPolicy"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.IBriskDictionary"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IInsistent"" />
      <interface name=""System.Collections.ICollection"" />
      <interface name=""System.Collections.IDictionary"" />
      <interface name=""System.Collections.IEnumerable"" />
      <interface name=""System.Collections.Specialized.INotifyCollectionChanged"" />
    </interfaces>
    <properties>
      <property name=""Item"" type=""IVSoftware.Portable.Collections.IObservableDictionary"" canRead=""true"" canWrite=""false"" />
      <property name=""Model"" type=""System.Xml.Linq.XElement"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""ContainsKey"" returns=""System.Boolean"">
        <parameters>
          <param name=""key1"" type=""System.Object"" />
          <param name=""keysN"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""ViewExpandedModel"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.IInsistent"">
    <interfaces />
    <properties />
    <methods />
  </type>
  <type name=""IVSoftware.Portable.Collections.IInsistentDictionary"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IInsistent"" />
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary"" />
      <interface name=""System.Collections.ICollection"" />
      <interface name=""System.Collections.IDictionary"" />
      <interface name=""System.Collections.IEnumerable"" />
      <interface name=""System.Collections.Specialized.INotifyCollectionChanged"" />
    </interfaces>
    <properties>
      <property name=""ActivationDlgt"" type=""System.Delegate"" canRead=""true"" canWrite=""true"" />
    </properties>
    <methods />
  </type>
  <type name=""IVSoftware.Portable.Collections.IInsistentDictionary&lt;TKey,TValue&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IInsistent"" />
      <interface name=""IVSoftware.Portable.Collections.IInsistentDictionary"" />
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary&lt;TKey,TValue&gt;"" />
      <interface name=""System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;TKey,TValue&gt;&gt;"" />
      <interface name=""System.Collections.Generic.IDictionary&lt;TKey,TValue&gt;"" />
      <interface name=""System.Collections.Generic.IEnumerable&lt;System.Collections.Generic.KeyValuePair&lt;TKey,TValue&gt;&gt;"" />
      <interface name=""System.Collections.ICollection"" />
      <interface name=""System.Collections.IDictionary"" />
      <interface name=""System.Collections.IEnumerable"" />
      <interface name=""System.Collections.Specialized.INotifyCollectionChanged"" />
    </interfaces>
    <properties>
      <property name=""ActivationDlgt"" type=""System.Func&lt;TValue&gt;"" canRead=""true"" canWrite=""true"" />
    </properties>
    <methods />
  </type>
  <type name=""IVSoftware.Portable.Collections.IModeledCollection"">
    <interfaces>
      <interface name=""System.Collections.ICollection"" />
      <interface name=""System.Collections.IEnumerable"" />
      <interface name=""System.Collections.IList"" />
    </interfaces>
    <properties>
      <property name=""AuthorityProviders"" type=""System.Collections.Generic.IDictionary&lt;System.Enum,IVSoftware.Portable.Disposable.IAuthorityEpochProvider&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""FilterQueryDatabase"" type=""IVSoftware.Portable.Collections.SQLiteQueryOnlyConnection"" canRead=""true"" canWrite=""false"" />
      <property name=""Histo"" type=""System.Collections.Generic.IReadOnlyDictionary&lt;IVSoftware.Portable.Collections.StdModelAttribute,System.Int32&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""Model"" type=""System.Xml.Linq.XElement"" canRead=""true"" canWrite=""false"" />
      <property name=""ModelDataExchangeAuthority"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" canRead=""true"" canWrite=""false"" />
      <property name=""ModelTracking"" type=""IVSoftware.Portable.Collections.ModelTrackingFlag"" canRead=""true"" canWrite=""true"" />
      <property name=""ObservableNetProjection"" type=""System.Collections.IList"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""HasAuthority"" returns=""System.Boolean"">
        <parameters>
          <param name=""authority"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""SetObservableNetProjection"" returns=""System.Void"">
        <parameters>
          <param name=""onp"" type=""IVSoftware.Portable.Collections.INotifyPreviewCollection"" />
          <param name=""topology"" type=""System.Nullable&lt;IVSoftware.Portable.Collections.NetProjectionTopology&gt;"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"">
    <interfaces />
    <properties>
      <property name=""CollectionChangingEventingPolicy"" type=""IVSoftware.Portable.Collections.CollectionChangingEventingPolicy"" canRead=""true"" canWrite=""false"" />
      <property name=""EventScope"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods />
  </type>
  <type name=""IVSoftware.Portable.Collections.INotifyPreviewCollection"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""System.Collections.Specialized.INotifyCollectionChanged"" />
    </interfaces>
    <properties />
    <methods />
  </type>
  <type name=""IVSoftware.Portable.Collections.Internal.GetFullPathDelegate&lt;T&gt;"">
    <interfaces>
      <interface name=""System.ICloneable"" />
      <interface name=""System.Runtime.Serialization.ISerializable"" />
    </interfaces>
    <properties>
      <property name=""Method"" type=""System.Reflection.MethodInfo"" canRead=""true"" canWrite=""false"" />
      <property name=""Target"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""BeginInvoke"" returns=""System.IAsyncResult"">
        <parameters>
          <param name=""item"" type=""T"" />
          <param name=""callback"" type=""System.AsyncCallback"" />
          <param name=""object"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Clone"" returns=""System.Object"">
        <parameters />
      </method>
      <method name=""DynamicInvoke"" returns=""System.Object"">
        <parameters>
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""EndInvoke"" returns=""System.String"">
        <parameters>
          <param name=""result"" type=""System.IAsyncResult"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetInvocationList"" returns=""System.Delegate[]"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""System.Void"">
        <parameters>
          <param name=""info"" type=""System.Runtime.Serialization.SerializationInfo"" />
          <param name=""context"" type=""System.Runtime.Serialization.StreamingContext"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""Invoke"" returns=""System.String"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Internal.ModelPreviewDelegate"">
    <interfaces>
      <interface name=""System.ICloneable"" />
      <interface name=""System.Runtime.Serialization.ISerializable"" />
    </interfaces>
    <properties>
      <property name=""Method"" type=""System.Reflection.MethodInfo"" canRead=""true"" canWrite=""false"" />
      <property name=""Target"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""BeginInvoke"" returns=""System.IAsyncResult"">
        <parameters>
          <param name=""item"" type=""System.Object"" />
          <param name=""callback"" type=""System.AsyncCallback"" />
          <param name=""object"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Clone"" returns=""System.Object"">
        <parameters />
      </method>
      <method name=""DynamicInvoke"" returns=""System.Object"">
        <parameters>
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""EndInvoke"" returns=""System.String"">
        <parameters>
          <param name=""result"" type=""System.IAsyncResult"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetInvocationList"" returns=""System.Delegate[]"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""System.Void"">
        <parameters>
          <param name=""info"" type=""System.Runtime.Serialization.SerializationInfo"" />
          <param name=""context"" type=""System.Runtime.Serialization.StreamingContext"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""Invoke"" returns=""System.String"">
        <parameters>
          <param name=""item"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.IObservableDictionary"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""System.Collections.ICollection"" />
      <interface name=""System.Collections.IDictionary"" />
      <interface name=""System.Collections.IEnumerable"" />
      <interface name=""System.Collections.Specialized.INotifyCollectionChanged"" />
    </interfaces>
    <properties>
      <property name=""DHostEphemeralMode"" type=""IVSoftware.Portable.Disposable.DisposableHost"" canRead=""true"" canWrite=""false"" />
      <property name=""Mode"" type=""IVSoftware.Portable.Collections.DictionaryMode"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""AddRange"" returns=""System.Void"">
        <parameters>
          <param name=""entries"" type=""System.Collections.Generic.IEnumerable&lt;IVSoftware.Portable.Collections.Dictionaries.DictionaryEntryPreview&gt;"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.IObservableDictionary&lt;TKey,TValue&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary"" />
      <interface name=""System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;TKey,TValue&gt;&gt;"" />
      <interface name=""System.Collections.Generic.IDictionary&lt;TKey,TValue&gt;"" />
      <interface name=""System.Collections.Generic.IEnumerable&lt;System.Collections.Generic.KeyValuePair&lt;TKey,TValue&gt;&gt;"" />
      <interface name=""System.Collections.ICollection"" />
      <interface name=""System.Collections.IDictionary"" />
      <interface name=""System.Collections.IEnumerable"" />
      <interface name=""System.Collections.Specialized.INotifyCollectionChanged"" />
    </interfaces>
    <properties />
    <methods />
  </type>
  <type name=""IVSoftware.Portable.Collections.IPathPlaceable"">
    <interfaces />
    <properties>
      <property name=""FullPath"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""Id"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""ParentId"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""ParentPath"" type=""System.String"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods />
  </type>
  <type name=""IVSoftware.Portable.Collections.IPredicated"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IRoutedEnumerable"" />
      <interface name=""System.Collections.IEnumerable"" />
    </interfaces>
    <properties>
      <property name=""ActiveFilters"" type=""System.Collections.Generic.IReadOnlyDictionary&lt;System.String,System.Enum&gt;"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""ActivatePredicates"" returns=""System.Void"">
        <parameters>
          <param name=""stdPredicate"" type=""System.Enum"" />
          <param name=""more"" type=""System.Enum[]"" />
        </parameters>
      </method>
      <method name=""BeginPredicateAtom"" returns=""System.IDisposable"">
        <parameters />
      </method>
      <method name=""ClearPredicates"" returns=""System.Void"">
        <parameters>
          <param name=""clearInputText"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""DeactivatePredicates"" returns=""System.Void"">
        <parameters>
          <param name=""stdPredicate"" type=""System.Enum"" />
          <param name=""more"" type=""System.Enum[]"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.IRangeable"">
    <interfaces />
    <properties />
    <methods>
      <method name=""AddRange"" returns=""System.Void"">
        <parameters>
          <param name=""items"" type=""System.Collections.IEnumerable"" />
        </parameters>
      </method>
      <method name=""AddRangeDistinct"" returns=""System.Int32"">
        <parameters>
          <param name=""items"" type=""System.Collections.IEnumerable"" />
        </parameters>
      </method>
      <method name=""InsertRange"" returns=""System.Void"">
        <parameters>
          <param name=""startingIndex"" type=""System.Int32"" />
          <param name=""items"" type=""System.Collections.IEnumerable"" />
        </parameters>
      </method>
      <method name=""RemoveMultiple"" returns=""System.Int32"">
        <parameters>
          <param name=""items"" type=""System.Collections.IEnumerable"" />
        </parameters>
      </method>
      <method name=""RemoveRange"" returns=""System.Void"">
        <parameters>
          <param name=""startingIndex"" type=""System.Int32"" />
          <param name=""endingIndex"" type=""System.Int32"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.IRangeable&lt;T&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IRangeable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""AddRange"" returns=""System.Void"">
        <parameters>
          <param name=""items"" type=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
        </parameters>
      </method>
      <method name=""AddRangeDistinct"" returns=""System.Int32"">
        <parameters>
          <param name=""items"" type=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
        </parameters>
      </method>
      <method name=""InsertRange"" returns=""System.Void"">
        <parameters>
          <param name=""startingIndex"" type=""System.Int32"" />
          <param name=""newItems"" type=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
        </parameters>
      </method>
      <method name=""RemoveMultiple"" returns=""System.Int32"">
        <parameters>
          <param name=""items"" type=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.IRoutedEnumerable"">
    <interfaces>
      <interface name=""System.Collections.IEnumerable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""GetCount"" returns=""System.Int32"">
        <parameters>
          <param name=""route"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.IEnumerator"">
        <parameters>
          <param name=""route"" type=""System.Enum"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.IRoutedEnumerable&lt;TItem,TRoute&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IRoutedEnumerable"" />
      <interface name=""System.Collections.IEnumerable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""GetCount"" returns=""System.Int32"">
        <parameters>
          <param name=""route"" type=""TRoute"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.Generic.IEnumerator&lt;TItem&gt;"">
        <parameters>
          <param name=""route"" type=""TRoute"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.ITolerant"">
    <interfaces />
    <properties>
      <property name=""Item"" type=""System.Object"" canRead=""true"" canWrite=""true"" />
    </properties>
    <methods />
  </type>
  <type name=""IVSoftware.Portable.Collections.ITolerantDictionary&lt;TKey,TValue&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary&lt;TKey,TValue&gt;"" />
      <interface name=""IVSoftware.Portable.Collections.ITolerant"" />
      <interface name=""System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;TKey,TValue&gt;&gt;"" />
      <interface name=""System.Collections.Generic.IDictionary&lt;TKey,TValue&gt;"" />
      <interface name=""System.Collections.Generic.IEnumerable&lt;System.Collections.Generic.KeyValuePair&lt;TKey,TValue&gt;&gt;"" />
      <interface name=""System.Collections.ICollection"" />
      <interface name=""System.Collections.IDictionary"" />
      <interface name=""System.Collections.IEnumerable"" />
      <interface name=""System.Collections.Specialized.INotifyCollectionChanged"" />
    </interfaces>
    <properties>
      <property name=""Item"" type=""TValue"" canRead=""true"" canWrite=""true"" />
    </properties>
    <methods />
  </type>
  <type name=""IVSoftware.Portable.Collections.IUpgradeableDictionary"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.INotifyCollectionChanging"" />
      <interface name=""IVSoftware.Portable.Collections.IObservableDictionary"" />
      <interface name=""System.Collections.ICollection"" />
      <interface name=""System.Collections.IDictionary"" />
      <interface name=""System.Collections.IEnumerable"" />
      <interface name=""System.Collections.Specialized.INotifyCollectionChanged"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""TransferEvents"" returns=""System.ValueTuple&lt;System.Int32,System.Int32&gt;"">
        <parameters>
          <param name=""to"" type=""IVSoftware.Portable.Collections.IObservableDictionary"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.ModelAuthorityEpochProvider"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Disposable.IAuthorityEpochProvider"" />
      <interface name=""IVSoftware.Portable.Disposable.IAuthorityEpochProvider&lt;IVSoftware.Portable.Collections.StdModelAuthority&gt;"" />
      <interface name=""System.Collections.Generic.ICollection&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.Object&gt;&gt;"" />
      <interface name=""System.Collections.Generic.IDictionary&lt;System.String,System.Object&gt;"" />
      <interface name=""System.Collections.Generic.IEnumerable&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.Object&gt;&gt;"" />
      <interface name=""System.Collections.IEnumerable"" />
    </interfaces>
    <properties>
      <property name=""Authorities"" type=""IVSoftware.Portable.Collections.StdModelAuthority[]"" canRead=""true"" canWrite=""false"" />
      <property name=""Authorities"" type=""System.Enum[]"" canRead=""true"" canWrite=""false"" />
      <property name=""Authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" canRead=""true"" canWrite=""false"" />
      <property name=""Authority"" type=""System.Enum"" canRead=""true"" canWrite=""false"" />
      <property name=""Count"" type=""System.Int32"" canRead=""true"" canWrite=""false"" />
      <property name=""IsCancelled"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""IsDisposing"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""IsReadOnly"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""System.Object"" canRead=""true"" canWrite=""true"" />
      <property name=""Keys"" type=""System.Collections.Generic.ICollection&lt;System.String&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""Values"" type=""System.Collections.Generic.ICollection&lt;System.Object&gt;"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Add"" returns=""System.Void"">
        <parameters>
          <param name=""item"" type=""System.Collections.Generic.KeyValuePair&lt;System.String,System.Object&gt;"" />
        </parameters>
      </method>
      <method name=""Add"" returns=""System.Void"">
        <parameters>
          <param name=""key"" type=""System.String"" />
          <param name=""value"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""CancelAuthorityEpoch"" returns=""System.Void"">
        <parameters>
          <param name=""throw"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""CanForwardEvent"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""CanForwardEvent"" returns=""System.Boolean"">
        <parameters>
          <param name=""e"" type=""System.EventArgs"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""Contains"" returns=""System.Boolean"">
        <parameters>
          <param name=""item"" type=""System.Collections.Generic.KeyValuePair&lt;System.String,System.Object&gt;"" />
        </parameters>
      </method>
      <method name=""ContainsKey"" returns=""System.Boolean"">
        <parameters>
          <param name=""key"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""CopyTo"" returns=""System.Void"">
        <parameters>
          <param name=""array"" type=""System.Collections.Generic.KeyValuePair`2[[System.String, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[System.Object, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]][]"" />
          <param name=""arrayIndex"" type=""System.Int32"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetAwaiter"" returns=""System.Runtime.CompilerServices.TaskAwaiter&lt;System.Enum&gt;"">
        <parameters />
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.Generic.IEnumerator&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.Object&gt;&gt;"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""HasRequestedAuthority"" returns=""System.Boolean"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""HasRequestedAuthority"" returns=""System.Boolean"">
        <parameters>
          <param name=""authority"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""IsZero"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Remove"" returns=""System.Boolean"">
        <parameters>
          <param name=""key"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""Remove"" returns=""System.Boolean"">
        <parameters>
          <param name=""item"" type=""System.Collections.Generic.KeyValuePair&lt;System.String,System.Object&gt;"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
          <param name=""properties"" type=""System.Collections.Generic.Dictionary&lt;System.String,System.Object&gt;"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""System.Enum"" />
          <param name=""properties"" type=""System.Collections.Generic.Dictionary&lt;System.String,System.Object&gt;"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""System.Enum"" />
          <param name=""properties"" type=""System.Collections.Generic.Dictionary&lt;System.String,System.Object&gt;"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""TryGetValue"" returns=""System.Boolean"">
        <parameters>
          <param name=""key"" type=""System.String"" />
          <param name=""value"" type=""System.Object&amp;"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.ModelEpochDisposeEventArgs"">
    <interfaces />
    <properties>
      <property name=""Digest"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" canRead=""true"" canWrite=""false"" />
      <property name=""FinalList"" type=""System.Collections.IList"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
      <property name=""KeyCount"" type=""System.Int32"" canRead=""true"" canWrite=""false"" />
      <property name=""Keys"" type=""System.Collections.Generic.IEnumerable&lt;System.String&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""ReleasedSenders"" type=""System.Object[]"" canRead=""true"" canWrite=""false"" />
      <property name=""Values"" type=""System.Collections.Generic.IEnumerable&lt;System.Object&gt;"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""ContainsKey"" returns=""System.Boolean"">
        <parameters>
          <param name=""key"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.Generic.IEnumerator&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.Object&gt;&gt;"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""TryGetValue"" returns=""System.Boolean"">
        <parameters>
          <param name=""key"" type=""System.String"" />
          <param name=""value"" type=""System.Object&amp;"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.ModelTrackingFlag"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.NetProjectionTopology"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.NotifyCollectionChangeAction"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.NotifyCollectionChangeReason"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.NotifyCollectionChangingEventHandler"">
    <interfaces>
      <interface name=""System.ICloneable"" />
      <interface name=""System.Runtime.Serialization.ISerializable"" />
    </interfaces>
    <properties>
      <property name=""Method"" type=""System.Reflection.MethodInfo"" canRead=""true"" canWrite=""false"" />
      <property name=""Target"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""BeginInvoke"" returns=""System.IAsyncResult"">
        <parameters>
          <param name=""sender"" type=""System.Object"" />
          <param name=""e"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" />
          <param name=""callback"" type=""System.AsyncCallback"" />
          <param name=""object"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Clone"" returns=""System.Object"">
        <parameters />
      </method>
      <method name=""DynamicInvoke"" returns=""System.Object"">
        <parameters>
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""EndInvoke"" returns=""System.Void"">
        <parameters>
          <param name=""result"" type=""System.IAsyncResult"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetInvocationList"" returns=""System.Delegate[]"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""System.Void"">
        <parameters>
          <param name=""info"" type=""System.Runtime.Serialization.SerializationInfo"" />
          <param name=""context"" type=""System.Runtime.Serialization.StreamingContext"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""Invoke"" returns=""System.Void"">
        <parameters>
          <param name=""sender"" type=""System.Object"" />
          <param name=""e"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.ObservableModeledCollection&lt;T&gt;"">
    <interfaces>
      <interface name=""IVSoftware.Portable.Collections.IModeledCollection"" />
      <interface name=""System.Collections.Generic.ICollection&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IEnumerable&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IList&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IReadOnlyCollection&lt;T&gt;"" />
      <interface name=""System.Collections.Generic.IReadOnlyList&lt;T&gt;"" />
      <interface name=""System.Collections.ICollection"" />
      <interface name=""System.Collections.IEnumerable"" />
      <interface name=""System.Collections.IList"" />
      <interface name=""System.Collections.Specialized.INotifyCollectionChanged"" />
      <interface name=""System.ComponentModel.INotifyPropertyChanged"" />
    </interfaces>
    <properties>
      <property name=""AuthorityProviders"" type=""System.Collections.Generic.IDictionary&lt;System.Enum,IVSoftware.Portable.Disposable.IAuthorityEpochProvider&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""Count"" type=""System.Int32"" canRead=""true"" canWrite=""false"" />
      <property name=""EventScope"" type=""IVSoftware.Portable.Collections.NotifyCollectionChangeScope"" canRead=""true"" canWrite=""true"" />
      <property name=""FilterQueryDatabase"" type=""IVSoftware.Portable.Collections.SQLiteQueryOnlyConnection"" canRead=""true"" canWrite=""true"" />
      <property name=""Histo"" type=""System.Collections.Generic.IReadOnlyDictionary&lt;IVSoftware.Portable.Collections.StdModelAttribute,System.Int32&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""T"" canRead=""true"" canWrite=""true"" />
      <property name=""Model"" type=""System.Xml.Linq.XElement"" canRead=""true"" canWrite=""false"" />
      <property name=""ModelDataExchangeAuthority"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" canRead=""true"" canWrite=""false"" />
      <property name=""ModelTracking"" type=""IVSoftware.Portable.Collections.ModelTrackingFlag"" canRead=""true"" canWrite=""true"" />
      <property name=""ObservableNetProjection"" type=""System.Collections.IList"" canRead=""true"" canWrite=""true"" />
    </properties>
    <methods>
      <method name=""Add"" returns=""System.Void"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""CancelModelAuthorityEpoch"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""Clear"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""Contains"" returns=""System.Boolean"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""CopyTo"" returns=""System.Void"">
        <parameters>
          <param name=""array"" type=""T[]"" />
          <param name=""index"" type=""System.Int32"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.Generic.IEnumerator&lt;T&gt;"">
        <parameters />
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.Generic.IEnumerator&lt;T&gt;"">
        <parameters />
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.IEnumerator"">
        <parameters>
          <param name=""route"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""HasAuthority"" returns=""System.Boolean"">
        <parameters>
          <param name=""authorityUnk"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""IndexOf"" returns=""System.Int32"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""System.Void"">
        <parameters>
          <param name=""index"" type=""System.Int32"" />
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""LoadCanon"" returns=""System.Void"">
        <parameters>
          <param name=""items"" type=""System.Collections.IList"" />
        </parameters>
      </method>
      <method name=""LoadCanonAsync"" returns=""System.Threading.Tasks.Task"">
        <parameters>
          <param name=""items"" type=""System.Collections.IList"" />
        </parameters>
      </method>
      <method name=""Move"" returns=""System.Void"">
        <parameters>
          <param name=""oldIndex"" type=""System.Int32"" />
          <param name=""newIndex"" type=""System.Int32"" />
        </parameters>
      </method>
      <method name=""Remove"" returns=""System.Boolean"">
        <parameters>
          <param name=""item"" type=""T"" />
        </parameters>
      </method>
      <method name=""RemoveAt"" returns=""System.Void"">
        <parameters>
          <param name=""index"" type=""System.Int32"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.ModelDataExchangeAuthority"" />
          <param name=""source"" type=""System.Collections.IList"" />
        </parameters>
      </method>
      <method name=""SetObservableNetProjection"" returns=""System.Void"">
        <parameters>
          <param name=""onp"" type=""IVSoftware.Portable.Collections.INotifyPreviewCollection"" />
          <param name=""topology"" type=""System.Nullable&lt;IVSoftware.Portable.Collections.NetProjectionTopology&gt;"" />
        </parameters>
      </method>
      <method name=""SortModel"" returns=""System.Void"">
        <parameters>
          <param name=""stdAttr"" type=""IVSoftware.Portable.Collections.StdModelAttribute"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""formatting"" type=""IVSoftware.Portable.Collections.FormattingOMC"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""formatting"" type=""IVSoftware.Portable.Collections.FormattingEHM"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.PathDiscoveryFSM"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.ReplaceItemsEventingPolicy"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.RequestStdModelAuthorityDlgt"">
    <interfaces>
      <interface name=""System.ICloneable"" />
      <interface name=""System.Runtime.Serialization.ISerializable"" />
    </interfaces>
    <properties>
      <property name=""Method"" type=""System.Reflection.MethodInfo"" canRead=""true"" canWrite=""false"" />
      <property name=""Target"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""BeginInvoke"" returns=""System.IAsyncResult"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
          <param name=""callback"" type=""System.AsyncCallback"" />
          <param name=""object"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Clone"" returns=""System.Object"">
        <parameters />
      </method>
      <method name=""DynamicInvoke"" returns=""System.Object"">
        <parameters>
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""EndInvoke"" returns=""System.IDisposable"">
        <parameters>
          <param name=""result"" type=""System.IAsyncResult"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetInvocationList"" returns=""System.Delegate[]"">
        <parameters />
      </method>
      <method name=""GetObjectData"" returns=""System.Void"">
        <parameters>
          <param name=""info"" type=""System.Runtime.Serialization.SerializationInfo"" />
          <param name=""context"" type=""System.Runtime.Serialization.StreamingContext"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""Invoke"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.StdModelAuthority"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.SemanticContribution"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.SQLiteAuthority"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.SQLiteConnectionMapper"">
    <interfaces />
    <properties />
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetFullPath"" returns=""System.String"">
        <parameters>
          <param name=""this"" type=""IVSoftware.Portable.Collections.IPathPlaceable"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetId"" returns=""System.String"">
        <parameters>
          <param name=""this"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetPK"" returns=""SQLite.TableMapping+Column"">
        <parameters>
          <param name=""type"" type=""System.Type"" />
        </parameters>
      </method>
      <method name=""GetSQLiteMapping"" returns=""SQLite.TableMapping"">
        <parameters>
          <param name=""type"" type=""System.Type"" />
          <param name=""createFlags"" type=""SQLite.CreateFlags"" />
          <param name=""contractType"" type=""System.Type"" />
        </parameters>
      </method>
      <method name=""GetSQLiteMapping"" returns=""SQLite.TableMapping"">
        <parameters>
          <param name=""type"" type=""System.Type"" />
          <param name=""pkName"" type=""System.String&amp;"" />
          <param name=""pkPropertyName"" type=""System.String&amp;"" />
          <param name=""createFlags"" type=""SQLite.CreateFlags"" />
          <param name=""contractType"" type=""System.Type"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.SQLiteQueryOnlyConnection"">
    <interfaces>
      <interface name=""SQLite.ISQLiteConnection"" />
      <interface name=""System.IDisposable"" />
    </interfaces>
    <properties>
      <property name=""BusyTimeout"" type=""System.TimeSpan"" canRead=""true"" canWrite=""true"" />
      <property name=""DatabasePath"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""DateTimeStringFormat"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""Handle"" type=""SQLitePCL.sqlite3"" canRead=""true"" canWrite=""false"" />
      <property name=""IsInTransaction"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""LibVersionNumber"" type=""System.Int32"" canRead=""true"" canWrite=""false"" />
      <property name=""StoreDateTimeAsTicks"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""StoreTimeSpanAsTicks"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""TableMappings"" type=""System.Collections.Generic.IEnumerable&lt;SQLite.TableMapping&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""TimeExecution"" type=""System.Boolean"" canRead=""true"" canWrite=""true"" />
      <property name=""Trace"" type=""System.Boolean"" canRead=""true"" canWrite=""true"" />
      <property name=""Tracer"" type=""System.Action&lt;System.String&gt;"" canRead=""true"" canWrite=""true"" />
    </properties>
    <methods>
      <method name=""Backup"" returns=""System.Void"">
        <parameters>
          <param name=""destinationDatabasePath"" type=""System.String"" />
          <param name=""databaseName"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""BeginTransaction"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""Close"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""Commit"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""CreateCommand"" returns=""SQLite.SQLiteCommand"">
        <parameters>
          <param name=""cmdText"" type=""System.String"" />
          <param name=""ps"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""CreateCommand"" returns=""SQLite.SQLiteCommand"">
        <parameters>
          <param name=""cmdText"" type=""System.String"" />
          <param name=""args"" type=""System.Collections.Generic.Dictionary&lt;System.String,System.Object&gt;"" />
        </parameters>
      </method>
      <method name=""CreateIndex"" returns=""System.Int32"">
        <parameters>
          <param name=""property"" type=""System.Linq.Expressions.Expression&lt;System.Func&lt;T,System.Object&gt;&gt;"" />
          <param name=""unique"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""CreateIndex"" returns=""System.Int32"">
        <parameters>
          <param name=""tableName"" type=""System.String"" />
          <param name=""columnName"" type=""System.String"" />
          <param name=""unique"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""CreateIndex"" returns=""System.Int32"">
        <parameters>
          <param name=""tableName"" type=""System.String"" />
          <param name=""columnNames"" type=""System.String[]"" />
          <param name=""unique"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""CreateIndex"" returns=""System.Int32"">
        <parameters>
          <param name=""indexName"" type=""System.String"" />
          <param name=""tableName"" type=""System.String"" />
          <param name=""columnNames"" type=""System.String[]"" />
          <param name=""unique"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""CreateIndex"" returns=""System.Int32"">
        <parameters>
          <param name=""indexName"" type=""System.String"" />
          <param name=""tableName"" type=""System.String"" />
          <param name=""columnName"" type=""System.String"" />
          <param name=""unique"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""CreateTable"" returns=""SQLite.CreateTableResult"">
        <parameters>
          <param name=""createFlags"" type=""SQLite.CreateFlags"" />
        </parameters>
      </method>
      <method name=""CreateTable"" returns=""SQLite.CreateTableResult"">
        <parameters>
          <param name=""ty"" type=""System.Type"" />
          <param name=""createFlags"" type=""SQLite.CreateFlags"" />
        </parameters>
      </method>
      <method name=""CreateTables"" returns=""SQLite.CreateTablesResult"">
        <parameters>
          <param name=""createFlags"" type=""SQLite.CreateFlags"" />
        </parameters>
      </method>
      <method name=""CreateTables"" returns=""SQLite.CreateTablesResult"">
        <parameters>
          <param name=""createFlags"" type=""SQLite.CreateFlags"" />
        </parameters>
      </method>
      <method name=""CreateTables"" returns=""SQLite.CreateTablesResult"">
        <parameters>
          <param name=""createFlags"" type=""SQLite.CreateFlags"" />
        </parameters>
      </method>
      <method name=""CreateTables"" returns=""SQLite.CreateTablesResult"">
        <parameters>
          <param name=""createFlags"" type=""SQLite.CreateFlags"" />
        </parameters>
      </method>
      <method name=""CreateTables"" returns=""SQLite.CreateTablesResult"">
        <parameters>
          <param name=""createFlags"" type=""SQLite.CreateFlags"" />
          <param name=""types"" type=""System.Type[]"" />
        </parameters>
      </method>
      <method name=""DeferredQuery"" returns=""System.Collections.Generic.IEnumerable&lt;T&gt;"">
        <parameters>
          <param name=""query"" type=""System.String"" />
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""DeferredQuery"" returns=""System.Collections.Generic.IEnumerable&lt;System.Object&gt;"">
        <parameters>
          <param name=""map"" type=""SQLite.TableMapping"" />
          <param name=""query"" type=""System.String"" />
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""Delete"" returns=""System.Int32"">
        <parameters>
          <param name=""objectToDelete"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Delete"" returns=""System.Int32"">
        <parameters>
          <param name=""primaryKey"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Delete"" returns=""System.Int32"">
        <parameters>
          <param name=""primaryKey"" type=""System.Object"" />
          <param name=""map"" type=""SQLite.TableMapping"" />
        </parameters>
      </method>
      <method name=""DeleteAll"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""DeleteAll"" returns=""System.Int32"">
        <parameters>
          <param name=""map"" type=""SQLite.TableMapping"" />
        </parameters>
      </method>
      <method name=""Dispose"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""DropTable"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""DropTable"" returns=""System.Int32"">
        <parameters>
          <param name=""map"" type=""SQLite.TableMapping"" />
        </parameters>
      </method>
      <method name=""EnableLoadExtension"" returns=""System.Void"">
        <parameters>
          <param name=""enabled"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""EnableWriteAheadLogging"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Execute"" returns=""System.Int32"">
        <parameters>
          <param name=""query"" type=""System.String"" />
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""ExecuteScalar"" returns=""T"">
        <parameters>
          <param name=""query"" type=""System.String"" />
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""Find"" returns=""T"">
        <parameters>
          <param name=""pk"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Find"" returns=""T"">
        <parameters>
          <param name=""predicate"" type=""System.Linq.Expressions.Expression&lt;System.Func&lt;T,System.Boolean&gt;&gt;"" />
        </parameters>
      </method>
      <method name=""Find"" returns=""System.Object"">
        <parameters>
          <param name=""pk"" type=""System.Object"" />
          <param name=""map"" type=""SQLite.TableMapping"" />
        </parameters>
      </method>
      <method name=""FindWithQuery"" returns=""T"">
        <parameters>
          <param name=""query"" type=""System.String"" />
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""FindWithQuery"" returns=""System.Object"">
        <parameters>
          <param name=""map"" type=""SQLite.TableMapping"" />
          <param name=""query"" type=""System.String"" />
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""Get"" returns=""T"">
        <parameters>
          <param name=""pk"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Get"" returns=""T"">
        <parameters>
          <param name=""predicate"" type=""System.Linq.Expressions.Expression&lt;System.Func&lt;T,System.Boolean&gt;&gt;"" />
        </parameters>
      </method>
      <method name=""Get"" returns=""System.Object"">
        <parameters>
          <param name=""pk"" type=""System.Object"" />
          <param name=""map"" type=""SQLite.TableMapping"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetMapping"" returns=""SQLite.TableMapping"">
        <parameters>
          <param name=""createFlags"" type=""SQLite.CreateFlags"" />
        </parameters>
      </method>
      <method name=""GetMapping"" returns=""SQLite.TableMapping"">
        <parameters>
          <param name=""type"" type=""System.Type"" />
          <param name=""createFlags"" type=""SQLite.CreateFlags"" />
        </parameters>
      </method>
      <method name=""GetTableInfo"" returns=""System.Collections.Generic.List&lt;SQLite.SQLiteConnection+ColumnInfo&gt;"">
        <parameters>
          <param name=""tableName"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""Insert"" returns=""System.Int32"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""System.Int32"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
          <param name=""objType"" type=""System.Type"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""System.Int32"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
          <param name=""extra"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""System.Int32"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
          <param name=""extra"" type=""System.String"" />
          <param name=""objType"" type=""System.Type"" />
        </parameters>
      </method>
      <method name=""InsertAll"" returns=""System.Int32"">
        <parameters>
          <param name=""objects"" type=""System.Collections.IEnumerable"" />
          <param name=""runInTransaction"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""InsertAll"" returns=""System.Int32"">
        <parameters>
          <param name=""objects"" type=""System.Collections.IEnumerable"" />
          <param name=""extra"" type=""System.String"" />
          <param name=""runInTransaction"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""InsertAll"" returns=""System.Int32"">
        <parameters>
          <param name=""objects"" type=""System.Collections.IEnumerable"" />
          <param name=""objType"" type=""System.Type"" />
          <param name=""runInTransaction"" type=""System.Boolean"" />
        </parameters>
      </method>
      <method name=""InsertOrReplace"" returns=""System.Int32"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""InsertOrReplace"" returns=""System.Int32"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
          <param name=""objType"" type=""System.Type"" />
        </parameters>
      </method>
      <method name=""Query"" returns=""System.Collections.Generic.List&lt;T&gt;"">
        <parameters>
          <param name=""query"" type=""System.String"" />
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""Query"" returns=""System.Collections.Generic.List&lt;System.Object&gt;"">
        <parameters>
          <param name=""map"" type=""SQLite.TableMapping"" />
          <param name=""query"" type=""System.String"" />
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""QueryScalars"" returns=""System.Collections.Generic.List&lt;T&gt;"">
        <parameters>
          <param name=""query"" type=""System.String"" />
          <param name=""args"" type=""System.Object[]"" />
        </parameters>
      </method>
      <method name=""ReKey"" returns=""System.Void"">
        <parameters>
          <param name=""key"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ReKey"" returns=""System.Void"">
        <parameters>
          <param name=""key"" type=""System.Byte[]"" />
        </parameters>
      </method>
      <method name=""Release"" returns=""System.Void"">
        <parameters>
          <param name=""savepoint"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""RequestAuthority"" returns=""System.IDisposable"">
        <parameters>
          <param name=""authority"" type=""IVSoftware.Portable.Collections.SQLiteAuthority"" />
        </parameters>
      </method>
      <method name=""Rollback"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""RollbackTo"" returns=""System.Void"">
        <parameters>
          <param name=""savepoint"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""RunInTransaction"" returns=""System.Void"">
        <parameters>
          <param name=""action"" type=""System.Action"" />
        </parameters>
      </method>
      <method name=""SaveTransactionPoint"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""Table"" returns=""SQLite.TableQuery&lt;T&gt;"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""Update"" returns=""System.Int32"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Update"" returns=""System.Int32"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
          <param name=""objType"" type=""System.Type"" />
        </parameters>
      </method>
      <method name=""UpdateAll"" returns=""System.Int32"">
        <parameters>
          <param name=""objects"" type=""System.Collections.IEnumerable"" />
          <param name=""runInTransaction"" type=""System.Boolean"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.StdModelAttribute"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.StdModelAuthority"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.StdModelAuthorityKey"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.StdModelElement"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.StdPreviewPath"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.StdReserved"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.TolerantValue"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.StdPredicate"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.TallyContext"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackAttribute"">
    <interfaces />
    <properties>
      <property name=""Condition"" type=""IVSoftware.Portable.Collections.Tracking.WherePredicate"" canRead=""true"" canWrite=""false"" />
      <property name=""Mode"" type=""IVSoftware.Portable.Collections.Tracking.TrackedPropertyMode"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackAttribute&lt;T&gt;"">
    <interfaces />
    <properties>
      <property name=""Sink"" type=""T"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackedPropertyMode"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackedState"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackedStateEphemeral"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackedValueDomain"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.TrackSinkAttribute"">
    <interfaces />
    <properties>
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.VisibilityPredicateAttribute"">
    <interfaces />
    <properties>
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
      <property name=""Visibility"" type=""IVSoftware.Portable.Collections.Tracking.VisibilityPredicateFlag"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.VisibilityPredicateFlag"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.WhereAttribute"">
    <interfaces />
    <properties>
      <property name=""Binding"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""Expr"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""Predicate"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.WherePredicate"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.Tracking.WherePredicateAttribute"">
    <interfaces />
    <properties>
      <property name=""Predicate"" type=""System.String"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.XList"">
    <interfaces>
      <interface name=""System.Collections.ICollection"" />
      <interface name=""System.Collections.IEnumerable"" />
      <interface name=""System.Collections.IList"" />
    </interfaces>
    <properties>
      <property name=""Count"" type=""System.Int32"" canRead=""true"" canWrite=""false"" />
      <property name=""EBcl"" type=""System.Collections.Specialized.NotifyCollectionChangedEventArgs"" canRead=""true"" canWrite=""true"" />
      <property name=""IsExplicitMatchPresent"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""IsFixedSize"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""IsReadOnly"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""IsSynchronized"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""System.Object"" canRead=""true"" canWrite=""true"" />
      <property name=""SyncRoot"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Add"" returns=""System.Int32"">
        <parameters>
          <param name=""value"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Clear"" returns=""System.Void"">
        <parameters />
      </method>
      <method name=""Contains"" returns=""System.Boolean"">
        <parameters>
          <param name=""value"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""CopyTo"" returns=""System.Void"">
        <parameters>
          <param name=""array"" type=""System.Array"" />
          <param name=""index"" type=""System.Int32"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.IEnumerator"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IndexOf"" returns=""System.Int32"">
        <parameters>
          <param name=""value"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Insert"" returns=""System.Void"">
        <parameters>
          <param name=""index"" type=""System.Int32"" />
          <param name=""value"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Remove"" returns=""System.Void"">
        <parameters>
          <param name=""value"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""RemoveAt"" returns=""System.Void"">
        <parameters>
          <param name=""index"" type=""System.Int32"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.XModelAttribute&lt;T&gt;"">
    <interfaces />
    <properties>
      <property name=""IsOptedIn"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
      <property name=""StdEnum"" type=""T"" canRead=""true"" canWrite=""false"" />
      <property name=""XAttr"" type=""System.Xml.Linq.XAttribute"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Collections.ZeroCountPolicy"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.Effect"">
    <interfaces>
      <interface name=""System.IComparable"" />
      <interface name=""System.IConvertible"" />
      <interface name=""System.IFormattable"" />
      <interface name=""System.ISpanFormattable"" />
    </interfaces>
    <properties />
    <methods>
      <method name=""CompareTo"" returns=""System.Int32"">
        <parameters>
          <param name=""target"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""GetTypeCode"" returns=""System.TypeCode"">
        <parameters />
      </method>
      <method name=""HasFlag"" returns=""System.Boolean"">
        <parameters>
          <param name=""flag"" type=""System.Enum"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters>
          <param name=""format"" type=""System.String"" />
          <param name=""provider"" type=""System.IFormatProvider"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogramIgnoreAttribute"">
    <interfaces />
    <properties>
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogramMemberAttribute"">
    <interfaces />
    <properties>
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogramMemberOptionAttribute"">
    <interfaces />
    <properties>
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogrammerAttribute"">
    <interfaces />
    <properties>
      <property name=""ExplicitFalsePolicy"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" canRead=""true"" canWrite=""false"" />
      <property name=""MemberOption"" type=""IVSoftware.Portable.Collections.HistogramParticipationPolicy"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
      <property name=""ZeroCountPolicy"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogrammerCommonAttribute"">
    <interfaces />
    <properties>
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogrammerFormatAttribute"">
    <interfaces />
    <properties>
      <property name=""Names"" type=""System.String[]"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogrammerFormatAttribute&lt;T&gt;"">
    <interfaces />
    <properties>
      <property name=""Keys"" type=""T[]"" canRead=""true"" canWrite=""false"" />
      <property name=""Names"" type=""System.String[]"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.HistogrammerFormatCurrentAttribute"">
    <interfaces />
    <properties>
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.ImplicitBooleanAttribute"">
    <interfaces />
    <properties>
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
      <property name=""Value"" type=""System.Boolean"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.ImplicitValueAttribute"">
    <interfaces />
    <properties>
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
      <property name=""Value"" type=""System.String"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.IncrementFalsePolicyAttribute"">
    <interfaces />
    <properties>
      <property name=""Option"" type=""IVSoftware.Portable.Collections.ExplicitFalsePolicy"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.ModelPathAttribute"">
    <interfaces />
    <properties>
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.UnilateralContractAttribute"">
    <interfaces />
    <properties>
      <property name=""ActivateAsType"" type=""System.Type"" canRead=""true"" canWrite=""false"" />
      <property name=""KnownCompatibleTypes"" type=""System.String[]"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xlm.Linq.Attributes.ZeroCountPolicyAttribute"">
    <interfaces />
    <properties>
      <property name=""Option"" type=""IVSoftware.Portable.Collections.ZeroCountPolicy"" canRead=""true"" canWrite=""false"" />
      <property name=""TypeId"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""IsDefaultAttribute"" returns=""System.Boolean"">
        <parameters />
      </method>
      <method name=""Match"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xml.Linq.Collections.ModelDataExchangeFinalDisposeEventArgs"">
    <interfaces />
    <properties>
      <property name=""Digest"" type=""IVSoftware.Portable.Collections.Events.NotifyCollectionChangingEventArgs"" canRead=""true"" canWrite=""false"" />
      <property name=""Item"" type=""System.Object"" canRead=""true"" canWrite=""false"" />
      <property name=""KeyCount"" type=""System.Int32"" canRead=""true"" canWrite=""false"" />
      <property name=""Keys"" type=""System.Collections.Generic.IEnumerable&lt;System.String&gt;"" canRead=""true"" canWrite=""false"" />
      <property name=""ReleasedSenders"" type=""System.Object[]"" canRead=""true"" canWrite=""false"" />
      <property name=""SnapshotPost"" type=""System.Collections.IList"" canRead=""true"" canWrite=""false"" />
      <property name=""SnapshotPre"" type=""System.Collections.IList"" canRead=""true"" canWrite=""false"" />
      <property name=""Values"" type=""System.Collections.Generic.IEnumerable&lt;System.Object&gt;"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods>
      <method name=""ContainsKey"" returns=""System.Boolean"">
        <parameters>
          <param name=""key"" type=""System.String"" />
        </parameters>
      </method>
      <method name=""Equals"" returns=""System.Boolean"">
        <parameters>
          <param name=""obj"" type=""System.Object"" />
        </parameters>
      </method>
      <method name=""GetEnumerator"" returns=""System.Collections.Generic.IEnumerator&lt;System.Collections.Generic.KeyValuePair&lt;System.String,System.Object&gt;&gt;"">
        <parameters />
      </method>
      <method name=""GetHashCode"" returns=""System.Int32"">
        <parameters />
      </method>
      <method name=""GetType"" returns=""System.Type"">
        <parameters />
      </method>
      <method name=""ToString"" returns=""System.String"">
        <parameters />
      </method>
      <method name=""TryGetValue"" returns=""System.Boolean"">
        <parameters>
          <param name=""key"" type=""System.String"" />
          <param name=""value"" type=""System.Object&amp;"" />
        </parameters>
      </method>
    </methods>
  </type>
  <type name=""IVSoftware.Portable.Xml.Linq.Collections.Tracking.IModelAmbientBindingContext"">
    <interfaces />
    <properties>
      <property name=""AmbientBindingContext"" type=""System.Object"" canRead=""true"" canWrite=""true"" />
    </properties>
    <methods />
  </type>
  <type name=""IVSoftware.Portable.Xml.Linq.Collections.Tracking.ITrackContext"">
    <interfaces>
      <interface name=""System.ComponentModel.INotifyPropertyChanged"" />
    </interfaces>
    <properties>
      <property name=""Count"" type=""System.Int32"" canRead=""true"" canWrite=""false"" />
      <property name=""CurrentItems"" type=""System.Array"" canRead=""true"" canWrite=""false"" />
    </properties>
    <methods />
  </type>
</assembly>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting V1 contract manifest as shown."
            );
        }
    }
}
