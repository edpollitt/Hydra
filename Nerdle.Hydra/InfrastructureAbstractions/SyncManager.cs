using System;
using System.Threading;

namespace Nerdle.Hydra.InfrastructureAbstractions
{
    class SyncManager : ISyncManager
    {
        readonly ReaderWriterLockSlim _rwLock;
        readonly TimeSpan _syncLockTimeout;

        public SyncManager(ReaderWriterLockSlim rwLock, TimeSpan? syncLockTimeout = null)
        {
            _rwLock = rwLock;
            _syncLockTimeout = syncLockTimeout ?? TimeSpan.FromSeconds(10);
        }

        public void ReadOnly(Action command)
        {
            WithLock(command, rw => rw.TryEnterReadLock(_syncLockTimeout), rw => rw.ExitReadLock());
        }

        public void UpgradeableRead(Action command)
        {
            WithLock(command, rw => rw.TryEnterUpgradeableReadLock(_syncLockTimeout), rw => rw.ExitUpgradeableReadLock());
        }

        public void Write(Action command)
        {
            WithLock(command, rw => rw.TryEnterWriteLock(_syncLockTimeout), rw => rw.ExitWriteLock());
        }

        void WithLock(Action synchronisedCommand, Func<ReaderWriterLockSlim, bool> tryEnterLock, Action<ReaderWriterLockSlim> exitLock)
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
        }

        public T ReadOnly<T>(Func<T> query)
        {
            return WithLock(query, rw => rw.TryEnterReadLock(_syncLockTimeout), rw => rw.ExitReadLock());
        }

        public T UpgradeableRead<T>(Func<T> query)
        {
            return WithLock(query, rw => rw.TryEnterUpgradeableReadLock(_syncLockTimeout), rw => rw.ExitUpgradeableReadLock());
        }

        public T Write<T>(Func<T> query)
        {
            return WithLock(query, rw => rw.TryEnterWriteLock(_syncLockTimeout), rw => rw.ExitWriteLock());
        }

        T WithLock<T>(Func<T> synchronisedQuery, Func<ReaderWriterLockSlim, bool> tryEnterLock, Action<ReaderWriterLockSlim> exitLock)
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
            return default(T);
        }
    }
}