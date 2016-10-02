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
    class When_adding_a_component : _on_a_dynamic_cluster_of<Queue>
    {
        [Test]
        public void A_write_lock_is_obtained()
        {
            Sut.Add(Mock.Of<IFailable<Queue>>(), ComponentPriority.First);
            SyncManagerProxy.WriteLocksTaken.Should().Be(1);
        }

        [Test]
        public void The_component_can_be_added_with_first_priority()
        {
            var newComponent = new Mock<IFailable<Queue>>();
            newComponent.Setup(component => component.ComponentId).Returns("foo");
            Sut.Add(newComponent.Object, ComponentPriority.First);
            Sut.ComponentList.Should().Equal("foo", Primary, Secondary, Tertiary);
        }

        [Test]
        public void The_component_can_be_added_with_last_priority()
        {
            var newComponent = new Mock<IFailable<Queue>>();
            newComponent.Setup(component => component.ComponentId).Returns("bar");
            Sut.Add(newComponent.Object, ComponentPriority.Last);
            Sut.ComponentList.Should().Equal(Primary, Secondary, Tertiary, "bar");
        }

        [Test]
        public void An_exception_is_thrown_if_a_sync_lock_cannot_be_obtained_within_the_configured_period()
        {
            Action adding = () => Sut.Add(Mock.Of<IFailable<Queue>>(), ComponentPriority.First);
            // enter the lock on a different thread
            Task.Run(() => RwLock.EnterWriteLock()).Wait();
            adding.ShouldThrow<LockEntryTimeoutException>();
        }
    }
}