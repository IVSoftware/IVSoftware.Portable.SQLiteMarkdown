using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    namespace Internal
    {
        /// <summary>
        /// A lightweight dictionary whose indexer tolerates missing keys by returning null semantics instead of throwing.
        /// </summary>
        /// <remarks>
        /// This internal helper mirrors the tolerant read behavior of the canonical
        /// <c>TolerantDictionary</c> hosted in <c>IVSoftware.Portable.Collections</c>,
        /// but without introducing a dependency on that package.
        ///
        /// Accessing a key that does not exist returns <c>default</c> through a nullable
        /// projection (<c>TValue?</c>) rather than throwing <see cref="KeyNotFoundException"/>.
        /// The underlying storage remains a normal <see cref="Dictionary{TKey,TValue}"/>; only
        /// the indexer access semantics are relaxed.
        ///
        /// When <typeparamref name="TValue"/> is a value type, the indexer surface exposes it
        /// as <c>Nullable&lt;TValue&gt;</c>, allowing callers to distinguish between "key not present"
        /// (<c>null</c>) and an actual stored value.
        ///
        /// Assigning <c>null</c> is permitted only when <typeparamref name="TValue"/> itself
        /// supports null (reference types or nullable value types). If <typeparamref name="TValue"/>
        /// is a non-nullable value type, attempting to assign <c>null</c> triggers a hard exception,
        /// since the underlying dictionary cannot represent that state.
        ///
        /// This implementation *intentionally omits* the observable semantics of the canonical
        /// version (such as CollectionChanging). It exists solely as a minimal internal
        /// utility for tolerant lookup behavior in libraries that must remain dependency-light.
        /// </remarks>
        class TolerantDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        {
            [Indexer]
            public new TValue? this[TKey key]
            {
                get
                {
                    if (!TryGetValue(key, out TValue value))
                    {
                        return default!;
                    }
                    return value;
                }
                /// <summary>
                /// Throws when a null assignment is attempted for a dictionary whose TValue is a non-nullable value type.
                /// </summary>
                /// <remarks>
                /// The tolerant indexer allows missing keys to return null semantics via <c>TValue?</c>.
                /// However, if <typeparamref name="TValue"/> is a non-nullable value type (e.g. <c>int</c>, <c>bool</c>, <c>DateTime</c>),
                /// assigning <c>null</c> cannot be represented in the underlying <see cref="Dictionary{TKey,TValue}"/> storage.
                /// This guard prevents that invalid assignment and signals the mismatch between the nullable indexer contract
                /// and the concrete storage type.
                /// </remarks>
                set
                {
                    if (value is null
                        && typeof(TValue).IsValueType
                        && Nullable.GetUnderlyingType(typeof(TValue)) is null)
                    {
                        this.ThrowHard<InvalidOperationException>(
                            $"Cannot assign null to {nameof(TolerantDictionary<TKey, TValue>)} " +
                            $"because {nameof(TValue)} ({typeof(TValue).Name}) is a non-nullable value type.");
                        // This must return, even if the Throw is deescalated.
                        return;
                    }

                    if (TryGetValue(key, out TValue current))
                    {
                        if (!Equals(current, value))
                        {
                            var e = new CollectionChangingEventArgs<TKey, TValue>(key, current, value);
                            CollectionChanging?.Invoke(this, e);
                            if (e.Cancel) return;
                            base[key] = value!;
                        }
                    }
                    else
                    {
                        var e = new CollectionChangingEventArgs<TKey, TValue>(key, default, value);
                        CollectionChanging?.Invoke(this, e);
                        if (e.Cancel) return;
                        base[key] = value!;
                    }
                }
            }
            public event EventHandler<CollectionChangingEventArgs<TKey, TValue>>? CollectionChanging;
        }

        class CollectionChangingEventArgs<TKey, TValue> : CancelEventArgs
        {
            public CollectionChangingEventArgs(TKey key, TValue? oldValue, TValue? newValue)
            {
                Key = key;
                OldValue = oldValue;
                NewValue = newValue;
            }

            public TKey Key { get; }
            public TValue? OldValue { get; }
            public TValue? NewValue { get; }
        }
    }
}
