using System;

namespace Nerdle.Hydra.StateManagement
{
    public class RollingWindow : IRollingWindow
    {
        public RollingWindow(TimeSpan windowLength)
        {
            throw new NotImplementedException();
        }

        public void Mark()
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
    }
}