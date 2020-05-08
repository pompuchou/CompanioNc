using CompanioNc.Models;
using System;
using System.Linq;

namespace CompanioNc.ViewModels
{
    public class CurrentPatient
    {
        private readonly string _uid;
        private readonly long _cid;
        private readonly string _cname;
        private readonly DateTime _bd;
        private readonly string _mf;
        private readonly string _p01;
        private readonly string _p02;
        private readonly string _p03;
        private readonly string _p04;

        public string UID  // 身分證字號
        {
            get { return _uid; }
        }

        public long CID  // 病歷號
        {
            get { return _cid; }
        }

        public string CNAME  // 姓名
        {
            get { return _cname; }
        }

        public DateTime BD  // 生日
        {
            get { return _bd; }
        }

        public string MF  // 性別
        {
            get { return _mf; }
        }

        public string P01  // 家中電話
        {
            get { return _p01; }
        }

        public string P02  // 手機
        {
            get { return _p02; }
        }

        public string P03  // 地址
        {
            get { return _p03; }
        }

        public string P04  // 註記
        {
            get { return _p04; }
        }

        public CurrentPatient(string StrUID)
        {
            Com_clDataContext dc = new Com_clDataContext();
            sp_ptdata_by_uidResult pt = dc.sp_ptdata_by_uid(StrUID).First();
            _uid = StrUID;
            _cid = (long)pt.cid;
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
