using System.Collections;
using Moq;
using Nerdle.Hydra.StateManagement;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    abstract class _on_a_failable_component
    {
        protected Failable<ISomeService> Sut;
        protected Mock<ISomeService> WrappedComponent;
        protected Mock<IStateManager> StateManager;

        [SetUp]
        public virtual void BeforeEach()
        {
            WrappedComponent = new Mock<ISomeService>();
            StateManager = new Mock<IStateManager>();
            Sut = new Failable<ISomeService>("foo123", WrappedComponent.Object, StateManager.Object);
        }
    }
}