namespace Bet.Extensions.Hosting.Abstractions
{
    /// <summary>
    /// Defines the ways errors are handle by <see cref="ITimedHostedService"/>.
    /// </summary>
    public enum FailMode
    {
        /// <summary>
        /// Throw any exceptions out of the service's context,
        /// thus causing an unhandled exception that will crash the application if not handled elsewhere.
        /// </summary>
        Unhandled,

        /// <summary>
        /// Log exceptions and continue normal operation.
        /// </summary>
        LogAndContinue,

        /// <summary>
        /// Log exceptions and retry sooner than normal.
        /// </summary>
        LogAndRetry
    }
}
