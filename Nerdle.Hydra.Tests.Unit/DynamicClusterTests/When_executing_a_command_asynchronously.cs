using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.Exceptions;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.DynamicClusterTests
{
    [TestFixture]
    class When_executing_a_command_asynchronously : _on_a_dynamic_cluster_of<ISomeService>
    {
        readonly Func<ISomeService, Task> _theCommand = service => service.SomeAsyncCommand();
        
        [TestCase(true, true, true, Primary)]
        [TestCase(true, false, false, Primary)]
        [TestCase(false, true, true, Secondary)]
        [TestCase(false, true, false, Secondary)]
        [TestCase(false, false, true, Tertiary)]
        public async Task The_call_is_routed_to_the_first_working_component(bool primaryAvailability, bool secondaryAvailability, bool tertiaryAvailability, string expectedHandler)
        {
            Components[Primary].Setup(component => component.IsAvailable).Returns(primaryAvailability);
            Components[Secondary].Setup(component => component.IsAvailable).Returns(secondaryAvailability);
            Components[Tertiary].Setup(component => component.IsAvailable).Returns(tertiaryAvailability);
            
            var result = await Sut.ExecuteAsync(_theCommand);

            foreach (var component in Components)
                component.Value.Verify(c => c.ExecuteAsync(_theCommand), component.Key == expectedHandler ? Times.Once() : Times.Never());

            result.HandledByComponentId.Should().Be(expectedHandler);
        }

        [Test]
        public void An_exception_is_thrown_if_no_working_component_is_available()
        {
            foreach (var component in Components)
                component.Value.Setup(c => c.IsAvailable).Returns(false);

            Action executing = () => Sut.ExecuteAsync(_theCommand).Wait();

            executing.ShouldThrow<ClusterFailureException>().WithMessage("There are no currently available components in the cluster to process the request.");
        }

        [Test]
        public void An_exception_is_thrown_if_components_are_available_but_no_component_successfully_handled_the_request()
        {
            foreach (var component in Components)
            {
                component.Value.Setup(c => c.IsAvailable).Returns(true);
                component.Value.Setup(c => c.ExecuteAsync(_theCommand)).Throws<InvalidOperationException>();
            }

            Action executing = () => Sut.ExecuteAsync(_theCommand).Wait();

            executing.ShouldThrow<ClusterFailureException>().WithMessage("There are available components in the cluster, but the request was not successfully processed by any component.")
                .And.InnerException.Should().BeOfType<AggregateException>().Which.InnerExceptions.Should().HaveCount(3);
        }

        [Test]
        public async Task Availability_of_components_is_evaluated_lazily()
        {
            Components[Primary].Setup(component => component.IsAvailable).Returns(false);
            Components[Secondary].Setup(component => component.IsAvailable).Returns(true);

            await Sut.ExecuteAsync(_theCommand);

            foreach (var component in Components)
                component.Value.Verify(c => c.IsAvailable, component.Key == Tertiary ? Times.Never() : Times.Once());
        }
    }
}
