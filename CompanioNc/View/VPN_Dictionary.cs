using CompanioNc.Models;
using CompanioNc.ViewModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanioNc.View
{
    internal class VPN_Dictionary
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

        public static VPN_Operation Making_new_operation(string tab_id, string uid, DateTime qdate)
        {
            VPN_Operation o = (from p in Operation_Dictionary
                               where p.TAB_ID == tab_id
                               select p).Single();
            o.UID = uid;
            o.QDate = qdate;
            return o;
        }

        public static async Task<List<Response_DataModel>> RunWriteSQL_Async(List<VPN_Retrieved> vrs)
        {
            log.Info($"    Enter RunWriteSQL_Async. Current ID: {vrs[0].UID}. Number of tables: {vrs.Count}");

            List<Task<Response_DataModel>> tasks = new List<Task<Response_DataModel>>();

            foreach (VPN_Retrieved vr in vrs)
            {
                log.Info($"      Task {vr.SQL_Tablename} of {vr.UID} added");
                tasks.Add(WriteSQL_Async(vr));
            }
            var output = await Task.WhenAll(tasks);

            log.Info($"    Exit RunWriteSQL_Async.Current ID: {vrs[0].UID}. Number of tables: {vrs.Count}");
            return new List<Response_DataModel>(output);
        }

        public static async Task<Response_DataModel> WriteSQL_Async(VPN_Retrieved vr)
        {
            log.Info($"      Enter WriteSQL_Async {vr.SQL_Tablename} writing.");
            Response_DataModel output = new Response_DataModel();
            int _count = 0;

            output.SQL_Tablename = vr.SQL_Tablename;
            /// bulky codes for transcripting
            await Task.Run(() =>
            {
                try
                {
                    List<int> header_order = new List<int>();
                    int order_n = 0;

                    HtmlDocument html = new HtmlDocument();
                    html.LoadHtml(vr.Retrieved_Table);

                    // 找出要的順序
                    foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
                    {
                        // 多出這行檢查是否有跨column, 這出現在管制藥品的表格
                        // 欄數最少的是allergy, 只有四欄
                        // 20200502 發現錯誤, 第二次SelectNodes仍會從整個Document的XPATH去找
                        HtmlDocument h_ = new HtmlDocument();
                        h_.LoadHtml(tr.InnerHtml);
                        HtmlNodeCollection ths = h_.DocumentNode.SelectNodes("//th");
                        if ((ths == null) || (ths.Count < 4)) continue;
                        foreach (HtmlNode th in ths)
                        {
                            string strT = th.InnerText.Replace(" ", string.Empty);
                            for (int i = 0; i < vr.Header_Want.Count(); i++)
                            {
                                // 這個版本可以用在排序後, 字會多一個上下的符號
                                if (strT == vr.Header_Want[i])
                                {
                                    // 20200515 發現"慢連籤", "慢連籤領藥日", 會重複加進去header_order
                                    // 因此改為全等, 因為並不會有排序的問題
                                    header_order.Add(i);
                                    break;
                                }
                            }
                            if (header_order.Count == order_n) header_order.Add(-1);
                            order_n++;
                        }
                    }

                    switch (vr.SQL_Tablename)
                    {
                        case SQLTableName.Medicine:
                            _count = Write_med(html, header_order, vr.UID, vr.QDate);
                            break;

                        case SQLTableName.Laboratory:
                            _count = Write_lab(html, header_order, vr.UID, vr.QDate);
                            break;

                        case SQLTableName.Schedule_report:
                            _count = Write_sch_re(html, header_order, vr.UID, vr.QDate);
                            break;

                        case SQLTableName.Schedule_upload:
                            _count = Write_sch_up(html, header_order, vr.UID, vr.QDate);
                            break;

                        case SQLTableName.Operations:
                            _count = Write_op(html, header_order, vr.UID, vr.QDate);
                            break;

                        case SQLTableName.Dental:
                            _count = Write_dental(html, header_order, vr.UID, vr.QDate);
                            break;

                        case SQLTableName.Allergy:
                            _count = Write_all(html, header_order, vr.UID, vr.QDate);
                            break;

                        case SQLTableName.Discharge:
                            _count = Write_dis(html, header_order, vr.UID, vr.QDate);
                            break;

                        case SQLTableName.Rehabilitation:
                            _count = Write_reh(html, header_order, vr.UID, vr.QDate);
                            break;

                        case SQLTableName.TraditionalChineseMedicine_group:
                            _count = Write_tcm_gr(html, header_order, vr.UID, vr.QDate);
                            break;

                        case SQLTableName.TraditionalChineseMedicine_detail:
                            _count = Write_tcm_de(html, header_order, vr.UID, vr.QDate);
                            break;

                        default:
                            _count = 0;
                            break;
                    }
                    log.Info($"Successfully write {vr.SQL_Tablename} into SQL database.");
                }
                catch (Exception ex)
                {
                    log.Error($"{vr.SQL_Tablename} header_want error: {ex.Message}");
                }
            });
            output.Count = _count;
            log.Info($"      Exit WriteSQL_Async {vr.SQL_Tablename} writing.");
            return output;
        }

        private static string MakeSure_source_3_lines(string temp_source)
        {
            Com_clDataContext dc = new Com_clDataContext();
            string[] s = temp_source.Replace("<br>", "|").Split('|');
            string o_source = s[2];
            var q1 = from p1 in dc.p_source
                     where p1.source_id == o_source
                     select p1;
            if (q1.Count() == 0)
            {
                p_source new_source = new p_source()
                {
                    source_id = s[2],
                    @class = s[1],
                    source_name = s[0]
                };
                dc.p_source.InsertOnSubmit(new_source);
                dc.SubmitChanges();
            }
            return o_source;
        }

        private static string MakeSure_source_2_lines(string temp_source)
        {
            Com_clDataContext dc = new Com_clDataContext();
            string[] s = temp_source.Replace("<br>", "|").Split('|');
            string o_source = s[1];
            var q1 = from p1 in dc.p_source
                     where p1.source_id == o_source
                     select p1;
            if (q1.Count() == 0)
            {
                p_source new_source = new p_source()
                {
                    source_id = s[1],
                    source_name = s[0]
                };
                dc.p_source.InsertOnSubmit(new_source);
                dc.SubmitChanges();
            }
            return o_source;
        }

        #region Write Part

        private static int Write_all(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Info($"        Enter Write_all. Current ID: {strUID}.");

            Com_clDataContext dc = new Com_clDataContext();
            int count = 0;
            string o_source = string.Empty, o_remark = string.Empty, o_drug_name = string.Empty;
            DateTime o_SDATE = new DateTime();
            int order_n = 0;

            try
            {
                // 寫入資料庫
                foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
                {
                    HtmlDocument h_ = new HtmlDocument();
                    h_.LoadHtml(tr.InnerHtml);
                    HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                    if ((tds == null) || (tds.Count == 0)) continue;
                    order_n = 0;
                    foreach (HtmlNode td in tds)
                    {
                        switch (header_order[order_n])
                        {
                            case 0:
                                // 上傳日期
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_SDATE = d;
                                }
                                break;

                            case 1:
                                // 醫療院所
                                // 20200504: 發現是兩行的不是三行的
                                // 來源, 與別人有所不同, 只有兩行
                                if (td.InnerText != string.Empty) o_source = MakeSure_source_2_lines(td.InnerHtml);
                                break;

                            case 2:
                                // 上傳註記
                                o_remark = td.InnerText;
                                break;

                            case 3:
                                // 過敏藥物
                                o_drug_name = td.InnerText;
                                break;
                        }
                        order_n++;
                    }

                    var q = from p in dc.tbl_cloudALL
                            where (p.uid == strUID) && (p.source == o_source) && (p.SDATE == o_SDATE) &&
                                  (p.remark == o_remark) && (p.drug_name == o_drug_name)
                            select p;
                    if (q.Count() == 0)
                    {
                        tbl_cloudALL newALL = new tbl_cloudALL()
                        {
                            uid = strUID,
                            QDATE = current_date,
                            source = o_source,
                            SDATE = o_SDATE,
                            remark = o_remark,
                            drug_name = o_drug_name
                        };
                        // 存檔

                        dc.tbl_cloudALL.InsertOnSubmit(newALL);
                        dc.SubmitChanges();
                    }
                    count++;
                }
                log.Info($"        Exit Write_all. Current ID: {strUID}.");
                return count;
            }
            catch (Exception ex)
            {
                log.Error($"        Allergy of {strUID}, Error: {ex.Message}");
                log.Error($"        Count: {count}; Order: {order_n}, [{o_drug_name}]");
                log.Info($"        Exit Write_all. Current ID: {strUID}.");
                return -1;
            }
        }

        private static int Write_dental(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Info($"        Enter Write_dental. Current ID: {strUID}.");
            Com_clDataContext dc = new Com_clDataContext();
            int count = 0;
            string o_source = string.Empty, o_diagnosis = string.Empty, o_NHI_code = string.Empty;
            string o_op_name = string.Empty, o_loca = string.Empty;
            int o_amt = 0;
            DateTime o_SDATE = new DateTime(), o_EDATE = new DateTime();

            int order_n = 0;

            try
            {
                // 寫入資料庫
                foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
                {
                    HtmlDocument h_ = new HtmlDocument();
                    h_.LoadHtml(tr.InnerHtml);
                    HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                    if ((tds == null) || (tds.Count == 0)) continue;
                    order_n = 0;
                    foreach (HtmlNode td in tds)
                    {
                        switch (header_order[order_n])
                        {
                            case 0:
                                // 來源
                                if (td.InnerText != string.Empty) o_source = MakeSure_source_3_lines(td.InnerHtml);
                                break;

                            case 1:
                                // 主診斷名稱
                                o_diagnosis = td.InnerHtml.Replace("<br>", " ");
                                break;

                            case 2:
                                // 牙醫處置代碼
                                o_NHI_code = td.InnerText;
                                break;

                            case 3:
                                // 牙醫處置名稱
                                o_op_name = td.InnerHtml.Replace("<br>", " ");
                                break;

                            case 4:
                                // 診療部位
                                o_loca = td.InnerText;
                                break;

                            case 5:
                                // 執行時間-起
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_SDATE = d;
                                }
                                break;

                            case 6:
                                // 執行時間-迄
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_EDATE = d;
                                }
                                break;

                            case 7:
                                // 醫令總量
                                if (td.InnerText != string.Empty) o_amt = int.Parse(td.InnerText);
                                break;
                        }
                        order_n++;
                    }

                    var q = from p in dc.tbl_cloudDEN
                            where (p.uid == strUID) && (p.source == o_source) && (p.NHI_code == o_NHI_code) &&
                                  (p.SDATE == o_SDATE) && (p.EDATE == o_EDATE)
                            select p;
                    if (q.Count() == 0)
                    {
                        tbl_cloudDEN newDEN = new tbl_cloudDEN()
                        {
                            uid = strUID,
                            QDATE = current_date,
                            source = o_source,
                            diagnosis = o_diagnosis,
                            NHI_code = o_NHI_code,
                            op_name = o_op_name,
                            loca = o_loca,
                            SDATE = o_SDATE,
                            EDATE = o_EDATE,
                            amt = (byte?)o_amt
                        };

                        // 存檔

                        dc.tbl_cloudDEN.InsertOnSubmit(newDEN);
                        dc.SubmitChanges();
                    }
                    count++;
                }
                log.Info($"        Exit Write_den. Current ID: {strUID}.");
                return count;
            }
            catch (Exception ex)
            {
                log.Error($"        Dental of {strUID}, Error: {ex.Message}");
                log.Error($"        Count: {count}; Order: {order_n}, [{o_op_name}]");
                log.Info($"        Exit Write_den. Current ID: {strUID}.");
                return 0;
            }
        }

        private static int Write_dis(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Info($"        Enter Write_dis. Current ID: {strUID}.");
            Com_clDataContext dc = new Com_clDataContext();
            int count = 0;
            string o_source = string.Empty, o_dep = string.Empty, o_diagnosis = string.Empty;
            DateTime o_SDATE = new DateTime(), o_EDATE = new DateTime();

            int order_n = 0;

            try
            {
                // 寫入資料庫
                foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
                {
                    HtmlDocument h_ = new HtmlDocument();
                    h_.LoadHtml(tr.InnerHtml);
                    HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                    if ((tds == null) || (tds.Count == 0)) continue;
                    order_n = 0;
                    foreach (HtmlNode td in tds)
                    {
                        switch (header_order[order_n])
                        {
                            case 0:
                                // 來源
                                if (td.InnerText != string.Empty)
                                {
                                    o_source = MakeSure_source_2_lines(td.InnerHtml);
                                }
                                break;

                            case 1:
                                // 出院科別
                                o_dep = td.InnerText;
                                break;

                            case 2:
                                // 出院診斷
                                o_diagnosis = td.InnerText;
                                break;

                            case 3:
                                // 住院日期
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_SDATE = d;
                                }
                                break;

                            case 4:
                                // 出院日期
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_EDATE = d;
                                }
                                break;
                        }
                        order_n++;
                    }

                    var q = from p in dc.tbl_cloudDIS
                            where (p.uid == strUID) && (p.source == o_source) &&
                                  (p.SDATE == o_SDATE) && (p.EDATE == o_EDATE)
                            select p;
                    if (q.Count() == 0)
                    {
                        tbl_cloudDIS newDIS = new tbl_cloudDIS()
                        {
                            uid = strUID,
                            QDATE = current_date,
                            source = o_source,
                            dep = o_dep,
                            diagnosis = o_diagnosis,
                            SDATE = o_SDATE,
                            EDATE = o_EDATE
                        };

                        // 存檔

                        dc.tbl_cloudDIS.InsertOnSubmit(newDIS);
                        dc.SubmitChanges();
                    }
                    count++;
                }
                log.Info($"        Exit Write_dis. Current ID: {strUID}.");
                return count;
            }
            catch (Exception ex)
            {
                log.Error($"        Discharge of {strUID}, Error: {ex.Message}");
                log.Error($"        Count: {count}; Order: {order_n}, [{o_diagnosis}]");
                log.Info($"        Exit Write_dis. Current ID: {strUID}.");
                return 0;
            }
        }

        private static int Write_lab(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Info($"        Enter Write_lab. Current ID: {strUID}.");
            int count = 0, order_n = 0;
            Com_clDataContext dc = new Com_clDataContext();
            count = 0;
            // 寫入資料庫
            foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
            {
                HtmlDocument h_ = new HtmlDocument();
                h_.LoadHtml(tr.InnerHtml);
                HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                if ((tds == null) || (tds.Count == 0)) continue;
                tbl_cloudlab_temp newLab = new tbl_cloudlab_temp()
                {
                    uid = strUID,
                    QDATE = current_date
                };

                order_n = 0;
                foreach (HtmlNode td in tds)
                {
                    try
                    {
                        switch (header_order[order_n])
                        {
                            case 0:
                                // 項次
                                if (td.InnerText != string.Empty) newLab.item_n = short.Parse(td.InnerText);
                                break;

                            case 1:
                                newLab.source = td.InnerHtml.Replace("<br>", " ");
                                break;

                            case 2:
                                newLab.dep = td.InnerText;
                                break;

                            case 3:
                                newLab.diagnosis = td.InnerHtml.Replace("<br>", " ");
                                break;

                            case 4:
                                newLab.@class = td.InnerText;
                                break;

                            case 5:
                                newLab.order_name = td.InnerText;
                                break;

                            case 6:
                                newLab.lab_item = td.InnerText;
                                break;

                            case 7:
                                newLab.result = td.InnerText;
                                break;

                            case 8:
                                newLab.range = td.InnerText;
                                break;

                            case 9:
                                // 原本空白日期會有錯誤, 一有錯誤就直接跳到最外層try-catch,該條之後整頁都沒有讀入, 20200514修正
                                // 加入if, 排除空白日期造成的錯誤, 把try 縮小範圍, 有錯不至於放棄整頁
                                if (!string.IsNullOrEmpty(td.InnerText))
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    newLab.SDATE = d;
                                }
                                break;

                            case 10:
                                newLab.NHI_code = td.InnerText;
                                break;
                        }
                        order_n++;
                    }
                    catch (Exception ex)
                    {
                        log.Error($"        lab of {strUID}, Error: {ex.Message}");
                        log.Error($"        Count: {count}; Order: {order_n}]");
                    }
                }
                dc.tbl_cloudlab_temp.InsertOnSubmit(newLab);
                dc.SubmitChanges();
                count++;
            }
            // 匯入大表
            try
            {
                dc.sp_insert_tbl_cloudlab(current_date);
            }
            catch (Exception ex)
            {
                log.Error($"        lab of {strUID}, Error: {ex.Message}");
                log.Error($"        Error with writing into big table");
                Logging.Record_error(ex.Message);
            }
            try
            {
                dc.sp_insert_p_cloudlab(current_date);
            }
            catch (Exception ex)
            {
                log.Error($"        lab of {strUID}, Error: {ex.Message}");
                log.Error($"        Error with writing into p_cloudlab.");
                Logging.Record_error(ex.Message);
            }
            // 處理source
            var r = (from p in dc.tbl_cloudlab_temp
                     where p.QDATE == current_date
                     select p.source).Distinct().ToList(); // this is a query
            for (int i = 0; i < r.Count(); i++)
            {
                string[] s = r[i].Split(' ');
                var qq = from pp in dc.p_source
                         where pp.source_id == s[2].Substring(1)
                         select pp;
                if (qq.Count() == 0)
                {
                    p_source so = new p_source()
                    {
                        source_id = s[2].Substring(1),
                        @class = s[1].Substring(1),
                        source_name = s[0]
                    };
                    dc.p_source.InsertOnSubmit(so);
                    dc.SubmitChanges();
                }
            }
            log.Info($"        Exit Write_lab. Current ID: {strUID}.");
            return count;
        }

        private static int Write_med(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Info($"        Enter Write_med. Current ID: {strUID}.");
            int count = 0, order_n = 0;
            try
            {
                Com_clDataContext dc = new Com_clDataContext();
                count = 0;
                // 寫入資料庫
                foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
                {
                    HtmlDocument h_ = new HtmlDocument();
                    h_.LoadHtml(tr.InnerHtml);
                    HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                    if ((tds == null) || (tds.Count == 0)) continue;
                    tbl_cloudmed_temp newCloud = new tbl_cloudmed_temp()
                    {
                        uid = strUID,
                        QDATE = current_date
                    };

                    order_n = 0;
                    foreach (HtmlNode td in tds)
                    {
                        switch (header_order[order_n])
                        {
                            case 0:
                                // 項次
                                if (td.InnerText != string.Empty) newCloud.item_n = short.Parse(td.InnerText);
                                break;

                            case 1:
                                newCloud.source = td.InnerHtml.Replace("<br>", " ");
                                break;

                            case 2:
                                newCloud.diagnosis = td.InnerHtml.Replace("<br>", " ");
                                break;

                            case 3:
                                newCloud.atc3 = td.InnerText;
                                break;

                            case 4:
                                newCloud.atc5 = td.InnerText;
                                break;

                            case 5:
                                newCloud.comp = td.InnerText;
                                break;

                            case 6:
                                newCloud.NHI_code = td.InnerText;
                                break;

                            case 7:
                                newCloud.drug_name = td.InnerText;
                                break;

                            case 8:
                                newCloud.dosing = td.InnerText;
                                break;

                            case 9:
                                newCloud.days = td.InnerText;
                                break;

                            case 10:
                                newCloud.amt = td.InnerText;
                                break;

                            case 11:
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    newCloud.SDATE = d;
                                }
                                break;

                            case 12:
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    newCloud.EDATE = d;
                                }
                                break;

                            case 13:
                                newCloud.o_source = td.InnerText;
                                break;
                        }
                        order_n++;
                    }
                    dc.tbl_cloudmed_temp.InsertOnSubmit(newCloud);
                    dc.SubmitChanges();
                    count++;
                }

                // 匯入大表
                try
                {
                    dc.sp_insert_tbl_cloudmed(current_date);
                }
                catch (Exception ex)
                {
                    log.Error($"        med of {strUID}, Error: {ex.Message}");
                    log.Error($"        Count: {count}; Order: {order_n}]");
                    Logging.Record_error(ex.Message);
                }
                try
                {
                    dc.sp_insert_p_cloudmed(current_date);
                }
                catch (Exception ex)
                {
                    log.Error($"        med of {strUID}, Error: {ex.Message}");
                    log.Error($"        Count: {count}; Order: {order_n}]");
                    Logging.Record_error(ex.Message);
                }
                // 這裡原本多了一次沒有try包覆的insert_p_cloudmed, 一但p_cloudmed有錯誤就沒辦法處理source
                // 處理source
                var r = (from p in dc.tbl_cloudmed_temp
                         where p.QDATE == current_date
                         select p.source).Distinct().ToList();  //this is a query
                for (int i = 0; i < r.Count(); i++)
                {
                    string[] s = r[i].Split(' ');
                    // source_id s(2).substring(1)
                    // class s(1).substring(1)
                    // source_name s(0)
                    var qq = from pp in dc.p_source
                             where pp.source_id == s[2].Substring(1)
                             select pp;
                    if (qq.Count() == 0)
                    {
                        p_source so = new p_source()
                        {
                            source_id = s[2].Substring(1),
                            @class = s[1].Substring(1),
                            source_name = s[0]
                        };
                        dc.p_source.InsertOnSubmit(so);
                        dc.SubmitChanges();
                    }
                }
                log.Info($"        Exit Write_med. Current ID: {strUID}.");
                return count;
            }
            catch (Exception ex)
            {
                log.Error($"        med of {strUID}, Error: {ex.Message}");
                log.Error($"        Count: {count}; Order: {order_n}]");
                log.Info($"        Exit Write_med. Current ID: {strUID}.");
                return 0;
            }
        }

        private static int Write_op(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Info($"        Enter Write_op. Current ID: {strUID}.");
            Com_clDataContext dc = new Com_clDataContext();
            int count = 0;
            string o_source = string.Empty, o_dep = string.Empty, o_diagnosis = string.Empty;
            string o_NHI_code = string.Empty, o_op_name = string.Empty, o_loca = string.Empty;
            int o_amt = 0;
            DateTime o_SDATE = new DateTime(), o_EDATE = new DateTime();
            int order_n = 0;

            try
            {
                // 寫入資料庫
                foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
                {
                    HtmlDocument h_ = new HtmlDocument();
                    h_.LoadHtml(tr.InnerHtml);
                    HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                    if ((tds == null) || (tds.Count == 0)) continue;
                    order_n = 0;
                    foreach (HtmlNode td in tds)
                    {
                        switch (header_order[order_n])
                        {
                            case 0:
                                // 來源
                                if (td.InnerText != string.Empty) o_source = MakeSure_source_3_lines(td.InnerHtml);
                                break;

                            case 1:
                                // 就醫科別
                                o_dep = td.InnerText;
                                break;

                            case 2:
                                // 主診斷名稱
                                o_diagnosis = td.InnerText;
                                break;

                            case 3:
                                // 手術明細代碼
                                o_NHI_code = td.InnerText;
                                break;

                            case 4:
                                // 手術明細名稱
                                o_op_name = td.InnerText;
                                break;

                            case 5:
                                // 診療部位
                                o_loca = td.InnerText;
                                break;

                            case 6:
                                // 執行時間-起
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_SDATE = d;
                                }
                                break;

                            case 7:
                                // 執行時間-迄
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_EDATE = d;
                                }
                                break;

                            case 8:
                                // 醫令總量
                                if (td.InnerText != string.Empty) o_amt = int.Parse(td.InnerText);
                                break;
                        }
                        order_n++;
                    }

                    var q = from p in dc.tbl_cloudOP
                            where (p.uid == strUID) && (p.source == o_source) && (p.NHI_code == o_NHI_code) &&
                                  (p.SDATE == o_SDATE) && (p.EDATE == o_EDATE)
                            select p;
                    if (q.Count() == 0)
                    {
                        tbl_cloudOP newOP = new tbl_cloudOP()
                        {
                            uid = strUID,
                            QDATE = current_date,
                            source = o_source,
                            dep = o_dep,
                            diagnosis = o_diagnosis,
                            NHI_code = o_NHI_code,
                            op_name = o_op_name,
                            loca = o_loca,
                            SDATE = o_SDATE,
                            EDATE = o_EDATE,
                            amt = (byte?)o_amt
                        };

                        // 存檔

                        dc.tbl_cloudOP.InsertOnSubmit(newOP);
                        dc.SubmitChanges();
                    }
                    count++;
                }
                log.Info($"        Exit Write_op. Current ID: {strUID}.");
                return count;
            }
            catch (Exception ex)
            {
                log.Error($"        op of {strUID}, Error: {ex.Message}");
                log.Error($"        Count: {count}; Order: {order_n}, [{o_op_name}]");
                log.Info($"        Exit Write_op. Current ID: {strUID}.");
                return 0;
            }
        }

        private static int Write_reh(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Info($"        Enter Write_reh. Current ID: {strUID}.");
            Com_clDataContext dc = new Com_clDataContext();
            int count = 0;
            string o_class = string.Empty, o_source = string.Empty, o_diagnosis = string.Empty, o_type = string.Empty;
            string o_curegrade = string.Empty, o_loca = string.Empty;
            int o_amt = 0;
            DateTime o_begin_date = new DateTime(), o_end_date = new DateTime(), o_SDATE = new DateTime(), o_EDATE = new DateTime();

            int order_n = 0;

            try
            {
                // 寫入資料庫
                foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
                {
                    HtmlDocument h_ = new HtmlDocument();
                    h_.LoadHtml(tr.InnerHtml);
                    HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                    if ((tds == null) || (tds.Count == 0)) continue;
                    order_n = 0;
                    foreach (HtmlNode td in tds)
                    {
                        switch (header_order[order_n])
                        {
                            case 0:
                                // 診別
                                o_class = td.InnerText;
                                break;

                            case 1:
                                // 來源, 與別人有所不同, 只有兩行
                                if (td.InnerText != string.Empty) o_source = MakeSure_source_2_lines(td.InnerHtml);
                                break;

                            case 2:
                                // 主診斷碼
                                o_diagnosis = td.InnerText;
                                break;

                            case 3:
                                // 治療類別
                                o_type = td.InnerText;
                                break;

                            case 4:
                                // 強度
                                o_curegrade = td.InnerText;
                                break;

                            case 5:
                                // 醫令總量
                                if (td.InnerText != string.Empty) o_amt = int.Parse(td.InnerText);
                                break;

                            case 6:
                                // 就醫日期/住院日期
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_begin_date = d;
                                }
                                break;

                            case 7:
                                // 治療結束日期
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_end_date = d;
                                }
                                break;

                            case 8:
                                // 診療部位
                                o_loca = td.InnerText;
                                break;

                            case 9:
                                // 執行時間-起
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_SDATE = d;
                                }
                                break;

                            case 10:
                                // 執行時間-迄
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_EDATE = d;
                                }
                                break;
                        }
                        order_n++;
                    }

                    var q = from p in dc.tbl_cloudREH
                            where (p.uid == strUID) && (p.source == o_source) && (p.SDATE == o_SDATE) && (p.EDATE == o_EDATE)
                            select p;
                    if (q.Count() == 0)
                    {
                        tbl_cloudREH newREH = new tbl_cloudREH()
                        {
                            uid = strUID,
                            QDATE = current_date,
                            @class = o_class,
                            source = o_source,
                            type = o_type,
                            diagnosis = o_diagnosis,
                            curegrade = o_curegrade,
                            amt = (byte?)o_amt,
                            begin_date = o_begin_date,
                            end_date = o_end_date,
                            loca = o_loca,
                            SDATE = o_SDATE,
                            EDATE = o_EDATE
                        };

                        // 存檔

                        dc.tbl_cloudREH.InsertOnSubmit(newREH);
                        dc.SubmitChanges();
                    }
                    count++;
                }
                log.Info($"        Exit Write_reh. Current ID: {strUID}.");
                return count;
            }
            catch (Exception ex)
            {
                log.Error($"        REH of {strUID}, Error: {ex.Message}");
                log.Error($"        Count: {count}; Order: {order_n}, [{o_diagnosis}]");
                log.Info($"        Exit Write_reh. Current ID: {strUID}.");
                return 0;
            }
        }

        private static int Write_sch_re(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Info($"        Enter Write_sch_re. Current ID: {strUID}.");
            Com_clDataContext dc = new Com_clDataContext();
            int count = 0;
            string o_drug = string.Empty, o_YM = string.Empty, drug_name = string.Empty;
            int o_visit_n = 0, o_clinic_n = 0, o_t_dose = 0, o_t_DDD = 0, row_left = 0, row_n = 0;
            int order_n = 0;

            try
            {
                // 寫入資料庫
                foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
                {
                    HtmlDocument h_ = new HtmlDocument();
                    h_.LoadHtml(tr.InnerHtml);
                    HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                    if ((tds == null) || (tds.Count == 0)) continue;
                    if (row_left > 0) row_left--;
                    order_n = 0;
                    foreach (HtmlNode td in tds)
                    {
                        //' header(order_n)是資料表的位置與實際table的對照
                        //' order_n是table的位置, header(order_n)的值是資料表的位置
                        //' 有rowspan會干擾
                        int actual_n;

                        if ((row_left != row_n) && (row_left > 0) && (order_n > 0))
                        {
                            actual_n = order_n + 1;
                            o_drug = drug_name;
                        }
                        else
                        {
                            actual_n = order_n;
                        }
                        //' 第一輪

                        if ((order_n == 1) && int.Parse(td.GetAttributeValue("rowspan", "1")) > 1)
                        {
                            //' order_n=1 名義上第一輪成分名稱的位置
                            drug_name = td.InnerHtml.Replace("<br>", " ");
                            row_n = int.Parse(td.GetAttributeValue("rowspan", "1"));
                            row_left = row_n;
                        }
                        switch (header_order[actual_n])
                        {
                            case 0:
                                // 成分名稱
                                o_drug = td.InnerHtml.Replace("<br>", " ");
                                break;

                            case 1:
                                // 就醫年月
                                o_YM = td.InnerText;
                                break;

                            case 2:
                                // 就醫次數
                                if (td.InnerText != string.Empty) o_visit_n = int.Parse(td.InnerText);
                                break;

                            case 3:
                                // 就醫院所數
                                if (td.InnerText != string.Empty) o_clinic_n = int.Parse(td.InnerText);
                                break;

                            case 4:
                                // 總劑量
                                if (td.InnerText != string.Empty) o_t_dose = int.Parse(td.InnerText);
                                break;

                            case 5:
                                // 總DDD數
                                if (td.InnerText != string.Empty) o_t_DDD = int.Parse(td.InnerText);
                                break;
                        }
                        order_n++;
                    }

                    var q = from p in dc.tbl_cloudSCH_R
                            where (p.uid == strUID) && (p.drug_name == o_drug) && (p.YM == o_YM)
                            select p;
                    if (q.Count() == 0)
                    {
                        tbl_cloudSCH_R newR = new tbl_cloudSCH_R()
                        {
                            uid = strUID,
                            QDATE = current_date,
                            YM = o_YM,
                            drug_name = o_drug,
                            visit_n = (byte?)o_visit_n,
                            clinic_n = (byte?)o_clinic_n,
                            t_dose = (short?)o_t_dose,
                            t_DDD = (short?)o_t_DDD
                        };

                        // 存檔

                        dc.tbl_cloudSCH_R.InsertOnSubmit(newR);
                        dc.SubmitChanges();
                    }
                    count++;
                }
                log.Info($"        Exit Write_sch_re. Current ID: {strUID}.");
                return count;
            }
            catch (Exception ex)
            {
                log.Error($"        SCH_RE of {strUID}, Error: {ex.Message}");
                log.Error($"        Count: {count}; Order: {order_n}, [{o_drug}]");
                log.Info($"        Exit Write_sch_re. Current ID: {strUID}.");
                return 0;
            }
        }

        private static int Write_sch_up(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Info($"        Enter Write_sch_up. Current ID: {strUID}.");
            Com_clDataContext dc = new Com_clDataContext();
            int count = 0;
            string o_drug = string.Empty, drug_name = string.Empty, o_STIME = string.Empty, o_clinic = string.Empty;
            int o_t_dose = 0, o_t_DDD = 0, row_left = 0, row_n = 0;
            DateTime o_SDATE = new DateTime();
            int order_n = 0;

            try
            {
                // 寫入資料庫
                foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
                {
                    HtmlDocument h_ = new HtmlDocument();
                    h_.LoadHtml(tr.InnerHtml);
                    HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                    if ((tds == null) || (tds.Count == 0)) continue;
                    if (row_left > 0) row_left--;
                    order_n = 0;
                    foreach (HtmlNode td in tds)
                    {
                        // header(order_n)是資料表的位置與實際table的對照
                        // order_n是table的位置, header(order_n)的值是資料表的位置
                        // 有rowspan會干擾
                        int actual_n;

                        if ((row_left != row_n) && (row_left > 0) && (order_n > 0))
                        {
                            actual_n = order_n + 1;
                            o_drug = drug_name;
                        }
                        else
                        {
                            actual_n = order_n;
                        }
                        //' 第一輪

                        if ((order_n == 1) && int.Parse(td.GetAttributeValue("rowspan", "1")) > 1)
                        {
                            //' order_n=1 名義上第一輪成分名稱的位置
                            drug_name = td.InnerHtml.Replace("<br>", " ");
                            row_n = int.Parse(td.GetAttributeValue("rowspan", "1"));
                            row_left = row_n;
                        }
                        switch (header_order[actual_n])
                        {
                            case 0:
                                // 成分名稱
                                o_drug = td.InnerHtml.Replace("<br>", " ");
                                break;

                            case 1:
                                // 就診日期
                                if (td.InnerText != string.Empty)
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_SDATE = d;
                                }
                                break;

                            case 2:
                                // 就診時間
                                o_STIME = td.InnerText;
                                break;

                            case 3:
                                // 本院/他院
                                o_clinic = td.InnerText;
                                break;

                            case 4:
                                // 總劑量
                                if (td.InnerText != string.Empty) o_t_dose = int.Parse(td.InnerText);
                                break;

                            case 5:
                                // 總DDD數
                                if (td.InnerText != string.Empty) o_t_DDD = int.Parse(td.InnerText);
                                break;
                        }
                        order_n++;
                    }

                    var q = from p in dc.tbl_cloudSCH_U
                            where (p.uid == strUID) && (p.drugname == o_drug) && (p.SDATE == o_SDATE)
                            select p;
                    if (q.Count() == 0)
                    {
                        tbl_cloudSCH_U newU = new tbl_cloudSCH_U()
                        {
                            uid = strUID,
                            QDATE = current_date,
                            SDATE = o_SDATE,
                            drugname = o_drug,
                            STIME = o_STIME,
                            clinic = o_clinic,
                            t_dose = (short?)o_t_dose,
                            t_DDD = (short?)o_t_DDD
                        };

                        // 存檔

                        dc.tbl_cloudSCH_U.InsertOnSubmit(newU);
                        dc.SubmitChanges();
                    }
                    count++;
                }
                log.Info($"        Exit Write_sch_up. Current ID: {strUID}.");
                return count;
            }
            catch (Exception ex)
            {
                log.Error($"        SCH_UP of {strUID}, Error: {ex.Message}");
                log.Error($"        Count: {count}; Order: {order_n}, [{o_drug}]");
                log.Info($"        Exit Write_sch_up. Current ID: {strUID}.");
                return 0;
            }
        }

        private static int Write_tcm_de(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Info($"        Enter Write_tcm_de. Current ID: {strUID}.");
            Com_clDataContext dc = new Com_clDataContext();
            int count = 0;
            string o_diagnosis = string.Empty, o_NHI_code = string.Empty, o_complex = string.Empty;
            string o_base = string.Empty, o_effect = string.Empty, o_dosing = string.Empty;
            string o_type = string.Empty, o_serial = string.Empty;
            int o_days = 0;
            float o_amt = 0;
            DateTime o_SDATE = new DateTime(), o_EDATE = new DateTime();

            int order_n = 0;
            // 寫入資料庫
            foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
            {
                HtmlDocument h_ = new HtmlDocument();
                h_.LoadHtml(tr.InnerHtml);
                HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                if ((tds == null) || (tds.Count == 0)) continue;
                order_n = 0;
                foreach (HtmlNode td in tds)
                {
                    try
                    {
                        switch (header_order[order_n])
                        {
                            case 0:
                                // 主診斷名稱
                                o_diagnosis = td.InnerHtml.Replace("<br>", " ");
                                break;

                            case 1:
                                // 藥品代碼
                                o_NHI_code = td.InnerText;
                                break;

                            case 2:
                                // 複方註記
                                o_complex = td.InnerText;
                                break;

                            case 3:
                                // 基準方名
                                o_base = td.InnerText;
                                break;

                            case 4:
                                // 效能名稱
                                o_effect = td.InnerText;
                                break;

                            case 5:
                                // 用法用量
                                o_dosing = td.InnerText;
                                break;

                            case 6:
                                // 給藥日數
                                if (!string.IsNullOrEmpty(td.InnerText)) o_days = int.Parse(td.InnerText);
                                break;

                            case 7:
                                // 濟型
                                o_type = td.InnerText;
                                break;

                            case 8:
                                // 給藥總量
                                if (!string.IsNullOrEmpty(td.InnerText)) o_amt = float.Parse(td.InnerText);
                                break;

                            case 9:
                                // 就醫(調劑)日期
                                if (!string.IsNullOrEmpty(td.InnerText))
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_SDATE = d;
                                }
                                break;

                            case 10:
                                // 慢連箋領藥日
                                if (!string.IsNullOrEmpty(td.InnerText))
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_EDATE = d;
                                }
                                break;

                            case 11:
                                // 就醫序號
                                o_serial = td.InnerText;
                                break;
                        }
                        order_n++;
                    }
                    catch (Exception ex)
                    {
                        log.Error($"        TCM_DE of {strUID}, Error: {ex.Message}");
                        log.Error($"        Count: {count}; Order: {order_n}, [{o_diagnosis}]");
                    }
                }
                // 20200515 有時候amt會有小數點, 直接轉換就會報錯, 因此先轉換成float, 再換回整數, 就可以通過

                var q = from p in dc.tbl_cloudTCM_D
                        where (p.uid == strUID) && (p.NHI_code == o_NHI_code) &&
                              (p.SDATE == o_SDATE) && (p.serial == o_serial)
                        select p;
                if (q.Count() == 0)
                {
                    tbl_cloudTCM_D newTCMD = new tbl_cloudTCM_D()
                    {
                        uid = strUID,
                        QDATE = current_date,
                        diagnosis = o_diagnosis,
                        NHI_code = o_NHI_code,
                        complex = o_complex,
                        @base = o_base,
                        effect = o_effect,
                        dosing = o_dosing,
                        days = (byte?)o_days,
                        type = o_type,
                        amt = (short?)o_amt,
                        SDATE = o_SDATE,
                        EDATE = o_EDATE,
                        serial = o_serial
                    };

                    // 存檔

                    dc.tbl_cloudTCM_D.InsertOnSubmit(newTCMD);
                    dc.SubmitChanges();
                }
                count++;
            }
            log.Info($"        Exit Write_tcm_de. Current ID: {strUID}.");
            return count;
        }

        private static int Write_tcm_gr(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Info($"        Enter Write_tcm_gr. Current ID: {strUID}.");
            Com_clDataContext dc = new Com_clDataContext();
            int count = 0;
            string o_source = string.Empty, o_diagnosis = string.Empty, o_chronic = string.Empty, o_serial = string.Empty;
            int o_days = 0;
            DateTime o_SDATE = new DateTime(), o_EDATE = new DateTime();

            int order_n = 0;

            // 寫入資料庫
            foreach (HtmlNode tr in html.DocumentNode.SelectNodes("//table/tbody/tr"))
            {
                HtmlDocument h_ = new HtmlDocument();
                h_.LoadHtml(tr.InnerHtml);
                HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                if ((tds == null) || (tds.Count == 0)) continue;
                order_n = 0;
                foreach (HtmlNode td in tds)
                {
                    try
                    {
                        switch (header_order[order_n])
                        {
                            case 0:
                                // 來源
                                if (!string.IsNullOrEmpty(td.InnerText)) o_source = MakeSure_source_3_lines(td.InnerHtml);
                                break;

                            case 1:
                                // 主診斷
                                o_diagnosis = td.InnerHtml.Replace("<br>", " ");
                                break;

                            case 2:
                                // 給藥日數
                                if (!string.IsNullOrEmpty(td.InnerText)) o_days = int.Parse(td.InnerText);
                                break;

                            case 3:
                                // 慢連箋
                                o_chronic = td.InnerText;
                                break;

                            case 4:
                                // 就醫(調劑)日期
                                if (!string.IsNullOrEmpty(td.InnerText))
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_SDATE = d;
                                }
                                break;

                            case 5:
                                // 慢連箋領藥日
                                if (!string.IsNullOrEmpty(td.InnerText))
                                {
                                    string[] temp_d = td.InnerText.Split('/');
                                    DateTime.TryParse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]}", out DateTime d);
                                    o_EDATE = d;
                                }
                                break;

                            case 6:
                                // 就醫序號
                                o_serial = td.InnerText;
                                break;
                        }
                        order_n++;
                    }
                    catch (Exception ex)
                    {
                        log.Error($"        TCM_GR of {strUID}, Error: {ex.Message}");
                        log.Error($"        Count: {count}; Order: {order_n}, [{o_diagnosis}]");
                    }
                }
                var q = from p in dc.tbl_cloudTCM_G
                        where (p.uid == strUID) && (p.SDATE == o_SDATE) && (p.serial == o_serial)
                        select p;
                if (q.Count() == 0)
                {
                    tbl_cloudTCM_G newTCMG = new tbl_cloudTCM_G()
                    {
                        uid = strUID,
                        QDATE = current_date,
                        source = o_source,
                        diagnosis = o_diagnosis,
                        days = (byte?)o_days,
                        chronic = o_chronic,
                        SDATE = o_SDATE,
                        EDATE = o_EDATE,
                        serial = o_serial
                    };

                    // 存檔

                    dc.tbl_cloudTCM_G.InsertOnSubmit(newTCMG);
                    dc.SubmitChanges();
                }
                count++;
            }
            log.Info($"        Exit Write_tcm_gr. Current ID: {strUID}.");
            return count;
        }

        #endregion Write Part
    }
}