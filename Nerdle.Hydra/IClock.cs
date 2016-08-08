using System;

namespace Nerdle.Hydra
{
    interface IClock
    {
        DateTime UtcNow { get; }
    }
}