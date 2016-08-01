using System;
using System.Collections;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_executing_a_command
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
        public void The_command_is_executed_against_the_wrapped_component()
        {
            object commandTarget = null;
            Action<IList> theCommand = list => commandTarget = list;

            _sut.Execute(theCommand);

            commandTarget.Should().Be(_wrappedComponent);
        }

        [Test]
        public void Successful_executions_are_registered_to_the_state_manager()
        {
            _sut.Execute(list => { });
            _stateManager.Verify(f => f.RegisterFailure(It.IsAny<Exception>()), Times.Never);
            _stateManager.Verify(f => f.RegisterSuccess(), Times.Once);
        }

        [Test]
        public void Failed_executions_are_registered_to_the_state_manager()
        {
            var theException = new IndexOutOfRangeException();
            try
            {
                _sut.Execute(list => { throw theException; });
            }
            catch (IndexOutOfRangeException) { }

            _stateManager.Verify(f => f.RegisterFailure(theException), Times.Once);
            _stateManager.Verify(f => f.RegisterSuccess(), Times.Never);
        }
    }
}
