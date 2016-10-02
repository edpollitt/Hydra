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

        public void ReadOnly(Action command, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            Interlocked.Increment(ref _readOnlyLocks);
            _internalSyncManager.ReadOnly(command, timeoutBehaviour);
        }

        public void UpgradeableRead(Action command, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            Interlocked.Increment(ref _upgradeableLocks);
            _internalSyncManager.UpgradeableRead(command, timeoutBehaviour);
        }

        public void Write(Action command, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            Interlocked.Increment(ref _writeLocks);
            _internalSyncManager.Write(command, timeoutBehaviour);
        }

        public T ReadOnly<T>(Func<T> query, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            Interlocked.Increment(ref _readOnlyLocks);
            return _internalSyncManager.ReadOnly(query, timeoutBehaviour);
        }

        public T UpgradeableRead<T>(Func<T> query, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            Interlocked.Increment(ref _upgradeableLocks);
            return _internalSyncManager.UpgradeableRead(query, timeoutBehaviour);
        }

        public T Write<T>(Func<T> query, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            Interlocked.Increment(ref _writeLocks);
            return _internalSyncManager.Write(query, timeoutBehaviour);
        }
    }
}