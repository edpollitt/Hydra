using System;
using System.Collections;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_executing_a_command_asynchronously : _on_a_failable_component
    {
        readonly Func<ISomeService, Task> _theCommand = service => service.SomeAsyncCommand();

        [Test]
        public async Task The_command_is_executed_against_the_wrapped_component()
        {
            await Sut.ExecuteAsync(_theCommand);
            WrappedComponent.Verify(component => component.SomeAsyncCommand(), Times.Once);
        }

        [Test]
        public async Task Successful_executions_are_registered_to_the_state_manager()
        {
            await Sut.ExecuteAsync(_theCommand);
            StateManager.Verify(f => f.RegisterFailure(It.IsAny<Exception>()), Times.Never);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Once);
        }

        [Test]
        public void Failed_executions_are_registered_to_the_state_manager()
        {
            var exception = new OutOfMemoryException();
            WrappedComponent.Setup(component => component.SomeAsyncCommand()).Throws(exception);

            Action executing = () => Sut.ExecuteAsync(_theCommand).Wait();

            executing.ShouldThrow<OutOfMemoryException>();

            StateManager.Verify(f => f.RegisterFailure(exception), Times.Once);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Never);
        }
    }
}