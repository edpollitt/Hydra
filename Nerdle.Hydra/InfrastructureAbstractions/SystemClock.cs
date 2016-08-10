using System;

namespace Nerdle.Hydra.InfrastructureAbstractions
{
    class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}