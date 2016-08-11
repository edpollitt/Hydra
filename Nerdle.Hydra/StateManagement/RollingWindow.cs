using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Nerdle.Hydra.InfrastructureAbstractions;

namespace Nerdle.Hydra.StateManagement
{
    /// <summary>
    /// Note that this class is not thread safe and should only be accessed from 
    /// within a synchronised block. Using ConcurrentQueue for the internal representation 
    /// would not help as we still need the peek/dequeue operation in Trim to be atomic.
    /// </summary>
    class RollingWindow : IRollingWindow
    {
        readonly Queue<DateTime> _queue;
        readonly IClock _clock;
        readonly TimeSpan _windowLength;

        public RollingWindow(TimeSpan windowLength) : this(windowLength, new Queue<DateTime>(), new SystemClock())
        {
            if (windowLength <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(windowLength), "Window length must be greater than 0");
        }

        internal RollingWindow(TimeSpan windowLength, Queue<DateTime> queue, IClock clock)
        {
            _windowLength = windowLength;
            _queue = queue;
            _clock = clock;
        }

        public void Mark()
        {
            var now = _clock.UtcNow;
            _queue.Enqueue(now);
            Trim(now);
        }

        public int Count
        {
            get
            {
                Trim(_clock.UtcNow);
                return _queue.Count;
            }
        }

        public void Reset()
        {
            _queue.Clear();
        }

        void Trim(DateTime windowEnd)
        {
            var windowStart = windowEnd.Subtract(_windowLength);

            while (_queue.Count > 0 && _queue.Peek() < windowStart)
                _queue.Dequeue();
        }
    }
}