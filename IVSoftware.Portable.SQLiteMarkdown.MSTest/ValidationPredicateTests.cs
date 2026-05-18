using Microsoft.VisualStudio.TestTools.UnitTesting;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class ValidationPredicateTests
    {
        [TestMethod]
        public void ParseSqlMarkdown_ValidationPredicateParameter()
        {
            ValidationState state = ValidationState.Empty;
            bool predicateCalled = false;
            Predicate<string> predicate = term => { predicateCalled = true; return false; };

            var context = "dog".ParseSqlMarkdown<SelfIndexedItem>(ref state, predicate);

            Assert.AreEqual(ValidationState.Valid, state, "Expression should parse as valid.");
            Assert.IsFalse(predicateCalled, "Validation predicate is not expected to be invoked.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(context.Query), "Query should be generated.");
        }
    }
}
