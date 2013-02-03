using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// A basic implementation of ICommand that wraps a method that takes no parameters or a method that takes one parameter. 
    /// </summary>
    public sealed class ActionCommand : ICommand
    {
        private readonly Action action;
        private readonly Action<object> objectAction;

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute. Will not be fired by ActionCommand.
        /// </summary>
        private event EventHandler CanExecuteChanged;

        event EventHandler ICommand.CanExecuteChanged
        {
            add { this.CanExecuteChanged += value; }
            remove { this.CanExecuteChanged -= value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XperiAndri.Expression.Interactivity.Core.ActionCommand"/> class. 
        /// </summary>
        /// <param name="objectAction">An action that takes an object parameter.</param>
        /// <remarks>Use this constructor to provide an action that uses the object parameter passed by the Execute method.</remarks>
        public ActionCommand(Action<object> objectAction)
        {
            this.objectAction = objectAction;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XperiAndri.Expression.Interactivity.Core.ActionCommand"/> class. 
        /// </summary>
        /// <param name="action">An action that takes an object parameter.</param>
        /// <remarks>Use this constructor to provide an action that uses the object parameter passed by the Execute method.</remarks>
        public ActionCommand(Action action)
        {
            this.action = action;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, then this object can be set to null.</param>
        public void Execute(object parameter)
        {
            if (this.objectAction != null)
            {
                this.objectAction(parameter);
            }
            else
            {
                this.action();
            }
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, then this object can be set to null.</param>
        /// <returns>
        /// Always returns true.
        /// </returns>
        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }
    }
}
