using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.StateManagement;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowAveragingStateManagerTests
{
    [TestFixture]
    class When_reading_current_state
    {
        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void The_state_is_returned(State state)
        {
            var config = new RollingWindowAveragingStateManagerConfig(TimeSpan.FromMinutes(1), 0.5, 10);
            var clock = new SystemClock();
            var sut = new RollingWindowAveragingStateManager(config, clock, state);

            sut.CurrentState.Should().Be(state);
        }

        [Test]
        public void A_failed_state_recovers_after_the_configured_period_has_elapsed()
        {
            var config = new RollingWindowAveragingStateManagerConfig(TimeSpan.FromMinutes(1), 0.5, 10);
            var clock = new Mock<IClock>();
            clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);
            var sut = new RollingWindowAveragingStateManager(config, clock.Object, State.Failed);
            clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow.AddMinutes(1));

            sut.CurrentState.Should().Be(State.Recovering);
        }

        [Test]
        public void The_state_changed_event_fires_if_the_state_is_changed()
        {
            StateChangedArgs changeArgs = null;
            var config = new RollingWindowAveragingStateManagerConfig(TimeSpan.FromMinutes(1), 0.5, 10);
            var clock = new Mock<IClock>();
            clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);
            var sut = new RollingWindowAveragingStateManager(config, clock.Object, State.Failed);
            sut.StateChanged += (sender, args) => changeArgs = args;
            clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow.AddMinutes(1));

            var _ = sut.CurrentState;

            changeArgs.Should().NotBeNull();
            changeArgs.PreviousState.Should().Be(State.Failed);
            changeArgs.CurrentState.Should().Be(State.Recovering);
        }

        [Test]
        public void An_unknown_state_is_returned_if_a_sync_lock_cannot_be_obtained_within_the_configured_period()
        {
            var config = new RollingWindowAveragingStateManagerConfig(TimeSpan.FromMinutes(1), 0.5, 10, TimeSpan.Zero);
            var clock = new Mock<IClock>();
            clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);
            var sut = new RollingWindowAveragingStateManager(config, clock.Object, State.Failed);
            clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow.AddMinutes(1));
            sut.StateChanged += (sender, args) => Thread.Sleep(500);

            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() => sut.CurrentState)).ToArray();

            Task.WhenAll(tasks).ContinueWith(completed =>
            {
                completed.Result.Where(state => state == State.Recovering).Should().HaveCount(1);
                completed.Result.Where(state => state == State.Unknown).Should().HaveCount(9);
            })
            .Wait();
        }
    }
}
