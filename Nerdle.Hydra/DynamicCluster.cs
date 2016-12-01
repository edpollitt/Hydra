using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nerdle.Hydra.Exceptions;
using Nerdle.Hydra.Extensions;
using Nerdle.Hydra.InfrastructureAbstractions;

namespace Nerdle.Hydra
{
    public class DynamicCluster<TComponent> : Cluster<TComponent>, IDynamicCluster<TComponent>
    {
        readonly ISyncManager _syncManager;

        public DynamicCluster(IEnumerable<IFailable<TComponent>> components, ISyncManager syncManager, ITraversal clusterTraversal = null)
            : base(components, clusterTraversal)
        {
            if (syncManager == null)
                throw new ArgumentNullException(nameof(syncManager));

            _syncManager = syncManager;
        }

        protected override TClusterResult ExecuteInternal<TClusterResult>(Func<IFailable<TComponent>, TClusterResult> operation)
        {
            var exceptions = new List<Exception>();

            // clone the components list in a thread safe manner as we may be trying to modify 
            // the Components reference elsewhere 
            var listCopy = _syncManager.ReadOnly(() => Components.ToList());

            foreach (var component in ClusterTraversal.Traverse(listCopy))
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

            // clone the components list in a thread safe manner as we may be trying to modify 
            // the Components reference elsewhere 
            var listCopy = _syncManager.ReadOnly(() => Components.ToList());

            foreach (var component in ClusterTraversal.Traverse(listCopy))
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

        public void Add(IFailable<TComponent> newComponent, ComponentPosition position)
        {
            Register(newComponent);

            EditComponents(list =>
            {
                switch (position)
                {
                    case ComponentPosition.First:
                        list.Insert(0, newComponent);
                        break;
                    case ComponentPosition.Last:
                        list.Add(newComponent);
                        break;
                    default:
                        throw new ClusterModificationException($"DynamicCluster 'Add' behaviour is undefined for priority '{position}'. Component {newComponent.ComponentId} has not been added to cluster.");
                }
            });
        }

        public void Remove(IFailable<TComponent> oldComponent)
        {
            EditComponents(list =>
            {
                list.Remove(oldComponent);
            });

            Deregister(oldComponent);
        }

        public void Replace(IFailable<TComponent> oldComponent, IFailable<TComponent> newComponent)
        {
            EditComponents(list =>
            {
                var index = list.IndexOf(oldComponent);

                if (index == -1)
                    throw new ClusterModificationException($"Component {oldComponent.ComponentId} not found in cluster. New component {newComponent.ComponentId} has not been added to cluster.");

                Register(newComponent);
                list[index] = newComponent;
            });

            Deregister(oldComponent);
        }

        void EditComponents(Action<List<IFailable<TComponent>>> editAction)
        {
            var clonedComponents = _syncManager.ReadOnly(() => new List<IFailable<TComponent>>(Components));
            editAction(clonedComponents);
            _syncManager.Write(() => Components = clonedComponents);
        }

        void Deregister(IFailable component)
        {
            component.Failed -= OnComponentFailed;
            component.Recovered -= OnComponentRecovered;
        }

        public override IEnumerable<string> ComponentIds
        {
            get { return _syncManager.ReadOnly(() => base.ComponentIds); }
        }
    }
}