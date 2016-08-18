namespace Nerdle.Hydra.StateManagement
{
    class FailurePercentageWithMinimumSampleSizeCondition : ICondition<int, int>
    {
        readonly double _failurePercentageThreshold;
        readonly int _minimumSampleSize;

        public FailurePercentageWithMinimumSampleSizeCondition(double failurePercentageThreshold, int minimumSampleSize)
        {
            _failurePercentageThreshold = failurePercentageThreshold;
            _minimumSampleSize = minimumSampleSize;
        }

        public bool IsMet(int successCount, int failureCount)
        {
            var totalCount = successCount + failureCount;

            if (totalCount < _minimumSampleSize)
                return false;

            var failurePercentage = failureCount / (double)totalCount;

            return failurePercentage >= _failurePercentageThreshold;
        }
    }
}