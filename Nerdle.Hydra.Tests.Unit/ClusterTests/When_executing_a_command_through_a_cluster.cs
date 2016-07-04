using System;
using System.Collections;
using System.Linq;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using FluentAssertions;

namespace Nerdle.Hydra.Tests.Unit.ClusterTests
{
    [TestFixture]
    class When_executing_a_command_through_a_cluster
    {
        IDictionary<string, Mock<IFailable<Stack>>> _components;

        Cluster<Stack> _sut;

        [SetUp]
        public void BeforeEach()
        {
            _components = new[] { "primary", "secondary", "tertiary" }
                .Select(id =>
                {
                    var component = new Mock<IFailable<Stack>>();
                    component.Setup(c => c.Id).Returns(id);
                    return component;
                })
                .ToDictionary(component => component.Object.Id, component => component);

            _sut = new Cluster<Stack>(_components.Values.Select(mock => mock.Object));
        }

        [TestCase(true, true, true, "primary")]
        [TestCase(true, false, false, "primary")]
        [TestCase(false, true, true, "secondary")]
        [TestCase(false, true, false, "secondary")]
        [TestCase(false, false, true, "tertiary")]
        public void The_call_is_routed_to_the_first_working_component(bool primaryAvailability, bool secondaryAvailability, bool tertiaryAvailability, string expectedHandler)
        {
            _components["primary"].Setup(component => component.IsAvailable).Returns(primaryAvailability);
            _components["secondary"].Setup(component => component.IsAvailable).Returns(secondaryAvailability);
            _components["tertiary"].Setup(component => component.IsAvailable).Returns(tertiaryAvailability);

            Action<Stack> theCommand = service => service.Clear();

            var result = _sut.Execute(theCommand);

            foreach (var component in _components)
                component.Value.Verify(c => c.Execute(theCommand), component.Key == expectedHandler ? Times.Once() : Times.Never());

            result.HandlerId.Should().Be(expectedHandler);
        }
    }
}
