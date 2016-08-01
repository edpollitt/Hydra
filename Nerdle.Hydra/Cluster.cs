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
            return ExecuteInternal(component =>
            {
                component.Execute(command);
                return new ClusterResult(component.ComponentId);
            });
        }

        public ClusterResult<TResult> Execute<TResult>(Func<TComponent, TResult> query)
        {
            return ExecuteInternal(component =>
            {
                var queryResult = component.Execute(query);
                return new ClusterResult<TResult>(component.ComponentId, queryResult);
            });
        }

        TClusterResult ExecuteInternal<TClusterResult>(Func<IFailable<TComponent>, TClusterResult> operation) where TClusterResult : ClusterResult
        {
            var exceptions = new List<Exception>();

            // avoid eagerly enumerating components, as evaluating availability may be expensive (depending on availability heuristic in use)
            foreach (var component in _components.Where(c => c.IsAvailable))
            {
                try
                {
                    return operation(component);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            throw exceptions.Count > 0
                ? new ClusterFailureException(
                    "There are available components in the cluster, but the request was not successfully processed by any component.",
                    exceptions.Count == 1 ? exceptions.First() : new AggregateException(exceptions))
                : new ClusterFailureException(
                    "There are no currently available components in the cluster to process the request.");
        }
    }
}