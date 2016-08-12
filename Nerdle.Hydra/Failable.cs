using System;
using Nerdle.Hydra.StateManagement;

namespace Nerdle.Hydra
{
    public class Failable<TComponent> : IFailable<TComponent>
    {
        public string ComponentId { get; }

        readonly TComponent _component;
        readonly IStateManager _stateManager;

        public event EventHandler<Exception> Failed;
        public event EventHandler Recovered;

        public Failable(string componentId, TComponent component, IStateManager stateManager)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            if (stateManager == null)
                throw new ArgumentNullException(nameof(stateManager));

            ComponentId = componentId;
            _component = component;
            _stateManager = stateManager;

            stateManager.StateChanged += OnStateChanged;
        }

        public void Execute(Action<TComponent> command)
        {
            try
            {
                command(_component);
            }
            catch (Exception ex)
            {
                _stateManager.RegisterFailure(ex);
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
                _stateManager.RegisterFailure(ex);
                throw;
            }
            _stateManager.RegisterSuccess();
            return result;
        }

        public bool IsAvailable => _stateManager.CurrentState >= State.Recovering;

        void OnStateChanged(object sender, StateChangedArgs args)
        {
            if (args.CurrentState == State.Working)
                Recovered?.Invoke(this, EventArgs.Empty);

            else if (args.PreviousState == State.Working && args.CurrentState == State.Failed)
                Failed?.Invoke(this, args.Exception);
        }
    }
}