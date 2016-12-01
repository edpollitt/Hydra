using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nerdle.Hydra.Extensions;

namespace Nerdle.Hydra
{
    public class StaticCluster<TComponent> : Cluster<TComponent>
    {   
        public StaticCluster(IEnumerable<IFailable<TComponent>> components, ITraversal clusterTraversal = null)
            : base(components, clusterTraversal)
        { }

        protected override TClusterResult ExecuteInternal<TClusterResult>(Func<IFailable<TComponent>, TClusterResult> operation)
        {
            var exceptions = new List<Exception>();

            // avoid eagerly enumerating components, as evaluating availability may be expensive (depending on availability heuristic in use)
            foreach (var component in ClusterTraversal.Traverse(Components))
            {
                try
                {
                    if (component.IsAvailable)
                        return operation(component);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            throw exceptions.ToClusterFailureException();
        }

        protected override async Task<TClusterResult> ExecuteInternalAsync<TClusterResult>(Func<IFailable<TComponent>, Task<TClusterResult>> operation)
        {
            var exceptions = new List<Exception>();

            // avoid eagerly enumerating components, as evaluating availability may be expensive (depending on availability heuristic in use)
            foreach (var component in ClusterTraversal.Traverse(Components))
            {
                try
                {
                    if (component.IsAvailable)
                        return await operation(component);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            throw exceptions.ToClusterFailureException();
        }
    }
}