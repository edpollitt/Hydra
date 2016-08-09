namespace Nerdle.Hydra.StateManagement
{
    interface IRollingWindow
    {
        void Mark();
        int Count { get; }
    }
}