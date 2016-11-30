using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nerdle.Hydra.Extensions;

namespace Nerdle.Hydra
{
    public class StaticCluster<TComponent> : Cluster<TComponent>
    {
        public StaticCluster(IEnumerable<IFailable<TComponent>> components)
            : base(components)
        {}

        //static IEnumerable<TComponent> IndexOver(IList<TComponent> components)
        //{
        //    Enumerable.Range(0, components.Count - 1)
        //} 

        protected override TClusterResult ExecuteInternal<TClusterResult>(Func<IFailable<TComponent>, TClusterResult> operation)
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

            throw exceptions.ToClusterFailureException();
        }

        protected override async Task<TClusterResult> ExecuteInternalAsync<TClusterResult>(Func<IFailable<TComponent>, Task<TClusterResult>> operation)
        {
            var exceptions = new List<Exception>();

            // avoid eagerly enumerating components, as evaluating availability may be expensive (depending on availability heuristic in use)
            foreach (var component in Components.Where(c => c.IsAvailable))
            {
                try
                {
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