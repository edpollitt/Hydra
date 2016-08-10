using System;
using System.Threading;
using Nerdle.Hydra.Infrastructure;

namespace Nerdle.Hydra.StateManagement
{
    public class RollingWindowAveragingStateManager : IStateManager
    {
        readonly double _failureTriggerPercentage;
        readonly int _minimumSampleSize;
        readonly IRollingWindow _failureWindow;
        readonly IRollingWindow _successWindow;
        readonly ISyncManager _sync;
        readonly TimeSpan _failFor;
        readonly IClock _clock;

        State _state;
        DateTime? _failedUntil;

        public event StateChangedHandler StateChanged;

        public RollingWindowAveragingStateManager(TimeSpan windowLength, double failureTriggerPercentage, int minimumSampleSize, TimeSpan failFor, TimeSpan? synchLockTimeout = null) 
            : this(new RollingWindow(windowLength), new RollingWindow(windowLength), new SyncManager(new ReaderWriterLockSlim(), synchLockTimeout), failFor, new SystemClock())
        {
            _failureTriggerPercentage = failureTriggerPercentage;
            _minimumSampleSize = minimumSampleSize;
        }

        internal RollingWindowAveragingStateManager(
            IRollingWindow successWindow,
            IRollingWindow failureWindow,
            ISyncManager syncManager,
            TimeSpan failFor,
            IClock clock,
            State initialState = State.Working)
        {
            _successWindow = successWindow;
            _failureWindow = failureWindow;
            _failFor = failFor;
            _sync = syncManager;
            _clock = clock;
            _state = initialState;
    
            if (_state == State.Failed)
                _failedUntil = _clock.UtcNow + failFor;
        }

        public void RegisterSuccess()
        {
            _sync.UpgradeableRead(() =>
            {
                if (_state == State.Recovering)
                    UpdateState(State.Working);

                if (_state == State.Working)
                    _sync.Write(() => _successWindow.Mark());
            });
        }

        public void RegisterFailure()
        {
            _sync.UpgradeableRead(() =>
            {
                if (_state == State.Recovering)
                {
                    UpdateState(State.Failed);
                }

                if (_state == State.Working)
                {
                    _sync.Write(() => _failureWindow.Mark());
                }
            });
        }

        public State CurrentState
        {
            get
            {
                // Using a double lock/evaluation pattern as we want to avoid taking an upgradeable lock if possible
                var result = _sync.ReadOnly(() =>
                {
                    var stateRequiresModification = _state == State.Failed && (_failedUntil == null || _failedUntil <= _clock.UtcNow);
                    return stateRequiresModification ? (State?)null : _state;
                });

                if (result.HasValue)
                    return result.Value;

                return _sync.UpgradeableRead(() =>
                {
                    var stateRequiresModification = _state == State.Failed && (_failedUntil == null || _failedUntil <= _clock.UtcNow);
                    if (stateRequiresModification)
                    {
                        UpdateState(State.Recovering);
                    }

                    return _state;
                });
            }
        }

        void UpdateState(State newState)
        {
            _sync.Write(() =>
            {
                if (_state == newState)
                    return;

                var oldState = _state;
                _state = newState;

                _failedUntil = newState == State.Failed ? (_clock.UtcNow + _failFor) : (DateTime?)null;

                StateChanged?.Invoke(this, new StateChangedArgs(oldState, newState));
            });
        }
    }
}