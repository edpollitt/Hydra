using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nerdle.Hydra.Simulator
{
    public static class ParallelAsync
    {
        public static async Task For(int fromInclusive, int toExclusive, int maxDegreeOfParallelism, Func<int, Task> operation)
        {
            var currentTasks = Enumerable.Range(fromInclusive, maxDegreeOfParallelism)
                .Select(i => Task.Run(() => operation(i)))
                .ToList();

            var taskCounter = currentTasks.Count;

            while (currentTasks.Count > 0)
            {
                var completed = await Task.WhenAny(currentTasks);
                var index = currentTasks.IndexOf(completed);

                if (taskCounter < toExclusive)
                {
                    var iteration = taskCounter;
                    currentTasks[index] = Task.Run(() => operation(iteration));
                    taskCounter++;
                }
                else
                {
                    currentTasks.RemoveAt(index);
                }
            }
        }
    }
}