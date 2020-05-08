using CompanioNc.Models;
using CompanioNc.View;
using CompanioNc.ViewModels;
using GlobalHotKey;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Windows;
using System.Windows.Input;
using WindowsInput;

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
    /// 20200419 started to MVVM
    /// 20200421 開始加入WebTEst
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log = LogHelper.GetLogger();

        private readonly TaskbarIcon tb = new TaskbarIcon();
        public HotKeyManager hotKeyManager;
        private WebTEst w;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Label1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Refresh();
        }

        private void Refresh ()
        {
            #region refresh all tabitem and listbox items
            this.TabCon.Items.Clear();
            this.TabCon.Items.Add(Tab1);
            this.TabCon.Items.Add(Tab2);
            this.TabCon.Items.Add(Tab3);
            this.TabCon.Items.Add(Tab4);
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
            #endregion refresh all tabitem and listbox items

            #region remove all unnessasary items
            if (DGMed.Items.Count == 0)
            {
                this.TabCon.Items.Remove(Tab1);
            }
            if (DGLab.Items.Count == 0)
            {
                this.TabCon.Items.Remove(Tab2);
            }
            if (DGQ01.Items.Count == 0)
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
            if (LB01.Items.Count == 0)
            {
                this.TabCon.Items.Remove(Tab3);
            }
            if (this.Label1.Text == string.Empty) this.TabCon.Items.Remove(Tab4); //20200417: 沒有strUID就移除基本資料
            #endregion remove all unnessasary items

            this.ACTextBox.Text = string.Empty;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string version = null;
            try
            {
                //// get deployment version
                version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch (InvalidDeploymentException)
            {
                //// you cannot read publish version when app isn't installed 
                //// (e.g. during debug)
                version = "debugging, not installed";
            }
            this.Title += $" v.{version}";

            ///WebTEst部分不採MVVM
            using (Com_clDataContext dc =new Com_clDataContext())
            {
                this.DGQuery.ItemsSource = dc.sp_querytable2();
            }

            // Create the hotkey manager.
            hotKeyManager = new HotKeyManager();

            // Register Ctrl+F2 hotkey. Save this variable somewhere for the further unregistering.
            try
            {
                hotKeyManager.Register(Key.F2, ModifierKeys.Control);
                hotKeyManager.Register(Key.T, ModifierKeys.Control);
                log.Info("Hotkey F2, Ctrl-T registered.");
            }
            catch (Exception ex)
            {
                log.Fatal($"Double Register Ctrl-F2, Ctrl-T. Fatal. Error: {ex.Message}");
                this.Close();
            }

            // Handle hotkey presses.
            hotKeyManager.KeyPressed += HotKeyManagerPressed;


            Refresh();

            Logging.Record_admin("Companion Log in", "");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Unregister Ctrl+Alt+F2 hotkey.
            try
            {
                hotKeyManager.Unregister(Key.F2, ModifierKeys.Control);
                hotKeyManager.Unregister(Key.T, ModifierKeys.Control);
                log.Info("Hotkey Ctrl-F2, Ctrl-T unregistered.");
            }
            catch (Exception ex)
            {
                log.Fatal($"Double Unregister Ctrl-F2, Ctrl-T. Fatal. Error: {ex.Message}");
            }

            // Dispose the hotkey manager.
            hotKeyManager.Dispose();

            // Close all windows at once, convenient
            if (w != null) w.Close();

            Logging.Record_admin("Companion Log out", "");
        }

        #region WebTEst PART, NO MVVM used
        private void VPNwindow_Checked(object sender, RoutedEventArgs e)
        {
            log.Info("WebTEst checkbox checked.");
            if (w is null) w = new WebTEst(this);
            // 20200508 因為不反應的功能, 這裡不可以register; 會先完成一輪, 造成重複register就會當機
            // 因此全部remark, 
            //// Register Ctrl+Y, Ctrl+G hotkey. Save this variable somewhere for the further unregistering.
            //hotKeyManager.Register(Key.Y, ModifierKeys.Control);
            //hotKeyManager.Register(Key.G, ModifierKeys.Control);
            //log.Info("Hotkey Ctrl-Y, Ctrl-G registered.");
            w.Show();
        }

        private void VPNwindow_Unchecked(object sender, RoutedEventArgs e)
        {
            log.Info("WebTEst checkbox unchecked.");
            w.Close();
            w = null;
        }
        
        private void WebTEst_refresh_Click(object sender, RoutedEventArgs e)
        {
            ///WebTEst部分不採MVVM
            Web_refresh();
            log.Info("WebTEst table refreshed.");
        }

        internal void Web_refresh()
        {
            ///WebTEst部分不採MVVM
            using (Com_clDataContext dc = new Com_clDataContext())
            {
                this.DGQuery.ItemsSource = dc.sp_querytable2();
            }
        }
        #endregion

        public void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            if ((e.HotKey.Key == Key.F2) && (e.HotKey.Modifiers == ModifierKeys.Control))
            {
                log.Info("Hotkey Ctrl-F2 pressed.");

                List<string> strAnswer = new List<string>{"OK.", "Stationary condition.", "For drug refill.", "No specific complaints.",
                        "No change in clinical picture.", "Satisfied with medication.", "Improved condition.", "Stable mental status.",
                        "Maintenance phase.", "Nothing particular."};
                // 先決定一句還是兩句
                Random crandom = new Random();
                int n = crandom.Next(2) + 1;
                int chosen = crandom.Next(10);
                string output = strAnswer[chosen];
                if (n == 2)
                {
                    strAnswer.Remove(output);
                    output += " " + strAnswer[crandom.Next(9)];
                }
                output = DateTime.Now.ToShortDateString() + ": " + output + "\n";
                InputSimulator sim = new InputSimulator();
                sim.Keyboard.TextEntry(output);
            }
            if ((e.HotKey.Key == Key.Y) && (e.HotKey.Modifiers == ModifierKeys.Control))
            {
                log.Info("Hotkey Ctrl-Y pressed.");

                //更新雲端資料
                w.HotKey_Ctrl_Y();
            }
            if ((e.HotKey.Key == Key.G) && (e.HotKey.Modifiers == ModifierKeys.Control))
            {
                log.Info("Hotkey Ctrl-G pressed.");

                //讀寫雲端資料
                w.HotKey_Ctrl_G();
            }
            if ((e.HotKey.Key == Key.T) && (e.HotKey.Modifiers == ModifierKeys.Control))
            {
                log.Info("Hotkey Ctrl-T pressed.");

                //讀寫雲端資料
                Com_clDataContext dc = new Com_clDataContext();
                var list_vpn = dc.sp_querytable2();
                string show_list = string.Empty;
                int order = 0;
                foreach (var a in list_vpn)
                {
                    if (order > 3) break;
                    show_list += $"[{a.uid}] {a.cname} \r\n";
                    order++;
                }
                tb.ShowBalloonTip("最近四人", show_list, BalloonIcon.Info);

            }
        }

    }
}