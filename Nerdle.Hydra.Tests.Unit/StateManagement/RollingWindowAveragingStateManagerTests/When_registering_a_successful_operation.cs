using FluentAssertions;
using Moq;
using Nerdle.Hydra.StateManagement;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowAveragingStateManagerTests
{
    [TestFixture]
    class When_registering_a_successful_operation : _to_a_rolling_window_averaging_state_manager
    {
        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void An_upgradeable_read_lock_is_obtained_before_the_state_is_read(State state)
        {
            var sut = RollingWindowAveragingStateManagerWithState(state);
            sut.RegisterSuccess();
            SyncManagerProxy.UpgradeableLocksTaken.Should().Be(1);
        }

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void A_recovering_state_is_upgraded_to_working(State state)
        {
            var sut = RollingWindowAveragingStateManagerWithState(state);
            sut.RegisterSuccess();
            sut.CurrentState.Should().Be(state == State.Recovering ? State.Working : state);
        }

        [Test]
        public void The_state_changed_event_fires_if_the_registration_causes_state_to_be_updated()
        {
            var sut = RollingWindowAveragingStateManagerWithState(State.Recovering);
            StateChangedArgs changeArgs = null;
            sut.StateChanged += (sender, args) => changeArgs = args;

            sut.RegisterSuccess();

            changeArgs.Should().NotBeNull();
            changeArgs.PreviousState.Should().Be(State.Recovering);
            changeArgs.CurrentState.Should().Be(State.Working);
        }

        [Test]
        public void A_write_lock_is_obtained_if_the_registration_causes_state_to_be_updated()
        {
            var sut = RollingWindowAveragingStateManagerWithState(State.Recovering);
            sut.StateChanged += (sender, args) =>
            {
                SyncManagerProxy.UpgradeableLocksTaken.Should().Be(1);
                SyncManagerProxy.WriteLocksTaken.Should().Be(1);
            };

            sut.RegisterSuccess();
        }

        [TestCase(State.Unknown, false)]
        [TestCase(State.Failed, false)]
        [TestCase(State.Recovering, true)]
        [TestCase(State.Working, true)]
        public void The_success_window_is_marked_if_the_status_is_working_or_upgraded_to_working(State state, bool markExpected)
        {
            var sut = RollingWindowAveragingStateManagerWithState(state);
            sut.RegisterSuccess();
            SuccessWindow.Verify(window => window.Mark(), markExpected ? Times.Once() : Times.Never());
        }

        [Test]
        public void A_write_lock_is_obtained_before_the_success_window_is_marked()
        {
            var sut = RollingWindowAveragingStateManagerWithState(State.Working);
            sut.RegisterSuccess();
            SyncManagerProxy.WriteLocksTaken.Should().Be(1);
        }
    }
}
