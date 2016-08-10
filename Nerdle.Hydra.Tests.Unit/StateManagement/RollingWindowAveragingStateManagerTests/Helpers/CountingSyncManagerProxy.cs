using System;
using System.Threading;
using Nerdle.Hydra.Infrastructure;

namespace Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowAveragingStateManagerTests.Helpers
{
    class CountingSyncManagerProxy : ISyncManager
    {
        readonly ISyncManager _internalSyncManager;

        int _readOnlyLocks;
        int _upgradeableLocks;
        int _writeLocks;

        public int ReadOnlyLocksTaken => _readOnlyLocks;
        public int UpgradeableLocksTaken => _upgradeableLocks;
        public int WriteLocksTaken => _writeLocks;

        public CountingSyncManagerProxy(ISyncManager internalSyncManager)
        {
            _internalSyncManager = internalSyncManager;
        }

        public void ReadOnly(Action synchronisedCommand)
        {
            Interlocked.Increment(ref _readOnlyLocks);
            _internalSyncManager.ReadOnly(synchronisedCommand);
        }

        public void UpgradeableRead(Action synchronisedCommand)
        {
            Interlocked.Increment(ref _upgradeableLocks);
            _internalSyncManager.UpgradeableRead(synchronisedCommand);
        }

        public void Write(Action synchronisedCommand)
        {
            Interlocked.Increment(ref _writeLocks);
            _internalSyncManager.Write(synchronisedCommand);
        }

        public T ReadOnly<T>(Func<T> synchronisedQuery)
        {
            Interlocked.Increment(ref _readOnlyLocks);
            return _internalSyncManager.ReadOnly(synchronisedQuery);
        }

        public T UpgradeableRead<T>(Func<T> synchronisedQuery)
        {
            Interlocked.Increment(ref _upgradeableLocks);
            return _internalSyncManager.UpgradeableRead(synchronisedQuery);
        }

        public T Write<T>(Func<T> synchronisedQuery)
        {
            Interlocked.Increment(ref _writeLocks);
            return _internalSyncManager.Write(synchronisedQuery);
        }
    }
}