using System;
using System.Collections;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Moq;
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
            var sut = new Failable<IEnumerable>(new ArrayList(), componentId, Mock.Of<IStateManager>());
            sut.ComponentId.Should().Be(componentId);
        }

        [Test]
        public void A_component_must_be_supplied()
        {
            Action instantiating = () => new Failable<ICollection>(null, string.Empty, Mock.Of<IStateManager>());

            instantiating.ShouldThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("component");
        }

        [Test]
        public void A_state_manager_must_be_supplied()
        {
            Action instantiating = () => new Failable<ICollection>(new int[0], string.Empty, null);

            instantiating.ShouldThrowExactly<ArgumentNullException>().Which.ParamName.Should().Be("stateManager");
        }
    }
}