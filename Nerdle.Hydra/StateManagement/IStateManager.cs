using System;

namespace Nerdle.Hydra.StateManagement
{
    public delegate void StateChangedHandler(object sender, StateChangedArgs args);

    public interface IStateManager
    {
        State CurrentState { get; }
        void RegisterError(Exception exception);
        void RegisterSuccess();

        event StateChangedHandler StateChanged;
    }
}

