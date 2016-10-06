using System;
using System.Runtime.Serialization;
using FluentAssertions;
using Nerdle.Hydra.StateManagement;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_subscribed_to_the_failed_event : _on_a_failable_component
    {
        [TestCase(State.Unknown, false)]
        [TestCase(State.Failed, false)]
        [TestCase(State.Recovering, false)]
        [TestCase(State.Working, true)]
        public void The_event_fires_when_the_state_transitions_from_working_to_failed(State previousState, bool eventExpected)
        {
            var eventsFired = 0;
            Sut.Failed += (sender, exception) => eventsFired++;

            StateManager.Raise(failable => failable.StateChanged += null, new StateChangedArgs(previousState, State.Failed));
            eventsFired.Should().Be(eventExpected ? 1 : 0);
        }

        [Test]
        public void The_exception_is_reported_when_the_event_fires()
        {
            Exception reportedException = null;
            var thrownException = new SerializationException();

            Sut.Failed += (sender, exception) => reportedException = exception;
            StateManager.Raise(failable => failable.StateChanged += null, new StateChangedArgs(State.Working, State.Failed, thrownException));

            reportedException.Should().Be(thrownException);
        }
    }
}
