using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanioNc.View
{
    class StoryBoard
    {
        /// <summary>
        /// 20200427 created
        /// aim: to make a storyboard for iterations of webpage manipulations
        /// </summary>

        #region Header_Wants
        // 雲端藥歷, ContentPlaceHolder1_a_0008, 雲端藥歷
        private readonly string[] hw_med = new string[] { "項次", "來源", "主診斷", "ATC3名稱", "ATC5名稱", "成分名稱", "藥品健保代碼", "藥品名稱",
            "用法用量", "給藥日數", "藥品用量", "就醫(調劑)日期(住院用藥起日)", "慢連箋領藥日(住院用藥迄日)", "慢連箋原處方醫事機構代碼" };
        // 檢驗結果, ContentPlaceHolder1_a_0060, 檢查檢驗結果
        private readonly string[] hw_lab = new string[] {"項次", "來源", "就醫科別", "主診斷", "檢查檢驗類別", "醫令名稱", "檢查檢驗項目",
            "檢查檢驗結果/報告結果/病理發現及診斷", "參考值", "報告日期", "醫令代碼"};
        // 管制藥品, ContentPlaceHolder1_a_0009, 特定管制藥品用藥資訊, re = report 申報資料, up = upload 上傳資料 
        private readonly string[] hw_sch_re = new string[] { "成分名稱（成分代碼）", "就醫年月", "就醫次數", "就醫院所數", "總劑量", "總DDD數" };
        private readonly string[] hw_sch_up = new string[] { "成分名稱（成分代碼）", "就診日期", "就診時間", "本院/他院", "總劑量", "總DDD數" };
        // 手術明細, ContentPlaceHolder1_a_0020, 手術明細紀錄
        private readonly string[] hw_op = new string[] { "來源", "就醫科別", "主診斷名稱", "手術明細代碼", "手術明細名稱", "診療部位",
            "執行時間-起", "執行時間-迄", "醫令總量" };
        // 牙科處置, ContentPlaceHolder1_a_0030, 牙科處置及手術
        private readonly string[] hw_dental = new string[] { "來源", "主診斷名稱", "牙醫處置代碼", "牙醫處置名稱", "診療部位", "執行時間-起", "執行時間-迄", "醫令總量" };
        // 過敏藥物, ContentPlaceHolder1_a_0040, 過敏藥, all = allergy
        private readonly string[] hw_all = new string[] { "上傳日期", "醫療院所", "上傳註記", "過敏藥物" };
        // 出院病歷, ContentPlaceHolder1_a_0070, 出院病歷摘要, dis = discharge
        private readonly string[] hw_dis = new string[] { "來源", "出院科別", "出院診斷", "住院日期", "出院日期" };
        // 復健醫療, ContentPlaceHolder1_a_0080, 復健醫療, reh = rehabilitation
        private readonly string[] hw_reh = new string[] { "診別", "來源", "主診斷碼", "治療類別", "強度", "醫令數量", "就醫日期/住院日期", "治療結束日期",
            "診療部位", "執行時間-起", "執行時間-迄" };
        // 中醫用藥, ContentPlaceHolder1_a_0090, 中醫用藥, gr = grade, de = description
        private readonly string[] hw_tcm_gr = new string[] { "來源", "主診斷", "給藥日數", "慢連籤", "就醫(調劑)日期", "慢連籤領藥日", "就醫序號" };
        private readonly string[] hw_tcm_de = new string[] { "主診斷", "藥品代碼", "複方註記", "基準方名", "效能名稱", "用法用量", "給藥日數", "劑型",
            "給藥總量", "就醫(調劑)日期", "慢連籤領藥日", "就醫序號" };
        #endregion
    }
}