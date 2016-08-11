using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Nerdle.Hydra.InfrastructureAbstractions;
using Nerdle.Hydra.StateManagement;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowTests
{
    [TestFixture]
    class When_marking_a_rolling_window
    {
        readonly TimeSpan _windowLength = TimeSpan.FromMinutes(1);
        Queue<DateTime> _queue;
        Mock<IClock> _clock;
        RollingWindow _sut;

        [SetUp]
        public void BeforeEach()
        {
            _queue = new Queue<DateTime>();
            _clock = new Mock<IClock>();
            _sut = new RollingWindow(_windowLength, _queue, _clock.Object);
        }

        [Test]
        public void The_current_timestamp_is_added_to_the_window()
        {
            var now = DateTime.UtcNow;

            var times = new[]
            {
                now - TimeSpan.FromSeconds(11),
                now - TimeSpan.FromMinutes(3),
                now,
                now,
            };

            foreach (var time in times)
            {
                _clock.Setup(clock => clock.UtcNow).Returns(time);
                _sut.Mark();
            }

            _queue.Count.Should().Be(4);
            _queue.Should().Equal(times);
        }

        [Test]
        public void Expired_timestamps_are_removed_from_the_window()
        {
            var now = DateTime.UtcNow;

            var times = new[]
            {
                now - _windowLength - TimeSpan.FromMinutes(1),
                now - _windowLength - TimeSpan.FromSeconds(1),
                now - _windowLength,
                now
            };

            foreach (var time in times)
            {
                _clock.Setup(clock => clock.UtcNow).Returns(time);
                _sut.Mark();
            }

            _queue.Count.Should().Be(2);
            _queue.Should().Equal(times.SkipWhile(time => time < now - _windowLength));
        }
    }
}