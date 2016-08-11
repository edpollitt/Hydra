using System;

namespace Nerdle.Hydra.StateManagement
{
    class FailureRatioWithMinimumSampleSizeCondition : ICondition<int, int>
    {
        readonly double _failureRatio;
        readonly int _minimumSampleSize;

        public FailureRatioWithMinimumSampleSizeCondition(double failureRatio, int minimumSampleSize)
        {
            _failureRatio = failureRatio;
            _minimumSampleSize = minimumSampleSize;
        }

        public bool Evaluate(int successCount, int failureCount)
        {
            var totalCount = successCount + failureCount;

            if (totalCount < _minimumSampleSize)
                return false;

            var failurePercentage = failureCount / (double)totalCount;

            return failurePercentage >= _failureRatio;
        }
    }
}