using System;
using System.Threading;
using Nerdle.Hydra.Exceptions;

namespace Nerdle.Hydra.InfrastructureAbstractions
{
    public class SyncManager : ISyncManager
    {
        readonly ReaderWriterLockSlim _rwLock;
        readonly TimeSpan _syncLockTimeout;

        public SyncManager(ReaderWriterLockSlim rwLock, TimeSpan? syncLockTimeout = null)
        {
            _rwLock = rwLock;
            _syncLockTimeout = syncLockTimeout ?? TimeSpan.FromSeconds(10);
        }

        public void ReadOnly(Action command, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            WithLock(command, rw => rw.TryEnterReadLock(_syncLockTimeout), rw => rw.ExitReadLock(), timeoutBehaviour);
        }

        public void UpgradeableRead(Action command, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            WithLock(command, rw => rw.TryEnterUpgradeableReadLock(_syncLockTimeout), rw => rw.ExitUpgradeableReadLock(), timeoutBehaviour);
        }

        public void Write(Action command, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            WithLock(command, rw => rw.TryEnterWriteLock(_syncLockTimeout), rw => rw.ExitWriteLock(), timeoutBehaviour);
        }

        void WithLock(Action synchronisedCommand, Func<ReaderWriterLockSlim, bool> tryEnterLock, Action<ReaderWriterLockSlim> exitLock, LockTimeoutBehaviour timeoutBehaviour)
        {
            if (tryEnterLock(_rwLock))
            {
                try
                {
                    synchronisedCommand();
                }
                finally
                {
                    exitLock(_rwLock);
                }
            }
            else
            {
                if (timeoutBehaviour == LockTimeoutBehaviour.Ignore)
                    return;

                throw new LockEntryTimeoutException($"Failed to obtain a sync lock. Waited for {_syncLockTimeout} before giving up.");
            }
        }

        public T ReadOnly<T>(Func<T> query, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            return WithLock(query, rw => rw.TryEnterReadLock(_syncLockTimeout), rw => rw.ExitReadLock(), timeoutBehaviour);
        }

        public T UpgradeableRead<T>(Func<T> query, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            return WithLock(query, rw => rw.TryEnterUpgradeableReadLock(_syncLockTimeout), rw => rw.ExitUpgradeableReadLock(), timeoutBehaviour);
        }

        public T Write<T>(Func<T> query, LockTimeoutBehaviour timeoutBehaviour = LockTimeoutBehaviour.Throw)
        {
            return WithLock(query, rw => rw.TryEnterWriteLock(_syncLockTimeout), rw => rw.ExitWriteLock(), timeoutBehaviour);
        }

        T WithLock<T>(Func<T> synchronisedQuery, Func<ReaderWriterLockSlim, bool> tryEnterLock, Action<ReaderWriterLockSlim> exitLock, LockTimeoutBehaviour timeoutBehaviour)
        {
            if (tryEnterLock(_rwLock))
            {
                try
                {
                    return synchronisedQuery();
                }
                finally
                {
                    exitLock(_rwLock);
                }
            }

            if (timeoutBehaviour == LockTimeoutBehaviour.Ignore)
                return default(T);

            throw new LockEntryTimeoutException($"Failed to obtain a sync lock. Waited for {_syncLockTimeout} before giving up.");
        }
    }
}