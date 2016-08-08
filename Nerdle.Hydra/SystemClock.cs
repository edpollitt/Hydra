using System;

namespace Nerdle.Hydra
{
    class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}