using System;

namespace Nerdle.Hydra.InfrastructureAbstractions
{
    public interface ISyncManager
    {
        void ReadOnly(Action command, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw);
        void UpgradeableRead(Action command, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw);
        void Write(Action command, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw);
        T ReadOnly<T>(Func<T> query, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw);
        T UpgradeableRead<T>(Func<T> query, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw);
        T Write<T>(Func<T> query, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw);
    }
}