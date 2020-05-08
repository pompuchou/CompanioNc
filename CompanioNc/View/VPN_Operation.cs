using System;
using System.Collections.Generic;

namespace CompanioNc.View
{
    internal class VPN_Operation
    {
        private readonly string _sname;

        private readonly string _tabid;

        private readonly List<Target_Table> _target;

        /// <summary>
        /// 20200428 created
        /// </summary>
        /// <param name="tab_id"></param>
        /// <param name="sname"></param>
        /// <param name="target"></param>
        public VPN_Operation(string tab_id, string sname, List<Target_Table> target)
        {
            _tabid = tab_id;
            _sname = sname;
            _target = target;
        }

        public DateTime QDate { get; set; }

        // 簡短名稱, 可以用來顯示訊息用, 例如: 檢驗檢查結果
        public string Short_Name
        {
            get { return _sname; }
        }

        // tab 的 ID, 可以用來點擊, 選擇tab, 例如: ContentPlaceHolder1_a_0008 是雲端藥歷
        public string TAB_ID
        {
            get { return _tabid; }
        }

        public List<Target_Table> Target
        {
            get { return _target; }
        }

        // 指向擷取的位置, 這個要好好設計一下,
        // 9個tabs, 11個tables: 9個tables有specific ID, 2個沒有; 8個在"ContentPlaceHolder1_divResult"之下, 3個在"ContentPlaceHolder1_PanS01"之下

        // 多頁也不用排序啊, 全部抓起來就好了, 不排序就快一點, 不限頁數的做法, 放棄排序, 但是要更好的翻頁程式
        public string UID { get; set; }
    }
}
