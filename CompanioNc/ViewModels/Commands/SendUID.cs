using CompanioNc.Models;
using System;
using System.Windows.Input;
using System.Linq;

namespace CompanioNc.ViewModels.Commands
{
    public class SendUID : ICommand
    {
        public MainVM MVM { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public SendUID(MainVM mainVM)
        {
            MVM = mainVM;
        }

        public bool CanExecute(object parameter)
        {
            // 這裡可以放一些查核的工作
            if (string.IsNullOrWhiteSpace((string)parameter))
                return false;
            Com_alDataContext dc = new Com_alDataContext();
            var q = from p in dc.tbl_patients
                    where p.uid == (string)parameter
                    select p.uid;
            if (q.Count() == 0) return false;
            return true;
        }

        public void Execute(object parameter)
        {
            MVM.StrUID = (string)parameter;
        }
    }
}