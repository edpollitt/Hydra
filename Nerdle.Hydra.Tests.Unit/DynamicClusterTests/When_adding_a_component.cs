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
    class When_adding_a_component : _on_a_dynamic_cluster_of<ISomeService>
    {
        [Test]
        public void A_write_lock_is_obtained()
        {
            Sut.Add(Mock.Of<IFailable<ISomeService>>(), ComponentPosition.First);
            SyncManagerProxy.WriteLocksTaken.Should().Be(1);
        }

        [Test]
        public void The_component_can_be_added_in_first_position()
        {
            var newComponent = new Mock<IFailable<ISomeService>>();
            newComponent.Setup(component => component.ComponentId).Returns("foo");
            Sut.Add(newComponent.Object, ComponentPosition.First);
            Sut.ComponentIds.Should().Equal("foo", Primary, Secondary, Tertiary);
        }

        [Test]
        public void The_component_can_be_added_in_last_position()
        {
            var newComponent = new Mock<IFailable<ISomeService>>();
            newComponent.Setup(component => component.ComponentId).Returns("bar");
            Sut.Add(newComponent.Object, ComponentPosition.Last);
            Sut.ComponentIds.Should().Equal(Primary, Secondary, Tertiary, "bar");
        }

        [Test]
        public void An_exception_is_thrown_if_a_sync_lock_cannot_be_obtained_within_the_configured_period()
        {
            Action adding = () => Sut.Add(Mock.Of<IFailable<ISomeService>>(), ComponentPosition.First);
            // enter the lock on a different thread
            Task.Run(() => RwLock.EnterWriteLock()).Wait();
            adding.ShouldThrow<LockEntryTimeoutException>();
        }

        [Test]
        public void The_added_components_failed_event_is_registered()
        {
            var newComponent = new Mock<IFailable<ISomeService>>();
            Sut.Add(newComponent.Object, ComponentPosition.First);
            object eventSource = null;
            Sut.ComponentFailed += (sender, exception) => eventSource = sender;

            newComponent.Raise(component => component.Failed += null, newComponent, new FormatException());

            eventSource.Should().Be(newComponent);
        }

        [Test]
        public void The_added_components_recovered_event_is_registered()
        {
            var newComponent = new Mock<IFailable<ISomeService>>();
            Sut.Add(newComponent.Object, ComponentPosition.Last);
            object eventSource = null;
            Sut.ComponentRecovered += (sender, exception) => eventSource = sender;

            newComponent.Raise(component => component.Recovered += null, newComponent, EventArgs.Empty);

            eventSource.Should().Be(newComponent);
        }
    }
}