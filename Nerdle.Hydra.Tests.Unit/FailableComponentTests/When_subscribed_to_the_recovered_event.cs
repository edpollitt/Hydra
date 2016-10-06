using FluentAssertions;
using Nerdle.Hydra.StateManagement;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_subscribed_to_the_recovered_event : _on_a_failable_component
    {
        [TestCase(State.Unknown, false)]
        [TestCase(State.Failed, false)]
        [TestCase(State.Recovering, true)]
        [TestCase(State.Working, false)]
        public void The_event_fires_when_the_state_transitions_from_recovering_to_working(State previousState, bool eventExpected)
        {
            var eventsFired = 0;
            Sut.Recovered += (sender, exception) => eventsFired++;

            StateManager.Raise(failable => failable.StateChanged += null, new StateChangedArgs(previousState, State.Working));
            eventsFired.Should().Be(eventExpected ? 1 : 0);
        }
    }
}