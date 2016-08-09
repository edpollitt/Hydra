using System;
using System.Threading;

namespace Nerdle.Hydra.Infrastructure
{
    class SyncManager : ISyncManager
    {
        readonly TimeSpan _syncLockTimeout;

        public SyncManager(TimeSpan? syncLockTimeout = null)
        {
            _syncLockTimeout = syncLockTimeout ?? TimeSpan.FromSeconds(10);
        }

        public void ReadOnly(ReaderWriterLockSlim rwLock, Action synchronisedCommand)
        {
            WithLock(synchronisedCommand, () => rwLock.TryEnterReadLock(_syncLockTimeout), rwLock.ExitReadLock);
        }

        public void UpgradeableRead(ReaderWriterLockSlim rwLock, Action synchronisedCommand)
        {
            WithLock(synchronisedCommand, () => rwLock.TryEnterUpgradeableReadLock(_syncLockTimeout), rwLock.ExitUpgradeableReadLock);
        }

        public void Write(ReaderWriterLockSlim rwLock, Action synchronisedCommand)
        {
            WithLock(synchronisedCommand, () => rwLock.TryEnterWriteLock(_syncLockTimeout), rwLock.ExitWriteLock);
        }

        static void WithLock(Action synchronisedCommand, Func<bool> tryEnterLock, Action exitLock)
        {
            if (tryEnterLock())
            {
                try
                {
                    synchronisedCommand();
                }
                finally
                {
                    exitLock();
                }
            }
        }

        public T ReadOnly<T>(ReaderWriterLockSlim rwLock, Func<T> synchronisedQuery)
        {
            return WithLock(synchronisedQuery, () => rwLock.TryEnterReadLock(_syncLockTimeout), rwLock.ExitReadLock);
        }

        public T UpgradeableRead<T>(ReaderWriterLockSlim rwLock, Func<T> synchronisedQuery)
        {
            return WithLock(synchronisedQuery, () => rwLock.TryEnterUpgradeableReadLock(_syncLockTimeout), rwLock.ExitUpgradeableReadLock);
        }

        public T Write<T>(ReaderWriterLockSlim rwLock, Func<T> synchronisedQuery)
        {
            return WithLock(synchronisedQuery, () => rwLock.TryEnterWriteLock(_syncLockTimeout), rwLock.ExitWriteLock);
        }

        static T WithLock<T>(Func<T> synchronisedQuery, Func<bool> tryEnterLock, Action exitLock)
        {
            if (tryEnterLock())
            {
                try
                {
                    return synchronisedQuery();
                }
                finally
                {
                    exitLock();
                }
            }
            return default(T);
        }
    }
}