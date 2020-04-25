using CompanioNc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanioNc.ViewModels
{
    class VPN_ViewModel : INotifyPropertyChanged
    {
        public VPN_ViewModel()
        {
            // Query table的更新, 應該與WebTEst連動
            ComDataDataContext dc = new ComDataDataContext();
            DGQuery = dc.sp_querytable().ToList<sp_querytableResult>();
        }

        #region Data Properties
        private List<sp_querytableResult> dgQuery;

        public List<sp_querytableResult> DGQuery
        {
            get { return dgQuery; }
            set
            {
                dgQuery = value;
                OnPropertyChanged("DGQuery");
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }        
    }
}
