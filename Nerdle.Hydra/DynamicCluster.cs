using System;
using System.Collections.Generic;
using System.Linq;
using Nerdle.Hydra.Exceptions;
using Nerdle.Hydra.InfrastructureAbstractions;

namespace Nerdle.Hydra
{
    public class DynamicCluster<TComponent> : Cluster<TComponent>, IDynamicCluster<TComponent>
    {
        readonly ISyncManager _syncManager;

        public DynamicCluster(IEnumerable<IFailable<TComponent>> components, ISyncManager syncManager)
            : base(components)
        {
            _syncManager = syncManager;
        }

        protected override TClusterResult ExecuteInternal<TClusterResult>(Func<IFailable<TComponent>, TClusterResult> operation)
        {
            var exceptions = new List<Exception>();

            // obtain an Enumerator instance in a thread safe manner as we may be trying to modify 
            // the Components reference elsewhere 
            using (var enumerator = _syncManager.ReadOnly(() => Components.GetEnumerator()))
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.IsAvailable)
                    {
                        try
                        {
                            return operation(enumerator.Current);
                        }
                        catch (Exception e)
                        {
                            exceptions.Add(e);
                        }
                    }
                }
            }

            throw exceptions.Count > 0 ? new ClusterFailureException("There are available components in the cluster, but the request was not successfully processed by any component.", exceptions.Count == 1 ? exceptions.First() : new AggregateException(exceptions)) : new ClusterFailureException("There are no currently available components in the cluster to process the request.");
        }

        public void Add(IFailable<TComponent> newComponent, ComponentPriority priority)
        {
            Register(newComponent);

            EditComponents(list =>
            {
                switch (priority)
                {
                    case ComponentPriority.First:
                        list.Insert(0, newComponent);
                        break;
                    case ComponentPriority.Last:
                        list.Add(newComponent);
                        break;
                    default:
                        throw new ClusterModificationException($"DynamicCluster 'Add' behaviour is undefined for priority '{priority}'. Component {newComponent.ComponentId} has not been added to cluster.");
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