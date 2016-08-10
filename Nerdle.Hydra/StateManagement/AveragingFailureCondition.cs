using System;

namespace Nerdle.Hydra.StateManagement
{
    class AveragingFailureCondition : ICondition<int, int>
    {
        public AveragingFailureCondition(double failureTriggerPercentage, int minimumSampleSize)
        {
            throw new NotImplementedException();
        }

        public bool Evaluate(int t1, int t2)
        {
            throw new NotImplementedException();
        }
    }
}