using System.Collections.Generic;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
        //private static readonly log4net.ILog log = LogHelper.GetLogger();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 20200427 created
        /// aim: to make a storyboard for iterations of webpage manipulations
        /// </summary>

        #region Header_Wants

        // 過敏藥物, ContentPlaceHolder1_a_0040, 過敏藥, all = allergy
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0040").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_all = new string[] { "上傳日期", "醫療院所", "上傳註記", "過敏藥物" };

        // 牙科處置, ContentPlaceHolder1_a_0030, 牙科處置及手術
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0030").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_dental = new string[] { "來源", "主診斷名稱", "牙醫處置代碼", "牙醫處置名稱", "診療部位", "執行時間-起", "執行時間-迄", "醫令總量" };

        // 出院病歷, ContentPlaceHolder1_a_0070, 出院病歷摘要, dis = discharge
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0070").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_dis = new string[] { "來源", "出院科別", "出院診斷", "住院日期", "出院日期" };

        // 檢驗結果, ContentPlaceHolder1_a_0060, 檢查檢驗結果
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0060").Click()
        // 頁數: f.getElementById("ContentPlaceHolder1_pg_gvList"), NULL => 1 page, ELSE
        // 排序: "報告日期" in th
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_lab = new string[] {"項次", "來源", "就醫科別", "主診斷", "檢查檢驗類別", "醫令名稱", "檢查檢驗項目",
            "檢查檢驗結果/報告結果/病理發現及診斷", "參考值", "報告日期", "醫令代碼"};

        // 資料都在ContentPlaceHolder1_divResult下, 只是部分table有命名: 例如: ContentPlaceHolder1_gvList
        // 雲端藥歷, ContentPlaceHolder1_a_0008, 雲端藥歷
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0008").Click()
        // 頁數: f.getElementById("ContentPlaceHolder1_pg_gvList"), NULL => 1 page, ELSE
        // 排序: "就醫(調劑)日期(住院用藥起日)" in th
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_med = new string[] { "項次", "來源", "主診斷", "ATC3名稱", "ATC5名稱", "成分名稱", "藥品健保代碼", "藥品名稱",
            "用法用量", "給藥日數", "藥品用量", "就醫(調劑)日期(住院用藥起日)", "慢連箋領藥日(住院用藥迄日)", "慢連箋原處方醫事機構代碼" };

        // 手術明細, ContentPlaceHolder1_a_0020, 手術明細紀錄
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0020").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_op = new string[] { "來源", "就醫科別", "主診斷名稱", "手術明細代碼", "手術明細名稱", "診療部位",
            "執行時間-起", "執行時間-迄", "醫令總量" };

        // 復健醫療, ContentPlaceHolder1_a_0080, 復健醫療, reh = rehabilitation
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0080").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_PanS01").children(0) || f.getElementById("ContentPlaceHolder1_gvList")
        private static readonly string[] hw_reh = new string[] { "診別", "來源", "主診斷碼", "治療類別", "強度", "醫令數量", "就醫日期/住院日期", "治療結束日期",
            "診療部位", "執行時間-起", "執行時間-迄" };

        // 管制藥品, ContentPlaceHolder1_a_0009, 特定管制藥品用藥資訊, re = report 申報資料, up = upload 上傳資料
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0009").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(0)
        private static readonly string[] hw_sch_re = new string[] { "成分名稱（成分代碼）", "就醫年月", "就醫次數", "就醫院所數", "總劑量", "總DDD數" };

        // 擷取: f.getElementById("ContentPlaceHolder1_divResult").children(1)
        private static readonly string[] hw_sch_up = new string[] { "成分名稱（成分代碼）", "就診日期", "就診時間", "本院/他院", "總劑量", "總DDD數" };

        // 擷取: f.getElementById("ContentPlaceHolder1_PanS01").children(1) || f.getElementById("ContentPlaceHolder1_gvDetail")
        private static readonly string[] hw_tcm_de = new string[] { "主診斷", "藥品代碼", "複方註記", "基準方名", "效能名稱", "用法用量", "給藥日數", "劑型",
            "給藥總量", "就醫(調劑)日期", "慢連籤領藥日", "就醫序號" };

        // 中醫用藥, ContentPlaceHolder1_a_0090, 中醫用藥, gr = group, de = detail
        // 按鍵: d.getElementById("ContentPlaceHolder1_a_0090").Click()
        // 頁數: 無
        // 排序: 無
        // 擷取: f.getElementById("ContentPlaceHolder1_PanS01").children(0) || f.getElementById("ContentPlaceHolder1_gvGroup")
        private static readonly string[] hw_tcm_gr = new string[] { "來源", "主診斷", "給藥日數", "慢連籤", "就醫(調劑)日期", "慢連籤領藥日", "就醫序號" };

        #endregion Header_Wants

        private static readonly List<VPN_Operation> Operation_Dictionary = new List<VPN_Operation>()
        {
            new VPN_Operation("ContentPlaceHolder1_a_0008", "雲端藥歷",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", SQLTableName.Medicine, null, hw_med) }),
            new VPN_Operation("ContentPlaceHolder1_a_0060", "檢驗結果",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", SQLTableName.Laboratory, null, hw_lab) }),
            new VPN_Operation("ContentPlaceHolder1_a_0009", "管制藥物",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_divResult", SQLTableName.Schedule_report, 0, hw_sch_re),
                                           new Target_Table("ContentPlaceHolder1_divResult", SQLTableName.Schedule_upload, 1, hw_sch_up) }),
            new VPN_Operation("ContentPlaceHolder1_a_0020", "手術明細",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", SQLTableName.Operations, null, hw_op) }),
            new VPN_Operation("ContentPlaceHolder1_a_0030", "牙科處置",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", SQLTableName.Dental, null, hw_dental) }),
            new VPN_Operation("ContentPlaceHolder1_a_0040", "過敏藥物",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", SQLTableName.Allergy, null, hw_all) }),
            new VPN_Operation("ContentPlaceHolder1_a_0070", "出院病歷",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", SQLTableName.Discharge, null, hw_dis) }),
            new VPN_Operation("ContentPlaceHolder1_a_0080", "復健醫療",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvList", SQLTableName.Rehabilitation, null, hw_reh) }),
            new VPN_Operation("ContentPlaceHolder1_a_0090", "中醫用藥",
                new List<Target_Table>() { new Target_Table("ContentPlaceHolder1_gvGroup", SQLTableName.TraditionalChineseMedicine_group, null, hw_tcm_gr),
                                           new Target_Table("ContentPlaceHolder1_gvDetail", SQLTableName.TraditionalChineseMedicine_detail, null, hw_tcm_de) })
        };
    }
}