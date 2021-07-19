using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using mshtml;
using HtmlAgilityPack;
using Hardcodet.Wpf.TaskbarNotification;
using CompanioNc.Models;

namespace CompanioNc.View
{
    public partial class WebTEst : Window
    {
        private async void F_Data_LoadCompleted(object sender, FrameLoadCompleteEventArgs e)
        {
            log.Info($"From {e.Message}, this is F_Data_LoadCompleted.");
            if (e.Message == FrameLoadStates.DocumentLoadCompletedButNotFrame) return;
            if (current_op?.QDate == DateTime.Parse("1901/01/01"))
            {
                log.Info($"Entered F_Data_LoadCompleted, {current_op?.UID} 最後存入資料庫.");
            }
            else
            {
                log.Info($"Entered F_Data_LoadCompleted, {current_op?.UID} {current_op?.Short_Name}");
            }
            log.Info($"  delete delegate F_Data_LoadComplated. [{current_op?.UID}]");
            fm.FrameLoadComplete -= F_Data_LoadCompleted;

            // 這時候已經確保是 active
            // 每當刷新後都要重新讀一次
            // d 是parent HTML document
            HTMLDocument d = (HTMLDocument)g.Document;
            // f 是frame(0) HTML document
            HTMLDocument f = d?.frames.item(0).document.body.document;

            /// 3. 網頁操弄與擷取資料: sequential
            ///     3-1. 判斷分頁, 有幾頁, 現在在第幾頁, 換頁不會觸發LoadCompleted; 疑問會不會來不及? -done
            ///     3-2. 要先排序, 排序也不會觸發LoadCompleted; 疑問會不會來不及?  -done, 不用再管排序
            ///     3-2. 都放在記憶體裡, 快速, in the LIST
            ///

            // special remark: current_op.QDate set to null 是表示來自多頁結束那裏

            if ((current_op != null) && (current_op?.QDate != DateTime.Parse("1901/01/01")))
            {
                log.Info($"  1. Reading operation: {current_op.Short_Name}, [{current_op?.UID}]");

                #region 讀取資料, 存入記憶體, 存入檔案

                foreach (Target_Table tt in current_op.Target)
                {
                    // 是否有多tables, 端看tt.Children, 除了管制藥物外, 其餘都不用
                    if (tt.Children == null)
                    {
                        try
                        {
                            // 20210718: 好像就是這裏出了問題
                            ListRetrieved.Add(new VPN_Retrieved(tt.Short_Name, tt.Header_Want, f.getElementById(tt.TargetID).outerHTML, current_op.UID, current_op.QDate));

                            // 20200606 stop recording tables in html
                            // 20210716 resume recording tables in html
                            SaveHTML($"{tt.Short_Name}_{current_op.UID}", f.getElementById(tt.TargetID).outerHTML);
                        }
                        catch (Exception ex)
                        {
                            // 存入空的HTML
                            ListRetrieved.Add(new VPN_Retrieved(tt.Short_Name, tt.Header_Want, "", current_op.UID, current_op.QDate));
                            log.Error($"  Failed to read html. From:  {tt.Short_Name}. Error: {ex.Message}");
                        }
                    }
                    else
                    {
                        // 有多個table, 使用情形僅有管制藥物
                        ListRetrieved.Add(new VPN_Retrieved(tt.Short_Name, tt.Header_Want, f.getElementById(tt.TargetID).children(tt.Children).outerHTML, current_op.UID, current_op.QDate));

                        SaveHTML($"{tt.Short_Name}_{tt.Children}_{current_op.UID}", f.getElementById(tt.TargetID).outerHTML);
                    }
                }

                #endregion 讀取資料, 存入記憶體, 存入檔案

                #region 判斷多頁

                // 目前重點, 如何判斷多頁?
                // 設定total_pages = ????
                HtmlDocument p = new HtmlDocument();
                if (f.getElementById(@"ctl00$ContentPlaceHolder1$pg_gvList_input") == null)
                {
                    log.Info($"  2. Only one page detected, {current_op?.UID} {current_op?.Short_Name}.");
                    total_pages = 1;
                }
                else
                {
                    // 如果多頁, 轉換loadcomplete, 呼叫pager by click
                    // 20200502: outerHTML的XPATH="//selection/option", innerHTML的XPATH="//option"
                    p.LoadHtml(f.getElementById(@"ctl00$ContentPlaceHolder1$pg_gvList_input").innerHTML);
                    HtmlNodeCollection o = p.DocumentNode.SelectNodes("//option");
                    total_pages = o.Count;
                    log.Info($"  2. {total_pages} pages detected, {current_op?.UID} {current_op?.Short_Name}");

                    // 剛剛已經讀了第一頁了, 從下一頁開始
                    current_page = 2;
                    // 按鈕機制
                    fm.FrameLoadComplete += F_Pager_LoadCompleted;
                    // 轉軌
                    log.Info($"  add delegate F_Pager_LoadCompleted.");
                    foreach (IHTMLElement a in f.getElementById("ContentPlaceHolder1_pg_gvList").all)
                    {
                        if (a.innerText == ">")
                        {
                            log.Info("  按了下一頁.");
                            log.Info($"Exit F_Data_LoadCompleted (1/3). Multipage, go to next page.");
                            a.click();
                            // 20200504 發現這裡執行完後還會執行後面的程序, 造成兩個程序的衝突
                            // 此段程式的一個出口點
                            return;
                        }
                    }
                }

                #endregion 判斷多頁
            }

            // 判斷是否最後一tab, 程序群的出口
            if (QueueOperation.Count == 0)
            {
                tb.ShowBalloonTip($"讀取完成 [{current_op?.UID}]", "開始解析與寫入資料庫", BalloonIcon.Info);
                log.Info($"  all datatable loaded into memory. start to analyze. [{current_op?.UID}]");

                // Count = 0 代表最後一個 tab
                // 20200504 這裡一個BUG, 漏了把F_DATA_Loadcompleted刪掉,以至於不斷重複多次. ******

                /// 確定是最後一個tab這段程式到此結束
                /// 4. Parsing & Saving to SQL: async
                ///     4-1. 多工同時處理, 快速
                ///     4-2. 依照欄位資料/位置 Parsing
                ///     4-3. 存入SQL
                ///     4-4. 製作Query
                /// 查核機制?
                log.Info($"  start async process, {current_op?.UID}");
                List<Response_DataModel> rds = await VPN_Dictionary.RunWriteSQL_Async(ListRetrieved);
                log.Info($"  end async process, {current_op?.UID}");

                /// 1. 讀取特殊註記, 如果有的話
                ///    這是在ContentPlaceHolder1_tab02
                ///    是個table
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
                    log.Info($"  4. Successfully write into tbl_Query2. From: {ListRetrieved.First().UID}, [{ListRetrieved.First().QDate}]");
                    tb.ShowBalloonTip($"寫入完成 [{current_op?.UID}]", "已寫入資料庫", BalloonIcon.Info);
                }
                catch (Exception ex)
                {
                    log.Error($"  4. Failed to write into tbl_Querry2. From:  {ListRetrieved.First().UID}, [{ListRetrieved.First().QDate}] Error: {ex.Message}");
                }

                // 更新顯示資料
                string tempSTR = s.m.Label1.Text;
                s.m.Label1.Text = string.Empty;
                s.m.Label1.Text = tempSTR;
                s.m.Web_refresh();

                // activate hotkeys 4
                Activate_Hotkeys();

                log.Info($"Exit F_Data_LoadCompleted (2/3). The REAL END! [{current_op.UID}]");
                return;
            }
            else
            {
                // 下一個tab
                current_op = QueueOperation.Dequeue();
                log.Info($"  next operation loaded.");
                log.Info($"  {current_op.TAB_ID} tab key pressed. [{current_op.UID}]");
                log.Info($"  add delegate F_Data_LoadCompleted. [{current_op.UID}]");
                log.Info($"Exit F_Data_LoadCompleted (3/3). Go to next tab. {QueueOperation.Count + 1} tabs to go.. [{current_op.UID}]");
                fm.FrameLoadComplete += F_Data_LoadCompleted;
                d.getElementById(current_op.TAB_ID).click();
                // 此段程式的一個出口點
                return;
            }
        }

    }
}
