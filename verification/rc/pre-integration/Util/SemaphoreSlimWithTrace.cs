
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Util
{
#if DEBUG
    // [Careful]
    // Hide the CLASS not the NAMESPACE in DEBUG
    public sealed class SemaphoreSlimWithTrace : SemaphoreSlim
    {
        public SemaphoreSlimWithTrace(int initialCount, int maxCount)
            : base(initialCount, maxCount) { }

        private static void Trace(string text)
        {
            Debug.WriteLine(text);
        }

        /* =======================
         * WAIT (SYNC)
         * ======================= */

        public new void Wait()
        {
            Trace("Id=260206.B Wait()");
            base.Wait();
        }

        public new bool Wait(int millisecondsTimeout)
        {
            Trace($"Id=260206.C Wait({millisecondsTimeout})");
            return base.Wait(millisecondsTimeout);
        }

        public new bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            Trace($"Id=260206.D Wait({millisecondsTimeout}, token)");
            return base.Wait(millisecondsTimeout, cancellationToken);
        }

        public new bool Wait(TimeSpan timeout)
        {
            Trace($"Id=260206.E Wait({timeout})");
            return base.Wait(timeout);
        }

        public new bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            Trace($"Id=260206.F Wait({timeout}, token)");
            return base.Wait(timeout, cancellationToken);
        }

        public new void Wait(CancellationToken cancellationToken)
        {
            Trace("Id=260206.G Wait(token)");
            base.Wait(cancellationToken);
        }

        /* =======================
         * WAIT (ASYNC)
         * ======================= */

        public new Task WaitAsync()
        {
            Trace("Id=260206.H WaitAsync()");
            return base.WaitAsync();
        }

        public new Task<bool> WaitAsync(int millisecondsTimeout)
        {
            Trace($"Id=260206.I WaitAsync({millisecondsTimeout})");
            return base.WaitAsync(millisecondsTimeout);
        }

        public new Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            Trace($"Id=260206.J WaitAsync({millisecondsTimeout}, token)");
            return base.WaitAsync(millisecondsTimeout, cancellationToken);
        }

        public new Task WaitAsync(CancellationToken cancellationToken)
        {
            Trace("Id=260206.K WaitAsync(token)");
            return base.WaitAsync(cancellationToken);
        }

        public new Task<bool> WaitAsync(TimeSpan timeout)
        {
            Trace($"Id=260206.L WaitAsync({timeout})");
            return base.WaitAsync(timeout);
        }

        public new Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            Trace($"Id=260206.M WaitAsync({timeout}, token)");
            return base.WaitAsync(timeout, cancellationToken);
        }

        /* =======================
         * RELEASE
         * ======================= */

        public new int Release()
        {
            Trace("Id=260206.N Release()");
            return base.Release();
        }

        public new int Release(int releaseCount)
        {
            Trace($"Id=260206.O Release({releaseCount})");
            return base.Release(releaseCount);
        }
    }
#endif
}
