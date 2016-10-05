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
            
            log.Info("Beginning simulation...");
            var simulation = config.DynamicCluster ? 
                Simulation.OfDynamicCluster(config, log) 
              : Simulation.OfStaticCluster(config, log);
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
