namespace Nerdle.Hydra.Simulator.Configuration
{
    public interface ISimulationConfiguration
    {
        int Iterations { get; }
        int Parallelism { get; }
        IComponentConfiguration Component { get; }
        IRollingWindowConfiguration RollingWindow { get; }
    }
}