using System;
using System.Threading;
using Nerdle.Hydra.InfrastructureAbstractions;

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

        public void ReadOnly(Action command)
        {
            Interlocked.Increment(ref _readOnlyLocks);
            _internalSyncManager.ReadOnly(command);
        }

        public void UpgradeableRead(Action command)
        {
            Interlocked.Increment(ref _upgradeableLocks);
            _internalSyncManager.UpgradeableRead(command);
        }

        public void Write(Action command)
        {
            Interlocked.Increment(ref _writeLocks);
            _internalSyncManager.Write(command);
        }

        public T ReadOnly<T>(Func<T> query)
        {
            Interlocked.Increment(ref _readOnlyLocks);
            return _internalSyncManager.ReadOnly(query);
        }

        public T UpgradeableRead<T>(Func<T> query)
        {
            Interlocked.Increment(ref _upgradeableLocks);
            return _internalSyncManager.UpgradeableRead(query);
        }

        public T Write<T>(Func<T> query)
        {
            Interlocked.Increment(ref _writeLocks);
            return _internalSyncManager.Write(query);
        }
    }
}