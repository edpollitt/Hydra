namespace Nerdle.Hydra
{
    public class ClusterResult
    {
        public string HandledByComponentId { get; }

        public ClusterResult(string handledByComponentId)
        {
            HandledByComponentId = handledByComponentId;
        }
    }

    public class ClusterResult<TResult> : ClusterResult
    {
        public TResult Result { get; }

        public ClusterResult(string handledByComponentId, TResult result) : base(handledByComponentId)
        {
            Result = result;
        }

        public static  implicit operator TResult(ClusterResult<TResult> result)
        {
            return result.Result;
        }
    }
}