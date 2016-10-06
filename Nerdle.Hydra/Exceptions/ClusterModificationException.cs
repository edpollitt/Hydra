using System;
using System.Runtime.Serialization;

namespace Nerdle.Hydra.Exceptions
{
    [Serializable]
    public class ClusterModificationException : ClusterException
    {
        public ClusterModificationException()
        {
        }

        public ClusterModificationException(string message)
            : base(message)
        {
        }

        public ClusterModificationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ClusterModificationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}