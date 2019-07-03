using System;
using System.Runtime.Serialization;

namespace Bet.AspNetCore.LetsEncrypt.Abstractions
{
    public class AcmeResponseException : Exception
    {
        public AcmeResponseException() : base()
        {
        }

        public AcmeResponseException(string message) : base(message)
        {
        }

        public AcmeResponseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AcmeResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
