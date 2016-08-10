using System;

namespace Nerdle.Hydra.InfrastructureAbstractions
{
    interface ISyncManager
    {
        void ReadOnly(Action synchronisedCommand);
        void UpgradeableRead(Action synchronisedCommand);
        void Write(Action synchronisedCommand);
        T ReadOnly<T>(Func<T> synchronisedQuery);
        T UpgradeableRead<T>(Func<T> synchronisedQuery);
        T Write<T>(Func<T> synchronisedQuery);
    }
}