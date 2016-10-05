using System;
using System.Collections.Generic;
using System.Linq;
using Nerdle.Hydra.Exceptions;

namespace Nerdle.Hydra
{
    public class Cluster<TComponent> : ICluster<TComponent>
    {
        protected readonly List<IFailable<TComponent>> Components;

        public event EventHandler<Exception> ComponentFailed;
        public event EventHandler ComponentRecovered;

        public Cluster(IEnumerable<IFailable<TComponent>> components)
        {
            Components = components.ToList();

            Components.ForEach(component =>
            {
                component.Failed += OnComponentFailed;
                component.Recovered += OnComponentRecovered;
            });
        }

        public virtual ClusterResult Execute(Action<TComponent> command)
        {
            return ExecuteInternal(component =>
            {
                component.Execute(command);
                return new ClusterResult(component.ComponentId);
            });
        }

        public virtual ClusterResult<TResult> Execute<TResult>(Func<TComponent, TResult> query)
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
            foreach (var component in Components.Where(c => c.IsAvailable))
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

            throw exceptions.Count > 0 ? new ClusterFailureException("There are available components in the cluster, but the request was not successfully processed by any component.", exceptions.Count == 1 ? exceptions.First() : new AggregateException(exceptions)) : new ClusterFailureException("There are no currently available components in the cluster to process the request.");
        }

        void OnComponentFailed(object sender, Exception exception)
        {
            ComponentFailed?.Invoke(sender, exception);
        }

        void OnComponentRecovered(object sender, EventArgs eventArgs)
        {
            ComponentRecovered?.Invoke(sender, eventArgs);
        }
    }
}