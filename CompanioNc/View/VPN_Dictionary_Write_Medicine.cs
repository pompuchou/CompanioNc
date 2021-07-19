using CompanioNc.Models;
using CompanioNc.ViewModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
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
    }
}
