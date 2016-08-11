using System;
using FluentAssertions;
using Nerdle.Hydra.StateManagement;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StateManagement.RollingWindowTests
{
    [TestFixture]
    class When_instantiating_a_rolling_window
    {
        static readonly TimeSpan[] NonPositiveTimeSpans =
        {
            TimeSpan.FromSeconds(-1),
            TimeSpan.Zero
        };

        [TestCaseSource(nameof(NonPositiveTimeSpans))]
        public void The_window_length_must_be_greater_than_zero(TimeSpan nonPositive)
        {
            Action instantiating = () => new RollingWindow(nonPositive);
            instantiating.ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("windowLength");
        }
    }
}