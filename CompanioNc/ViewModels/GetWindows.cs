using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CompanioNc
{
    public struct WindowInfo
    {
        public IntPtr hWnd;
        public string szWindowName;
    }

    internal class GetWindow
    {
        private readonly string _key = string.Empty;
        private readonly string _type = string.Empty;

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
}