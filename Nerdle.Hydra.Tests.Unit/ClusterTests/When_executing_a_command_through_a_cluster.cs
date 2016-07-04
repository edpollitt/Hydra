using System;
using System.Collections;
using System.Linq;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using FluentAssertions;
using Nerdle.Hydra.Exceptions;

namespace Nerdle.Hydra.Tests.Unit.ClusterTests
{
    [TestFixture]
    class When_executing_a_command_through_a_cluster
    {
        const string Primary = "primary";
        const string Secondary = "secondary";
        const string Tertiary = "tertiary";

        IDictionary<string, Mock<IFailable<Stack>>> _components;
        readonly Action<Stack> _theCommand = service => service.Clear();

        Cluster<Stack> _sut;
        
        [SetUp]
        public void BeforeEach()
        {
            _components = new[] { Primary, Secondary, Tertiary }
                .Select(id =>
                {
                    var component = new Mock<IFailable<Stack>>();
                    component.Setup(c => c.Id).Returns(id);
                    return component;
                })
                .ToDictionary(component => component.Object.Id, component => component);

            _sut = new Cluster<Stack>(_components.Values.Select(mock => mock.Object));
        }

        [TestCase(true, true, true, Primary)]
        [TestCase(true, false, false, Primary)]
        [TestCase(false, true, true, Secondary)]
        [TestCase(false, true, false, Secondary)]
        [TestCase(false, false, true, Tertiary)]
        public void The_call_is_routed_to_the_first_working_component(bool primaryAvailability, bool secondaryAvailability, bool tertiaryAvailability, string expectedHandler)
        {
            _components[Primary].Setup(component => component.IsAvailable).Returns(primaryAvailability);
            _components[Secondary].Setup(component => component.IsAvailable).Returns(secondaryAvailability);
            _components[Tertiary].Setup(component => component.IsAvailable).Returns(tertiaryAvailability);
            
            var result = _sut.Execute(_theCommand);

            foreach (var component in _components)
                component.Value.Verify(c => c.Execute(_theCommand), component.Key == expectedHandler ? Times.Once() : Times.Never());

            result.HandlerId.Should().Be(expectedHandler);
        }

        [Test]
        public void An_exception_is_thrown_if_no_working_component_is_available()
        {
            foreach (var component in _components)
                component.Value.Setup(c => c.IsAvailable).Returns(false);

            Action executing = () => _sut.Execute(_theCommand);

            executing.ShouldThrow<ClusterFailureException>().WithMessage("There are no currently available components in the cluster to process the request.");
        }

        [Test]
        public void An_exception_is_thrown_if_components_are_available_but_no_component_successfully_handled_the_request()
        {
            foreach (var component in _components)
            {
                component.Value.Setup(c => c.IsAvailable).Returns(true);
                component.Value.Setup(c => c.Execute(_theCommand)).Throws<InvalidOperationException>();
            }

            Action executing = () => _sut.Execute(_theCommand);

            executing.ShouldThrow<ClusterFailureException>().WithMessage("There are available components in the cluster, but the request was not successfully processed by any component.")
                .And.InnerException.Should().BeOfType<AggregateException>().Which.InnerExceptions.Count.Should().Be(3);
        }

        [Test]
        public void Availability_of_components_is_evaluated_lazily()
        {
            _components[Primary].Setup(component => component.IsAvailable).Returns(false);
            _components[Secondary].Setup(component => component.IsAvailable).Returns(true);

            var result = _sut.Execute(_theCommand);

            foreach (var component in _components)
                component.Value.Verify(c => c.IsAvailable, component.Key == Tertiary ? Times.Never() : Times.Once());
        }
    }
}
