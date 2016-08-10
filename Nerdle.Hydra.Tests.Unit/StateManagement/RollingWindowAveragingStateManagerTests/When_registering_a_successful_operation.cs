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
        ReaderWriterLockSlim _rwLock;
        CountingSyncManagerProxy _syncManagerProxy;
        Mock<IClock> _clock;
        Mock<IRollingWindow> _successWindow;

        readonly TimeSpan _failFor = TimeSpan.FromMinutes(1);

        [SetUp]
        public void BeforeEach()
        {
            _rwLock = new ReaderWriterLockSlim();
            _syncManagerProxy = new CountingSyncManagerProxy(new SyncManager(_rwLock, TimeSpan.Zero));
            _clock = new Mock<IClock>();
            _clock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);
            _successWindow = new Mock<IRollingWindow>();
        }

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void An_upgradeable_read_lock_is_obtained_before_the_state_is_read(State state)
        {
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _syncManagerProxy, _failFor, _clock.Object, state);
            sut.RegisterSuccess();
            _syncManagerProxy.UpgradeableLocksTaken.Should().Be(1);
        }

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void A_recovering_state_is_upgraded_to_working(State state)
        {
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _syncManagerProxy, _failFor, _clock.Object, state);
            sut.RegisterSuccess();
            sut.CurrentState.Should().Be(state == State.Recovering ? State.Working : state);
        }

        [Test]
        public void The_state_changed_event_fires_if_the_registration_causes_state_to_be_updated()
        {
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _syncManagerProxy, _failFor, _clock.Object, State.Recovering);
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
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _syncManagerProxy, _failFor, _clock.Object, State.Recovering);
            sut.StateChanged += (sender, args) =>
            {
                _syncManagerProxy.UpgradeableLocksTaken.Should().Be(1);
                _syncManagerProxy.WriteLocksTaken.Should().Be(1);
            };

            sut.RegisterSuccess();
        }

        [TestCase(State.Unknown, false)]
        [TestCase(State.Failed, false)]
        [TestCase(State.Recovering, true)]
        [TestCase(State.Working, true)]
        public void The_success_window_is_marked_if_the_status_is_working_or_upgraded_to_working(State state, bool markExpected)
        {
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _syncManagerProxy, _failFor, _clock.Object, state);
            sut.RegisterSuccess();
            _successWindow.Verify(window => window.Mark(), markExpected ? Times.Once() : Times.Never());
        }

        [Test]
        public void A_write_lock_is_obtained_before_the_success_window_is_marked()
        {
            var sut = new RollingWindowAveragingStateManager(_successWindow.Object, Mock.Of<IRollingWindow>(), _syncManagerProxy, _failFor, _clock.Object);
            sut.RegisterSuccess();
            _syncManagerProxy.WriteLocksTaken.Should().Be(1);
        }
    }
}
