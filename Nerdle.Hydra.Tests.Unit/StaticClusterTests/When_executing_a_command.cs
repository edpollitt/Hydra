﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.Exceptions;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StaticClusterTests
{
    [TestFixture]
    class When_executing_a_command : _on_a_static_cluster_of<ISomeService>
    {
        readonly Action<ISomeService> _theCommand = service => service.SomeCommand();

        [TestCase(true, true, true, Primary)]
        [TestCase(true, false, false, Primary)]
        [TestCase(false, true, true, Secondary)]
        [TestCase(false, true, false, Secondary)]
        [TestCase(false, false, true, Tertiary)]
        public void The_call_is_routed_to_the_first_working_component(bool primaryAvailability, bool secondaryAvailability, bool tertiaryAvailability, string expectedHandler)
        {
            Components[Primary].Setup(component => component.IsAvailable).Returns(primaryAvailability);
            Components[Secondary].Setup(component => component.IsAvailable).Returns(secondaryAvailability);
            Components[Tertiary].Setup(component => component.IsAvailable).Returns(tertiaryAvailability);
            
            var result = Sut.Execute(_theCommand);

            foreach (var component in Components)
                component.Value.Verify(c => c.Execute(_theCommand), component.Key == expectedHandler ? Times.Once() : Times.Never());

            result.HandledByComponentId.Should().Be(expectedHandler);
        }

        [TestCase(true, true, true, Tertiary)]
        [TestCase(true, false, false, Primary)]
        [TestCase(false, true, true, Tertiary)]
        [TestCase(false, true, false, Secondary)]
        [TestCase(false, false, true, Tertiary)]
        public void Default_component_order_can_be_overridden(bool primaryAvailability, bool secondaryAvailability, bool tertiaryAvailability, string expectedHandler)
        {
            Traversal.Setup(t => t.Traverse(It.IsAny<IList<IFailable<ISomeService>>>()))
                .Returns<IList<IFailable<ISomeService>>>(components => components.Reverse());

            Components[Primary].Setup(component => component.IsAvailable).Returns(primaryAvailability);
            Components[Secondary].Setup(component => component.IsAvailable).Returns(secondaryAvailability);
            Components[Tertiary].Setup(component => component.IsAvailable).Returns(tertiaryAvailability);

            var result = Sut.Execute(_theCommand);

            foreach (var component in Components)
                component.Value.Verify(c => c.Execute(_theCommand), component.Key == expectedHandler ? Times.Once() : Times.Never());

            result.HandledByComponentId.Should().Be(expectedHandler);
        }

        [Test]
        public void An_exception_is_thrown_if_no_working_component_is_available()
        {
            foreach (var component in Components)
                component.Value.Setup(c => c.IsAvailable).Returns(false);

            Action executing = () => Sut.Execute(_theCommand);

            executing.ShouldThrow<ClusterFailureException>().WithMessage("There are no currently available components in the cluster to process the request.");
        }

        [Test]
        public void An_exception_is_thrown_if_components_are_available_but_no_component_successfully_handled_the_request()
        {
            foreach (var component in Components)
            {
                component.Value.Setup(c => c.IsAvailable).Returns(true);
                component.Value.Setup(c => c.Execute(_theCommand)).Throws<InvalidOperationException>();
            }

            Action executing = () => Sut.Execute(_theCommand);

            executing.ShouldThrow<ClusterFailureException>().WithMessage("There are available components in the cluster, but the request was not successfully processed by any component.")
                .And.InnerException.Should().BeOfType<AggregateException>().Which.InnerExceptions.Should().HaveCount(3);
        }

        [Test]
        public void Availability_of_components_is_evaluated_lazily()
        {
            Components[Primary].Setup(component => component.IsAvailable).Returns(false);
            Components[Secondary].Setup(component => component.IsAvailable).Returns(true);

            Sut.Execute(_theCommand);

            foreach (var component in Components)
                component.Value.Verify(c => c.IsAvailable, component.Key == Tertiary ? Times.Never() : Times.Once());
        }
    }
}
