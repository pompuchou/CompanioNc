using CompanioNc.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
        private static int Write_dis(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Debug($"        Enter Write_dis. Current ID: {strUID}.");
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
                log.Debug($"        Exit Write_dis. Current ID: {strUID}.");
                return count;
            }
            catch (Exception ex)
            {
                log.Error($"        Discharge of {strUID}, Error: {ex.Message}");
                log.Error($"        Count: {count}; Order: {order_n}, [{o_diagnosis}]");
                log.Debug($"        Exit Write_dis. Current ID: {strUID}.");
                return 0;
            }
        }
    }
}
