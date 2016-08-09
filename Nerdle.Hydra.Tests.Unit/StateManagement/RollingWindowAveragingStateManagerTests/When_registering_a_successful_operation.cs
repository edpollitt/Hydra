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
    class When_registering_a_successful_operation
    {
        ReaderWriterLockSlim _stateLock;
        ReaderWriterLockSlim _successWindowLock;
        CountingSyncManagerProxy _syncManagerProxy;
        Mock<IClock> _clock;
        Mock<IRollingWindow> _successWindow;

        readonly TimeSpan _failFor = TimeSpan.FromMinutes(1);

        [SetUp]
        public void BeforeEach()
        {
            _stateLock = new ReaderWriterLockSlim();
            _successWindowLock = new ReaderWriterLockSlim();
            _syncManagerProxy = new CountingSyncManagerProxy(new SyncManager(TimeSpan.Zero));
            _clock = new Mock<IClock>();
            _clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);
            _successWindow = new Mock<IRollingWindow>();
        }

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void A_read_lock_is_obtained_before_the_state_is_read(State state)
        {
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, state, _stateLock);
            sut.RegisterSuccess();
            _syncManagerProxy.ReadOnlyLocks[_stateLock].Should().Be(1);
        }

        [TestCase(State.Unknown, false)]
        [TestCase(State.Failed, false)]
        [TestCase(State.Recovering, false)]
        [TestCase(State.Working, true)]
        public void The_success_window_is_marked_if_the_status_is_working(State state, bool markExpected)
        {
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, state, _stateLock);
            sut.RegisterSuccess();
            _successWindow.Verify(window => window.Mark(), markExpected ? Times.Once() : Times.Never());
        }

        [Test]
        public void A_write_lock_is_obtained_before_the_success_window_is_marked()
        {
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, State.Working, _stateLock, _successWindowLock);
            sut.RegisterSuccess();
            _syncManagerProxy.WriteLocks[_successWindowLock].Should().Be(1);
        }

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void A_recovering_state_is_updated_to_working(State state)
        {
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, state, _stateLock);
            sut.RegisterSuccess();
            sut.CurrentState.Should().Be(state == State.Recovering ? State.Working : state);
        }

        [Test]
        public void The_state_changed_event_fires_if_the_registration_causes_state_to_be_updated()
        {
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, State.Recovering, _stateLock);
            StateChangedArgs changeArgs = null;
            sut.StateChanged += (sender, args) => changeArgs = args;

            sut.RegisterSuccess();

            changeArgs.Should().NotBeNull();
            changeArgs.PreviousState.Should().Be(State.Recovering);
            changeArgs.CurrentState.Should().Be(State.Working);
        }

        [Test]
        public void A_write_lock_is_obtained_if_the_registration_causes_state_to_be_update()
        {
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _failFor, _syncManagerProxy, _clock.Object, State.Recovering, _stateLock);

            sut.RegisterSuccess();

            _syncManagerProxy.UpgradeableLocks[_stateLock].Should().Be(1);
            _syncManagerProxy.WriteLocks[_stateLock].Should().Be(1);
        }
    }
}
