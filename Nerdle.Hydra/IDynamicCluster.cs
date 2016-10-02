namespace Nerdle.Hydra
{
    interface IDynamicCluster<TComponent> : ICluster<TComponent>
    {
        void Add(IFailable<TComponent> newComponent, ComponentPriority priority);
        void Remove(IFailable<TComponent> oldComponent);
        void Replace(IFailable<TComponent> oldComponent, IFailable<TComponent> newComponent);
    }
}