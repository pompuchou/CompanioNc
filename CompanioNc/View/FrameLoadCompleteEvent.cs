﻿using CompanioNc.ViewModels;
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
        private string fSTATE; // the readyState of frame(0)
        private string dSTATE; // the readyState of upmost document 

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
            log.Info("timer2 for monitoring VPN started.");

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
                    if (d.frames.length != 0) f = d.frames.item(0).document.body.document;
                    if ((fSTATE == "loading" || fSTATE == "interactive") && (f.readyState == "complete"))
                    {
                        log.Info($"***** frame readystate fired; f:{fSTATE}[{f?.readyState}]; d:{dSTATE}[{d.readyState}]. *****");
                        fSTATE = f?.readyState;
                        // f is becoming ready
                        FrameLoadCompleteEventArgs ex = new FrameLoadCompleteEventArgs()
                        {
                            Message = FrameLoadStates.FrameLoadCompleted
                        };
                        OnFrameLoadComplete(ex);
                    }
                    if ((dSTATE == "loading" || dSTATE == "interactive") && (d.readyState == "complete"))
                    {
                        log.Info($"***** document only readystate fired; f:{fSTATE}[{f?.readyState}]; d:{dSTATE}[{d.readyState}]. *****");
                        // f is not becoming ready, but d is becoming ready
                        // this is like the situation of no NHI card
                        FrameLoadCompleteEventArgs ex = new FrameLoadCompleteEventArgs()
                        {
                            Message = FrameLoadStates.DocumentLoadCompletedButNotFrame
                        };
                        OnFrameLoadComplete(ex);
                    }
                    //Debug.WriteLine($"before fSTATE={fSTATE}");
                    dSTATE = d.readyState;
                    fSTATE = f.readyState;
                    //Debug.WriteLine($"Main: {d.readyState}, Child: {f.readyState}");
                    //Debug.WriteLine($"after fSTATE={fSTATE}");
                }
                catch (Exception)
                {
                    // Do nothing
                    // System.Windows.Forms.MessageBox.Show(ex.Message);
                    // log.Error(ex.Message);
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
                log.Info("timer2 for monitoring VPN stopped.");
            }

            disposed = true;
        }

        public event FrameLoadCompleteEventHandler FrameLoadComplete;

    }

    public delegate void FrameLoadCompleteEventHandler(Object sender, FrameLoadCompleteEventArgs e);

}