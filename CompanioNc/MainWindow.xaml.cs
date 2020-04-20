using CompanioNc.Models;
using CompanioNc.ViewModels;
using GlobalHotKey;
using System;
using System.Deployment.Application;
using System.Windows;
using System.Windows.Input;

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
    /// </summary>
    public partial class MainWindow : Window
    {
        private HotKeyManager hotKeyManager;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
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
            this.Title += " " + version;
            // Create the hotkey manager.
            hotKeyManager = new HotKeyManager();
            // Register Ctrl+F2 hotkey. Save this variable somewhere for the further unregistering.
            hotKeyManager.Register(Key.F2, ModifierKeys.Control);
            // Handle hotkey presses.
            hotKeyManager.KeyPressed += Hkp.HotKeyManagerPressed;
            Logging.Record_admin("Companion Log in", "");
            Refresh_data();
        }

        private void Refresh_data()
        {
            #region refresh all tabitem and listbox items
            this.TabCon.Items.Clear();
            this.TabCon.Items.Add(Tab1);
            this.TabCon.Items.Add(Tab2);
            this.TabCon.Items.Add(Tab3);
            this.TabCon.Items.Add(Tab4);
            this.TabCon.Items.Add(Tab5);
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

            ComDataDataContext dc = new ComDataDataContext();
            DGQuerry.ItemsSource = dc.sp_querytable();
            if (this.Label1.Content.ToString() == string.Empty) this.TabCon.Items.Remove(Tab5); //20200417: 沒有strUID就移除基本資料

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
            #endregion remove all unnessasary items
        }

        private void Main_Closed(object sender, EventArgs e)
        {
            Logging.Record_admin("Companion Log out", "");
            // Unregister Ctrl+Alt+F2 hotkey.
            hotKeyManager.Unregister(Key.F2, ModifierKeys.Control);
            // Dispose the hotkey manager.
            hotKeyManager.Dispose();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //strID = string.Empty;
            Refresh_data();
            //this._timer1.Stop();
            //this.Label1.Visibility = Visibility.Hidden;
            this.ACTextBox.Visibility = Visibility.Visible;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //strID = "";
            //this._timer1.Start();
            //this.Label1.Visibility = Visibility.Visible;
            this.ACTextBox.Visibility = Visibility.Hidden;
            this.ACTextBox.Text = string.Empty; //20200417 新增, uncheck時將Textbox清空
        }

        private void ACTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //strUID = ACTextBox.Text;
                Refresh_data();
            }
        }
    }
}