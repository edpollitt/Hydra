using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Nerdle.Hydra.Simulator.Configuration;
using Nerdle.Hydra.StateManagement;

namespace Nerdle.Hydra.Simulator
{
    class Simulation
    {
        readonly ICluster<ComponentStub> _cluster;
        readonly int _iterations;
        readonly int _parallelism;
        readonly IRollingWindowConfiguration _config;
        readonly ILog _log;

        public Simulation(IEnumerable<ComponentStub> components, IRollingWindowConfiguration config, int iterations, int parallelism, ILog log)
        {
            _iterations = iterations;
            _parallelism = parallelism;
            _config = config;
            _log = log;

            var failableComponents = components.Select(component =>
            {
                var stateManager = new RollingWindowAveragingStateManager(_config.WindowLength, _config.FailureTriggerPercentage, _config.MinimumSampleSize, _config.FailFor);
                stateManager.StateChanged += (sender, args) =>
                {
                    component.Reset();
                    _log.Info($"{args.PreviousState} -> {args.CurrentState}");
                };

                var failable = new Failable<ComponentStub>(component.ToString(), component, stateManager);
                failable.Failed += (sender, exception) => _log.Warn($"{((IFailable<ComponentStub>) sender).ComponentId} -> {State.Failed}");
                failable.Recovered += (sender, exception) => _log.Warn($"{((IFailable<ComponentStub>)sender).ComponentId} -> {State.Working}");

                return failable;
            })
            .ToList();
        
            _cluster = new Cluster<ComponentStub>(failableComponents);
        }

        public void Run()
        {
            Parallel.For(0, _iterations, 
                new ParallelOptions { MaxDegreeOfParallelism = _parallelism },
                _ =>
                {
                    try
                    {
                        _cluster.Execute(component => component.DoSomething());
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.GetType().Name);
                    }
                });
        }
    }
}