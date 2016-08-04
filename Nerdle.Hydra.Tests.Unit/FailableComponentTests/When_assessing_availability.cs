using FluentAssertions;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_assessing_availability : _against_a_failable_component
    {
        [TestCase(State.Unknown, false)]
        [TestCase(State.Failed, false)]
        [TestCase(State.Recovering, true)]
        [TestCase(State.Working, true)]
        public void The_decision_maps_the_current_state_reported_by_the_state_manager(State currentState, bool expectedDecision)
        {
            StateManager.Setup(sm => sm.CurrentState).Returns(currentState);
            Sut.IsAvailable.Should().Be(expectedDecision);
        }
    }
}