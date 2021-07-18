using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CompanioNc.View
{
    /// <summary>
    /// 20210717建立
    /// </summary>
    public partial class Start : Window
    {

        private async Task Refresh()
        {
            this.RefreshButton.Visibility = Visibility.Hidden;
            this.Background = Brushes.Red;
            // 如何判斷連線成功?
            // connection string
            string cs = ConfigurationManager.ConnectionStrings["CompanioNc.Properties.Settings.alConnectionString"].ConnectionString;
            // 是否連結成功
            bool bSuccess = false;

            using (SqlConnection sc = new SqlConnection(cs))
            {
                await Task.Run(() =>
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
                });
            };

            if (bSuccess)
            {
                // 連結成功, 打開
                m = new MainWindow(this);
                m.Show();
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }
            this.RefreshButton.Visibility = Visibility.Visible;
            this.Background = Brushes.Beige;

        }

    }
}
