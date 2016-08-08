using System;

namespace Nerdle.Hydra.StateManagement
{
    public class StateChangedArgs : EventArgs
    {
        public State PreviousState { get; }
        public State CurrentState { get; }

        public StateChangedArgs(State previousState, State currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }
}