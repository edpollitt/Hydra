using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nerdle.Hydra.StateManagement;

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


    class Simulator
    {
        readonly ICluster<ComponentStub> _cluster;

        public Simulator(IEnumerable<ComponentStub> components)
        {
            var failableComponents = components.Select(component =>
            {
                var stateManager = new RollingWindowAveragingStateManager(TimeSpan.FromSeconds(1), 0.5, 10, TimeSpan.FromSeconds(10));
                stateManager.StateChanged += (sender, args) => Console.WriteLine($"{sender}: {args.PreviousState} -> {args.CurrentState}");
                return new Failable<ComponentStub>(component, component.ToString(), stateManager);
            }).ToList();
        
            _cluster = new Cluster<ComponentStub>(failableComponents);
        }

        public void Run(int iterations, int parallelism)
        {
            Parallel.For(0, iterations, 
                new ParallelOptions { MaxDegreeOfParallelism = parallelism },
                _ =>
                {
                    try
                    {
                        _cluster.Execute(component => component.DoSomething());
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("----");
                    }
                });
        }
    }

    class ComponentStub
    {
        readonly Random _random;
        readonly double _baseFailureRate;
        double _currentFailureRate;

        public ComponentStub(int id, double baseFailureRate)
        {
            Id = id;
            _baseFailureRate = baseFailureRate;
            _currentFailureRate = _baseFailureRate;
            _random = new Random(Id);
        }

        public int Id { get; set; }

        public void DoSomething()
        {
            if (_random.NextDouble() <= _currentFailureRate)
            {
                Console.WriteLine(Id + " #");
                _currentFailureRate = Math.Min(1.0, _currentFailureRate * 2);
                throw new Exception("boom!");
            }

            Console.WriteLine(Id);
            _currentFailureRate = Math.Max(_baseFailureRate, _currentFailureRate / 2);
        }

        public void Reset()
        {
            _currentFailureRate = _baseFailureRate;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
