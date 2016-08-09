using System;
using System.Collections.Generic;
using System.Threading;
using Nerdle.Hydra.StateManagement;

namespace Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowAveragingStateManagerTests.Helpers
{
    class CountingSyncManagerProxy : ISyncManager
    {
        readonly ISyncManager _internalSyncManager;

        public IDictionary<ReaderWriterLockSlim, int> ReadOnlyLocks { get; }

        public IDictionary<ReaderWriterLockSlim, int> UpgradeableLocks { get; }

        public IDictionary<ReaderWriterLockSlim, int> WriteLocks { get; }

        public CountingSyncManagerProxy(ISyncManager internalSyncManager)
        {
            ReadOnlyLocks = new Dictionary<ReaderWriterLockSlim, int>();
            UpgradeableLocks = new Dictionary<ReaderWriterLockSlim, int>();
            WriteLocks = new Dictionary<ReaderWriterLockSlim, int>();

            _internalSyncManager = internalSyncManager;
        }

        public void ReadOnly(ReaderWriterLockSlim rwLock, Action synchronisedCommand)
        {
            Increment(ReadOnlyLocks, rwLock);
            _internalSyncManager.ReadOnly(rwLock, synchronisedCommand);
        }

        public void UpgradeableRead(ReaderWriterLockSlim rwLock, Action synchronisedCommand)
        {
            Increment(UpgradeableLocks, rwLock);
            _internalSyncManager.UpgradeableRead(rwLock, synchronisedCommand);
        }

        public void Write(ReaderWriterLockSlim rwLock, Action synchronisedCommand)
        {
            Increment(WriteLocks, rwLock);
            _internalSyncManager.Write(rwLock, synchronisedCommand);
        }

        public T ReadOnly<T>(ReaderWriterLockSlim rwLock, Func<T> synchronisedQuery)
        {
            Increment(ReadOnlyLocks, rwLock);
            return _internalSyncManager.ReadOnly(rwLock, synchronisedQuery);
        }

        public T UpgradeableRead<T>(ReaderWriterLockSlim rwLock, Func<T> synchronisedQuery)
        {
            Increment(UpgradeableLocks, rwLock);
            return _internalSyncManager.UpgradeableRead(rwLock, synchronisedQuery);
        }

        public T Write<T>(ReaderWriterLockSlim rwLock, Func<T> synchronisedQuery)
        {
            Increment(WriteLocks, rwLock);
            return _internalSyncManager.Write(rwLock, synchronisedQuery);
        }

        static void Increment(IDictionary<ReaderWriterLockSlim, int> lockType, ReaderWriterLockSlim rwLock)
        {
            if (lockType.ContainsKey(rwLock))
                lockType[rwLock]++;
            else
                lockType[rwLock] = 1;
        }
    }
}