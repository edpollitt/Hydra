using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nerdle.Hydra.StateManagement;

namespace Nerdle.Hydra.Simulator
{
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
}