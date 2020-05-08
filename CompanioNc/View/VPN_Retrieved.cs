using System;

namespace CompanioNc.View
{
    internal class VPN_Retrieved
    {
        private readonly string[] _header_want;
        private readonly DateTime _qdate;
        private readonly SQLTableName _sname;
        private readonly string _table;
        private readonly string _uid;

        public VPN_Retrieved(SQLTableName sname, string[] hw, string doc, string uid, DateTime qdate)
        {
            _sname = sname;
            _table = doc;
            _header_want = hw;
            _uid = uid;
            _qdate = qdate;
        }

        public string[] Header_Want
        {
            get { return _header_want; }
        }

        public DateTime QDate
        {
            get { return _qdate; }
        }

        public string Retrieved_Table
        {
            get { return _table; }
        }

        public SQLTableName SQL_Tablename
        {
            get { return _sname; }
        }

        public string UID
        {
            get { return _uid; }
        }
    }
}
