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
    public class FrameMonitor : IDisposable
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;
        private readonly int _interval;
        private readonly WebTEst w;
        private readonly System.Timers.Timer _timer2;
        private string rSTATE;

        public FrameMonitor(WebTEst webTEst, int Interval)
        {
            w = webTEst;
            _interval = Interval;

            this._timer2 = new System.Timers.Timer
            {
                Interval = _interval
            };
            this._timer2.Elapsed += new System.Timers.ElapsedEventHandler(TimersTimer_Elapsed);
            _timer2.Start();
        }

        private void TimersTimer_Elapsed(object sender, ElapsedEventArgs e)
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
                        FrameLoadCompleteEventArgs args = new FrameLoadCompleteEventArgs
                        {
                            MyProperty = 0
                        };
                        rSTATE = f.readyState;
                        OnFrameLoadComplete(args);
                    }
                    //Debug.WriteLine($"before rSTATE={rSTATE}");
                    rSTATE = f.readyState;
                    //Debug.WriteLine($"Main: {d.readyState}, Child: {f.readyState}");
                    //Debug.WriteLine($"after rSTATE={rSTATE}");
                }
                catch (Exception ex)
                {
                    // Do nothing
                    // System.Windows.Forms.MessageBox.Show(ex.Message);
                    Debug.WriteLine(ex.Message);
                }
            }));
        }

        protected virtual void OnFrameLoadComplete(FrameLoadCompleteEventArgs e)
        {
            FrameLoadComplete?.Invoke(this, e);
            // 原本是
            //FrameLoadCompleteEventHandler handler = FrameLoadComplete;
            //if (handler != null)
            //{
            //    handler(this, e);
            //}
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // Free any other managed objects here.
                //
                _timer2.Stop();
                _timer2.Dispose();
            }

            disposed = true;
        }

        public event FrameLoadCompleteEventHandler FrameLoadComplete;

    }

    public class FrameLoadCompleteEventArgs : EventArgs
    {
        public int MyProperty { get; set; }
    }

    public delegate void FrameLoadCompleteEventHandler(Object sender, FrameLoadCompleteEventArgs e);
}