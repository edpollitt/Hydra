using System;
using System.Runtime.Serialization;
using FluentAssertions;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.ClusterTests
{
    [TestFixture]
    class When_subscribed_to_the_component_failed_event : _on_a_cluster_of<int>
    {
        [Test]
        public void The_event_fires_each_time_a_component_fails()
        {
            var eventsFired = 0;
            Sut.ComponentFailed += (sender, exception) => eventsFired++;
            Components[Primary].Raise(component => component.Failed += null, Components[Primary], new InvalidOperationException());
            Components[Secondary].Raise(component => component.Failed += null, Components[Secondary], new DivideByZeroException());
            Components[Primary].Raise(component => component.Failed += null, Components[Primary], new ArgumentOutOfRangeException());
            eventsFired.Should().Be(3);
        }

        [Test]
        public void The_exception_is_reported_when_the_event_fires()
        {
            Exception reportedException = null;
            var thrownException = new SerializationException();
            Sut.ComponentFailed += (sender, exception) => reportedException = exception;
            Components[Primary].Raise(component => component.Failed += null, Components[Primary], thrownException);
            reportedException.Should().Be(thrownException);
        }
    }
}
