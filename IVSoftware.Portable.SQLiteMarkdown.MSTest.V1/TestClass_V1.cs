using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Reflection;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.V1
{
    [TestClass]
    public sealed class TestClass_V1
    {
        /// <summary>
        /// This is more of a reference than a test. We're looking for 
        /// confirmation of what was and wasn't visible in v1.
        /// </summary>
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
    }
}
