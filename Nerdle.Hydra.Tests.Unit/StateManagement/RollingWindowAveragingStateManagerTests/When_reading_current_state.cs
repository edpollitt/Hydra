using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.Infrastructure;
using Nerdle.Hydra.StateManagement;
using Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowAveragingStateManagerTests.Helpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowAveragingStateManagerTests
{
    [TestFixture]
    class When_reading_the_current_state
    {
        ReaderWriterLockSlim _stateLock;
        CountingSyncManagerProxy _syncManagerProxy;
        Mock<IClock> _clock;

        readonly TimeSpan _failFor = TimeSpan.FromMinutes(1);

        [SetUp]
        public void BeforeEach()
        {
            _stateLock = new ReaderWriterLockSlim();
            _syncManagerProxy = new CountingSyncManagerProxy(new SyncManager(TimeSpan.Zero));
            _clock = new Mock<IClock>();
            _clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);
        }

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void A_read_lock_is_obtained(State state)
        {
            var sut = new RollingWindowAveragingStateManager(Mock.Of<IRollingWindow>(), Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, state, _stateLock);

            var _ = sut.CurrentState;

            _syncManagerProxy.ReadOnlyLocks[_stateLock].Should().Be(1);
        }

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void The_state_is_returned(State state)
        {
            var sut = new RollingWindowAveragingStateManager(Mock.Of<IRollingWindow>(), Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, state, _stateLock);

            sut.CurrentState.Should().Be(state);
        }

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void The_unknown_state_is_returned_if_a_sync_lock_cannot_be_obtained_within_the_configured_period(State state)
        {
            var sut = new RollingWindowAveragingStateManager(Mock.Of<IRollingWindow>(), Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, state, _stateLock);
            Task.Run(() => _stateLock.EnterWriteLock()).Wait();

            sut.CurrentState.Should().Be(State.Unknown);
        }

        [Test]
        public void A_failed_state_recovers_after_the_configured_period()
        {
            var sut = new RollingWindowAveragingStateManager(Mock.Of<IRollingWindow>(), Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, State.Failed, _stateLock);
            _clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow.Add(_failFor));

            sut.CurrentState.Should().Be(State.Recovering);
        }

        [Test]
        public void A_write_lock_is_obtained_if_a_read_causes_state_to_be_update()
        {
            var sut = new RollingWindowAveragingStateManager(Mock.Of<IRollingWindow>(), Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, State.Failed, _stateLock);
            _clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow.Add(_failFor));

            var _ = sut.CurrentState;

            _syncManagerProxy.UpgradeableLocks[_stateLock].Should().Be(1);
            _syncManagerProxy.WriteLocks[_stateLock].Should().Be(1);
        }


        [Test]
        public void The_state_changed_event_fires_if_a_read_causes_state_to_be_update()
        {
            var sut = new RollingWindowAveragingStateManager(Mock.Of<IRollingWindow>(), Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, State.Failed, _stateLock);
            _clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow.Add(_failFor));
            StateChangedArgs changeArgs = null;
            sut.StateChanged += (sender, args) => changeArgs = args;

            var _ = sut.CurrentState;

            changeArgs.Should().NotBeNull();
            changeArgs.PreviousState.Should().Be(State.Failed);
            changeArgs.CurrentState.Should().Be(State.Recovering);
        }
    }
}
