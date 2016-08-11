using System;

namespace Nerdle.Hydra.Simulator
{
    class ComponentStub
    {
        readonly Random _random;
        readonly double _baseFailureRate;
        double _currentFailureRate;

        public ComponentStub(int id, double baseFailureRate)
        {
            Id = id;
            _baseFailureRate = baseFailureRate;
            _currentFailureRate = _baseFailureRate;
            _random = new Random(Id);
        }

        public int Id { get; set; }

        public void DoSomething()
        {
            if (_random.NextDouble() <= _currentFailureRate)
            {
                Console.WriteLine(Id + " #");
                _currentFailureRate = Math.Min(1.0, _currentFailureRate * 2);
                throw new Exception("boom!");
            }

            Console.WriteLine(Id);
            _currentFailureRate = Math.Max(_baseFailureRate, _currentFailureRate / 2);
        }

        public void Reset()
        {
            _currentFailureRate = _baseFailureRate;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}