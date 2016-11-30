using System;
using System.Linq;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StaticClusterTests
{
    [TestFixture]
    class When_instantiating_a_static_cluster
    {
        [Test]
        public void A_component_list_must_be_supplied()
        {
            Action instantiating = () => new StaticCluster<ISomeService>(null);
            instantiating.ShouldThrow<ArgumentNullException>()
                .Which.ParamName.Should().Be("components");
        }

        [Test]
        public void The_component_list_is_cloned()
        {
            var originalComponentIds = new[] { "foo", "bar", "baz" };
            var components = originalComponentIds.Select(id =>
            {
                var component = new Mock<IFailable<ISomeService>>();
                component.Setup(m => m.ComponentId).Returns(id);
                return component.Object;
            })
            .ToList();

            var cluster = new StaticCluster<ISomeService>(components);

            components.RemoveAt(0);
     
            cluster.ComponentIds.Should().Equal(originalComponentIds);
        }
    }
}
