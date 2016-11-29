using System;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_executing_a_command : _on_a_failable_component
    {
        readonly Action<ISomeService> _theCommand = service => service.SomeCommand();

        [Test]
        public void The_command_is_executed_against_the_wrapped_component()
        {
            Sut.Execute(_theCommand);
            WrappedComponent.Verify(component => component.SomeCommand(), Times.Once);
        }

        [Test]
        public void Successful_executions_are_registered_to_the_state_manager()
        {
            Sut.Execute(_theCommand);
            StateManager.Verify(f => f.RegisterFailure(It.IsAny<Exception>()), Times.Never);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Once);
        }

        [Test]
        public void Failed_executions_are_registered_to_the_state_manager()
        {
            var exception = new IndexOutOfRangeException();
            WrappedComponent.Setup(component => component.SomeCommand()).Throws(exception);

            Action executing = () => Sut.Execute(_theCommand);

            executing.ShouldThrow<IndexOutOfRangeException>();

            StateManager.Verify(f => f.RegisterFailure(exception), Times.Once);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Never);
        }
    }
}
