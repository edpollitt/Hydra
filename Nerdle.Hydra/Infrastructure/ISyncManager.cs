using System;
using System.Threading;

namespace Nerdle.Hydra.Infrastructure
{
    interface ISyncManager
    {
        void ReadOnly(ReaderWriterLockSlim rwLock, Action synchronisedCommand);
        void UpgradeableRead(ReaderWriterLockSlim rwLock, Action synchronisedCommand);
        void Write(ReaderWriterLockSlim rwLock, Action synchronisedCommand);
        T ReadOnly<T>(ReaderWriterLockSlim rwLock, Func<T> synchronisedQuery);
        T UpgradeableRead<T>(ReaderWriterLockSlim rwLock, Func<T> synchronisedQuery);
        T Write<T>(ReaderWriterLockSlim rwLock, Func<T> synchronisedQuery);
    }
}