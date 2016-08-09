using System;

namespace Nerdle.Hydra.Infrastructure
{
    interface IClock
    {
        DateTime UtcNow { get; }
    }
}