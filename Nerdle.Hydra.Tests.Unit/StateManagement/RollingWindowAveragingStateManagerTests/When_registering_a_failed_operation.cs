using FluentAssertions;
using Moq;
using Nerdle.Hydra.StateManagement;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowAveragingStateManagerTests
{
    [TestFixture]
    class When_registering_a_failed_operation : _to_a_rolling_window_averaging_state_manager
    {
        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void An_upgradeable_read_lock_is_obtained_before_the_state_is_read(State state)
        {
            RollingWindowAveragingStateManagerWithState(state).RegisterFailure();
            SyncManagerProxy.UpgradeableLocksTaken.Should().Be(1);
        }

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void A_recovering_state_is_downgraded_to_failed(State state)
        {
            var sut = RollingWindowAveragingStateManagerWithState(state);
            sut.RegisterFailure();
            sut.CurrentState.Should().Be(state == State.Recovering ? State.Failed : state);
        }

        [Test]
        public void A_write_lock_is_obtained_before_a_recovering_state_is_downgraded()
        {
            RollingWindowAveragingStateManagerWithState(State.Recovering).RegisterFailure();
            SyncManagerProxy.UpgradeableLocksTaken.Should().Be(1);
            SyncManagerProxy.WriteLocksTaken.Should().Be(1);
        }

        [Test]
        public void The_state_changed_event_fires_if_a_recovering_state_is_downgraded()
        {
            var sut = RollingWindowAveragingStateManagerWithState(State.Recovering);
            StateChangedArgs changeArgs = null;
            sut.StateChanged += (sender, args) => changeArgs = args;

            sut.RegisterFailure();

            changeArgs.Should().NotBeNull();
            changeArgs.PreviousState.Should().Be(State.Recovering);
            changeArgs.CurrentState.Should().Be(State.Failed);
        }

        [TestCase(State.Unknown, false)]
        [TestCase(State.Failed, false)]
        [TestCase(State.Recovering, false)]
        [TestCase(State.Working, true)]
        public void The_failure_window_is_marked_if_the_state_is_working(State state, bool markExpected)
        {
            RollingWindowAveragingStateManagerWithState(state).RegisterFailure();
            FailureWindow.Verify(window => window.Mark(), markExpected ? Times.Once() : Times.Never());
        }

        [Test]
        public void A_write_lock_is_obtained_before_marking_the_window()
        {
            RollingWindowAveragingStateManagerWithState(State.Working).RegisterFailure();
            SyncManagerProxy.UpgradeableLocksTaken.Should().Be(1);
            SyncManagerProxy.WriteLocksTaken.Should().Be(1);
        }

        [Test]
        public void The_failure_condition_is_evaluated_after_the_window_is_marked()
        {
            SuccessWindow.Setup(window => window.Count).Returns(99);
            FailureWindow.Setup(window => window.Count).Returns(6);
            RollingWindowAveragingStateManagerWithState(State.Working).RegisterFailure();
            FailureCondition.Verify(condition => condition.Evaluate(99, 6), Times.Once);
        }

        [Test]
        public void The_state_is_unchanged_if_the_failure_condition_is_not_met()
        {
            FailureCondition.Setup(condition => condition.Evaluate(It.IsAny<int>(), It.IsAny<int>())).Returns(false);
            var sut = RollingWindowAveragingStateManagerWithState(State.Working);
            sut.RegisterFailure();
            sut.CurrentState.Should().Be(State.Working);
        }

        [Test]
        public void The_state_is_downgraded_to_failed_if_the_failure_condition_is_met()
        {
            FailureCondition.Setup(condition => condition.Evaluate(It.IsAny<int>(), It.IsAny<int>())).Returns(true);
            var sut = RollingWindowAveragingStateManagerWithState(State.Working);
            sut.RegisterFailure();
            sut.CurrentState.Should().Be(State.Failed);
        }
    }
}