using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Configuration;
using System.Windows;
using System.Data.SqlClient;

namespace CompanioNc.View
{
    /// <summary>
    /// Start.xaml 的互動邏輯
    /// </summary>
    public partial class Start : Window
    {
        private readonly TaskbarIcon tb = new TaskbarIcon();

        public Start()
        {
            InitializeComponent();
            // 20210716: 創立
            // 目的是承載
            // 判斷有連線就啟動, 沒有就不啟動

            Refresh();

        }

        private void Refresh()
        {
            // 如何判斷連線成功?
            // connection string
            string cs = ConfigurationManager.ConnectionStrings["CompanioNc.Properties.Settings.alConnectionString"].ConnectionString;
            // 是否連結成功
            bool bSuccess = false;

            using (SqlConnection sc = new SqlConnection(cs))
            {
                try
                {
                    // 嘗試打開連結, 成功了就打開MainWindow
                    sc.Open();
                    sc.Close();
                    bSuccess = true;
                    tb.ShowBalloonTip("連結成功", "Success", BalloonIcon.Info);
                }
                catch (Exception)
                {
                    bSuccess = false;
                    tb.ShowBalloonTip("連結逾期", "Timeout", BalloonIcon.Info);
                }
            };

            if (bSuccess)
            {
                // 連結成功, 打開
                MainWindow m = new MainWindow();
                m.Show();
            }
        }
    }
}
