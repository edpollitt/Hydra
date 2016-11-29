using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Nerdle.Hydra.InfrastructureAbstractions;
using Nerdle.Hydra.Simulator.Configuration;
using Nerdle.Hydra.StateManagement;

namespace Nerdle.Hydra.Simulator
{
    class Simulation : IRunnable
    {
        readonly ICluster<ComponentStub> _cluster;
        readonly ISimulationConfiguration _config;
        readonly ILog _log;

        static int _componentId;

        Simulation(ICluster<ComponentStub> cluster, ISimulationConfiguration config, ILog log)
        {
            _cluster = cluster;
            _config = config;
            _log = log;
        }

        public static Simulation OfStaticCluster(ISimulationConfiguration config, ILog log)
        {
            var components = new IFailable<ComponentStub>[3];

            for (var i = 0; i < components.Length; i++)
                components[i] = CreateComponent(config, log);

            var cluster = new StaticCluster<ComponentStub>(components);

            log.Info($"Generated cluster: {string.Join(", ", cluster.ComponentIds)}");

            return new Simulation(cluster, config, log);
        }

        public static Simulation OfDynamicCluster(ISimulationConfiguration config, ILog log)
        {
            var components = new IFailable<ComponentStub>[3];

            for (var i = 0; i < components.Length; i++)
                components[i] = CreateComponent(config, log);

            var cluster = new DynamicCluster<ComponentStub>(components, new SyncManager(new ReaderWriterLockSlim()));
            cluster.ComponentFailed += (sender, exception) =>
            {
                var oldComponent = (IFailable<ComponentStub>)sender;
                log.Info($"Component failed, ID: {oldComponent.ComponentId}");
                var newComponent = CreateComponent(config, log);
                log.Info($"Spawned new component, ID: {newComponent.ComponentId}");
                cluster.Replace(oldComponent, newComponent);
                log.Info($"Replaced cluster component {oldComponent.ComponentId} with component {newComponent.ComponentId}");
                log.Info($"Current cluster: {string.Join(", ", cluster.ComponentIds)}");
            };

            log.Info($"Generated cluster: {string.Join(", ", cluster.ComponentIds)}");

            return new Simulation(cluster, config, log);
        }

        static IFailable<ComponentStub> CreateComponent(ISimulationConfiguration config, ILog defaultLog)
        {
            var id = Interlocked.Increment(ref _componentId).ToString("#0000");
            var componentLog = LoggerFactory.CreateLogger(id);
            var stateManager = new RollingWindowAveragingStateManager(config.RollingWindow.WindowLength, config.RollingWindow.FailureTriggerPercentage, config.RollingWindow.MinimumSampleSize, config.RollingWindow.FailFor);
            var component = new ComponentStub(id, config.Component.BaseFailureRate, config.Component.OperationDelay, componentLog);
            var failableComponent = new Failable<ComponentStub>(id, component, stateManager);

            stateManager.StateChanged += (sender, args) =>
            {
                component.Reset();
                defaultLog.Info($"{id} state changed: {args.PreviousState} -> {args.CurrentState}");
            };

            return failableComponent;
        }

        public void Run()
        {
            Parallel.For(0, _config.Iterations, new ParallelOptions { MaxDegreeOfParallelism = _config.Parallelism },
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