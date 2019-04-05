using System;

namespace Bet.Extensions
{
    /// <summary>
    /// Used with <see cref="AsyncExpiringLazy{T}"/> for OAuth token retrieval.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct AsyncExpirationValue<T>
    {
        /// <summary>
        /// Result of the AsyncLazy Operation.
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Validate the OAuth Token.
        /// </summary>
        public DateTimeOffset ValidUntil { get; set; }
    }
}
