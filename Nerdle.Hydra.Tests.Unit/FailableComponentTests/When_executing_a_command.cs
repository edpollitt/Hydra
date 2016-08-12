using System;
using System.Collections;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_executing_a_command : _against_a_failable_component
    {
        [Test]
        public void The_command_is_executed_against_the_wrapped_component()
        {
            object commandTarget = null;
            Action<IList> theCommand = list => commandTarget = list;

            Sut.Execute(theCommand);

            commandTarget.Should().Be(WrappedComponent);
        }

        [Test]
        public void Successful_executions_are_registered_to_the_state_manager()
        {
            Sut.Execute(list => { });
            StateManager.Verify(f => f.RegisterFailure(It.IsAny<Exception>()), Times.Never);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Once);
        }

        [Test]
        public void Failed_executions_are_registered_to_the_state_manager()
        {
            var exception = new IndexOutOfRangeException();
            try
            {
                Sut.Execute(list => { throw exception; });
            }
            catch (IndexOutOfRangeException) { }

            StateManager.Verify(f => f.RegisterFailure(exception), Times.Once);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Never);
        }
    }
}
