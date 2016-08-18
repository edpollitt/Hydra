using FluentAssertions;
using Nerdle.Hydra.StateManagement;
using NUnit.Framework;

namespace Nerdle.Hydra.Tests.Unit.StateManagement.FailureRatioWithMinimumSampleSizeConditionTests
{
    [TestFixture]
    class When_evaluating_the_condition
    {
        const double FailureRatio = 0.5;
        const int MinimumSampleSize = 10;

        readonly FailurePercentageWithMinimumSampleSizeCondition _sut = new FailurePercentageWithMinimumSampleSizeCondition(FailureRatio, MinimumSampleSize);

        [TestCase(5, 5, true)]
        [TestCase(5, 6, true)]
        [TestCase(6, 5, false)]
        [TestCase(0, 10, true)]
        [TestCase(10, 0, false)]
        public void The_condition_requires_the_failure_percentage_threshold_to_be_met(int successCount, int failureCount, bool conditionMet)
        {
            _sut.IsMet(successCount, failureCount).Should().Be(conditionMet);
        }

        [TestCase(4, 4)]
        [TestCase(0, 9)]
        [TestCase(9, 0)]
        [TestCase(0, 0)]
        public void The_condition_requires_the_minimum_sample_size_to_be_met(int successCount, int failureCount)
        {
            _sut.IsMet(successCount, failureCount).Should().BeFalse();
        }
    }
}
