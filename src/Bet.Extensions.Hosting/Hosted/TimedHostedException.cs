using System;
using System.Runtime.Serialization;

namespace Bet.Extensions.Hosting.Hosted
{
    public class TimedHostedException : Exception
    {
        public TimedHostedException()
        {
        }

        public TimedHostedException(string message) : base(message)
        {
        }

        public TimedHostedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TimedHostedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
