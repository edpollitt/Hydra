namespace Nerdle.Hydra.Simulator.Configuration
{
    public interface ISimulationConfiguration
    {
        bool DynamicCluster { get; }
        int ClusterSize { get; }
        int Iterations { get; }
        int Parallelism { get; }
        IComponentConfiguration Component { get; }
        IRollingWindowConfiguration RollingWindow { get; }
    }
}