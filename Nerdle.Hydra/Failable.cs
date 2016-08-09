using System;
using Nerdle.Hydra.StateManagement;

namespace Nerdle.Hydra
{
    public class Failable<TComponent> : IFailable<TComponent>
    {
        public string ComponentId { get; }

        readonly TComponent _component;
        readonly IStateManager _stateManager;

        public Failable(TComponent component, string componentId, IStateManager stateManager)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            if (stateManager == null)
                throw new ArgumentNullException(nameof(stateManager));

            _component = component;
            _stateManager = stateManager;
            ComponentId = componentId;
        }

        public void Execute(Action<TComponent> command)
        {
            try
            {
                command(_component);
            }
            catch (Exception ex)
            {
                _stateManager.RegisterFailure();
                throw;
            }
            _stateManager.RegisterSuccess();
        }

        public TResult Execute<TResult>(Func<TComponent, TResult> query)
        {
            TResult result;
            try
            {
                result = query(_component);
            }
            catch (Exception ex)
            {
                _stateManager.RegisterFailure();
                throw;
            }
            _stateManager.RegisterSuccess();
            return result;
        }

        public bool IsAvailable => _stateManager.CurrentState >= State.Recovering;
    }
}