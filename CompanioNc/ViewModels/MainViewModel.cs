﻿using CompanioNc.Models;
using CompanioNc.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Globalization;
using System.Linq;
using System.Timers;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

// 20200419 與MainWindow Binding 用的
// 20200503 add logging
namespace CompanioNc.ViewModels
{
    public class MainVM : INotifyPropertyChanged
    {
        //private static readonly log4net.ILog log = LogHelper.GetLogger();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly System.Timers.Timer _timer1;

        public MainVM() //constructor
        {
            this._timer1 = new System.Timers.Timer
            {
                Interval = 500
            };
            this._timer1.Elapsed += new System.Timers.ElapsedEventHandler(TimersTimer_Elapsed);
            //_timer1.Start();

            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                StrUID = "A123871035";
            }

            string version = null;
            try
            {
                //// get deployment version
                version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch (InvalidDeploymentException)
            {
                //// you cannot read publish version when app isn't installed 
                //// (e.g. during debug)
                version = "debugging, not installed";
            }
            log.Info($" ");
            log.Info($"*****************************************");
            log.Info($"* CompanioNc log in, version: {version}. *");
            log.Info($"*****************************************");
            log.Info($" ");

            // initially not unplug
            UnPlug = false;
            // initialization of command property
            Send_UID = new SendUID(this);
        }

        private CurrentPatient _currentPatient;

        public CurrentPatient CPatient
        {
            get { return _currentPatient; }
            set
            {
                _currentPatient = value;
                OnPropertyChanged("CPatient");
            }
        }

        private void TimersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                GetWindow g = new GetWindow("問診");
                // vb.NET里面的vbCr & vbLf 在c# 是\r\n, chr(13) chr(10)
                // 第二個是身分證字號
                //tempID = g.Key.Split('\n')[2];
                tempID = g.Key;
                //string[] ss = tempID.Split('\n');
            }
            catch
            {
                tempID = string.Empty;
            }
            if (StrID == string.Empty)
            {
                if (tempID == string.Empty)
                {
                    // condition 1, strID = "" => ""                     do nothing
                    return;
                }
                else
                {
                    // 檢單查核, 如果分解後數目小於8, 應該就不是正確的
                    // 20190930似乎有效
                    if (tempID.Split(' ').Length < 8)
                    {
                        //'MessageBox.Show("抓到了")
                        return;
                    }
                    // condition 2, strID = "" => something A            record A, starttime
                    // 要做很多事情, 分解
                    // 20190930 有些"問診畫面"的狀態,文字是不一樣的,這樣的話會有錯誤
                    StrID = tempID;
                    Com_clDataContext dc = new Com_clDataContext();
                    string[] s = strID.Split(' ');   //0 SDATE, 1 VIST, 2 RMNO, 4 Nr, 7 uid, 8 cname
                    if (s[7].Substring(1, 10) == "A000000000")
                    {
                        strID = string.Empty;
                        StrUID = string.Empty;
                        return;
                    }
                    StrUID = s[7].Substring(1, 10);  // Propertychanged
                    dc.sp_insert_access(DateTime.Parse(s[0]), s[1], byte.Parse(s[2]), byte.Parse(s[4]), strUID, s[8], true);
                }
            }
            else
            {
                if (StrID == tempID)
                {
                    // condition 3, strID = something A => something A   do nothing
                    return;
                }
                else if (tempID == string.Empty)
                {
                    //' condition 4, strID = something A => ""            record endtime, write into database
                    //' 做的事情也不少
                    Com_clDataContext dc = new Com_clDataContext();
                    string[] s = strID.Split(' ');   //'0 SDATE, 1 VIST, 2 RMNO, 4 Nr, 7 uid, 8 cname
                    dc.sp_insert_access(DateTime.Parse(s[0]), s[1], byte.Parse(s[2]), byte.Parse(s[4]), strUID, s[8], false);
                    StrID = tempID;
                    StrUID = string.Empty;  // Propertychanged
                }
                else
                {
                    //' condition 5, strID = something A => something B   I don't know if this is possible
                    //' 有可能嗎? 我不知道
                    //' 20191001 答案揭曉了,有可能,因為THESIS在畫form時會有A000000000臨時的資料,然後再讀資料庫蓋上,就會出現something A => something B的情況
                    //' 我採用檢核若A000000000的情形就不要寫入的方式處理
                    //' 檢單查核, 如果分解後數目小於8, 應該就不是正確的
                    //'                MessageBox.Show("抓到了! " + vbCrLf + strID + "=>" + tempID)
                    if (tempID.Split(' ').Length < 8)
                    {
                        return;
                    }
                }
            }
        }

        private void Update_Data(string sUID)
        {
            if (sUID == string.Empty)
            {
                CPatient = null;
                DGMed = null;
                DGLab = null;
                DGQ01 = null;
                DGQ02 = null;
                DGQ03 = null;
                DGQ04 = null;
                DGQ05 = null;
                DGQ06 = null;
                DGQ07 = null;
                DGQ08 = null;
                DGQ09 = null;
                DGQ10 = null;
            }
            else
            {
                Com_clDataContext dc = new Com_clDataContext();
                CPatient = new CurrentPatient(sUID);
                DGMed = dc.sp_meddata_by_uid(sUID).ToList<sp_meddata_by_uidResult>();
                DGLab = dc.sp_labdata_by_uid(sUID).ToList<sp_labdata_by_uidResult>();
                DGQ01 = dc.sp_cloudmed_by_uid(sUID).ToList<sp_cloudmed_by_uidResult>();
                DGQ02 = dc.sp_cloudlab_by_uid(sUID).ToList<sp_cloudlab_by_uidResult>();
                DGQ03 = dc.sp_cloudDEN_by_uid(sUID).ToList<sp_cloudDEN_by_uidResult>();
                DGQ04 = dc.sp_cloudOP_by_uid(sUID).ToList<sp_cloudOP_by_uidResult>();
                DGQ05 = dc.sp_cloudTCM_by_uid(sUID).ToList<sp_cloudTCM_by_uidResult>();
                DGQ06 = dc.sp_cloudREH_by_uid(sUID).ToList<sp_cloudREH_by_uidResult>();
                DGQ07 = dc.sp_cloudDIS_by_uid(sUID).ToList<sp_cloudDIS_by_uidResult>();
                DGQ08 = dc.sp_cloudALL_by_uid(sUID).ToList<sp_cloudALL_by_uidResult>();
                DGQ09 = dc.sp_cloudSCH_R_by_uid(sUID).ToList<sp_cloudSCH_R_by_uidResult>();
                DGQ10 = dc.sp_cloudSCH_U_by_uid(sUID).ToList<sp_cloudSCH_U_by_uidResult>();
                // StrID: "2020/04/25 上午 02 診 015 號 - (H223505857) 陳俞潔"
                if (unPlug) StrID = $"{DateTime.Now.ToString("g", CultureInfo.CurrentUICulture)} - ({_currentPatient.CID}) {_currentPatient.CNAME}";
            }
        }

        #region Control Property

        // Key property 所有資料幾乎跟著連動
        private string strUID = string.Empty;

        public string StrUID
        {
            get { return strUID; }
            set
            {
                strUID = value;
                Update_Data(value);
                if (!string.IsNullOrEmpty(value)) log.Info($"{value} is being displayed.");
                OnPropertyChanged("StrUID");
            }
        }

        private bool unPlug;

        public bool UnPlug
        {
            get { return unPlug; }
            set
            {
                unPlug = value;
                if (value)
                {
                    // true, which means UNPLUG
                    this._timer1.Stop();
                    log.Info("%% timer1 for monitoring Thesis stopped.");
                    StrUID = string.Empty;
                    StrID = string.Empty;
                }
                else
                {
                    // false, default value
                    StrUID = string.Empty;
                    StrID = string.Empty;
                    log.Info("%% timer1 for monitoring Thesis started by unPlug checkbox.");
                    this._timer1.Start();
                }
                OnPropertyChanged("UnPlug");
            }
        }

        // Command property
        public SendUID Send_UID { get; set; }

        #endregion Control Property

        #region Data Properties

        private string tempID = string.Empty;
        private string strID = string.Empty;

        public string StrID
        {
            get { return strID; }
            set
            {
                strID = value;
                OnPropertyChanged("StrID");
            }
        }

        private List<sp_cloudSCH_U_by_uidResult> dgq10;

        public List<sp_cloudSCH_U_by_uidResult> DGQ10
        {
            get { return dgq10; }
            set
            {
                dgq10 = value;
                OnPropertyChanged("DGQ10");
            }
        }

        private List<sp_cloudSCH_R_by_uidResult> dgq09;

        public List<sp_cloudSCH_R_by_uidResult> DGQ09
        {
            get { return dgq09; }
            set
            {
                dgq09 = value;
                OnPropertyChanged("DGQ09");
            }
        }

        private List<sp_cloudALL_by_uidResult> dgq08;

        public List<sp_cloudALL_by_uidResult> DGQ08
        {
            get { return dgq08; }
            set
            {
                dgq08 = value;
                OnPropertyChanged("DGQ08");
            }
        }

        private List<sp_cloudDIS_by_uidResult> dgq07;

        public List<sp_cloudDIS_by_uidResult> DGQ07
        {
            get { return dgq07; }
            set
            {
                dgq07 = value;
                OnPropertyChanged("DGQ07");
            }
        }

        private List<sp_cloudREH_by_uidResult> dgq06;

        public List<sp_cloudREH_by_uidResult> DGQ06
        {
            get { return dgq06; }
            set
            {
                dgq06 = value;
                OnPropertyChanged("DGQ06");
            }
        }

        private List<sp_cloudTCM_by_uidResult> dgq05;

        public List<sp_cloudTCM_by_uidResult> DGQ05
        {
            get { return dgq05; }
            set
            {
                dgq05 = value;
                OnPropertyChanged("DGQ05");
            }
        }

        private List<sp_cloudOP_by_uidResult> dgq04;

        public List<sp_cloudOP_by_uidResult> DGQ04
        {
            get { return dgq04; }
            set
            {
                dgq04 = value;
                OnPropertyChanged("DGQ04");
            }
        }

        private List<sp_cloudDEN_by_uidResult> dgq03;

        public List<sp_cloudDEN_by_uidResult> DGQ03
        {
            get { return dgq03; }
            set
            {
                dgq03 = value;
                OnPropertyChanged("DGQ03");
            }
        }

        private List<sp_labdata_by_uidResult> dgLab;

        public List<sp_labdata_by_uidResult> DGLab
        {
            get { return dgLab; }
            set
            {
                dgLab = value;
                OnPropertyChanged("DGLab");
            }
        }

        private List<sp_meddata_by_uidResult> dgMed;

        public List<sp_meddata_by_uidResult> DGMed
        {
            get { return dgMed; }
            set
            {
                dgMed = value;
                OnPropertyChanged("DGMed");
            }
        }

        private List<sp_cloudlab_by_uidResult> dgq02;

        public List<sp_cloudlab_by_uidResult> DGQ02
        {
            get { return dgq02; }
            set
            {
                dgq02 = value;
                OnPropertyChanged("DGQ02");
            }
        }

        private List<sp_cloudmed_by_uidResult> dgq01;

        public List<sp_cloudmed_by_uidResult> DGQ01
        {
            get { return dgq01; }
            set
            {
                dgq01 = value;
                OnPropertyChanged("DGQ01");
            }
        }

        #endregion Data Properties

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        ~MainVM()  // destructor
        {
            _timer1.Stop();
            log.Info("%% timer1 for monitoring Thesis stopped.");

        }
    }

}