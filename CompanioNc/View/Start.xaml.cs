using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;

namespace CompanioNc.View
{
    /// <summary>
    /// Start.xaml 的互動邏輯
    /// 20210716建立
    /// </summary>
    public partial class Start : Window
    {
        private readonly TaskbarIcon tb = new TaskbarIcon();
        public WebTEst w;
        public MainWindow m;

        public Start()
        {
            InitializeComponent();
            // 20210716: 創立
            // 目的是承載
            // 判斷有連線就啟動, 沒有就不啟動
            Refresh();
        }
    }
}
