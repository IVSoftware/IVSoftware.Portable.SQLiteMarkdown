using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Common
{
    /// <summary>
    /// Aggregates multiple disposable tokens into a single scope that disposes them in reverse acquisition order.
    /// </summary>
    /// <remarks>
    /// Intended for situations where several conditional capabilities (e.g. epoch reset,
    /// collection-change authority) may be acquired before entering an execution window.
    ///
    /// The constructor accepts a variadic list of tokens and filters out null values so
    /// callers can supply conditional expressions without branching. Disposal unwinds the
    /// tokens in LIFO order, preserving the expected semantics of nested scopes.
    ///
    /// This class is typically used with a single <c>using</c> declaration to create a
    /// composite lifetime boundary around an operation such as FSM execution.
    /// </remarks>
    sealed class TokenDisposer : IDisposable
    {
        readonly IDisposable?[] _tokens;
        bool _disposed;

        public TokenDisposer(params IDisposable?[] tokens)
            => _tokens = tokens.OfType<IDisposable>().ToArray();

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            for (int i = _tokens.Length - 1; i >= 0; i--)
            {
                _tokens[i]?.Dispose();
            }
        }
    }
}
