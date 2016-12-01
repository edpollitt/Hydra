using System;
using FluentAssertions;
using Nerdle.Hydra.Tests.Unit.TestHelpers;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StaticClusterTests
{
    [TestFixture]
    class When_subscribed_to_the_component_recovered_event : _on_a_static_cluster_of<ISomeService>
    {
        [Test]
        public void The_event_fires_each_time_a_component_recovers()
        {
            var eventsFired = 0;
            Sut.ComponentRecovered += (sender, exception) => eventsFired++;
            Components[Tertiary].Raise(component => component.Recovered += null, Components[Tertiary], EventArgs.Empty);
            Components[Primary].Raise(component => component.Recovered += null, Components[Primary], EventArgs.Empty);
            eventsFired.Should().Be(2);
        }
    }
}