using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nerdle.Hydra
{
    public interface ICluster<out TComponent>
    {
        IEnumerable<string> ComponentIds { get; }
        ClusterResult Execute(Action<TComponent> command);
        ClusterResult<TResult> Execute<TResult>(Func<TComponent, TResult> query);
        Task<ClusterResult> ExecuteAsync(Func<TComponent, Task> command);
        Task<ClusterResult<TResult>> ExecuteAsync<TResult>(Func<TComponent, Task<TResult>> query);

        event EventHandler<Exception> ComponentFailed;
        event EventHandler ComponentRecovered;
    }
}