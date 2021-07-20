using CompanioNc.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
        private static int Write_reh(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Debug($"        Enter Write_reh. Current ID: {strUID}.");
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
                log.Debug($"        Exit Write_reh. Current ID: {strUID}.");
                return count;
            }
            catch (Exception ex)
            {
                log.Error($"        REH of {strUID}, Error: {ex.Message}");
                log.Error($"        Count: {count}; Order: {order_n}, [{o_diagnosis}]");
                log.Debug($"        Exit Write_reh. Current ID: {strUID}.");
                return 0;
            }
        }
    }
}
