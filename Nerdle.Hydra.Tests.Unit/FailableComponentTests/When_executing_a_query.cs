using System;
using System.Collections;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_executing_a_query
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

        [Test]
        public void The_query_is_executed_against_the_wrapped_component()
        {
            object queryTarget = null;
            Func<IList, int> theQuery = list => { queryTarget = list; return list.Count; };

            _sut.Execute(theQuery);

            queryTarget.Should().Be(_wrappedComponent);
        }

        [TestCase(99)]
        [TestCase(new[] { 1, 2, 3 })]
        [TestCase("foo")]
        public void The_result_of_the_command_is_returned_if_no_error_occurs(object componentResult)
        {
            var result = _sut.Execute(component => componentResult);
            result.Should().Be(componentResult);
        }

        [Test]
        public void Successful_executions_are_registered_to_the_state_manager()
        {
            _sut.Execute(list => 1);
            _stateManager.Verify(f => f.RegisterFailure(It.IsAny<Exception>()), Times.Never);
            _stateManager.Verify(f => f.RegisterSuccess(), Times.Once);
        }

        [Test]
        public void Failed_executions_are_registered_to_the_state_manager()
        {
            var theException = new IndexOutOfRangeException();
            try
            {
                Func<IList, int> theQuery = list => { throw theException; };
                _sut.Execute(theQuery);
            }
            catch (IndexOutOfRangeException) { }

            _stateManager.Verify(f => f.RegisterFailure(theException), Times.Once);
            _stateManager.Verify(f => f.RegisterSuccess(), Times.Never);
        }
    }
}