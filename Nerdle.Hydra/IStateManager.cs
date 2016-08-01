using System;

namespace Nerdle.Hydra
{
    public interface IStateManager
    {
        State CurrentState { get; set; }
        void RegisterFailure(Exception exception);
        void RegisterSuccess();
    }
}