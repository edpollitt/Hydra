using System;
using System.Threading;
using System.Threading.Tasks;
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

        public ComponentStub(string id, double baseFailureRate, TimeSpan operationTime, ILog log)
        {
            Id = id;
            _baseFailureRate = baseFailureRate;
            _operationTime = operationTime;
            _log = log;
            _currentFailureRate = _baseFailureRate;
            _random = new Random(id.GetHashCode());
        }

        public string Id { get; }

        public void DoSomething()
        {
            Thread.Sleep(_operationTime);
            MaybeError();
        }

        public async Task DoSomethingAsync()
        {
            await Task.Delay(_operationTime);
            MaybeError();
        }

        void MaybeError()
        {
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