namespace Nerdle.Hydra.Simulator.Configuration
{
    public interface ISimulationConfiguration
    {
        bool UseDynamicCluster { get; }
        bool UseAsyncOperations { get; }
        int ClusterSize { get; }
        int Iterations { get; }
        int Parallelism { get; }
        IComponentConfiguration Component { get; }
        IRollingWindowConfiguration RollingWindow { get; }
    }
}