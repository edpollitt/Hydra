using System;
using System.Collections.Generic;
using System.Linq;
using Nerdle.Hydra.Exceptions;

namespace Nerdle.Hydra
{
    public class Cluster<TComponent> : ICluster<TComponent>
    {
        readonly IEnumerable<IFailable<TComponent>> _components;

        public Cluster(IEnumerable<IFailable<TComponent>> components)
        {
            _components = components;
        }

        public ClusterResult Execute(Action<TComponent> command)
        {
            // avoid eagerly enumerating past the first working component, as testing availability may be expensive (depending on availability heuristic in use)
            var exceptions = new List<Exception>();

            foreach (var component in _components.Where(c => c.IsAvailable))
            {
                try
                {
                    component.Execute(command);
                    return new ClusterResult(component.Id);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            var exception = CreateClusterFailureException(exceptions);
            throw exception;
        }

        static ClusterFailureException CreateClusterFailureException(IReadOnlyCollection<Exception> exceptions)
        {
            return exceptions.Count > 0
                ? new ClusterFailureException(
                    "There are available components in the cluster, but the request was not successfully processed by any component.",
                    exceptions.Count == 1 ? exceptions.First() : new AggregateException(exceptions))
                : new ClusterFailureException(
                    "There are no currently available components in the cluster to process the request.");
        }

        public ClusterResult<TResult> Execute<TResult>(Func<TComponent, TResult> query)
        {
            throw new NotImplementedException();
        }
    }
}