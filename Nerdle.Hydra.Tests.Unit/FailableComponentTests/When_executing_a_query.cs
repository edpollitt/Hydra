using System;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_executing_a_query : _on_a_failable_component
    {
        readonly Func<ISomeService, object> _theQuery = service => service.SomeQuery<object>();

        [Test]
        public void The_query_is_executed_against_the_wrapped_component()
        {
            Sut.Execute(_theQuery);
            WrappedComponent.Verify(service => service.SomeQuery<object>(), Times.Once);
        }

        [TestCase(99)]
        [TestCase(new[] { 1, 2, 3 })]
        [TestCase("foo")]
        public void The_result_of_the_command_is_returned_if_no_error_occurs(object expectedResult)
        {
            WrappedComponent.Setup(service => service.SomeQuery<object>()).Returns(expectedResult);
            var result = Sut.Execute(_theQuery);
            result.Should().Be(expectedResult);
        }

        [Test]
        public void Successful_executions_are_registered_to_the_state_manager()
        {
            Sut.Execute(_theQuery);
            StateManager.Verify(f => f.RegisterFailure(It.IsAny<Exception>()), Times.Never);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Once);
        }

        [Test]
        public void Failed_executions_are_registered_to_the_state_manager()
        {
            var exception = new MissingMethodException();
            WrappedComponent.Setup(component => component.SomeQuery<object>()).Throws(exception);

            Action executing = () => Sut.Execute(_theQuery);

            executing.ShouldThrow<MissingMethodException>();

            StateManager.Verify(f => f.RegisterFailure(exception), Times.Once);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Never);
        }
    }
}