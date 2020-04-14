using HDLibrary.Wpf.Input;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

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

        public string SSDATE
        {
            get { return strSDATE; }
        }

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
            HotKeyHost hotKeyHost = new HotKeyHost((HwndSource)HwndSource.FromVisual(App.Current.MainWindow));
            hotKeyHost.AddHotKey(new CustomHotKey("ShowPopup", Key.Q, ModifierKeys.Control | ModifierKeys.Shift, true));
            hotKeyHost.AddHotKey(new CustomHotKey("Paste", Key.F2, ModifierKeys.Control, true));
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
                    dc.sp_insert_access(DateTime.Parse(s[0]), s[1], byte.Parse(s[2]), byte.Parse(s[4]), strUID, s[8], true);
                    // do something
                    this.Dispatcher.Invoke((Action)(() =>
                    {
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
                    dc.sp_insert_access(DateTime.Parse(s[0]), s[1], byte.Parse(s[2]), byte.Parse(s[4]), strUID, s[8], false);
                    strID = tempID;
                    strUID = string.Empty;
                    strSDATE = string.Empty;
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        // 更新資料
                        Refresh_data();
                    }));
                    if (System.IO.File.Exists(@"C:\vpn\current_uid.txt"))
                    {
                        //' 如果有檔案就殺了它
                        System.IO.File.Delete(@"C:\vpn\current_uid.txt");
                    }
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
            #region refresh all tabitem and listbox items
            this.TabCon.Items.Clear();
            this.TabCon.Items.Add(Tab1);
            this.TabCon.Items.Add(Tab2);
            this.TabCon.Items.Add(Tab3);
            this.LB01.Items.Clear();
            this.LB01.Items.Add(LBDG01);
            this.LB01.Items.Add(DGQ01);
            this.LB01.Items.Add(LBDG02);
            this.LB01.Items.Add(DGQ02);
            this.LB01.Items.Add(LBDG03);
            this.LB01.Items.Add(DGQ03);
            this.LB01.Items.Add(LBDG04);
            this.LB01.Items.Add(DGQ04);
            this.LB01.Items.Add(LBDG05);
            this.LB01.Items.Add(DGQ05);
            this.LB01.Items.Add(LBDG06);
            this.LB01.Items.Add(DGQ06);
            this.LB01.Items.Add(LBDG07);
            this.LB01.Items.Add(DGQ07);
            this.LB01.Items.Add(LBDG08);
            this.LB01.Items.Add(DGQ08);
            this.LB01.Items.Add(LBDG09);
            this.LB01.Items.Add(DGQ09);
            this.LB01.Items.Add(LBDG10);
            this.LB01.Items.Add(DGQ10);
            #endregion
            this.Label1.Content = strUID;
            this.Label2.Content = strID;
            ComDataDataContext dc = new ComDataDataContext();
            DGQuerry.ItemsSource = dc.sp_querytable();
            if (strUID=="")
            {
                DGQ01.ItemsSource = null;
                DGQ02.ItemsSource = null;
                DGQ03.ItemsSource = null;
                DGQ04.ItemsSource = null;
                DGQ05.ItemsSource = null;
                DGQ06.ItemsSource = null;
                DGQ07.ItemsSource = null;
                DGQ08.ItemsSource = null;
                DGQ09.ItemsSource = null;
                DGQ10.ItemsSource = null;
                DGLab.ItemsSource = null;
            }
            else
            {
                DGQ01.ItemsSource = dc.sp_cloudmed_by_uid(strUID);
                DGQ02.ItemsSource = dc.sp_cloudlab_by_uid(strUID);
                DGQ03.ItemsSource = dc.sp_cloudDEN_by_uid(strUID);
                DGQ04.ItemsSource = dc.sp_cloudOP_by_uid(strUID);
                DGQ05.ItemsSource = dc.sp_cloudTCM_by_uid(strUID);
                DGQ06.ItemsSource = dc.sp_cloudREH_by_uid(strUID);
                DGQ07.ItemsSource = dc.sp_cloudDIS_by_uid(strUID);
                DGQ08.ItemsSource = dc.sp_cloudALL_by_uid(strUID);
                DGQ09.ItemsSource = dc.sp_cloudSCH_R_by_uid(strUID);
                DGQ10.ItemsSource = dc.sp_cloudSCH_U_by_uid(strUID);
                DGLab.ItemsSource = dc.sp_labdata_by_uid(strUID);
            }
            #region remove all unnessasary items
            if (DGLab.Items.Count == 0)
            {
                this.TabCon.Items.Remove(Tab1);
            }
            if (DGQ01.Items.Count ==0)
            {
                this.LB01.Items.Remove(LBDG01);
                this.LB01.Items.Remove(DGQ01);
            }
            if (DGQ02.Items.Count == 0)
            {
                this.LB01.Items.Remove(LBDG02);
                this.LB01.Items.Remove(DGQ02);
            }
            if (DGQ03.Items.Count == 0)
            {
                this.LB01.Items.Remove(LBDG03);
                this.LB01.Items.Remove(DGQ03);
            }
            if (DGQ04.Items.Count == 0)
            {
                this.LB01.Items.Remove(LBDG04);
                this.LB01.Items.Remove(DGQ04);
            }
            if (DGQ05.Items.Count == 0)
            {
                this.LB01.Items.Remove(LBDG05);
                this.LB01.Items.Remove(DGQ05);
            }
            if (DGQ06.Items.Count == 0)
            {
                this.LB01.Items.Remove(LBDG06);
                this.LB01.Items.Remove(DGQ06);
            }
            if (DGQ07.Items.Count == 0)
            {
                this.LB01.Items.Remove(LBDG07);
                this.LB01.Items.Remove(DGQ07);
            }
            if (DGQ08.Items.Count == 0)
            {
                this.LB01.Items.Remove(LBDG08);
                this.LB01.Items.Remove(DGQ08);
            }
            if (DGQ09.Items.Count == 0)
            {
                this.LB01.Items.Remove(LBDG09);
                this.LB01.Items.Remove(DGQ09);
            }
            if (DGQ10.Items.Count == 0)
            {
                this.LB01.Items.Remove(LBDG10);
                this.LB01.Items.Remove(DGQ10);
            }
            if (LB01.Items.Count ==0)
            {
                this.TabCon.Items.Remove(Tab2);
            }
            #endregion
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            strID = string.Empty;
            strUID = string.Empty;
            Refresh_data();
            this._timer1.Stop();
            this.Label1.Visibility = Visibility.Hidden;
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            strID = "";
            this._timer1.Start();
            this.Label1.Visibility = Visibility.Visible;
        }
    }
}