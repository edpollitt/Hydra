namespace Nerdle.Hydra
{
    public class ClusterResult
    {
        public string HandlerId { get; }

        public ClusterResult(string handlerId)
        {
            HandlerId = handlerId;
        }
    }


    public class ClusterResult<TResult> : ClusterResult
    {
        public TResult Result { get; }

        public ClusterResult(string handlerId, TResult result) : base(handlerId)
        {
            Result = result;
        }
    }
}