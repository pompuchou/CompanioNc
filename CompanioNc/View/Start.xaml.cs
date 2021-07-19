using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;

namespace CompanioNc.View
{
    /// <summary>
    /// Start.xaml 的互動邏輯
    /// 20210718: 接下來改善自動病歷的部分
    ///           1. Ctrl-F2, 簡短, 日期, 主訴, 無特別
    /// 20210718: 希望改成每一步驟都有紀錄, 且一人一天只一筆紀錄
    ///           避開當機現象 -加入try, 應該已經解決問題了
    ///           async: 連結測試    -done, 不同windows    -failed, 讀取 vpn時
    /// 20210716: 創立
    /// 目的是承載
    /// </summary>
    public partial class Start : Window
    {
        private readonly TaskbarIcon tb = new TaskbarIcon();
        public WebTEst w;
        public MainWindow m;

        public Start()
        {
            InitializeComponent();
            // 判斷有連線就啟動, 沒有就不啟動
            Go();
        }

        private async void Go()
        {
            await Refresh();
        }
    }
}
