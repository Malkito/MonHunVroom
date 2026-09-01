using System;

namespace Niki.UI
{
    /// <summary>A unit of interaction the model layer can trigger (e.g. pressing an ability button).</summary>
    public interface ICommand
    {
        bool CanExecute();
        void Execute();
    }

    /// <summary>
    /// Minimal command wrapper around a delegate.
    /// The view model owns the instance; the model layer plugs the actual logic in with
    /// <see cref="SetAction"/> so the view never has to know about gameplay code.
    /// </summary>
    public sealed class Command : ICommand
    {
        private Action _action;

        public Command()
        {
        }

        public Command(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            _action = action;
        }

        public void SetAction(Action action)
        {
            _action = action;
        }

        public bool CanExecute() => _action != null;

        public void Execute() => _action?.Invoke();
    }
}
