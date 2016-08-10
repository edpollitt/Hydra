using System;

namespace Nerdle.Hydra.InfrastructureAbstractions
{
    interface IClock
    {
        DateTime UtcNow { get; }
    }
}