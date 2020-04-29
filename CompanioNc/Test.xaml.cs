using HtmlAgilityPack;
using mshtml;
using System.Windows;

namespace CompanioNc
{
    /// <summary>
    /// Test.xaml 的互動邏輯
    /// </summary>
    public partial class Test : Window
    {
        //private System.Timers.Timer _timer1;
        //private string rSTATE;

        //public event FrameLoadCompleteEventHandler FrameLoadComplete;

        //protected virtual void OnFrameLoadComplete(FrameLoadCompleteEventArgs e)
        //{
        //    FrameLoadCompleteEventHandler handler = FrameLoadComplete;
        //    if (handler != null)
        //    {
        //        handler(this, e);
        //    }
        //}

        //public Test()
        //{
        //    InitializeComponent();
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this._timer1 = new System.Timers.Timer();
            //this._timer1.Interval = 100;
            //this._timer1.Elapsed += new System.Timers.ElapsedEventHandler(_TimersTimer_Elapsed);
            //_timer1.Start();

            this.g.Navigate(@"https://medcloud.nhi.gov.tw/imme0008/IMME0008S01.aspx");

            //this.FrameLoadComplete += G_FrameLoadComplete;
        }

        //private void G_FrameLoadComplete(object sender, FrameLoadCompleteEventArgs e)
        //{
        //    System.Windows.Forms.MessageBox.Show("測試frameload complete成功");
        //}

        //private void _TimersTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    this.Dispatcher.Invoke((Action)(() =>
        //    {
        //        try
        //        {
        //            // readyState 是可以讀的
        //            // 加event handler
        //            HTMLDocument d = (HTMLDocument)g.Document;
        //            HTMLDocument f = d.frames.item(0).document.body.document;
        //            if ((rSTATE == "loading" || rSTATE == "interactive") && (f.readyState == "complete"))
        //            {
        //                FrameLoadCompleteEventArgs args = new FrameLoadCompleteEventArgs();
        //                args.MyProperty = 0;
        //                rSTATE = f.readyState;
        //                OnFrameLoadComplete(args);
        //            }
        //            Debug.WriteLine($"before rSTATE={rSTATE}");
        //            rSTATE = f.readyState;
        //            Debug.WriteLine($"Main: {d.readyState}, Child: {f.readyState}");
        //            Debug.WriteLine($"after rSTATE={rSTATE}");
        //        }
        //        catch (Exception)
        //        {
        //            // Do nothing
        //        }
        //    }));
        //}

        //~Test()
        //{
        //    _timer1.Stop();
        //    _timer1.Dispose();
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /// 按tab成功
            /// HTMLDocument d = (HTMLDocument)g.Document;
            /// IHTMLElement a = d.getElementById("ContentPlaceHolder1_a_0060");
            /// a.click();
            /// 按排序鍵
            //HTMLDocument d = (HTMLDocument)g.Document;
            //HTMLDocument f = d.frames.item(0).document.body.document;
            //HTMLDocument htmlgvList = f.getElementById("ContentPlaceHolder1_gvList").document;
            //foreach (IHTMLElement th in htmlgvList.getElementsByTagName("th"))
            //{
            //    if (th.innerText == "就醫(調劑)日期(住院用藥起日)")
            //    {
            //        th.children(0).click();
            //    }
            //}
            // test1();
            // test2();

            //System.Windows.Forms.MessageBox.Show("Test Success!");

            // 找出頁數
            HTMLDocument d = (HTMLDocument)g.Document;
            HTMLDocument f = d.frames.item(0).document.body.document;
            //HtmlDocument p = new HtmlDocument();
            //p.LoadHtml(f.getElementById(@"ctl00$ContentPlaceHolder1$pg_gvList_input").outerHTML);
            //HtmlNodeCollection o = p.DocumentNode.SelectNodes("//option");

            //System.Windows.Forms.MessageBox.Show(o.Count.ToString());
            //string id = "ctl00$ContentPlaceHolder1$pg_gvList";
            //string pa = "2";
            foreach (IHTMLElement a in f.getElementById("ContentPlaceHolder1_pg_gvList").all)
            {
                if (a.innerText == ">")
                {
                    a.click();
                }
            }
            //}
            //string id = "ctl00$ContentPlaceHolder1$a_0030";
            //string pa = "";
            //object[] args = new object[2];
            //args[0] = (object)id;
            //args[1] = (object)pa;
            //f.InvokeScript("__doPostBack", args);
        }

        //private void test1()
        //{
        //    // 測試按鈕後, javascript繼續前進還是等sleep完才前進?
        //    Debug.WriteLine("begin to sleep");
        //    System.Threading.Thread.Sleep(2000);
        //    Debug.WriteLine("Sleep ends");
        //}

        //private void test2()
        //{
        //    // 測試按鈕後, iframe裡的readyState是否繼續改變?
        //    int i = 0;
        //    int j = 0;
        //    while ((i == 0) || (j > 3))
        //    {
        //        HTMLDocument d = (HTMLDocument)g.Document;
        //        HTMLDocument f = d?.frames.item(0).document.body.document;
        //        if (f.readyState == "complete") i = 1;
        //        System.Threading.Thread.Sleep(2000);
        //        Debug.WriteLine($"{j}");
        //        j++;
        //    }
        //}
    }
}