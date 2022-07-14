using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gaussians.Commands
{
    internal class DefaultCommand : ICommand
    {
        public DefaultCommand(Action<object> command)
        {
            Command = command;
        }
        public DefaultCommand(Action<object> command, Func<object, bool> canCommand):this(command)
        {
            CanCommand = canCommand;
        }

        private Action<object?> Command { get; set; }
        private Func<object?, bool>? CanCommand { get; set; }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return CanCommand == null || CanCommand.Invoke(parameter);
        }

        public void Execute(object? parameter)
        {
            Command.Invoke(parameter);
        }
    }
}
