using mshtml;
using System.Collections.Generic;
using System.Linq;

namespace CompanioNc.View
{
    internal class VPN_Dictionary
    {
        /// <summary>
        /// 20200427 created
        /// aim: to make a storyboard for iterations of webpage manipulations
        /// </summary>

        #region Header_Wants

        // 資料都在ContentPlaceHolder1_divResult下, 只是部分table有命名: 例如: ContentPlaceHolder1_gvList
        // 雲端藥歷, ContentPlaceHolder1_a_0008, 雲端藥歷
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0008").Click()
        // 頁數: f.getElementById("ContentPlaceHolder1_pg_gvList"), NULL => 1 page, ELSE
        // 排序: "就醫(調劑)日期(住院用藥起日)" in th
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_med = new string[] { "項次", "來源", "主診斷", "ATC3名稱", "ATC5名稱", "成分名稱", "藥品健保代碼", "藥品名稱",
            "用法用量", "給藥日數", "藥品用量", "就醫(調劑)日期(住院用藥起日)", "慢連箋領藥日(住院用藥迄日)", "慢連箋原處方醫事機構代碼" };

        // 檢驗結果, ContentPlaceHolder1_a_0060, 檢查檢驗結果
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0060").Click()
        // 頁數: f.getElementById("ContentPlaceHolder1_pg_gvList"), NULL => 1 page, ELSE
        // 排序: "報告日期" in th
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_lab = new string[] {"項次", "來源", "就醫科別", "主診斷", "檢查檢驗類別", "醫令名稱", "檢查檢驗項目",
            "檢查檢驗結果/報告結果/病理發現及診斷", "參考值", "報告日期", "醫令代碼"};

        // 管制藥品, ContentPlaceHolder1_a_0009, 特定管制藥品用藥資訊, re = report 申報資料, up = upload 上傳資料
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0009").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0)
        private static readonly string[] hw_sch_re = new string[] { "成分名稱（成分代碼）", "就醫年月", "就醫次數", "就醫院所數", "總劑量", "總DDD數" };

        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(1)
        private static readonly string[] hw_sch_up = new string[] { "成分名稱（成分代碼）", "就診日期", "就診時間", "本院/他院", "總劑量", "總DDD數" };

        // 手術明細, ContentPlaceHolder1_a_0020, 手術明細紀錄
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0020").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_op = new string[] { "來源", "就醫科別", "主診斷名稱", "手術明細代碼", "手術明細名稱", "診療部位",
            "執行時間-起", "執行時間-迄", "醫令總量" };

        // 牙科處置, ContentPlaceHolder1_a_0030, 牙科處置及手術
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0030").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_dental = new string[] { "來源", "主診斷名稱", "牙醫處置代碼", "牙醫處置名稱", "診療部位", "執行時間-起", "執行時間-迄", "醫令總量" };

        // 過敏藥物, ContentPlaceHolder1_a_0040, 過敏藥, all = allergy
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0040").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_all = new string[] { "上傳日期", "醫療院所", "上傳註記", "過敏藥物" };

        // 出院病歷, ContentPlaceHolder1_a_0070, 出院病歷摘要, dis = discharge
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0070").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_dis = new string[] { "來源", "出院科別", "出院診斷", "住院日期", "出院日期" };

        // 復健醫療, ContentPlaceHolder1_a_0080, 復健醫療, reh = rehabilitation
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0080").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_PanS01").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_reh = new string[] { "診別", "來源", "主診斷碼", "治療類別", "強度", "醫令數量", "就醫日期/住院日期", "治療結束日期",
            "診療部位", "執行時間-起", "執行時間-迄" };

        // 中醫用藥, ContentPlaceHolder1_a_0090, 中醫用藥, gr = group, de = detail
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0090").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_PanS01").children(0) || f.getElementById("ContentPlaceHolder1_gvGroup")
        private static readonly string[] hw_tcm_gr = new string[] { "來源", "主診斷", "給藥日數", "慢連籤", "就醫(調劑)日期", "慢連籤領藥日", "就醫序號" };

        // 擷取: f.getElementById("ContentPlaceHolder1_PanS01").children(1) || f.getElementById("ContentPlaceHolder1_gvDetail")
        private static readonly string[] hw_tcm_de = new string[] { "主診斷", "藥品代碼", "複方註記", "基準方名", "效能名稱", "用法用量", "給藥日數", "劑型",
            "給藥總量", "就醫(調劑)日期", "慢連籤領藥日", "就醫序號" };

        #endregion Header_Wants

        private static List<VPN_Operation> Operation_Dictionary = new List<VPN_Operation>()
        {
            new VPN_Operation("ContentPlaceHolder1_a_0008", "雲端藥歷",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", "med", null, hw_med) }),
            new VPN_Operation("ContentPlaceHolder1_a_0060", "檢驗結果",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", "lab", null, hw_lab) }),
            new VPN_Operation("ContentPlaceHolder1_a_0009", "管制藥物",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_divResult", "sch_re", 0, hw_sch_re),
                                           new Target_Table("ContentPlaceHolder1_divResult", "sch_up", 1, hw_sch_up) }),
            new VPN_Operation("ContentPlaceHolder1_a_0020", "手術明細",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", "op", null, hw_op) }),
            new VPN_Operation("ContentPlaceHolder1_a_0030", "牙科處置",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", "dental", null, hw_dental) }),
            new VPN_Operation("ContentPlaceHolder1_a_0040", "過敏藥物",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", "all", null, hw_all) }),
            new VPN_Operation("ContentPlaceHolder1_a_0070", "出院病歷",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", "dis", null, hw_dis) }),
            new VPN_Operation("ContentPlaceHolder1_a_0080", "復健醫療",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", "reh", null, hw_reh) }),
            new VPN_Operation("ContentPlaceHolder1_a_0090", "中醫用藥",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvGroup", "tcm_gr", null, hw_tcm_gr),
                                           new Target_Table("ContentPlaceHolder1_gvDetail", "tcm_de", null, hw_tcm_de) })
        };

        public static VPN_Operation Making_new_operation(string tab_id)
        {
            VPN_Operation o = (from p in Operation_Dictionary
                               where p.TAB_ID == tab_id
                               select p).Single();
            return o;
        }
    }

    internal class VPN_Operation
    {
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

        private string _tabid;
        private string _sname;
        private List<Target_Table> _target;

        // tab 的 ID, 可以用來點擊, 選擇tab, 例如: ContentPlaceHolder1_a_0008 是雲端藥歷
        public string TAB_ID
        {
            get { return _tabid; }
        }

        // 簡短名稱, 可以用來顯示訊息用, 例如: 檢驗檢查結果
        public string Short_Name
        {
            get { return _sname; }
        }

        public List<Target_Table> Target
        {
            get { return _target; }
        }

        // 指向擷取的位置, 這個要好好設計一下,
        // 9個tabs, 11個tables: 9個tables有specific ID, 2個沒有; 8個在"ContentPlaceHolder1_divResult"之下, 3個在"ContentPlaceHolder1_PanS01"之下

        // 多頁也不用排序啊, 全部抓起來就好了, 不排序就快一點, 不限頁數的做法, 放棄排序, 但是要更好的翻頁程式
    }

    internal class Target_Table
    {
        /// <summary>
        /// 20200428 created
        /// </summary>
        /// <param name="target"></param>
        /// <param name="sname"></param>
        /// <param name="child"></param>
        /// <param name="hw"></param>
        public Target_Table(string target, string sname, int? child, string[] hw)
        {
            _targetid = target;
            _sname = sname;
            _children = child;
            _header_want = hw;
        }

        private string _targetid;
        private string _sname;
        private int? _children;
        private string[] _header_want;

        public string TargetID
        {
            get { return _targetid; }
        }

        public string Short_Name
        {
            get { return _sname; }
        }

        public int? Children
        {
            get { return _children; }
            set { _children = value; }
        }

        public string[] Header_Want
        {
            get { return _header_want; }
        }
    }

    internal class VPN_Retrieved
    {
        private string _sname;
        private string[] _header_want;
        private HTMLDocument _table;

        public VPN_Retrieved(string sname, string[] hw, HTMLDocument doc)
        {
            _sname = sname;
            _table = doc;
            _header_want = hw;
        }

        public string SQL_Tablename
        {
            get { return _sname; }
        }

        public HTMLDocument Retrieved_Table
        {
            get { return _table; }
        }

        public string[] Header_Want
        {
            get { return _header_want; }
        }
    }
}