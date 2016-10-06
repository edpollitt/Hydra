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

        [Test]
        public void The_added_components_failed_event_is_registered()
        {
            var newComponent = new Mock<IFailable<IList>>();
            Sut.Replace(Components[Primary].Object, newComponent.Object);
            object eventSource = null;
            Sut.ComponentFailed += (sender, exception) => eventSource = sender;

            newComponent.Raise(component => component.Failed += null, newComponent, new FormatException());

            eventSource.Should().Be(newComponent);
        }

        [Test]
        public void The_added_components_recovered_event_is_registered()
        {
            var newComponent = new Mock<IFailable<IList>>();
            Sut.Replace(Components[Secondary].Object, newComponent.Object);
            object eventSource = null;
            Sut.ComponentRecovered += (sender, exception) => eventSource = sender;

            newComponent.Raise(component => component.Recovered += null, newComponent, EventArgs.Empty);

            eventSource.Should().Be(newComponent);
        }

        [Test]
        public void The_replaced_components_failed_event_is_deregistered()
        {
            var eventFired = false;
            Sut.ComponentFailed += (sender, exception) => eventFired = true;
            Sut.Remove(Components[Primary].Object);
            Components[Primary].Raise(component => component.Failed += null, Components[Primary], new InvalidCastException());
            eventFired.Should().BeFalse();
        }

        [Test]
        public void The_replaced_components_recovered_event_is_deregistered()
        {
            var eventFired = false;
            Sut.ComponentRecovered += (sender, exception) => eventFired = true;
            Sut.Remove(Components[Secondary].Object);
            Components[Secondary].Raise(component => component.Recovered += null, Components[Secondary], EventArgs.Empty);
            eventFired.Should().BeFalse();
        }
    }
}