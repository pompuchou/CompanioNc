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
    }
}
