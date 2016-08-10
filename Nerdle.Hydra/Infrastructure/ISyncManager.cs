using System;
using System.Threading;

namespace Nerdle.Hydra.Infrastructure
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