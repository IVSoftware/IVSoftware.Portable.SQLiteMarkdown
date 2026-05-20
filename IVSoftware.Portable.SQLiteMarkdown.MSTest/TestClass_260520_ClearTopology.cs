using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Collections;
using IgnoreAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute;


namespace IVSoftware.Portable.SQLiteMarkdown.MSTest
{
    [TestClass]
    public class TestClass_260520_ClearTopology
    {
        /// <summary>
        /// Verifies inherited clearable MDCs advise on missing explicit dual clear.
        /// </summary>
        /// <remarks>
        /// This guard is constructor-time only, so type shape is sufficient.
        /// </remarks>
        [TestMethod, DoNotParallelize] // * because of static Throw
        public void TestMethod_ClearTopologyGuard()
        {
            string actual, expected;
            List<string> builderThrow = new();

            #region L o c a l F x
            using var local = this.WithOnDispose(
                onInit: (sender, e) =>
                {
                    Throw.BeginThrowOrAdvise += localOnBeginThrowOrAdvise;
                },
                onDispose: (sender, e) =>
                {
                    Throw.BeginThrowOrAdvise -= localOnBeginThrowOrAdvise;
                });
            void localOnBeginThrowOrAdvise(object? sender, Throw e)
            {
                var msg = $"{e.GetType().Name} {e.FormattedMessage}";
                builderThrow.Add(msg);
                e.Handled = true;
            }
            #endregion L o c a l F x

            _ = new InheritMDCwithIList<SelectableQFModel>();

            actual = string.Join(Environment.NewLine, builderThrow);
            actual.ToClipboardExpected();
            { }
            expected = @" 
Throw MarkdownContextPolicy.ExplicitClearAdvisory | ExplicitClearAdvisory Policy advisory:
- Inherited MarkdownContext detected, but no parameterless Clear() was found.
- Clear(bool all = false) participates in the MDC filtering state machine and may not
  immediately empty the collection. 
- If your callers expect IList-style behavior, consider implementing Clear() => Clear(true)
  to provide a deterministic terminal clear. You may also expose Clear(bool all) without a 
  default parameter to make the stateful semantics explicit."
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                $"Expecting {nameof(InheritMDCwithIList<SelectableQFModel>)} advises on missing explicit dual-clear surface."
            );
        }
    }

    /// <summary>
    /// Uses routing for the net projection.
    /// </summary>
    class InheritMDCwithIList<T>
        : MarkdownContext<T>
        , IList<T>
        where T : new()
    {
        List<T> @base = new();

        public T this[int index] { get => ((IList<T>)@base)[index]; set => ((IList<T>)@base)[index] = value; }

        public int Count => ((ICollection<T>)@base).Count;

        public bool IsReadOnly => ((ICollection<T>)@base).IsReadOnly;

        public void Add(T item)
        {
            ((ICollection<T>)@base).Add(item);
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)@base).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)@base).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)@base).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)@base).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)@base).Insert(index, item);
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)@base).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)@base).RemoveAt(index);
        }

        void ICollection<T>.Clear() => @base.Clear();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)@base).GetEnumerator();
        }
    }
}
