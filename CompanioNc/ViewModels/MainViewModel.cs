using CompanioNc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;

// 20200419 與MainWindow Binding 用的

namespace CompanioNc.ViewModels
{
    public class MainVM : INotifyPropertyChanged
	{
		private System.Timers.Timer _timer1;
		private string tempID = string.Empty;
		private string strID = string.Empty;
		private string strUID = string.Empty;
		private string strSDATE = string.Empty;

		public MainVM() //constructor
		{
            this._timer1 = new System.Timers.Timer();
            this._timer1.Interval = 500;
            this._timer1.Elapsed += new System.Timers.ElapsedEventHandler(_TimersTimer_Elapsed);
            _timer1.Start();
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

        private void _TimersTimer_Elapsed(object sender, ElapsedEventArgs e)
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
                    ComDataDataContext dc = new ComDataDataContext();
                    string[] s = strID.Split(' ');   //0 SDATE, 1 VIST, 2 RMNO, 4 Nr, 7 uid, 8 cname
                    if (s[7].Substring(1, 10) == "A000000000")
                    {
                        strID = string.Empty;
                        StrUID = string.Empty;
                        return;
                    }
                    StrUID = s[7].Substring(1, 10);  // Propertychanged
                    strSDATE = s[0];
                    dc.sp_insert_access(DateTime.Parse(s[0]), s[1], byte.Parse(s[2]), byte.Parse(s[4]), strUID, s[8], true);
                    // do something
                    //this.Dispatcher.Invoke((Action)(() =>
                    //{
                    //    // 更新資料
                    //    Refresh_data();
                    //}));
                    //' 寫入current_uid
                    if (System.IO.File.Exists(@"C:\vpn\current_uid.txt"))
                    {
                        // 如果有檔案就殺了它
                        System.IO.File.Delete(@"C:\vpn\current_uid.txt");
                    }
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(@"C:\vpn\current_uid.txt");
                    sw.Write(strUID);
                    sw.Close();
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
                    ComDataDataContext dc = new ComDataDataContext();
                    string[] s = strID.Split(' ');   //'0 SDATE, 1 VIST, 2 RMNO, 4 Nr, 7 uid, 8 cname
                    dc.sp_insert_access(DateTime.Parse(s[0]), s[1], byte.Parse(s[2]), byte.Parse(s[4]), strUID, s[8], false);
                    StrID = tempID;
                    StrUID = string.Empty;  // Propertychanged
                    strSDATE = string.Empty;
                    //this.Dispatcher.Invoke((Action)(() =>
                    //{
                    //    // 更新資料
                    //    Refresh_data();
                    //}));
                    if (System.IO.File.Exists(@"C:\vpn\current_uid.txt"))
                    {
                        //' 如果有檔案就殺了它
                        System.IO.File.Delete(@"C:\vpn\current_uid.txt");
                    }
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

        #region Properties
        public string StrUID
        {
            get { return strUID; }
            set
            {
                strUID = value;
                if (value == string.Empty)
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
                    ComDataDataContext dc = new ComDataDataContext();
                    CPatient = new CurrentPatient(value);
                    DGMed = dc.sp_meddata_by_uid(strUID).ToList<sp_meddata_by_uidResult>();
                    DGLab = dc.sp_labdata_by_uid(strUID).ToList<sp_labdata_by_uidResult>();
                    DGQ01 = dc.sp_cloudmed_by_uid(strUID).ToList<sp_cloudmed_by_uidResult>();
                    DGQ02 = dc.sp_cloudlab_by_uid(strUID).ToList<sp_cloudlab_by_uidResult>();
                    DGQ03 = dc.sp_cloudDEN_by_uid(strUID).ToList<sp_cloudDEN_by_uidResult>();
                    DGQ04 = dc.sp_cloudOP_by_uid(strUID).ToList<sp_cloudOP_by_uidResult>();
                    DGQ05 = dc.sp_cloudTCM_by_uid(strUID).ToList <sp_cloudTCM_by_uidResult>();
                    DGQ06 = dc.sp_cloudREH_by_uid(strUID).ToList <sp_cloudREH_by_uidResult>();
                    DGQ07 = dc.sp_cloudDIS_by_uid(strUID).ToList<sp_cloudDIS_by_uidResult>();
                    DGQ08 = dc.sp_cloudALL_by_uid(strUID).ToList<sp_cloudALL_by_uidResult>();
                    DGQ09 = dc.sp_cloudSCH_R_by_uid(strUID).ToList<sp_cloudSCH_R_by_uidResult>();
                    DGQ10 = dc.sp_cloudSCH_U_by_uid(strUID).ToList<sp_cloudSCH_U_by_uidResult>();
                }
                OnPropertyChanged("StrUID");
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

        public string StrID
        {
            get { return strID; }
            set 
            { 
                strID = value;
                OnPropertyChanged("StrID");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

        #endregion

        ~MainVM()  // destructor
		{
            _timer1.Stop();
        }
    }

	public class CurrentPatient
	{
		private string _uid;

		public string UID  // 身分證字號
		{
			get { return _uid; }
		}

		private int _cid;

		public int CID  // 病歷號
		{
			get { return _cid; }
		}

		private string _cname;

		public string CNAME  // 姓名
		{
			get { return _cname; }
		}

		private DateTime _bd;

		public DateTime BD  // 生日
		{
			get { return _bd; }
		}

		private string _mf;

		public string MF  // 性別
		{
			get { return _mf; }
		}

		private string _p01;

		public string P01  // 家中電話
		{
			get { return _p01; }
		}

		private string _p02;

		public string P02  // 手機
		{
			get { return _p02; }
		}

		private string _p03;

		public string P03  // 地址
		{
			get { return _p03; }
		}

		private string _p04;

		public string P04  // 註記
		{
			get { return _p04; }
			set { _p04 = value; }
		}

		public CurrentPatient(string StrUID)
		{
			ComDataDataContext dc = new ComDataDataContext();
			sp_ptdata_by_uidResult pt = dc.sp_ptdata_by_uid(StrUID).First();
			_uid = StrUID;
			_cid = (int)pt.cid;
			_cname = pt.cname;
			_mf = pt.mf;
			_bd = pt.bd;
			_p01 = pt.p01;
			_p02 = pt.p02;
			_p03 = pt.p03;
			_p04 = pt.p04;
		}
	}
}
