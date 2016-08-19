using System;

namespace Nerdle.Hydra.Simulator.Configuration
{
    public interface IComponentConfiguration
    {
        double BaseFailureRate { get; }
        TimeSpan OperationDelay { get; set; }
    }
}