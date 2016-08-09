using System;
using System.Threading;

namespace Nerdle.Hydra.StateManagement
{
    public class RollingWindowAveragingStateManager : IStateManager
    {
        readonly IRollingWindow _failureWindow;
        readonly IRollingWindow _successWindow;
        readonly IClock _clock;

        readonly ISyncManager _sync;

        readonly ReaderWriterLockSlim _stateLock;
        readonly ReaderWriterLockSlim _successWindowLock;
        readonly ReaderWriterLockSlim _failureWindowLock;

        State _state;
        DateTime? _failedUntil;
        readonly TimeSpan _failFor;

        public event StateChangedHandler StateChanged;

        public RollingWindowAveragingStateManager(TimeSpan windowLength, double failureTriggerPercentage, int minimumSampleSize, TimeSpan failFor, TimeSpan? synchLockTimeout = null) 
            : this(new RollingWindow(windowLength), new RollingWindow(windowLength), failFor, new SyncManager(synchLockTimeout))
        {
        }

        internal RollingWindowAveragingStateManager(
            IRollingWindow failureWindow, 
            IRollingWindow successWindow,
            TimeSpan failFor,
            ISyncManager syncManager,
            IClock clock = null,
            State initialState = State.Working,
            ReaderWriterLockSlim stateLock = null,
            ReaderWriterLockSlim successWindowLock = null,
            ReaderWriterLockSlim failureWindowLock = null)
        {
            _failureWindow = failureWindow;
            _successWindow = successWindow;
            _failFor = failFor;
            _sync = syncManager;
            _clock = clock ?? new SystemClock();
            _state = initialState;
            _stateLock = stateLock ?? new ReaderWriterLockSlim();
            _successWindowLock = successWindowLock ?? new ReaderWriterLockSlim();
            _failureWindowLock = failureWindowLock ?? new ReaderWriterLockSlim();

            if (_state == State.Failed)
                _failedUntil = _clock.UtcNow.AddMinutes(1);
        }

        public void RegisterSuccess()
        {
            _sync.Write(_successWindowLock, () => _successWindow.Mark());
        }

        public void RegisterError(Exception exception)
        {
        }

        public State CurrentState
        {
            get
            {
                // Using a double lock/evaluation pattern as we want to avoid taking an upgradeable lock if possible

                var result = _sync.ReadOnly(_stateLock, () =>
                {
                    var stateRequiresModification = _state == State.Failed && (_failedUntil == null || _failedUntil <= _clock.UtcNow);
                    return stateRequiresModification ? (State?)null : _state;
                });

                if (result.HasValue)
                    return result.Value;

                return _sync.UpgradeableRead(_stateLock, () =>
                {
                    var stateRequiresModification = _state == State.Failed && (_failedUntil == null || _failedUntil <= _clock.UtcNow);
                    if (stateRequiresModification)
                    {
                        _sync.Write(_stateLock, () =>
                        {
                            UpdateState(State.Recovering);
                            _failedUntil = null;
                        });
                    }

                    return _state;
                });
            }
        }

        void UpdateState(State newState)
        {
            if (_state == newState)
                return;

            var oldState = _state;
            _state = newState;

            StateChanged?.Invoke(this, new StateChangedArgs(oldState, newState));
        }
    }
}