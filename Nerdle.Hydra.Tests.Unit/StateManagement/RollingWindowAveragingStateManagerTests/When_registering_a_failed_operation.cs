using System;
using System.Threading;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.Infrastructure;
using Nerdle.Hydra.StateManagement;
using Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowAveragingStateManagerTests.Helpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowAveragingStateManagerTests
{
    [TestFixture]
    class When_registering_a_failed_operation
    {
        ReaderWriterLockSlim _stateLock;
        CountingSyncManagerProxy _syncManagerProxy;
        Mock<IClock> _clock;
        Mock<IRollingWindow> _failureWindow;

        readonly TimeSpan _failFor = TimeSpan.FromMinutes(1);

        [SetUp]
        public void BeforeEach()
        {
            _stateLock = new ReaderWriterLockSlim();
            _syncManagerProxy = new CountingSyncManagerProxy(new SyncManager(TimeSpan.Zero));
            _clock = new Mock<IClock>();
            _clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);
            _failureWindow = new Mock<IRollingWindow>();
        }

        [TestCase(State.Unknown, false)]
        [TestCase(State.Failed, false)]
        [TestCase(State.Recovering, false)]
        [TestCase(State.Working, true)]
        public void The_failure_window_is_marked_if_the_status_is_working(State state, bool markExpected)
        {
            var sut = new RollingWindowAveragingStateManager(Mock.Of<IRollingWindow>(), _failureWindow.Object, _failFor, _syncManagerProxy, _clock.Object, state, _stateLock);
            sut.RegisterFailure();
            _failureWindow.Verify(window => window.Mark(), markExpected ? Times.Once() : Times.Never());
        }

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void A_read_lock_is_obtained_before_the_state_is_read(State state)
        {
            var sut = new RollingWindowAveragingStateManager(Mock.Of<IRollingWindow>(), _failureWindow.Object, _failFor, _syncManagerProxy, _clock.Object, state, _stateLock);
            sut.RegisterFailure();
            _syncManagerProxy.ReadOnlyLocks[_stateLock].Should().Be(1);
        }

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void A_recovering_state_is_updated_to_failed(State state)
        {
            var sut = new RollingWindowAveragingStateManager(Mock.Of<IRollingWindow>(), _failureWindow.Object, _failFor, _syncManagerProxy, _clock.Object, state, _stateLock);
            sut.RegisterFailure();
            sut.CurrentState.Should().Be(state == State.Recovering ? State.Failed : state);
        }

        [Test]
        public void The_state_changed_event_fires_if_the_registration_causes_state_to_be_updated()
        {
            var sut = new RollingWindowAveragingStateManager(Mock.Of<IRollingWindow>(), _failureWindow.Object, _failFor, _syncManagerProxy, _clock.Object, State.Recovering, _stateLock);
            StateChangedArgs changeArgs = null;
            sut.StateChanged += (sender, args) => changeArgs = args;

            sut.RegisterFailure();

            changeArgs.Should().NotBeNull();
            changeArgs.PreviousState.Should().Be(State.Recovering);
            changeArgs.CurrentState.Should().Be(State.Failed);
        }

        [Test]
        public void A_write_lock_is_obtained_if_a_read_causes_state_to_be_updated()
        {
            var sut = new RollingWindowAveragingStateManager(Mock.Of<IRollingWindow>(), Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, State.Recovering, _stateLock);

            sut.RegisterFailure();

            _syncManagerProxy.UpgradeableLocks[_stateLock].Should().Be(1);
            _syncManagerProxy.WriteLocks[_stateLock].Should().Be(1);
        }
    }
}