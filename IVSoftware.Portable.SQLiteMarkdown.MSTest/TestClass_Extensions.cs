using IVSoftware.Portable.Common.Collections;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Internal;
using IVSoftware.Portable.SQLiteMarkdown.MSTest.LocalEnums;
using IVSoftware.WinOS.MSTest.Extensions;
using System.ComponentModel;
using System.Xml.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_Extensions
    {
        [TestMethod]
        public void Test_GetAttributeValue()
        {
            string actual, expected;
            Enum @enum;
            XElement model = new(nameof(StdModelElement.model));

            int @int;

            subtest_ObjectUnconstrained();
            subtest_IntFromDefaultAttribute();
            subtest_IntFromDefaultArg();
            subtest_EnumCast();
            subtest_AttributeSimplyPresent();

            #region S U B T E S T S

            subtest_ThrowPolicy();
            void subtest_ThrowPolicy()
            {
                #region L o c a l F x
                var builderThrow = new List<string>();
                void localOnBeginThrowOrAdvise(object? sender, Throw e)
                {
                    builderThrow.Add(e.Message);
                    e.Handled = true;
                }
                #endregion L o c a l F x
                using (this.WithOnDispose(
                    onInit: (sender, e) =>
                    {
                        Throw.BeginThrowOrAdvise += localOnBeginThrowOrAdvise;
                    },
                    onDispose: (sender, e) =>
                    {
                        Throw.BeginThrowOrAdvise -= localOnBeginThrowOrAdvise;
                    }))
                {
                    // No error for int?
                    _ = model.GetAttributeValue<int?>(StdModelAttribute.order);
                    Assert.AreEqual(0, builderThrow.Count);

                    // Error expected for int (non-nullable)
                    _ = model.GetAttributeValue<int>(StdModelAttribute.text);
                    actual = string.Join(Environment.NewLine, builderThrow);
                    expected = @" 
Non-nullable type(Int32) requires default";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting throw policy reports error."
                    );

                    // Successfully uses default if not convertible
                    builderThrow.Clear();
                    model.SetAttributeValue(nameof(StdModelAttribute.text), "banana");
                    @int = model.GetAttributeValue<int>(StdModelAttribute.text, 9);

                    // Conversion succeeds without complaining.
                    Assert.AreEqual(9, @int);
                    Assert.AreEqual(0, builderThrow.Count);

                    // Error: no default is provided.
                    @int = model.GetAttributeValue<int>(StdModelAttribute.text);

                    actual = string.Join(Environment.NewLine, builderThrow);
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
The string provided 'banana' is not numeric.";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting throw policy reports error."
                    );
                }
            }

            void subtest_ObjectUnconstrained()
            {
                object? @object;
                @object = model.GetAttributeValue<object?>(StdModelAttribute.order);
                Assert.IsNull(@object);

                @object = model.GetAttributeValue<object?>(StdModelAttribute.order, @default: 0);
                Assert.AreEqual(0, @object);
            }

            void subtest_IntFromDefaultAttribute()
            {
                @int = model.GetAttributeValue<int>(DefaultValuesForTest.Two);
                Assert.AreEqual(2, @int);
            }

            void subtest_IntFromDefaultArg()
            {
                @int = model.GetAttributeValue<int>(StdModelAttribute.order, 7);
                Assert.AreEqual(7, @int);
            }

            void subtest_EnumCast()
            {
                // Subtle:
                // The fallback default is obtained from the argument enum via its
                // [DefaultValue] attribute. In this case [DefaultValue(2)] supplies "2".
                // That value is then parsed into the target enum type
                // (StdMarkdownAttribute), where 2 corresponds to 'model'.
                //
                // Note that the declared value of DefaultValuesForTest.Two (0x10000002)
                // is irrelevant here; only the DefaultValue attribute participates.

                StdModelAttribute
                    expectedEnum = (StdModelAttribute)2,
                    stdActual = model.GetAttributeValue<StdModelAttribute>(DefaultValuesForTest.Two);
                // Strict equality not Equals.
                Assert.IsTrue(expectedEnum == stdActual);
            }

            void subtest_AttributeSimplyPresent()
            {
                model.SetAttributeValue(nameof(StdModelAttribute.order), "42");
                int value = model.GetAttributeValue<int>(StdModelAttribute.order);
                Assert.AreEqual(42, value);
            }
            #endregion S U B T E S T S
        }
    }
    namespace LocalEnums
    {
        enum DefaultValuesForTest
        {
            [DefaultValue(2)]
            Two = 0x10000002, // This has got nothing to do with it!
        }
    }
}
