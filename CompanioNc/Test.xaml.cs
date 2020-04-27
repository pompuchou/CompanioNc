using mshtml;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System;

namespace CompanioNc
{
    /// <summary>
    /// Test.xaml 的互動邏輯
    /// </summary>
    public partial class Test : Window
    {
        private System.Timers.Timer _timer1;
        public Test()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this._timer1 = new System.Timers.Timer();
            this._timer1.Interval = 1;
            this._timer1.Elapsed += new System.Timers.ElapsedEventHandler(_TimersTimer_Elapsed);
            _timer1.Start();

            this.g.Navigate(@"https://medcloud.nhi.gov.tw/imme0008/IMME0008S01.aspx");
        }

        private void _TimersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                HTMLDocument d = (HTMLDocument)g.Document;
                if (d != null)
                {
                    //try
                    //{
                    //    HTMLDocument f = d.frames.item(0).document.body.document;
                    //    Debug.WriteLine($"Main: {d.readyState}, Child: {f.readyState}");
                    //}
                    //catch (Exception)
                    //{
                    //    Debug.WriteLine($"Main: {d.readyState}, Child is null.");
                    //}
                    Debug.WriteLine($"Main: {d.readyState}, Child is null.");
                }
                else
                {
                    Debug.WriteLine("Main is null, and Child is null.");
                }
            }));
        }
    }
}
