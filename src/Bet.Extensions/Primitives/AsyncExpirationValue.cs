namespace System.Threading
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    /// <summary>
    /// Used with <see cref="AsyncExpiringLazy{T}"/> for OAuth token retrieval.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct AsyncExpirationValue<T>
#pragma warning restore CA1815 // Override equals and operator equals on value types
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
