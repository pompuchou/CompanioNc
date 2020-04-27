using mshtml;
using System;
using System.Diagnostics;
using System.Timers;

namespace CompanioNc.View
{
    /// <summary>
    /// 20200427 created
    /// 用來取代WebTEst.g.LoadComplete
    /// Dependency: WebTEst.xaml, 專門為其設計, 並沒有廣泛性
    /// </summary>
    public class FrameMonitor
    {
        private int _interval;
        private WebTEst w;
        private System.Timers.Timer _timer2;
        private string rSTATE;

        public FrameMonitor(WebTEst webTEst, int Interval)
        {
            w = webTEst;
            _interval = Interval;

            this._timer2 = new System.Timers.Timer();
            this._timer2.Interval = _interval;
            this._timer2.Elapsed += new System.Timers.ElapsedEventHandler(_TimersTimer_Elapsed);
            _timer2.Start();
        }

        private void _TimersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            w.Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    // readyState 是可以讀的
                    // 加event handler
                    HTMLDocument d = (HTMLDocument)w.g.Document;
                    HTMLDocument f = d.frames.item(0).document.body.document;
                    if ((rSTATE == "loading" || rSTATE == "interactive") && (f.readyState == "complete"))
                    {
                        FrameLoadCompleteEventArgs args = new FrameLoadCompleteEventArgs();
                        args.MyProperty = 0;
                        rSTATE = f.readyState;
                        OnFrameLoadComplete(args);
                    }
                    Debug.WriteLine($"before rSTATE={rSTATE}");
                    rSTATE = f.readyState;
                    Debug.WriteLine($"Main: {d.readyState}, Child: {f.readyState}");
                    Debug.WriteLine($"after rSTATE={rSTATE}");
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }));
        }

        protected virtual void OnFrameLoadComplete(FrameLoadCompleteEventArgs e)
        {
            FrameLoadCompleteEventHandler handler = FrameLoadComplete;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event FrameLoadCompleteEventHandler FrameLoadComplete;

        ~FrameMonitor()
        {
            _timer2.Stop();
            _timer2.Dispose();
        }
    }

    public class FrameLoadCompleteEventArgs : EventArgs
    {
        public int MyProperty { get; set; }
    }

    public delegate void FrameLoadCompleteEventHandler(Object sender, FrameLoadCompleteEventArgs e);
}