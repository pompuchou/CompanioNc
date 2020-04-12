using HDLibrary.Wpf.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WindowsInput;
using WindowsInput.Native;

namespace CompanioNc
{
    [Serializable]
    public class CustomHotKey : HotKey
    {
        public CustomHotKey(string name, Key key, ModifierKeys modifiers, bool enabled)
            : base(key, modifiers, enabled)
        {
            Name = name;
        }

        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged(name);
                }
            }
        }

        protected override void OnHotKeyPress()
        {
            //MessageBox.Show(string.Format("'{0}' has been pressed ({1})", Name, this));
            switch (Name)
            {
                case "ShowPopup":
                    MessageBox.Show(string.Format("'{0}' has been pressed ({1})", Name, this));
                    break;

                case "Paste":
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
                    break;
            }

            base.OnHotKeyPress();
        }

        protected CustomHotKey(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            Name = info.GetString("Name");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Name", Name);
        }
    }

    public class BooleanToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                bool b;
                if (bool.TryParse((string)value, out b))
                {
                    if (b == true) return Brushes.Red;
                }
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                bool b;
                if (bool.TryParse((string)value, out b))
                {
                    if (b == true) return FontWeights.UltraBold;
                }
            }
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public struct WindowInfo
    {
        public IntPtr hWnd;
        public string szWindowName;
    }

    internal class GetWindow
    {
        private string _key = string.Empty;
        private string _type = string.Empty;

        //枚举所有屏幕上的顶层窗口，并将窗口句柄传送给应用程序定义的回调函数,lpEnumFunc回调函数的指针。
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, int lParam);

        //取的子視窗控制
        [DllImport("user32.dll")]
        private static extern int EnumChildWindows(IntPtr hWndParent, CallBack lpfn, int lParam);

        //這是user32的功能, 取得視窗名字
        // 取得控件的文字(Text)
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowText(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);

        // 函數代理
        private delegate bool WNDENUMPROC(IntPtr hWnd, int lParam);

        // 回調函數代理
        private delegate bool CallBack(int hwnd, int lParam);

        private WindowInfo[] GetAllDesktopWindows()
        {
            List<WindowInfo> wndList = new List<WindowInfo>();

            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                WindowInfo wnd = new WindowInfo();
                StringBuilder sb = new StringBuilder(256);
                //get hwnd
                wnd.hWnd = hWnd;
                //获取窗口名
                GetWindowText(hWnd, sb, sb.Capacity);
                wnd.szWindowName = sb.ToString();
                //add it into list
                // the point of selection *******************
                if (sb.ToString().Contains(_type))
                {
                    wndList.Add(wnd);
                }
                return true;
            }, 0);
            return wndList.ToArray();
        }

        // pHWD stands for parent Handle
        private WindowInfo[] GetAllChildWindows(IntPtr pHWD)
        {
            List<WindowInfo> chiList = new List<WindowInfo>();
            EnumChildWindows(pHWD, delegate (int hWnd, int lParam)
            {
                WindowInfo chi = new WindowInfo();
                StringBuilder sb = new StringBuilder(256);
                //get hwd
                chi.hWnd = new IntPtr(hWnd);
                //取得文字
                int len;
                len = GetWindowText(new IntPtr(hWnd), sb, sb.Capacity);
                if (len > 25)
                {
                    chi.szWindowName = sb.ToString();
                    chiList.Add(chi);
                }
                return true;
            }, 0);
            return chiList.ToArray();
        }

        public GetWindow(string type)
        {
            _type = type;
            WindowInfo[] a = GetAllDesktopWindows();
            if (a.Length > 0)
            {
                // >0表示有貨, 取第一位就好
                WindowInfo[] key = GetAllChildWindows(a[0].hWnd);
                if (key.Length > 0) _key = key[0].szWindowName.ToString();
            }
        }

        public string Key
        {
            get { return _key; }
        }
    }

    public partial class MainWindow
    {
        public void Record_error(string er)
        {
            ///created on 2020/03/28, transcribed from vb.net
            ///寫入錯誤訊息
            ComDataContext dc = new ComDataContext();
            log_Err newErr = new log_Err()
            {
                error_date = DateTime.Now,
                application_name = System.Reflection.Assembly.GetExecutingAssembly().FullName,
                machine_name = Dns.GetHostName(),
                ip_address = Dns.GetHostEntry(Dns.GetHostName()).AddressList.ToString(),
                userid = "Ethan",
                error_message = er
            };
            dc.log_Err.InsertOnSubmit(newErr);
            dc.SubmitChanges();
        }

        public void Record_admin(string op, string des)
        {
            ///寫入作業訊息
            ComDataContext dc = new ComDataContext();
            log_Adm newLog = new log_Adm()
            {
                regdate = DateTime.Now,
                application_name = System.Reflection.Assembly.GetExecutingAssembly().FullName.Substring(0, 49),
                machine_name = Dns.GetHostName(),
                ip_address = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString(),
                userid = "Ethan",
                operation_name = op,
                description = des
            };
            dc.log_Adm.InsertOnSubmit(newLog);
            dc.SubmitChanges();
        }
    }
}