using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CompanioNc.View
{
    /// <summary>
    /// WebTEstView.xaml 的互動邏輯
    /// 20210716: 再次修改結構, 把一些主要的副程式都單獨成檔了, 以方便閱讀
    ///           現在面臨到問題, 因為健保署又改程式, 改的時間大約是六月左右, 造成檢驗報告是空白的,會當機
    /// 20200425: 決定不採用MVVM了, 要用code behind
    ///             幾個要求:
    ///             01. 分辨需要的幾個tab
    ///             02. 主動去找要的tab,而不是被動一個個看哪個有哪個沒有
    ///             03. 每個tab都確實要抓到,不能漏
    ///             04. 新個案可以直接寫入tbl_patients
    ///             05. 每次讀完卡,可以自動更新資料
    ///             06. 不再透過uid.txt直接用程式內部的變數傳遞UID
    ///             07. 儲存特殊註記
    ///             08. 儲存提醒
    ///             09. 改善找到UID, 之前怎麼新個案就找不到?
    ///             10. 新個案等於是要在開杏翔的情況下才能輸入,這樣就避免了輸入中間三位身分證號可能的錯誤
    /// </summary>
    public partial class WebTEst : Window
    {
        #region FLAGS

        //private static readonly log4net.ILog log = LogHelper.GetLogger();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string VPN_URL = @"https://medcloud.nhi.gov.tw/imme0008/IMME0008S01.aspx";
        private const string DEFAULT_URL = @"https://www.googl.com";
        private const string HTML_DIRECTORY = @"C:\vpn\html";

        private readonly string[] tab_id_wanted = new string[] { "ContentPlaceHolder1_a_0008", "ContentPlaceHolder1_a_0009", "ContentPlaceHolder1_a_0020",
                                                                 "ContentPlaceHolder1_a_0030", "ContentPlaceHolder1_a_0040", "ContentPlaceHolder1_a_0060",
                                                                 "ContentPlaceHolder1_a_0070", "ContentPlaceHolder1_a_0080", "ContentPlaceHolder1_a_0090" };

        private readonly Queue<VPN_Operation> QueueOperation = new Queue<VPN_Operation>();
        private VPN_Operation current_op;
        private readonly List<VPN_Retrieved> ListRetrieved = new List<VPN_Retrieved>();
        private int current_page, total_pages;

        private readonly TaskbarIcon tb = new TaskbarIcon();

        private readonly Start s;
        private readonly FrameMonitor fm;

        #endregion FLAGS

        #region Constructor, Loaded, and Closed

        public WebTEst(Start ss)
        {
            // 把Caller傳遞過來
            s = ss;
            fm = new FrameMonitor(this, 100);
            InitializeComponent();
        }

        private void WebTEst_Loaded(object sender, RoutedEventArgs e)
        {
            // 漏了 +=, 難怪不fire
            log.Info($" ");
            log.Info("===========================================================================");
            fm.FrameLoadComplete += F_LoadCompleted;
            log.Info($"-- add delegate F_LoadCompleted.");

            // 20200508 加上此段, 因為如果沒有健保卡, 根本不會觸發F_LoadCompleted.
            // activate hotkeys 0
            // 如果我讓它可以觸發的話, 就不用activate hotkeys了
            // Activate_Hotkeys();

            log.Info($"Start to load {VPN_URL} not by hotkey.");
            // 20210716: g 是什麼啊?
            // 原來g就是連覽器
            this.g.Navigate(VPN_URL);
        }

        private void WebTEst_Closed(object sender, EventArgs e)
        {
            // deactivate hotkeys 1
            Deactivate_Hotkeys();
            log.Info($"WebTEst is being closed.");
            // 20210717 改m為s.m
            s.m.VPNwindow.IsChecked = false;
            fm.Dispose();
        }

        #endregion Constructor, Loaded, and Closed

        #region Hotkeys, Functions

        public void HotKey_Ctrl_Y()
        {
            /// 目的: 更新雲端資料, 讀寫雲端資料
            /// 現在可以合併兩個步驟為一個步驟
            /// 想到一個複雜的方式, 不斷利用LoadCompleted
            log.Info($" ");
            log.Info("===========================================================================");
            fm.FrameLoadComplete += F_LoadCompleted;
            log.Info("-- add delegate F_LoadCompleted.");

            // deactivate hotkeys 2
            Deactivate_Hotkeys();

            this.g.Navigate(VPN_URL);
            log.Info($"Start to load {VPN_URL} by hotkey.");
        }

        public void HotKey_Ctrl_G()
        {
            log.Info($" ");
            log.Info("===========================================================================");
            fm.FrameLoadComplete += F_LoadCompleted;
            log.Info("-- add delegate F_LoadCompleted.");
            //this.g.Navigate(DEFAULT_URL);

            // deactivate hotkeys 2
            Deactivate_Hotkeys();

            FrameLoadCompleteEventArgs ex = new FrameLoadCompleteEventArgs()
            {
                Message = FrameLoadStates.DirectCall
            };
            F_LoadCompleted(this, ex);
        }

        private void Activate_Hotkeys()
        {
            // 20200508 已經完成了, 又開始可以有反應了
            try
            {
                s.hotKeyManager.Register(Key.Y, ModifierKeys.Control);
                s.hotKeyManager.Register(Key.G, ModifierKeys.Control);
                log.Info("Hotkey Ctrl-Y, Ctrl-G registered.");
            }
            catch (Exception ex)
            {
                log.Fatal($"Double Register Ctrl-Y, Ctrl-G. Fatal. Error: {ex.Message}");
            }
        }

        private void Deactivate_Hotkeys()
        {
            // 20200508 加上不反應期的功能
            try
            {
                s.hotKeyManager.Unregister(Key.Y, ModifierKeys.Control);
                s.hotKeyManager.Unregister(Key.G, ModifierKeys.Control);
                log.Info("Hotkey Ctrl-Y, Ctrl-G 1 unregistered.");
            }
            catch (Exception ex)
            {
                log.Fatal($"Double Unregister Ctrl-Y, Ctrl-G. Fatal. Error: {ex.Message}");
            }
        }

        #endregion Hotkeys, Functions    
    }
}