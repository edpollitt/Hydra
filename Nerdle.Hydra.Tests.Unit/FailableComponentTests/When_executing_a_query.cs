using System;
using System.Collections;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_executing_a_query : _on_a_failable_component
    {
        [Test]
        public void The_query_is_executed_against_the_wrapped_component()
        {
            object queryTarget = null;
            Func<IList, int> theQuery = list => { queryTarget = list; return list.Count; };

            Sut.Execute(theQuery);

            queryTarget.Should().Be(WrappedComponent);
        }

        [TestCase(99)]
        [TestCase(new[] { 1, 2, 3 })]
        [TestCase("foo")]
        public void The_result_of_the_command_is_returned_if_no_error_occurs(object componentResult)
        {
            var result = Sut.Execute(component => componentResult);
            result.Should().Be(componentResult);
        }

        [Test]
        public void Successful_executions_are_registered_to_the_state_manager()
        {
            Sut.Execute(list => 1);
            StateManager.Verify(f => f.RegisterFailure(It.IsAny<Exception>()), Times.Never);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Once);
        }

        [Test]
        public void Failed_executions_are_registered_to_the_state_manager()
        {
            var exception = new IndexOutOfRangeException();
            try
            {
                Func<IList, int> theQuery = list => { throw exception; };
                Sut.Execute(theQuery);
            }
            catch (IndexOutOfRangeException) { }

            StateManager.Verify(f => f.RegisterFailure(exception), Times.Once);
            StateManager.Verify(f => f.RegisterSuccess(), Times.Never);
        }
    }
}