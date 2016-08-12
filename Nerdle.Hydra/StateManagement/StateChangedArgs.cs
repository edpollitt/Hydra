using System;

namespace Nerdle.Hydra.StateManagement
{
    public class StateChangedArgs : EventArgs
    {
        public State PreviousState { get; }
        public State CurrentState { get; }

        public Exception Exception { get; }

        public StateChangedArgs(State previousState, State currentState, Exception exception = null)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            Exception = exception;
        }
    }
}