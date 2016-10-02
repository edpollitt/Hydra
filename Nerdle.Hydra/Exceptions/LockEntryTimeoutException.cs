using System;
using System.Runtime.Serialization;

namespace Nerdle.Hydra.Exceptions
{
    [Serializable]
    public class LockEntryTimeoutException : Exception
    {
        public LockEntryTimeoutException()
        {
        }

        public LockEntryTimeoutException(string message)
            : base(message)
        {
        }

        public LockEntryTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public LockEntryTimeoutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}