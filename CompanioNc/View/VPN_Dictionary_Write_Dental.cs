using CompanioNc.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
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
    }
}
