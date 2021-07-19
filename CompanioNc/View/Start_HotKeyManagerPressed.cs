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

                // 20210718 簡單版本, Subjective

                // 決定日期, 有strID就用, 沒有就用今天
                string D = DateTime.Now.ToShortDateString();
                if (DateTime.TryParse(m.strID.Content.ToString().Split(' ')[0], out DateTime o)) D = o.ToShortDateString();

                Random crandom = new Random();
                // 1句的機率, 2/3*1*2/3=4/9
                // 3句的機率, 1/3*1*1/3=1/9
                // 2句的機率, 4/9
                // Short sentence
                // 1/3 機會有這一段
                int n;
                string output;
                List<string> strShort = new List<string>{"OK.", "No specific complaints.", 
                    "Satisfied with medication.", "Nothing particular.",
                    "Mostly unchanged since last visit.", "Aloof to inquiry.",
                    "For drug refill.", "Maintenance phase.",
                    "", "", "", "", "", "", "", "",
                    "", "", "", "", "", "", "", ""};
                n = crandom.Next(strShort.Count);
                output = strShort[crandom.Next(n)];

                // Adjectives
                List<string> strAdj = new List<string>{"Stationary ", 
                    "No change in ", "Improved ", 
                    "Stable ", "Unchanged ",
                    "Stable ", "Unchanged ",
                    "Fluctuated ", "Acceptable ", 
                    "Tolerable ", "Fairly good ", 
                    "Pretty good ", "Awesome ", 
                    "Fantastic ", "Uneventful ",
                    "Stationary ", "Stationary "};
                n = crandom.Next(strAdj.Count);
                if (output.Length != 0) output += " ";
                output += strAdj[crandom.Next(n)];

                // Nouns
                List<string> strNoun = new List<string>{"condition.", "clinical picture.",
                    "mental status.", "state.", 
                    "course.", "situation.", 
                    "being.", "progress.", 
                    "psychiatric state.", "circumstance.", 
                    "condition.", "condition.", 
                    "mental status.", "clinical picture.", 
                    "state.", "sate.", 
                    "mental status.", "clinical picture.", 
                    "situation.", "condition."};
                n = crandom.Next(strNoun.Count);
                output += strNoun[crandom.Next(n)];

                // Ask for
                // 1/3 有這一段
                n = crandom.Next(3);
                if (n == 0)
                {
                    List<string> strVerb = new List<string>{"Ask for ", "Request for ", 
                        "Need ", "Keep ", "Continue ", "Carry on ", "Keep on "};
                    n = crandom.Next(strVerb.Count);
                    output += $" {strVerb[crandom.Next(n)]}";

                    List<string> strAdj2 = new List<string>{"same ", "the same ",
                    "present ", "current ", "identical ", "", "", "", "", ""};
                    n = crandom.Next(strAdj2.Count);
                    output += strAdj2[crandom.Next(n)];

                    List<string> strNoun2 = new List<string>{"medication.",
                    "prescription refill.", "prescription.",
                    "treatment plan.", "drug treatment.",
                    "drug.", "psychiatric treatment."};
                    n = crandom.Next(strNoun2.Count);
                    output += strNoun2[crandom.Next(n)];
                }

                output =  $"{D}: {output}\n";
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
