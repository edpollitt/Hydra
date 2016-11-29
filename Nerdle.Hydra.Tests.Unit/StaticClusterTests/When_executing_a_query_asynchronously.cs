using System;
using System.Collections;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.Exceptions;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StaticClusterTests
{
    [TestFixture]
    class When_executing_a_query_asynchronously : _on_a_static_cluster_of<ISomeService>
    {
        readonly Func<ISomeService, Task<object>> _theQuery = service => service.SomeAsyncQuery<object>();

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

            var result = await Sut.ExecuteAsync(_theQuery);

            foreach (var component in Components)
                component.Value.Verify(c => c.ExecuteAsync(_theQuery), component.Key == expectedHandler ? Times.Once() : Times.Never());

            result.HandledByComponentId.Should().Be(expectedHandler);
        }

        [TestCase(1)]
        [TestCase(true)]
        [TestCase("foo")]
        public async Task The_query_result_is_returned(object expectedResult)
        {
            Components[Primary].Setup(component => component.IsAvailable).Returns(true);
            Components[Primary].Setup(component => component.ExecuteAsync(_theQuery)).ReturnsAsync(expectedResult);

            var actual = await Sut.ExecuteAsync(_theQuery);

            actual.Result.Should().Be(expectedResult);
        }

        [Test]
        public void An_exception_is_thrown_if_no_working_component_is_available()
        {
            foreach (var component in Components)
                component.Value.Setup(c => c.IsAvailable).Returns(false);

            Action executing = () => Sut.ExecuteAsync(_theQuery).Wait();

            executing.ShouldThrow<ClusterFailureException>().WithMessage("There are no currently available components in the cluster to process the request.");
        }

        [Test]
        public void An_exception_is_thrown_if_components_are_available_but_no_component_successfully_handled_the_request()
        {
            foreach (var component in Components)
            {
                component.Value.Setup(c => c.IsAvailable).Returns(true);
                component.Value.Setup(c => c.ExecuteAsync(_theQuery)).Throws<ArithmeticException>();
            }

            Action executing = () => Sut.ExecuteAsync(_theQuery).Wait();

            executing.ShouldThrow<ClusterFailureException>().WithMessage("There are available components in the cluster, but the request was not successfully processed by any component.")
                .And.InnerException.Should().BeOfType<AggregateException>().Which.InnerExceptions.Should().HaveCount(3);
        }

        [Test]
        public async Task Availability_of_components_is_evaluated_lazily()
        {
            Components[Primary].Setup(component => component.IsAvailable).Returns(false);
            Components[Secondary].Setup(component => component.IsAvailable).Returns(true);

            await Sut.ExecuteAsync(_theQuery);

            foreach (var component in Components)
                component.Value.Verify(c => c.IsAvailable, component.Key == Tertiary ? Times.Never() : Times.Once());
        }
    }
}