using System;
using System.Threading;
using Nerdle.Hydra.InfrastructureAbstractions;

namespace Nerdle.Hydra.StateManagement
{
    public class RollingWindowStateManager : IStateManager
    {
        readonly IRollingWindow _successWindow;
        readonly IRollingWindow _failureWindow;
        readonly ISyncManager _sync;
        readonly TimeSpan _failFor;
        readonly IClock _clock;
        readonly ICondition<int, int> _failureCondition;

        State _state;
        DateTime? _failedUntil;

        public event EventHandler<StateChangedArgs> StateChanged;

        public RollingWindowStateManager(TimeSpan windowLength, double failureTriggerPercentage, int minimumSampleSize, TimeSpan failFor, TimeSpan? synchLockTimeout = null) 
            : this(new RollingWindow(windowLength), 
                   new RollingWindow(windowLength), 
                   new SyncManager(new ReaderWriterLockSlim(), synchLockTimeout), 
                   failFor, 
                   new SystemClock(), 
                   new FailurePercentageWithMinimumSampleSizeCondition(failureTriggerPercentage, minimumSampleSize))
        {}

        internal RollingWindowStateManager(
            IRollingWindow successWindow,
            IRollingWindow failureWindow,
            ISyncManager syncManager,
            TimeSpan failFor,
            IClock clock,
            ICondition<int, int> failureCondition,
            State initialState = State.Working)
        {
            _successWindow = successWindow;
            _failureWindow = failureWindow;
            _failFor = failFor;
            _sync = syncManager;
            _clock = clock;
            _failureCondition = failureCondition;
            _state = initialState;
    
            if (_state == State.Failed)
                _failedUntil = _clock.UtcNow + failFor;
        }

        public void RegisterSuccess()
        {
            _sync.UpgradeableRead(() =>
            {
                if (_state == State.Recovering)
                    UpdateStateTo(State.Working);

                if (_state == State.Working)
                    _sync.Write(() => _successWindow.Mark());
            });
        }

        public void RegisterFailure(Exception exception)
        {
            _sync.UpgradeableRead(() =>
            {
                switch (_state)
                {
                    case State.Recovering:
                        UpdateStateTo(State.Failed, exception);
                        return;

                    case State.Working:
                        bool failed = false;
                        _sync.Write(() =>
                        {
                            _failureWindow.Mark();
                            failed = _failureCondition.IsMet(_successWindow.TrimAndCount(), _failureWindow.TrimAndCount());
                            if (failed)
                            {
                                _successWindow.Reset();
                                _failureWindow.Reset();
                            }
                        });
                        if (failed)
                            UpdateStateTo(State.Failed, exception);

                        break;
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
                    var stateIsStale = _state == State.Failed && (_failedUntil == null || _failedUntil <= _clock.UtcNow);
                    return stateIsStale ? (State?)null : _state;
                }, LockTimeoutBehaviour.Ignore);

                if (result.HasValue)
                    return result.Value;

                return _sync.UpgradeableRead(() =>
                {
                    var stateIsStale = _state == State.Failed && (_failedUntil == null || _failedUntil <= _clock.UtcNow);
                    if (stateIsStale)
                    {
                        UpdateStateTo(State.Recovering);
                    }

                    return _state;
                }, LockTimeoutBehaviour.Ignore);
            }
        }

        void UpdateStateTo(State newState, Exception exception = null)
        {
            _sync.Write(() =>
            {
                if (_state == newState)
                    return;

                var oldState = _state;
                _state = newState;

                _failedUntil = newState == State.Failed ? (_clock.UtcNow + _failFor) : (DateTime?)null;

                StateChanged?.Invoke(this, new StateChangedArgs(oldState, newState, exception));
            });
        }
    }
}