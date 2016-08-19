using System;
using System.Diagnostics;
using System.Linq;
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
            
            var components = new [] { "Primary", "Secondary", "Tertiary" }
                .Select(id => new ComponentStub(id, config.Component.BaseFailureRate, config.Component.OperationDelay))
                .ToList();

            var sim = new Simulation(components, config.RollingWindow, config.Iterations, config.Parallelism, log);

            var sw = new Stopwatch();
            sw.Start();
            sim.Run();
            sw.Stop();

            log.Info($"Finished {config.Iterations} iterations in {sw.Elapsed.TotalSeconds} s.");
            Console.ReadKey();
        }
    }
}
