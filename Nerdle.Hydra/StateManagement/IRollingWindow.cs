namespace Nerdle.Hydra.StateManagement
{
    interface IRollingWindow
    {
        void Mark();
        int TrimAndCount();
        void Reset();
    }
}