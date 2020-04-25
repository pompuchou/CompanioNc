using GlobalHotKey;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using WindowsInput;

namespace CompanioNc.ViewModels
{
    // hkp stands for hotkey pressed
    public class Hkp
    {
        private static readonly string VPN_URL = @"https://medcloud.nhi.gov.tw/imme0008/IMME0008S01.aspx"; 
        public static void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            if ((e.HotKey.Key == Key.F2) && (e.HotKey.Modifiers == ModifierKeys.Control))
            {
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
                //更新雲端資料
            }
            if ((e.HotKey.Key == Key.G) && (e.HotKey.Modifiers == ModifierKeys.Control))
            {
                //讀寫雲端資料
            }
        }
    }
}