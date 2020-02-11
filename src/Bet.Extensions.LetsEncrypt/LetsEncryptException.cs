using System;
using System.Runtime.Serialization;

namespace Bet.Extensions.LetsEncrypt
{
    public class LetsEncryptException : Exception
    {
        public LetsEncryptException()
        {
        }

        public LetsEncryptException(string message) : base(message)
        {
        }

        public LetsEncryptException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LetsEncryptException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
