using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_executing_a_query_asynchronously : _on_a_failable_component
    {
        readonly Func<ISomeService, Task<object>> _theQuery = service => service.SomeAsyncQuery<object>();

        [Test]
        public async Task The_query_is_executed_against_the_wrapped_component()
        {
            await Sut.ExecuteAsync(_theQuery);
            WrappedComponent.Verify(service => service.SomeAsyncQuery<object>(), Times.Once);
        }

        [TestCase(99)]
        [TestCase(new[] { 1, 2, 3 })]
        [TestCase("foo")]
        public async Task The_result_of_the_command_is_returned_if_no_error_occurs(object expectedResult)
        {
            WrappedComponent.Setup(service => service.SomeAsyncQuery<object>()).ReturnsAsync(expectedResult);
            var result = await Sut.ExecuteAsync(_theQuery);
            result.Should().Be(expectedResult);
        }

        [Test]
        public async Task Successful_executions_are_registered_to_the_state_manager()
        {
            await Sut.ExecuteAsync(_theQuery);
            StateManager.Verify(f => f.RegisterFailure(It.IsAny<Exception>()), Times.Never);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Once);
        }

        [Test]
        public void Failed_executions_are_registered_to_the_state_manager()
        {
            var exception = new InvalidCastException();
            WrappedComponent.Setup(component => component.SomeAsyncQuery<object>()).Throws(exception);

            Action executing = () => Sut.ExecuteAsync(_theQuery).Wait();

            executing.ShouldThrow<InvalidCastException>();

            StateManager.Verify(f => f.RegisterFailure(exception), Times.Once);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Never);
        }
    }
}