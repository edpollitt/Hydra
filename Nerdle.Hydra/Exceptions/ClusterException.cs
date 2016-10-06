using System;
using System.Runtime.Serialization;

namespace Nerdle.Hydra.Exceptions
{
    [Serializable]
    public class ClusterException : Exception
    {
        public ClusterException()
        {
        }

        public ClusterException(string message)
            : base(message)
        {
        }

        public ClusterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ClusterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}