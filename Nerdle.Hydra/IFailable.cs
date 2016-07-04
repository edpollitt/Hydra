using System;

namespace Nerdle.Hydra
{
    public interface IFailable<out TComponent>
    {
        string Id { get; }
        void Execute(Action<TComponent> command);
        TResult Execute<TResult>(Func<TComponent, TResult> query);
        bool IsAvailable { get; }
    }
}