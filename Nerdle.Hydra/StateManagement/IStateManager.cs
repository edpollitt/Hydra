using System;

namespace Nerdle.Hydra.StateManagement
{
    public interface IStateManager
    {
        State CurrentState { get; }
        void RegisterSuccess();
        void RegisterFailure(Exception exception);

        event EventHandler<StateChangedArgs> StateChanged;
    }
}

