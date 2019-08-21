using System.Threading.Tasks;

namespace System.Threading
{
    // Code based on http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx

    /// <summary>
    /// Used as an asynchronous semaphore for internal Event Hubs operations.
    /// </summary>
    public class AsyncLock : IDisposable
    {
        private readonly SemaphoreSlim _asyncSemaphore = new SemaphoreSlim(1);
        private readonly Task<LockRelease> _lockRelease;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock"/> class.
        /// Returns a new AsyncLock.
        /// </summary>
        public AsyncLock()
        {
            _lockRelease = Task.FromResult(new LockRelease(this));
        }

        /// <summary>
        /// Sets a lock, which allows for cancellation, using a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> which can be used to cancel the lock.</param>
        /// <returns>An asynchronous operation.</returns>
        public Task<LockRelease> LockAsync(CancellationToken cancellationToken = default)
        {
            var waitTask = _asyncSemaphore.WaitAsync(cancellationToken);
            if (waitTask.IsCompleted)
            {
                // Avoid an allocation in the non-contention case.
                return _lockRelease;
            }

            return waitTask.ContinueWith(
                (_, state) => new LockRelease((AsyncLock)state),
                this,
                cancellationToken,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        /// <summary>
        /// Closes and releases any resources associated with the AsyncLock.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _asyncSemaphore.Dispose();
                    (_lockRelease as IDisposable)?.Dispose();
                }

                _disposed = true;
            }
        }

#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA1034 // Nested types should not be visible
        /// <summary>
        /// Used coordinate lock releases.
        /// </summary>
        public struct LockRelease : IDisposable
#pragma warning restore CA1034 // Nested types should not be visible
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            private readonly AsyncLock _asyncLockRelease;

            internal LockRelease(AsyncLock release)
            {
                _asyncLockRelease = release;
            }

            /// <summary>
            /// Closes and releases resources associated with <see cref="LockRelease"/>.
            /// </summary>
            public void Dispose()
            {
                _asyncLockRelease?._asyncSemaphore.Release();
            }
        }
    }
}
