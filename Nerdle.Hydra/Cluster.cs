using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nerdle.Hydra
{
    public abstract class Cluster<TComponent> : ICluster<TComponent>
    {
        protected readonly ITraversal ClusterTraversal;
        protected IList<IFailable<TComponent>> Components;

        public event EventHandler<Exception> ComponentFailed;
        public event EventHandler ComponentRecovered;
        
        protected Cluster(IEnumerable<IFailable<TComponent>> components, ITraversal clusterTraversal = null)
        {
            clusterTraversal = clusterTraversal ?? Traversal.Default;

            if (components == null)
                throw new ArgumentNullException(nameof(components));
            
            Components = components.ToList();

            ClusterTraversal = clusterTraversal;

            foreach (var component in Components)
                Register(component);
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

        public async Task<ClusterResult> ExecuteAsync(Func<TComponent, Task> command)
        {
            return await ExecuteInternalAsync(async component =>
            {
                await component.ExecuteAsync(command);
                return new ClusterResult(component.ComponentId);
            });
        }

        public async Task<ClusterResult<TResult>> ExecuteAsync<TResult>(Func<TComponent, Task<TResult>> query)
        {
            return await ExecuteInternalAsync(async component =>
            {
                var queryResult = await component.ExecuteAsync(query);
                return new ClusterResult<TResult>(component.ComponentId, queryResult);
            });
        }

        protected abstract TClusterResult ExecuteInternal<TClusterResult>(
            Func<IFailable<TComponent>, TClusterResult> operation) where TClusterResult : ClusterResult;

        protected abstract Task<TClusterResult> ExecuteInternalAsync<TClusterResult>(
            Func<IFailable<TComponent>, Task<TClusterResult>> operation) where TClusterResult : ClusterResult;
        
        protected void Register(IFailable component)
        {
            component.Failed += OnComponentFailed;
            component.Recovered += OnComponentRecovered;
        }

        protected void OnComponentFailed(object sender, Exception exception)
        {
            ComponentFailed?.Invoke(sender, exception);
        }

        protected void OnComponentRecovered(object sender, EventArgs eventArgs)
        {
            ComponentRecovered?.Invoke(sender, eventArgs);
        }

        public virtual IEnumerable<string> ComponentIds
        {
            get { return Components.Select(component => component.ComponentId).ToList(); }
        }
    }
}