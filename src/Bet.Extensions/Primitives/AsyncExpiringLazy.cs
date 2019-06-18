using System.Threading.Tasks;

namespace System.Threading
{
    public class AsyncExpiringLazy<T>
    {
        private readonly AsyncLock _valueLock = new AsyncLock();

        private readonly Func<AsyncExpirationValue<T>, Task<AsyncExpirationValue<T>>> _valueProvider;

        private AsyncExpirationValue<T> _value;

        private bool IsValueCreatedInternal => _value.Result != null && _value.ValidUntil > DateTimeOffset.UtcNow;

        public AsyncExpiringLazy(Func<AsyncExpirationValue<T>, Task<AsyncExpirationValue<T>>> vauleProvider)
        {
            _valueProvider = vauleProvider ?? throw new ArgumentNullException(nameof(vauleProvider));
        }

        public async Task<bool> IsValueCreated()
        {
            using (await _valueLock.LockAsync().ConfigureAwait(false))
            {
               return IsValueCreatedInternal;
            }
        }

        public async Task<T> Value()
        {
            using(await _valueLock.LockAsync().ConfigureAwait(false))
            {
                if (IsValueCreatedInternal)
                {
                    return _value.Result;
                }

                _value = await _valueProvider(_value).ConfigureAwait(false);

                return _value.Result;
            }
        }

        public async Task Invalidate()
        {
            using(await _valueLock.LockAsync().ConfigureAwait(false))
            {
                _value = default;
            }
        }
    }
}
