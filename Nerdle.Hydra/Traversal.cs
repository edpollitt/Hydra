using System.Collections.Generic;

namespace Nerdle.Hydra
{
    class Traversal : ITraversal
    {
        Traversal() { }

        static Traversal()
        {
            Default = new Traversal();
        }

        public static ITraversal Default { get; }

        public IEnumerable<T> Traverse<T>(IList<T> items)
        {
            return items;
        }
    }
}