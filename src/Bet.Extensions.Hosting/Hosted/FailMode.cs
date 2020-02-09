using System.Runtime.Serialization;

namespace Bet.Extensions.Hosting.Hosted
{
    public enum FailMode
    {
        /// <summary>
        /// Throw any exceptions out of the service's context,
        /// thus causing an unhandled exception that will crash the application if not handled elsewhere.
        /// </summary>
        [EnumMember(Value = "Unhandled")]
        Unhandled,

        /// <summary>
        /// Log exceptions and continue normal operation.
        /// </summary>
        [EnumMember(Value = "LogAndContinue")]
        LogAndContinue,

        /// <summary>
        /// Log exceptions and retry sooner than normal.
        /// </summary>
        [EnumMember(Value = "LogAndRetry")]
        LogAndRetry
    }
}
