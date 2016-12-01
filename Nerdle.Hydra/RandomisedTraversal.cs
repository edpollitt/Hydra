using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nerdle.Hydra
{
    class RandomisedTraversal : ITraversal
    {
        static int _seed;

        static readonly ThreadLocal<Random> Random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        public IEnumerable<T> Traverse<T>(IList<T> items)
        {
            return items.OrderBy(_ => Random.Value.Next());
        }
    }
}