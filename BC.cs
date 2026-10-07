using System;
using System.Windows.Input;

namespace WpfApp26
{
    internal class BC : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        readonly Action action;
        readonly Func<bool> canExecute;

        public BC(Action action)
        {
            this.action = action;
        }

        public BC(Action action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        public void Execute(object parameter)
        {
            action();
        }
    }
}
