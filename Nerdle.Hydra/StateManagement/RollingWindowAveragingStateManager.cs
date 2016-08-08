using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Nerdle.Hydra.StateManagement
{
    public class RollingWindowAveragingStateManagerConfig
    {
        public RollingWindowAveragingStateManagerConfig(TimeSpan windowLength, double failureTriggerPercentage, int minimumSampleSize, TimeSpan? synchLockTimeout = null)
        {
            // TODO: verify config ranges
            WindowLength = windowLength;
            FailureTriggerPercentage = failureTriggerPercentage;
            MinimumSampleSize = minimumSampleSize;
            SyncLockTimeout = synchLockTimeout ?? TimeSpan.FromSeconds(2);
        }

        public TimeSpan WindowLength { get; }
        public double FailureTriggerPercentage { get; }
        public int MinimumSampleSize { get; }
        public TimeSpan SyncLockTimeout { get; }
    }

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
                // Using a double lock/evaluation pattern as we want to avoid taking an upgradable lock if possible

                if (_rwLock.TryEnterReadLock(_configuration.SyncLockTimeout))
                {
                    try
                    {
                        var stateRequiresModification = _state == State.Failed && (_failedUntil == null || _failedUntil <= _clock.UtcNow);
                        if (!stateRequiresModification)
                            return _state;
                    }
                    finally
                    {
                        _rwLock.ExitReadLock();
                    }
                }
                else
                {
                    return State.Unknown;
                }

                if (_rwLock.TryEnterUpgradeableReadLock(_configuration.SyncLockTimeout))
                {
                    try
                    {
                        var stateRequiresModification = _state == State.Failed && (_failedUntil == null || _failedUntil <= _clock.UtcNow);
                        if (stateRequiresModification)
                        {
                            if (_rwLock.TryEnterWriteLock(_configuration.SyncLockTimeout))
                            {
                                try
                                {
                                    var previousState = _state;
                                    _state = State.Recovering;
                                    OnStateChanged(previousState, _state);
                                    _failedUntil = null;
                                }
                                finally
                                {
                                    _rwLock.ExitWriteLock();
                                }
                            }
                        }
                        return _state;
                    }
                    finally
                    {
                        _rwLock.ExitUpgradeableReadLock();
                    }
                }

                return State.Unknown;
            }
        }


        void OnStateChanged(State previousState, State currentState)
        {
            StateChanged?.Invoke(this, new StateChangedArgs(previousState, currentState));
        }
    }
}