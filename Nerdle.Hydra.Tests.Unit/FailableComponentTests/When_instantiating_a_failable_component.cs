using System;
using System.Collections;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.StateManagement;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.FailableComponentTests
{
    [TestFixture]
    class When_instantiating_a_failable_component
    {
        [TestCase("123")]
        [TestCase("foo")]
        [TestCase(null)]
        public void The_component_id_is_set(string componentId)
        {
            var sut = new Failable<ISomeService>(componentId, Mock.Of<ISomeService>(), Mock.Of<IStateManager>());
            sut.ComponentId.Should().Be(componentId);
        }

        [Test]
        public void A_component_must_be_supplied()
        {
            Action instantiating = () => new Failable<ISomeService>("foo", null, Mock.Of<IStateManager>());
            instantiating.ShouldThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("component");
        }

        [Test]
        public void A_state_manager_must_be_supplied()
        {
            Action instantiating = () => new Failable<ISomeService>("bar", Mock.Of<ISomeService>(), null);
            instantiating.ShouldThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("stateManager");
        }
    }
}