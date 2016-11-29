using System;
using System.Diagnostics;
using log4net;
using log4net.Config;
using Nerdle.Hydra.Simulator.Configuration;
using Config = Nerdle.AutoConfig.AutoConfig;

namespace Nerdle.Hydra.Simulator
{
    class Program
    {
        static void Main()
        {
            XmlConfigurator.Configure();

            var log = LogManager.GetLogger("Default");
            var config = Config.Map<ISimulationConfiguration>();

            var simulation = config.UseDynamicCluster ? 
                Simulation.OfDynamicCluster(config, log) 
              : Simulation.OfStaticCluster(config, log);
        
            log.Info("Simulation parameters:");
            log.Info($"\t=> Cluster type: {simulation.ClusterType.Name}");
            log.Info($"\t=> Async operations: {config.UseAsyncOperations}");
            log.Info($"\t=> Cluster size: {config.ClusterSize}");
            log.Info($"\t=> Iterations: {config.Iterations}");
            log.Info($"\t=> Max parallelism: {config.Parallelism}");
            log.Info($"\t=> Component base failure rate: {config.Component.BaseFailureRate}");
            log.Info($"\t=> Component operation delay: {config.Component.OperationDelay}");
            log.Info($"\t=> Failure window length: {config.RollingWindow.WindowLength}");
            log.Info($"\t=> Failure window trigger percentage: {config.RollingWindow.FailureTriggerPercentage}");
            log.Info($"\t=> Failure window min sample size: {config.RollingWindow.MinimumSampleSize}");
            log.Info($"\t=> Failure window failure duration: {config.RollingWindow.FailFor}");

            log.Info("Beginning simulation...");

            var elapsedTime = Time(() => simulation.Run());

            log.Info($"Finished {config.Iterations} iterations in {elapsedTime.TotalSeconds} s.");

            Console.ReadKey();
        }

        static TimeSpan Time(Action action)
        {
            var sw = new Stopwatch();
            sw.Start();
            action();
            sw.Stop();
            return sw.Elapsed;
        }
    }
}
