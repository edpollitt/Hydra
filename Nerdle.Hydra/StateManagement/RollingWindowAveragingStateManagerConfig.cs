using System;

namespace Nerdle.Hydra.StateManagement
{
    public class RollingWindowAveragingStateManagerConfig
    {
        public RollingWindowAveragingStateManagerConfig(TimeSpan windowLength, double failureTriggerPercentage, int minimumSampleSize, TimeSpan? synchLockTimeout = null)
        {
            // TODO: verify config ranges
            WindowLength = windowLength;
            FailureTriggerPercentage = failureTriggerPercentage;
            MinimumSampleSize = minimumSampleSize;
            SyncLockTimeout = synchLockTimeout ?? TimeSpan.FromSeconds(2);
        }

        public TimeSpan WindowLength { get; }
        public double FailureTriggerPercentage { get; }
        public int MinimumSampleSize { get; }
        public TimeSpan SyncLockTimeout { get; }
    }
}