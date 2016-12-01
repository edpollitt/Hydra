using System.Collections.Generic;

namespace Nerdle.Hydra
{
    public interface ITraversal
    {
        IEnumerable<T> Traverse<T>(IList<T> items);
    }
}