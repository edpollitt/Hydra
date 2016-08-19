using System;

namespace Nerdle.Hydra.Simulator.Configuration
{
    public interface IRollingWindowConfiguration
    {
        TimeSpan WindowLength { get; }
        double FailureTriggerPercentage { get; }
        int MinimumSampleSize { get; }
        TimeSpan FailFor { get; }
    }
}