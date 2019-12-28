using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace TemperatureAlerter
{
    public class Command : ICommand
    {
        private readonly Action _command;

        public event EventHandler CanExecuteChanged;

        public Command(Action command)
        {
            _command = command;
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _command?.Invoke();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    }
}
