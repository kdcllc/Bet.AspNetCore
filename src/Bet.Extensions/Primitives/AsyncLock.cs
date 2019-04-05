using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions
{
    // Code based on http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx

    /// <summary>
    /// Used as an asynchronous semaphore for internal Event Hubs operations.
    /// </summary>
    public class AsyncLock : IDisposable
    {
        private readonly SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);
        private readonly Task<LockRelease> lockRelease;
        private bool disposed;

        /// <summary>
        /// Returns a new AsyncLock.
        /// </summary>
        public AsyncLock()
        {
            lockRelease = Task.FromResult(new LockRelease(this));
        }

        /// <summary>
        /// Sets a lock.
        /// </summary>
        /// <returns>An asynchronous operation</returns>
        public Task<LockRelease> LockAsync()
        {
            return LockAsync(CancellationToken.None);
        }

        /// <summary>
        /// Sets a lock, which allows for cancellation, using a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> which can be used to cancel the lock</param>
        /// <returns>An asynchronous operation</returns>
        public Task<LockRelease> LockAsync(CancellationToken cancellationToken)
        {
            var waitTask = asyncSemaphore.WaitAsync(cancellationToken);
            if (waitTask.IsCompleted)
            {
                // Avoid an allocation in the non-contention case.
                return lockRelease;
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
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    asyncSemaphore.Dispose();
                    (lockRelease as IDisposable)?.Dispose();
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Used coordinate lock releases.
        /// </summary>
        public struct LockRelease : IDisposable
        {
            private readonly AsyncLock asyncLockRelease;

            internal LockRelease(AsyncLock release)
            {
                asyncLockRelease = release;
            }

            /// <summary>
            /// Closes and releases resources associated with <see cref="LockRelease"/>.
            /// </summary>
            /// <returns>An asynchronous operation</returns>
            public void Dispose()
            {
                asyncLockRelease?.asyncSemaphore.Release();
            }
        }
    }
}
