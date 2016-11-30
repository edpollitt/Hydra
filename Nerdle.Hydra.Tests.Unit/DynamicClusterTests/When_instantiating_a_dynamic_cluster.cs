using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.InfrastructureAbstractions;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.DynamicClusterTests
{
    [TestFixture]
    class When_instantiating_a_dynamic_cluster
    {
        [Test]
        public void A_component_list_must_be_supplied()
        {
            Action instantiating = () => new DynamicCluster<ISomeService>(null, Mock.Of<ISyncManager>());
            instantiating.ShouldThrow<ArgumentNullException>()
                .Which.ParamName.Should().Be("components");
        }

        [Test]
        public void A_sync_manager_must_be_supplied()
        {
            Action instantiating = () => new DynamicCluster<ISomeService>(Enumerable.Empty<IFailable<ISomeService>>(), null);
            instantiating.ShouldThrow<ArgumentNullException>()
                .Which.ParamName.Should().Be("syncManager");
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

            var cluster = new DynamicCluster<ISomeService>(components, new SyncManager(new ReaderWriterLockSlim()));

            components.RemoveAt(0);

            cluster.ComponentIds.Should().Equal(originalComponentIds);
        }
    }
}