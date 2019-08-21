using System;
using System.Threading;

namespace Microsoft.Extensions.Primitives
{
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    /// <summary>
    /// Implements <see cref="IChangeToken"/>.
    /// </summary>
    public class ReloadToken : IChangeToken
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Indicates if this token will pro-actively raise callbacks. Callbacks are still guaranteed to be invoked, eventually.
        /// </summary>
        public bool ActiveChangeCallbacks => true;

        /// <summary>
        /// Gets a value that indicates if a change has occurred.
        /// </summary>
        public bool HasChanged => _cts.IsCancellationRequested;

        /// <summary>
        /// Registers for a callback that will be invoked when the entry has changed. <see cref="Microsoft.Extensions.Primitives.IChangeToken.HasChanged"/>
        /// MUST be set before the callback is invoked.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <param name="state">State to be passed into the callback.</param>
        /// <returns></returns>
        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            return _cts.Token.Register(callback, state);
        }

        /// <summary>
        /// Used to trigger the change token when a reload occurs.
        /// </summary>
        public void OnReload()
        {
            _cts.Cancel();
        }
    }
}
