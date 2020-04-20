using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CompanioNc.ViewModels.Commands
{
    public class SendUID : ICommand
    {
        public MainVM MVM { get; set; }

        public event EventHandler CanExecuteChanged;

        public SendUID(MainVM mainVM)
        {
            MVM = mainVM;
        }

        public bool CanExecute(object parameter)
        {
            // 這裡可以放一些查核的工作
            return true;
        }

        public void Execute(object parameter)
        {
            MVM.StrUID = (string)parameter;
        }
    }
}
