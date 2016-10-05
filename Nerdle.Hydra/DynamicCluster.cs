using System;
using System.Collections.Generic;
using System.Linq;
using Nerdle.Hydra.InfrastructureAbstractions;

namespace Nerdle.Hydra
{
    public class DynamicCluster<TComponent> : Cluster<TComponent>, IDynamicCluster<TComponent>
    {
        readonly ISyncManager _syncManager;

        public DynamicCluster(IEnumerable<IFailable<TComponent>> components, ISyncManager syncManager) : base(components)
        {
            _syncManager = syncManager;
        }

        public override ClusterResult Execute(Action<TComponent> command)
        {
            return _syncManager.ReadOnly(() => base.Execute(command));
        }

        public override ClusterResult<TResult> Execute<TResult>(Func<TComponent, TResult> query)
        {
            return _syncManager.ReadOnly(() => base.Execute(query));
        }

        public void Add(IFailable<TComponent> newComponent, ComponentPriority priority)
        {
            switch (priority)
            {
                case ComponentPriority.First:
                    _syncManager.Write(() => Components.Insert(0, newComponent));
                    break;
                case ComponentPriority.Last:
                    _syncManager.Write(() => Components.Add(newComponent));
                    break;
                default:
                    throw new InvalidOperationException($"DynamicCluster 'Add' behaviour is undefined for priority '{priority}'");
            }
        }

        public void Remove(IFailable<TComponent> oldComponent)
        {
            _syncManager.Write(() => Components.Remove(oldComponent));
        }

        public void Replace(IFailable<TComponent> oldComponent, IFailable<TComponent> newComponent)
        {
            _syncManager.Write(() =>
            {
                var index = Components.IndexOf(oldComponent);
                if (index != -1)
                    Components[index] = newComponent;
            });
        }

        public IEnumerable<string> ComponentList
        {
            get { return _syncManager.ReadOnly(() => Components.Select(component => component.ComponentId).ToList()); }
        } 
    }
}