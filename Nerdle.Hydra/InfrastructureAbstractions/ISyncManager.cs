using System;

namespace Nerdle.Hydra.InfrastructureAbstractions
{
    interface ISyncManager
    {
        void ReadOnly(Action command);
        void UpgradeableRead(Action command);
        void Write(Action command);
        T ReadOnly<T>(Func<T> query);
        T UpgradeableRead<T>(Func<T> query);
        T Write<T>(Func<T> query);
    }
}