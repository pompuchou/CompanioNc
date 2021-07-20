using CompanioNc.Models;
using Hardcodet.Wpf.TaskbarNotification;
using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CompanioNc.View
{
    /// <summary>
    /// 20210719 建立, 將解析從F_Data_LoadCompleted獨立出來
    /// 兩個地方會呼叫, 一個是在F_Data_LoadCompleted, 當最後一個TAB(單頁)
    ///               另一個是在F_Pager_LoadCompleted, 當最後一個TAB(多頁)的最後一頁
    /// </summary>
    public partial class WebTEst : Window
    {
        private async void ParsingTables(string o)
        {
            tb.ShowBalloonTip($"讀取完成 [{current_op?.UID}]", "開始解析與寫入資料庫", BalloonIcon.Info);
            log.Info($"[Info] all HTML loaded into memory. start to analyze. [{current_op?.UID}]");

            // Count = 0 代表最後一個 tab
            // 20200504 這裡一個BUG, 漏了把F_DATA_Loadcompleted刪掉,以至於不斷重複多次. ******

            /// 確定是最後一個tab這段程式到此結束
            /// 4. Parsing & Saving to SQL: async
            ///     4-1. 多工同時處理, 快速
            ///     4-2. 依照欄位資料/位置 Parsing
            ///     4-3. 存入SQL
            ///     4-4. 製作Query
            /// 查核機制?
            log.Debug($"[Start] async process, {current_op?.UID}");
            List<Response_DataModel> rds = await VPN_Dictionary.RunWriteSQL_Async(ListRetrieved);
            log.Debug($"[End] async process, {current_op?.UID}");
            
            /// 1. 讀取特殊註記, 如果有的話
            ///    這是在ContentPlaceHolder1_tab02
            ///    是個table
            // 每當刷新後都要重新讀一次
            // d 是parent HTML document
            HTMLDocument d = (HTMLDocument)g.Document;

            IHTMLElement Special_remark = d?.getElementById("ContentPlaceHolder1_tab02");
            string _special_remark = Special_remark?.innerText;

            // 製作紀錄by rd
            try
            {
                short? med_N = (short?)(from p1 in rds
                                        where p1.SQL_Tablename == SQLTableName.Medicine
                                        select p1.Count).Sum(),
                       lab_N = (short?)(from p1 in rds
                                        where p1.SQL_Tablename == SQLTableName.Laboratory
                                        select p1.Count).Sum(),
                       schedule_N = (short?)(from p1 in rds
                                             where p1.SQL_Tablename == SQLTableName.Schedule_report
                                             select p1.Count).Sum(),
                       op_N = (short?)(from p1 in rds
                                       where p1.SQL_Tablename == SQLTableName.Operations
                                       select p1.Count).Sum(),
                       dental_N = (short?)(from p1 in rds
                                           where p1.SQL_Tablename == SQLTableName.Dental
                                           select p1.Count).Sum(),
                       allergy_N = (short?)(from p1 in rds
                                            where p1.SQL_Tablename == SQLTableName.Allergy
                                            select p1.Count).Sum(),
                       discharge_N = (short?)(from p1 in rds
                                              where p1.SQL_Tablename == SQLTableName.Discharge
                                              select p1.Count).Sum(),
                       rehab_N = (short?)(from p1 in rds
                                          where p1.SQL_Tablename == SQLTableName.Rehabilitation
                                          select p1.Count).Sum(),
                       tcm_N = (short?)(from p1 in rds
                                        where p1.SQL_Tablename == SQLTableName.TraditionalChineseMedicine_detail
                                        select p1.Count).Sum();
                Com_clDataContext dc = new Com_clDataContext();
                tbl_Query2 q = new tbl_Query2()
                {
                    uid = ListRetrieved.First().UID,
                    QDATE = ListRetrieved.First().QDate,
                    EDATE = DateTime.Now,
                    remark = _special_remark
                };
                q.cloudmed_N = med_N;
                q.cloudlab_N = lab_N;
                q.schedule_N = schedule_N;
                q.op_N = op_N;
                q.dental_N = dental_N;
                q.allergy_N = allergy_N;
                q.discharge_N = discharge_N;
                q.rehab_N = rehab_N;
                q.tcm_N = tcm_N;
                dc.tbl_Query2.InsertOnSubmit(q);
                dc.SubmitChanges();
                log.Info($"[Final Step]Successfully write into tbl_Query2. From: {ListRetrieved.First().UID}, [{ListRetrieved.First().QDate}]");
                tb.ShowBalloonTip($"寫入完成 [{current_op?.UID}]", "已寫入資料庫", BalloonIcon.Info);
            }
            catch (Exception ex)
            {
                log.Error($"[Final Step]Failed to write into tbl_Querry2. From:  {ListRetrieved.First().UID}, [{ListRetrieved.First().QDate}] Error: {ex.Message}");
            }

            // 更新顯示資料
            string tempSTR = s.m.Label1.Text;
            s.m.Label1.Text = string.Empty;
            s.m.Label1.Text = tempSTR;
            s.m.Web_refresh();

            // activate hotkeys 4
            Activate_Hotkeys();

            log.Debug(o);
            log.Info("===========================================================================");
            log.Info($" ");

        }
    }
}
