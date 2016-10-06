using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Nerdle.Hydra.Exceptions;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.DynamicClusterTests
{
    [TestFixture]
    class When_removing_a_component : _on_a_dynamic_cluster_of<IComparable>
    {
        [Test]
        public void A_write_lock_is_obtained()
        {
            Sut.Remove(Components[Primary].Object);
            SyncManagerProxy.WriteLocksTaken.Should().Be(1);
        }

        [TestCase(Primary, new[] { Secondary, Tertiary })]
        [TestCase(Secondary, new[] { Primary, Tertiary })]
        [TestCase(Tertiary, new[] { Primary, Secondary })]
        public void The_component_is_removed(string componentToRemove, string[] remainingComponents)
        {
            Sut.Remove(Components[componentToRemove].Object);
            Sut.ComponentIds.Should().Equal(remainingComponents);
        }

        [Test]
        public void An_exception_is_thrown_if_a_sync_lock_cannot_be_obtained_within_the_configured_period()
        {
            Action removing = () => Sut.Remove(Components[Secondary].Object);
            // enter the lock on a different thread
            Task.Run(() => RwLock.EnterWriteLock()).Wait();
            removing.ShouldThrow<LockEntryTimeoutException>();
        }

        [Test]
        public void The_removed_components_failed_event_is_deregistered()
        {
            var eventFired = false;
            Sut.ComponentFailed += (sender, exception) => eventFired = true;
            Sut.Remove(Components[Primary].Object);
            Components[Primary].Raise(component => component.Failed += null, Components[Primary], new InvalidDataException());
            eventFired.Should().BeFalse();
        }

        [Test]
        public void The_removed_components_recovered_event_is_deregistered()
        {
            var eventFired = false;
            Sut.ComponentRecovered += (sender, exception) => eventFired = true;
            Sut.Remove(Components[Secondary].Object);
            Components[Secondary].Raise(component => component.Recovered += null, Components[Secondary], EventArgs.Empty);
            eventFired.Should().BeFalse();
        }
    }
}