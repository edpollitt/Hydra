using System;
using System.Runtime.Serialization;

namespace Nerdle.Hydra.Exceptions
{
    [Serializable]
    public class ClusterFailureException : ClusterException
    {
        public ClusterFailureException()
        {
        }

        public ClusterFailureException(string message)
            : base(message)
        {
        }

        public ClusterFailureException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ClusterFailureException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}