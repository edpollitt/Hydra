using System;
using System.Threading;
using log4net;

namespace Nerdle.Hydra.Simulator
{
    class ComponentStub
    {
        readonly Random _random;
        readonly double _baseFailureRate;
        readonly TimeSpan _operationTime;
        double _currentFailureRate;
        readonly ILog _log;

        public ComponentStub(string id, double baseFailureRate, TimeSpan operationTime)
        {
            Id = id;
            _baseFailureRate = baseFailureRate;
            _operationTime = operationTime;
            _currentFailureRate = _baseFailureRate;
            _random = new Random(id.GetHashCode());
            _log = LogManager.GetLogger(id);
        }

        public string Id { get; }

        public void DoSomething()
        {
            Thread.Sleep(_operationTime);

            if (_random.NextDouble() <= _currentFailureRate)
            {
                _log.Error(this);
                _currentFailureRate = Math.Min(1.0, _currentFailureRate * 2);
                throw new Exception("boom!");
            }

            _log.Info(this);
            _currentFailureRate = Math.Max(_baseFailureRate, _currentFailureRate / 2);
        }

        public void Reset()
        {
            _currentFailureRate = _baseFailureRate;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}