using System;
using System.Collections;
using System.Threading.Tasks;
using FluentAssertions;
using Nerdle.Hydra.Exceptions;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.DynamicClusterTests
{
    [TestFixture]
    class When_removing_a_component : _on_a_dynamic_cluster_of<Queue>
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
            Sut.ComponentList.Should().Equal(remainingComponents);
        }

        [Test]
        public void An_exception_is_thrown_if_a_sync_lock_cannot_be_obtained_within_the_configured_period()
        {
            Action removing = () => Sut.Remove(Components[Secondary].Object);
            // enter the lock on a different thread
            Task.Run(() => RwLock.EnterWriteLock()).Wait();
            removing.ShouldThrow<LockEntryTimeoutException>();
        }
    }
}