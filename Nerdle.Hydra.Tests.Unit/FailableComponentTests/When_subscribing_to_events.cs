using System;
using System.Collections;
using System.Runtime.Serialization;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.StateManagement;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_subscribing_to_events
    {
        Mock<IStateManager> _stateManager;
        Failable<ISerializable> _sut;

        int _recoveredFiredCount;
        int _failedFiredCount;

        [SetUp]
        public void BeforeEach()
        {
            _stateManager = new Mock<IStateManager>();
            _sut = new Failable<ISerializable>("whatever", new Hashtable(), _stateManager.Object);

            _sut.Recovered += (sender, args) => _recoveredFiredCount++;
            _sut.Failed += (sender, exception) => _failedFiredCount++;

            _recoveredFiredCount = _failedFiredCount = 0;
        }

        [Test]
        public void The_recovered_event_fires_when_the_state_transitions_to_working()
        {
            _stateManager.Raise(failable => failable.StateChanged += null, new StateChangedArgs(State.Recovering, State.Working));
            _recoveredFiredCount.Should().Be(1);
            _failedFiredCount.Should().Be(0);
        }

        [TestCase(State.Unknown, false)]
        [TestCase(State.Failed, false)]
        [TestCase(State.Recovering, false)]
        [TestCase(State.Working, true)]
        public void The_failed_event_fires_when_the_state_transitions_to_failed_if_the_previous_state_was_working(State previousState, bool eventExpected)
        {
            _stateManager.Raise(failable => failable.StateChanged += null, new StateChangedArgs(previousState, State.Failed));
            _recoveredFiredCount.Should().Be(0);
            _failedFiredCount.Should().Be(eventExpected ? 1 : 0);
        }

        [Test]
        public void The_exception_is_reported_when_the_failed_event_fires()
        {
            Exception reportedException = null;
            var thrownException = new SerializationException();

            _sut.Failed += (sender, exception) => reportedException = exception;
            _stateManager.Raise(failable => failable.StateChanged += null, new StateChangedArgs(State.Working, State.Failed, thrownException));

            _failedFiredCount.Should().Be(1);
            reportedException.Should().Be(thrownException);
        }
    }
}