using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.WinOS.MSTest.Extensions;

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
        public void Test_Capabilities()
        {
            string actual, expected;

            var asmFullName = typeof(MarkdownContext<SelectableQFModel>).Assembly.FullName;

            actual = asmFullName;
            actual.ToClipboardExpected();
            { }
            expected = @" 
IVSoftware.Portable.SQLiteMarkdown, Version=1.0.1.0, Culture=neutral, PublicKeyToken=becf53b24b0b41eb";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting result to match."
            );

            MarkdownContext<SelectableQFModel> mdc = new();
            var cnx = mdc.MemoryDatabase;

            var ct = mdc.ContractType;

#if false && AVAILABLE
            var tn = mdc.TableName;
#endif            
            var builder = new List<string>();
            foreach (var pi in typeof(IObservableQueryFilterSource).GetProperties())
            {
                builder.Add($"{pi.Name}: {pi.PropertyType.Name}");
            }
            foreach (var pi in typeof(IObservableQueryFilterSource<object>).GetProperties())
            {
                builder.Add($"{pi.Name}: {pi.PropertyType.Name}");
            }

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboardExpected();
            { } // <- FIRST TIME ONLY: Adjust the message.
            actual.ToClipboardAssert("Expecting builder content to match.");
            { }
            expected = @" 
IsFiltering: Boolean
InputText: String
SearchEntryState: SearchEntryState
FilteringState: FilteringState
Placeholder: String
Busy: Boolean
QueryFilterConfig: QueryFilterConfig
Title: String
SQL: String
MemoryDatabase: SQLiteConnection
DHostBusy: DisposableHost"
            ;

            var opc = new ObservableQueryFilterSource<object>();
            Assert.IsTrue(
                opc is IObservableQueryFilterSource<object>,
                @"Asserting the claim: [Canonical(""Contract published in v1"")]");
        }
    }
}
