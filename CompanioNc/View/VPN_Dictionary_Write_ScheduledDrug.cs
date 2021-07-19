using CompanioNc.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
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
    }
}
