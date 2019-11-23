using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Yugen.Mosaic.Uwp.Helpers
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute = null;
        
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> execute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
