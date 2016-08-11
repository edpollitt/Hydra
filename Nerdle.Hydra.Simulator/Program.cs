using System;
using System.Diagnostics;
using System.Linq;

namespace Nerdle.Hydra.Simulator
{
    class Program
    {
        static void Main(string[] args)
        {
            const int iterations = 10000;
            const int parallelism = 5;
            const double baseFailureRate = 0.1;
            const int numberOfComponents = 3;

            var components = Enumerable.Range(1, numberOfComponents).Select(id => new ComponentStub(id, baseFailureRate)).ToList();
            var sim = new Simulator(components);

            var sw = new Stopwatch();
            sw.Start();
            sim.Run(iterations, parallelism);
            sw.Stop();

            Console.WriteLine($"Finished {iterations} commands in {sw.Elapsed.TotalSeconds} s.");
            Console.ReadKey();
        }
    }
}
