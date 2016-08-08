using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Nerdle.Hydra.StateManagement
{
    public class RollingWindowAveragingStateManager : IStateManager
    {
        readonly RollingWindowAveragingStateManagerConfig _configuration;
        readonly IClock _clock;
        readonly ConcurrentQueue<DateTime> _failures = new ConcurrentQueue<DateTime>();
        readonly ConcurrentQueue<DateTime> _successes = new ConcurrentQueue<DateTime>();

        readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        State _state;
        DateTime? _failedUntil;

        public event StateChangedHandler StateChanged;

        public RollingWindowAveragingStateManager(RollingWindowAveragingStateManagerConfig configuration) : this(configuration, new SystemClock())
        {
        }

        internal RollingWindowAveragingStateManager(RollingWindowAveragingStateManagerConfig configuration, IClock clock, State initialState = State.Working)
        {
            _configuration = configuration;
            _clock = clock;
            _state = initialState;

            if (_state == State.Failed)
                _failedUntil = _clock.UtcNow.AddMinutes(1);
        }

        public void RegisterSuccess()
        {
        }

        public void RegisterError(Exception exception)
        {
        }

        public State CurrentState
        {
            get
            {
                State? result = null;

                // Using a double lock/evaluation pattern as we want to avoid taking an upgradable lock if possible

                WithReadOnlyLock(() =>
                {
                    var stateRequiresModification = _state == State.Failed && (_failedUntil == null || _failedUntil <= _clock.UtcNow);
                    if (!stateRequiresModification) result = _state;
                });

                if (result == null)
                {
                    WithUpgradeableReadLock(() =>
                    {
                        var stateRequiresModification = _state == State.Failed && (_failedUntil == null || _failedUntil <= _clock.UtcNow);
                        if (stateRequiresModification)
                        {
                            WithWriteLock(() =>
                            {
                                var previousState = _state;
                                _state = State.Recovering;
                                OnStateChanged(previousState, _state);
                                _failedUntil = null;
                            });
                        }

                        result = _state;
                    });
                }

                return result ?? State.Unknown;
            }
        }

        void WithReadOnlyLock(Action synchronisedCommand)
        {
            WithLock(synchronisedCommand, rwlock => rwlock.TryEnterReadLock(_configuration.SyncLockTimeout), rwlock => rwlock.ExitReadLock());
        }

        void WithUpgradeableReadLock(Action synchronisedCommand)
        {
            WithLock(synchronisedCommand, rwlock => rwlock.TryEnterUpgradeableReadLock(_configuration.SyncLockTimeout), rwlock => rwlock.ExitUpgradeableReadLock());
        }

        void WithWriteLock(Action synchronisedCommand)
        {
            WithLock(synchronisedCommand, rwlock => rwlock.TryEnterWriteLock(_configuration.SyncLockTimeout), rwlock => rwlock.ExitWriteLock());
        }

        void WithLock(Action synchronisedCommand, Func<ReaderWriterLockSlim, bool> enterLock, Action<ReaderWriterLockSlim> exitLock)
        {
            if (enterLock(_rwLock))
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

        void OnStateChanged(State previousState, State currentState)
        {
            StateChanged?.Invoke(this, new StateChangedArgs(previousState, currentState));
        }
    }
}