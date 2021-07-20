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
        //private static readonly log4net.ILog log = LogHelper.GetLogger();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        bool disposed = false;
        private readonly int _interval;
        private readonly WebTEst w;
        private readonly System.Timers.Timer _timer2;
        private string old_fSTATE; // the readyState of old frame(0)
        private string old_dSTATE; // the readyState of old upmost document 
        private string new_fSTATE; // the readyState of new frame(0)
        private string new_dSTATE; // the readyState of new upmost document 

        public FrameMonitor(WebTEst webTEst, int Interval)
        {
            w = webTEst;
            _interval = Interval;
            old_fSTATE = "non-exist";
            old_dSTATE = "non-exist";
            new_fSTATE = "non-exist";
            new_dSTATE = "non-exist";

            this._timer2 = new System.Timers.Timer
            {
                Interval = _interval
            };
            this._timer2.Elapsed += new System.Timers.ElapsedEventHandler(TimersTimer_Elapsed);
            _timer2.Start();
            log.Info("%%% timer2 for monitoring VPN started.");
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
                    HTMLDocument f = new HTMLDocument();
                    if (d.frames.length != 0)
                    {
                        // 有frame
                        f = d.frames.item(0).document.body.document;
                        new_dSTATE = d.readyState;
                        new_fSTATE = f.readyState;
                        if ((old_fSTATE == "loading" || old_fSTATE == "interactive" || old_fSTATE == "non-exist") && (new_fSTATE == "complete"))
                        {
                            // 20210719: mark this log to simplify logging
                            //log.Debug($"***** frame readystate fired; f(old):{old_fSTATE} f(new):{new_fSTATE}; d(old):{old_dSTATE} d(new):{new_dSTATE}. *****");
                            //fSTATE = f?.readyState;
                            // f is becoming ready
                            FrameLoadCompleteEventArgs ex = new FrameLoadCompleteEventArgs()
                            {
                                Message = FrameLoadStates.FrameLoadCompleted
                            };
                            OnFrameLoadComplete(ex);
                        }
                    }
                    else
                    {
                        new_dSTATE = d.readyState;
                        new_fSTATE = "non-exist";
                        if ((old_dSTATE == "loading" || old_dSTATE == "interactive") && (new_dSTATE == "complete"))
                        {
                            // 20210719: mark this log to simplify logging
                            //log.Debug($"***** document only readystate fired; f(old):{old_fSTATE} f(new):{new_fSTATE}; d(old):{old_dSTATE} d(new):{new_dSTATE}. *****");
                            // f is not becoming ready, but d is becoming ready
                            // this is like the situation of no NHI card
                            FrameLoadCompleteEventArgs ex = new FrameLoadCompleteEventArgs()
                            {
                                Message = FrameLoadStates.DocumentLoadCompletedButNotFrame
                            };
                            OnFrameLoadComplete(ex);
                        }
                    }
                    //log.Debug($"Before Document:{old_dSTATE}, Frame:{old_fSTATE}; After Document:{new_dSTATE}, Frame:{new_fSTATE}");
                    old_dSTATE = new_dSTATE;
                    old_fSTATE = new_fSTATE;
                    //Debug.WriteLine($"before fSTATE={fSTATE}");
                    //Debug.WriteLine($"Main: {d.readyState}, Child: {f.readyState}");
                    //Debug.WriteLine($"after fSTATE={fSTATE}");
                }
                catch (Exception)
                {
                    // Do nothing
                    // System.Windows.Forms.MessageBox.Show(ex.Message);
                    //log.Error(ex.Message);
                    //Debug.WriteLine(ex.Message);
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
                log.Info("%%% timer2 for monitoring VPN stopped.");
            }

            disposed = true;
        }

        public event FrameLoadCompleteEventHandler FrameLoadComplete;

    }

    public delegate void FrameLoadCompleteEventHandler(Object sender, FrameLoadCompleteEventArgs e);

}