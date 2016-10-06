using System;
using System.Collections;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.Exceptions;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.DynamicClusterTests
{
    [TestFixture]
    class When_replacing_a_component : _on_a_dynamic_cluster_of<IList>
    {
        [Test]
        public void A_write_lock_is_obtained()
        {
            Sut.Replace(Components[Primary].Object, Mock.Of<IFailable<IList>>());
            SyncManagerProxy.WriteLocksTaken.Should().Be(1);
        }

        [TestCase(Primary, "foo", new[] { "foo", Secondary, Tertiary} )]
        [TestCase(Secondary, "bar", new[] { Primary, "bar", Tertiary })]
        [TestCase(Tertiary, "baz", new[] { Primary, Secondary, "baz" })]
        public void The_component_is_replaced(string oldComponentId, string newComponentId, string[] resultingComponentIds)
        {
            var newComponent = new Mock<IFailable<IList>>();
            newComponent.Setup(component => component.ComponentId).Returns(newComponentId);
            Sut.Replace(Components[oldComponentId].Object, newComponent.Object);
            Sut.ComponentIds.Should().Equal(resultingComponentIds);
        }

        [Test]
        public void An_exception_is_thrown_if_a_sync_lock_cannot_be_obtained_within_the_configured_period()
        {
            Action replacing = () => Sut.Replace(Components[Primary].Object, Mock.Of<IFailable<IList>>());
            // enter the lock on a different thread
            Task.Run(() => RwLock.EnterWriteLock()).Wait();
            replacing.ShouldThrow<LockEntryTimeoutException>();
        }
    }
}