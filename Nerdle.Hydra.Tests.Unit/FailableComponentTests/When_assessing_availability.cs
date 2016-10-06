using FluentAssertions;
using Moq;
using Nerdle.Hydra.StateManagement;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_assessing_availability : _on_a_failable_component
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

        [TestCase(State.Unknown)]
        [TestCase(State.Failed)]
        [TestCase(State.Recovering)]
        [TestCase(State.Working)]
        public void The_state_should_only_be_queried_once_to_avoid_synchronization_problems(State currentState)
        {
            StateManager.Setup(sm => sm.CurrentState).Returns(currentState);
            var _ = Sut.IsAvailable;
            StateManager.Verify(sm => sm.CurrentState, Times.AtMostOnce);
        }
    }
}