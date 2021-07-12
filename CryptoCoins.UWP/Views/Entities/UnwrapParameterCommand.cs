using System;
using System.Windows.Input;
using CryptoCoins.UWP.Helpers;

namespace CryptoCoins.UWP.Views.Entities
{
    public class UnwrapParameterCommand : Observable, ICommand
    {
        private ICommand _command;

        public ICommand Command
        {
            get => _command;
            set
            {
                if (_command != null)
                {
                    _command.CanExecuteChanged -= OnCanExecuteChanged;
                }
                Set(ref _command, value);
                _command.CanExecuteChanged += OnCanExecuteChanged;
            }
        }

        public Func<object, object> UnwrapParameter { get; set; }

        public bool CanExecute(object parameter)
        {
            if (Command != null && UnwrapParameter != null)
            {
                return Command.CanExecute(UnwrapParameter(parameter));
            }
            return false;
        }

        public void Execute(object parameter)
        {
            if (Command != null && UnwrapParameter != null)
            {
                var p = UnwrapParameter(parameter);
                if (Command.CanExecute(p))
                {
                    Command.Execute(p);
                }
            }
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged(object sender, EventArgs eventArgs)
        {
            CanExecuteChanged?.Invoke(sender, eventArgs);
        }
    }
}
