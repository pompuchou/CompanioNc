using System;
using System.Timers;
using System.Windows;

namespace CompanioNc
{
    /// <summary>
    ///     '20200411 created
    ///目的有: 1. 監控問診畫面, 紀錄 -done
    ///        2. 顯示該人的檢驗或其他有用資料  -done
    ///        3. 可貼上routine template -plausible
    ///        4. 可貼上檢驗結果 -plausible
    ///        5. 可copy 雲端    -done
    ///        6. 可紀錄是否有查雲端, 是否有查關懷名單 -done
    ///        7. 量表 -plausible
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Timers.Timer _timer1;
        private string strID = string.Empty;
        private string strUID = string.Empty;
        private string strSDATE = string.Empty;
        private string tempID = string.Empty;
        public const int MOD_ALT = 0x1; //Alt key for hotkey
        public const int MOD_SHIFT = 0x4; //Alt key for hotkey
        public const int MOD_CONTROL = 0x2; //Alt key for hotkey
        public const int MOD_WINKEY = 0x8; //Alt key for hotkey
        public const int WM_HOTKEY = 0x312;   //Hotkey

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            this._timer1 = new System.Timers.Timer();
            this._timer1.Interval = 500;
            this._timer1.Elapsed += new System.Timers.ElapsedEventHandler(_TimersTimer_Elapsed);
            _timer1.Start();
            Record_admin("Companion Log in", "");
            Refresh_data();
            this.Label1.Content = string.Empty;
            this.Label2.Content = string.Empty;
        }

        private void _TimersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                GetWindow g = new GetWindow("問診");
                // vb.NET里面的vbCr & vbLf 在c# 是\r\n, chr(13) chr(10)
                // 第二個是身分證字號
                //tempID = g.Key.Split('\n')[2];
                tempID = g.Key;
                //string[] ss = tempID.Split('\n');
            }
            catch
            {
                tempID = string.Empty;
            }
            if (strID == string.Empty)
            {
                if (tempID == string.Empty)
                {
                    // condition 1, strID = "" => ""                     do nothing
                    return;
                }
                else
                {
                    // 檢單查核, 如果分解後數目小於8, 應該就不是正確的
                    // 20190930似乎有效
                    if (tempID.Split(' ').Length < 8)
                    {
                        //'MessageBox.Show("抓到了")
                        return;
                    }
                    // condition 2, strID = "" => something A            record A, starttime
                    // 要做很多事情, 分解
                    // 20190930 有些"問診畫面"的狀態,文字是不一樣的,這樣的話會有錯誤
                    strID = tempID;
                    ComDataDataContext dc = new ComDataDataContext();
                    string[] s = strID.Split(' ');   //0 SDATE, 1 VIST, 2 RMNO, 4 Nr, 7 uid, 8 cname
                    strUID = s[7].Substring(1, 10);
                    if (strUID == "A000000000")
                    {
                        strID = "";
                        strUID = "";
                        strSDATE = "";
                        return;
                    }
                    strSDATE = s[0];
                    //dc.sp_insert_access(CDate(s[0]), s[1], CInt(s[2]), CInt(s[4]), strUID, s[8], 1);
                    // do something
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        this.Label1.Content = strUID;
                        this.Label2.Content = strID;
                        // 更新資料
                        Refresh_data();
                    }));
                    //' 寫入current_uid
                    if (System.IO.File.Exists(@"C:\vpn\current_uid.txt"))
                    {
                        // 如果有檔案就殺了它
                        System.IO.File.Delete(@"C:\vpn\current_uid.txt");
                    }
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(@"C:\vpn\current_uid.txt");
                    sw.Write(strUID);
                    sw.Close();
                }
            }
            else
            {
                if (strID == tempID)
                {
                    // condition 3, strID = something A => something A   do nothing
                    return;
                }
                else if (tempID == "")
                {
                    //' condition 4, strID = something A => ""            record endtime, write into database
                    //' 做的事情也不少
                    ComDataDataContext dc = new ComDataDataContext();
                    string[] s = strID.Split(' ');   //'0 SDATE, 1 VIST, 2 RMNO, 4 Nr, 7 uid, 8 cname
                    //dc.sp_insert_access(CDate(s[0]), s[1], CInt(s[2]), CInt(s[4]), s[7].Substring(1, 10), s[8], 0);
                    strID = tempID;
                    strUID = string.Empty;
                    strSDATE = string.Empty;
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        this.Label1.Content = strUID;
                        this.Label2.Content = strID;
                    }));
                    if (System.IO.File.Exists(@"C:\vpn\current_uid.txt"))
                    {
                        //' 如果有檔案就殺了它
                        System.IO.File.Delete(@"C:\vpn\current_uid.txt");
                    }
                    //' 清理檢驗資料
                    //Me.DataGridView1.Visible = False
                    //Me.DataGridView2.Visible = False
                    //Me.DataGridView3.Visible = False
                    //Me.DataGridView4.Visible = False
                    //Me.DataGridView5.Visible = False
                    //Me.DataGridView6.Visible = False
                    //Me.DataGridView7.Visible = False
                    //Me.DataGridView8.Visible = False
                }
                else
                {
                    //' condition 5, strID = something A => something B   I don't know if this is possible
                    //' 有可能嗎? 我不知道
                    //' 20191001 答案揭曉了,有可能,因為THESIS在畫form時會有A000000000臨時的資料,然後再讀資料庫蓋上,就會出現something A => something B的情況
                    //' 我採用檢核若A000000000的情形就不要寫入的方式處理
                    //' 檢單查核, 如果分解後數目小於8, 應該就不是正確的
                    //'                MessageBox.Show("抓到了! " + vbCrLf + strID + "=>" + tempID)
                    if (tempID.Split(' ').Length < 8)
                    {
                        return;
                    }
                }
            }
        }

        private void Refresh_data()
        {
            ComDataDataContext dc = new ComDataDataContext();
            DGQuerry.ItemsSource = dc.sp_querytable();
        }

        private void Main_Closed(object sender, EventArgs e)
        {
            Record_admin("Companion Log out", "");
            _timer1.Stop();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh_data();
        }
    }
}