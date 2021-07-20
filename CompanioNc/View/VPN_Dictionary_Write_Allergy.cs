using CompanioNc.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
        private static int Write_all(HtmlDocument html, List<int> header_order, string strUID, DateTime current_date)
        {
            log.Debug($"        Enter Write_all. Current ID: {strUID}.");

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
                log.Debug($"        Exit Write_all. Current ID: {strUID}.");
                return count;
            }
            catch (Exception ex)
            {
                log.Error($"        Allergy of {strUID}, Error: {ex.Message}");
                log.Error($"        Count: {count}; Order: {order_n}, [{o_drug_name}]");
                log.Debug($"        Exit Write_all. Current ID: {strUID}.");
                return -1;
            }
        }
    }
}
