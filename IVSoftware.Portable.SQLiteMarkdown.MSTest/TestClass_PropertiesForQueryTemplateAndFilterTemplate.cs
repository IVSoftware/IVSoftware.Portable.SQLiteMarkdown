using IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.QFTemplates;
using IVSoftware.WinOS.MSTest.Extensions;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest;

[TestClass]
public class TestClass_PropertiesForQueryClassAndFilterClass
{
    [TestClass]
    public class PetProfileSqlMarkdownTests
    {
        [TestMethod]
        public void Test_ExpressionTable()
        {
            string actual, expected;

            // Currently passing
            SimpleTerm();
            ImplicitAnd();
            ExplicitAnd();
            RedundantAnd();
            AndNot();
            NotOperator();
            SingleQuotedPhrase();
            DoubleQuotedPhrase();

            // Currently failing
            ExplicitOr();
            RedundantOr();
            GroupedNegation();
            EscapedNot();
            EscapedBrackets();

            #region S U B T E S T S

            void SimpleTerm()
            {
                actual = "pet".ParseSqlMarkdown<PetProfile>();
                expected = @"
SELECT * FROM pets WHERE
(Name LIKE '%pet%' OR Species LIKE '%pet%')";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Simple term failed."
                );
            }

            void ImplicitAnd()
            {
                actual = "cat dog".ParseSqlMarkdown<PetProfile>();
                expected = @"
SELECT * FROM pets WHERE
(Name LIKE '%cat%' OR Species LIKE '%cat%')
AND (Name LIKE '%dog%' OR Species LIKE '%dog%')";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Implicit AND failed."
                );
            }

            void AndNot()
            {
                actual = "cat !dog".ParseSqlMarkdown<PetProfile>();
                expected = @" 
SELECT * FROM pets WHERE
(Name LIKE '%cat%' OR Species LIKE '%cat%') AND NOT ((Name LIKE '%dog%' OR Species LIKE '%dog%'))"
                ;
            }

            void NotOperator()
            {
                actual = "!cat".ParseSqlMarkdown<PetProfile>();
                expected = @" 
SELECT * FROM pets WHERE
NOT ((Name LIKE '%cat%' OR Species LIKE '%cat%'))"
                ;
            }

            void SingleQuotedPhrase()
            {
                actual = "'exact phrase'".ParseSqlMarkdown<PetProfile>();
                expected = @"
SELECT * FROM pets WHERE
(Name LIKE '%exact phrase%' OR Species LIKE '%exact phrase%')";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Single-quoted phrase failed."
                );
            }

            void DoubleQuotedPhrase()
            {
                actual = "\"exact phrase\"".ParseSqlMarkdown<PetProfile>();
                expected = @"
SELECT * FROM pets WHERE
(Name LIKE '%exact phrase%' OR Species LIKE '%exact phrase%')";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Double-quoted phrase failed."
                );
            }

            void ExplicitAnd()
            {
                actual = "cat & dog".ParseSqlMarkdown<PetProfile>();
                expected = @"
SELECT * FROM pets WHERE
(Name LIKE '%cat%' OR Species LIKE '%cat%')
AND (Name LIKE '%dog%' OR Species LIKE '%dog%')";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Explicit AND failed."
                );
            }

            void RedundantAnd()
            {
                actual = "cat &&& dog".ParseSqlMarkdown<PetProfile>();
                expected = @"
SELECT * FROM pets WHERE
(Name LIKE '%cat%' OR Species LIKE '%cat%')
AND (Name LIKE '%dog%' OR Species LIKE '%dog%')";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Redundant AND syntax failed."
                );
            }


            void ExplicitOr()
            {
                actual = "cat | dog".ParseSqlMarkdown<PetProfile>();
                actual.ToClipboardExpected();
                { }
                expected = @" 
SELECT * FROM pets WHERE
(Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')"
                ;

                expected = @"
SELECT * FROM pets WHERE
(Name LIKE '%cat%' OR Species LIKE '%cat%')
OR (Name LIKE '%dog%' OR Species LIKE '%dog%')";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Explicit OR failed."
                );
            }

            void RedundantOr()
            {
                actual = "cat || dog".ParseSqlMarkdown<PetProfile>();
                actual.ToClipboardExpected();
                { }

                expected = @" 
SELECT * FROM pets WHERE
(Name LIKE '%cat%' OR Species LIKE '%cat%') AND (Name LIKE '%dog%' OR Species LIKE '%dog%')"
                ;
                expected = @"
SELECT * FROM pets WHERE
(Name LIKE '%cat%' OR Species LIKE '%cat%')
OR (Name LIKE '%dog%' OR Species LIKE '%dog%')";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Redundant OR syntax failed."
                );
            }

            void GroupedNegation()
            {
                actual = "!(cat | dog)".ParseSqlMarkdown<PetProfile>();
                actual.ToClipboardExpected();
                { }

                expected = @" 
SELECT * FROM pets WHERE
NOT ((Name LIKE '%cat%' OR Species LIKE '%cat%')) AND (Name LIKE '%dog%' OR Species LIKE '%dog%')"
                ;
                expected = @"
SELECT * FROM pets WHERE
NOT ((Name LIKE '%cat%' OR Species LIKE '%cat%') OR (Name LIKE '%dog%' OR Species LIKE '%dog%'))";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Grouped negation failed."
                );
            }

            void EscapedNot()
            {
                actual = "\\!cat".ParseSqlMarkdown<PetProfile>();
                actual.ToClipboardExpected();
                { }
                expected = @" 
SELECT * FROM pets WHERE
(Name LIKE '%\%' OR Species LIKE '%\%') AND NOT ((Name LIKE '%cat%' OR Species LIKE '%cat%'))"
                ;
                expected = @"
SELECT * FROM pets WHERE
(Name LIKE '%!cat%' OR Species LIKE '%!cat%')";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Escaped NOT failed."
                );
            }

            void EscapedBrackets()
            {
                actual = "\\[bracket\\]".ParseSqlMarkdown<PetProfile>();
                actual.ToClipboardExpected();
                { }

                expected = @" 
SELECT * FROM pets WHERE
(Name LIKE '%\%' OR Species LIKE '%\%') AND ()"
                ;
                expected = @"
SELECT * FROM pets WHERE
(Name LIKE '%[bracket]%' OR Species LIKE '%[bracket]%')";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Escaped brackets failed."
                );
            }

            #endregion S U B T E S T S
        }

    }

}
