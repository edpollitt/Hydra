using System.Threading.Tasks;

namespace Nerdle.Hydra.Tests.Unit.TestHelpers
{
    interface ISomeService
    {
        void SomeCommand();
        T SomeQuery<T>();
        Task SomeAsyncCommand();
        Task<T> SomeAsyncQuery<T>();
    }
}