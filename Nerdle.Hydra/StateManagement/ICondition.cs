namespace Nerdle.Hydra.StateManagement
{
    interface ICondition<in T1, in T2>
    {
        bool IsMet(T1 t1, T2 t2);
    }
}