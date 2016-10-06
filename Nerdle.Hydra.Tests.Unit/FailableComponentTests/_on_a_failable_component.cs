using System.Collections;
using Moq;
using Nerdle.Hydra.StateManagement;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    abstract class _on_a_failable_component
    {
        protected Failable<IList> Sut;
        protected IList WrappedComponent;
        protected Mock<IStateManager> StateManager;

        [SetUp]
        public virtual void BeforeEach()
        {
            WrappedComponent = new ArrayList();
            StateManager = new Mock<IStateManager>();
            Sut = new Failable<IList>("foo123", WrappedComponent, StateManager.Object);
        }
    }
}