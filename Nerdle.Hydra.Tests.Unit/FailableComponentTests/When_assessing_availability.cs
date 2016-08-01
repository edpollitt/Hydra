using System.Collections;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_assessing_availability
    {
        IFailable<IList> _sut;
        IList _wrappedComponent;
        Mock<IStateManager> _stateManager;

        [SetUp]
        public void BeforeEach()
        {
            _wrappedComponent = new ArrayList();
            _stateManager = new Mock<IStateManager>();
            _sut = new Failable<IList>(_wrappedComponent, "foo", _stateManager.Object);
        }

        [TestCase(State.Unknown, false)]
        [TestCase(State.Failed, false)]
        [TestCase(State.Recovering, true)]
        [TestCase(State.Working, true)]
        public void The_decision_maps_the_current_state_reported_by_the_state_manager(State currentState, bool expectedDecision)
        {
            _stateManager.Setup(sm => sm.CurrentState).Returns(currentState);
            _sut.IsAvailable.Should().Be(expectedDecision);
        }
    }
}