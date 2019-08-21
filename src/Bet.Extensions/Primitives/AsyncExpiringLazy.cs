using System.Threading.Tasks;

namespace System.Threading
{
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public class AsyncExpiringLazy<T>
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    {
        private readonly AsyncLock _valueLock = new AsyncLock();

        private readonly Func<AsyncExpirationValue<T>, Task<AsyncExpirationValue<T>>> _valueProvider;

        private AsyncExpirationValue<T> _value;

        public AsyncExpiringLazy(Func<AsyncExpirationValue<T>, Task<AsyncExpirationValue<T>>> vauleProvider)
        {
            _valueProvider = vauleProvider ?? throw new ArgumentNullException(nameof(vauleProvider));
        }

        private bool IsValueCreatedInternal => _value.Result != null && _value.ValidUntil > DateTimeOffset.UtcNow;

        public async Task<bool> IsValueCreated(CancellationToken cancellationToken = default)
        {
            using (await _valueLock.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                return IsValueCreatedInternal;
            }
        }

        public async Task<T> Value(CancellationToken cancellationToken = default)
        {
            using (await _valueLock.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                if (IsValueCreatedInternal)
                {
                    return _value.Result;
                }

                _value = await _valueProvider(_value).ConfigureAwait(false);

                return _value.Result;
            }
        }

        public async Task Invalidate(CancellationToken cancellationToken = default)
        {
            using (await _valueLock.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                _value = default;
            }
        }
    }
}
