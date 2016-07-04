using System;
using System.Collections.Generic;
using System.Linq;

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

            throw exceptions.Count == 1 ? exceptions.First() : new AggregateException(exceptions);
        }

        public ClusterResult<TResult> Execute<TResult>(Func<TComponent, TResult> query)
        {
            throw new NotImplementedException();
        }
    }
}