using System;

namespace Nerdle.Hydra.Infrastructure
{
    class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}