namespace CompanioNc.View
{
    internal class Target_Table
    {
        private readonly int? _children;
        private readonly string[] _header_want;
        private readonly SQLTableName _sname;
        private readonly string _targetid;

        /// <summary>
        /// 20200428 created
        /// target 是目標的DOM id, 例如: "ContentPlaceHolder1_gvList"
        /// sname 是存入SQL 的table name, 例如: med, 20200508改成 enum
        /// child 是指是否有children, 僅適用於關懷名單 
        /// hw header want, 需要的欄位
        /// </summary>
        /// <param name="target"></param>
        /// <param name="sname"></param>
        /// <param name="child"></param>
        /// <param name="hw"></param>

        public Target_Table(string target, SQLTableName sname, int? child, string[] hw)
        {
            _targetid = target;
            _sname = sname;
            _children = child;
            _header_want = hw;
        }

        /// <summary>
        /// 是否有children, 僅有關懷名單才有children的選項, 其餘都可以有直接的ID
        /// </summary>
        public int? Children
        {
            get { return _children; }
        }

        public string[] Header_Want
        {
            get { return _header_want; }
        }

        public SQLTableName Short_Name
        {
            get { return _sname; }
        }

        public string TargetID
        {
            get { return _targetid; }
        }
    }
}
