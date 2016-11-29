using System;
using System.Collections.Generic;
using System.Linq;
using Nerdle.Hydra.Exceptions;

namespace Nerdle.Hydra.Extensions
{
    static class CollectionExtensions
    {
        public static ClusterFailureException ToClusterFailureException(this IReadOnlyCollection<Exception> componentExceptions)
        {
            return componentExceptions.Count > 0
                ? new ClusterFailureException("There are available components in the cluster, but the request was not successfully processed by any component.", componentExceptions.Count == 1 ? componentExceptions.First() : new AggregateException(componentExceptions))
                : new ClusterFailureException("There are no currently available components in the cluster to process the request.");
        }
    }
}
