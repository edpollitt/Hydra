using System;

namespace Nerdle.Hydra
{
    public interface ICluster<out TComponent>
    {
        ClusterResult Execute(Action<TComponent> command);
        ClusterResult<TResult> Execute<TResult>(Func<TComponent, TResult> query);
    }
}