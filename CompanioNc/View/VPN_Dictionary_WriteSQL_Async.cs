using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
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
    }
}
