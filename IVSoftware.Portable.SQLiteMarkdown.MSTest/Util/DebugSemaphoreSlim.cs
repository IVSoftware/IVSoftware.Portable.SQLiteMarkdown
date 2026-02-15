using System.Diagnostics;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Util
{
    internal sealed class DebugSemaphoreSlim : IDisposable
    {
        private readonly SemaphoreSlim _inner;
        private readonly string _name;

        public DebugSemaphoreSlim(int initialCount, int maxCount, string name = null)
        {
            _inner = new SemaphoreSlim(initialCount, maxCount);
            _name = name ?? $"SemaphoreSlim#{GetHashCode():X}";
        }

        public int CurrentCount => _inner.CurrentCount;

        public WaitHandle AvailableWaitHandle => _inner.AvailableWaitHandle;

        #region WAIT (SYNC)

        public void Wait()
        {
            Trace("Wait()");
            _inner.Wait();
            Trace("Wait() acquired");
        }

        public bool Wait(int millisecondsTimeout)
        {
            Trace($"Wait({millisecondsTimeout})");
            var result = _inner.Wait(millisecondsTimeout);
            Trace($"Wait({millisecondsTimeout}) => {result}");
            return result;
        }

        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            Trace($"Wait({millisecondsTimeout}, token)");
            var result = _inner.Wait(millisecondsTimeout, cancellationToken);
            Trace($"Wait({millisecondsTimeout}, token) => {result}");
            return result;
        }

        public bool Wait(TimeSpan timeout)
        {
            Trace($"Wait({timeout})");
            var result = _inner.Wait(timeout);
            Trace($"Wait({timeout}) => {result}");
            return result;
        }

        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            Trace($"Wait({timeout}, token)");
            var result = _inner.Wait(timeout, cancellationToken);
            Trace($"Wait({timeout}, token) => {result}");
            return result;
        }

        public void Wait(CancellationToken cancellationToken)
        {
            Trace("Wait(token)");
            _inner.Wait(cancellationToken);
            Trace("Wait(token) acquired");
        }

        #endregion

        #region WAITASYNC

        public async Task WaitAsync()
        {
            Trace("WaitAsync()");
            await _inner.WaitAsync().ConfigureAwait(false);
            Trace("WaitAsync() acquired");
        }

        public async Task<bool> WaitAsync(int millisecondsTimeout)
        {
            Trace($"WaitAsync({millisecondsTimeout})");
            var result = await _inner.WaitAsync(millisecondsTimeout).ConfigureAwait(false);
            Trace($"WaitAsync({millisecondsTimeout}) => {result}");
            return result;
        }

        public async Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            Trace($"WaitAsync({millisecondsTimeout}, token)");
            var result = await _inner.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(false);
            Trace($"WaitAsync({millisecondsTimeout}, token) => {result}");
            return result;
        }

        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            Trace("WaitAsync(token)");
            await _inner.WaitAsync(cancellationToken).ConfigureAwait(false);
            Trace("WaitAsync(token) acquired");
        }

        public async Task<bool> WaitAsync(TimeSpan timeout)
        {
            Trace($"WaitAsync({timeout})");
            var result = await _inner.WaitAsync(timeout).ConfigureAwait(false);
            Trace($"WaitAsync({timeout}) => {result}");
            return result;
        }

        public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            Trace($"WaitAsync({timeout}, token)");
            var result = await _inner.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
            Trace($"WaitAsync({timeout}, token) => {result}");
            return result;
        }

        #endregion

        #region RELEASE

        public int Release()
        {
            Trace("Release()");
            var result = _inner.Release();
            Trace($"Release() => {result}");
            return result;
        }

        public int Release(int releaseCount)
        {
            Trace($"Release({releaseCount})");
            var result = _inner.Release(releaseCount);
            Trace($"Release({releaseCount}) => {result}");
            return result;
        }

        #endregion

        private void Trace(string message)
        {
            Debug.WriteLine($"[{_name}] {message} (count={_inner.CurrentCount})");
        }

        public void Dispose()
        {
            Trace("Dispose()");
            _inner.Dispose();
        }
    }

}
