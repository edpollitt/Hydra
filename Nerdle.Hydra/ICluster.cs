using System;
using System.Collections.Generic;

namespace Nerdle.Hydra
{
    public interface ICluster<out TComponent>
    {
        IEnumerable<string> ComponentIds { get; }
        ClusterResult Execute(Action<TComponent> command);
        ClusterResult<TResult> Execute<TResult>(Func<TComponent, TResult> query);

        event EventHandler<Exception> ComponentFailed;
        event EventHandler ComponentRecovered;
    }
}