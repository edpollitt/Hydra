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
    class When_reading_the_count_from_a_rolling_window
    {
        readonly TimeSpan _windowLength = TimeSpan.FromMinutes(5);
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
        public void The_count_is_returned()
        {
            var now = DateTime.UtcNow;
            _clock.Setup(clock => clock.UtcNow).Returns(now);

            for (var i = 0; i < 10; i++)
            {
                _queue.Enqueue(now);
            }

            _sut.Count.Should().Be(10);
        }

        [Test]
        public void Expired_timestamps_are_removed_from_the_window()
        {
            var now = DateTime.UtcNow;
            _clock.Setup(clock => clock.UtcNow).Returns(now);

            var times = new[]
            {
                now - _windowLength - TimeSpan.FromSeconds(1),
                now - _windowLength - TimeSpan.FromMilliseconds(1),
                now - _windowLength,
                now
            };

            foreach (var time in times)
            {
                _queue.Enqueue(time);
            }

            _sut.Count.Should().Be(2);
            _queue.Should().Equal(times.SkipWhile(time => time < now - _windowLength));
        }
    }

    [TestFixture]
    class When_resetting_a_rolling_window
    {
        [Test]
        public void All_timestamps_in_the_window_are_cleared()
        {
            var queue = new Queue<DateTime>(new[] { DateTime.UtcNow, DateTime.UtcNow });
            var sut = new RollingWindow(TimeSpan.FromSeconds(10), queue, Mock.Of<IClock>());
            sut.Reset();
            queue.Should().BeEmpty();
        }
    }
}