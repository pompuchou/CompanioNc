using CompanioNc.Models;
using GlobalHotKey;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using WindowsInput;
using System.Windows;
using System.Windows.Input;

namespace CompanioNc.View
{
    /// <summary>
    /// 20210717建立
    /// </summary>
    public partial class Start : Window
    {
        //private static readonly log4net.ILog log = LogHelper.GetLogger();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
