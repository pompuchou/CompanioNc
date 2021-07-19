using CompanioNc.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
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
    }
}
