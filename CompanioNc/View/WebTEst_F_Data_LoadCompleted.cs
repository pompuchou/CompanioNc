using HtmlAgilityPack;
using mshtml;
using System;
using System.Threading;
using System.Windows;

namespace CompanioNc.View
{
    public partial class WebTEst : Window
    {
        private void F_Data_LoadCompleted(object sender, FrameLoadCompleteEventArgs e)
        {
            log.Info($"++ Enter F_Data_LoadCompleted from {e.Message}.");
            if (e.Message == FrameLoadStates.DocumentLoadCompletedButNotFrame) return;
            // 每當刷新後都要重新讀一次
            // d 是parent HTML document
            HTMLDocument d = (HTMLDocument)g.Document;

            #region 感知目前位置, 與current_op比較, 如果不是,就還是要再按一次

            // 目標: current_op.TAB_ID
            // 實際: d.getElementById(current_op.TAB_ID.Replace("_a_", "_li_")).className == "active"
            if (d.getElementById(current_op.TAB_ID.Replace("_a_", "_li_")).className != "active")
            {
                // 不active反而可以用按鍵, 自動會觸發F_Data_LoadCompleted
                // 20210719 有時候不fire
                Thread.Sleep(100);
                d.getElementById(current_op.TAB_ID).click();
                log.Info($"[Action] push TAB {current_op.TAB_ID} Button.");
                log.Info($"++ Exit F_Data_LoadCompleted (1/4). Not in the right page, click again.");
                return;
            }
            #endregion

            /// 20210719: 將QDATE設定為1901/1/1, 是傳達這是最後一頁了, 設定在F_Pager_LoadCompleted
            /// 20210719: 刪除掉蠢蠢的current_op?.QDate = 1901/1/1
            ///if (current_op?.QDate == DateTime.Parse("1901/01/01"))
            ///{
            ///    // 不用再讀取資料了, 直接到存入資料庫部分
            ///    log.Info($"Entered F_Data_LoadCompleted, {current_op?.UID} 已經讀完所有資料, 最後存入資料庫.");
            ///}
            ///else
            ///{
            /// 還需要讀取資料
            log.Info($"Current OP: {current_op?.UID} {current_op?.Short_Name}");
            //}
            log.Info($"@@ delete delegate F_Data_LoadComplated. [{current_op?.UID}]");
            fm.FrameLoadComplete -= F_Data_LoadCompleted;

            // 這時候已經確保是 active
            // f 是frame(0) HTML document
            HTMLDocument f = d?.frames.item(0).document.body.document;

            /// 3. 網頁操弄與擷取資料: sequential
            ///     3-1. 判斷分頁, 有幾頁, 現在在第幾頁, 換頁不會觸發LoadCompleted; 疑問會不會來不及? -done
            ///     3-2. 要先排序, 排序也不會觸發LoadCompleted; 疑問會不會來不及?  -done, 不用再管排序
            ///     3-2. 都放在記憶體裡, 快速, in the LIST
            ///

            // special remark: current_op.QDate set to null 是表示來自多頁結束那裏

            // 20210719: 將QDATE設定為1901/1/1, 是傳達這是最後一頁了, 設定在F_Pager_LoadCompleted
            // 20210719: 刪除掉蠢蠢的current_op?.QDate = 1901/1/1
            //if ((current_op != null) && (current_op?.QDate != DateTime.Parse("1901/01/01")))
            if (current_op != null)
            {
                // 表示不是最後一頁, 還要讀取資料
                log.Info($"[Action] Reading HTML: {current_op.Short_Name}, [{current_op?.UID}]");

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
                    log.Info($"[Info] Only one page detected, {current_op?.UID} {current_op?.Short_Name}.");
                    total_pages = 1;
                }
                else
                {
                    // 如果多頁, 轉換loadcomplete, 呼叫pager by click
                    // 20200502: outerHTML的XPATH="//selection/option", innerHTML的XPATH="//option"
                    p.LoadHtml(f.getElementById(@"ctl00$ContentPlaceHolder1$pg_gvList_input").innerHTML);
                    HtmlNodeCollection o = p.DocumentNode.SelectNodes("//option");
                    total_pages = o.Count;
                    log.Info($"[Info] {total_pages} pages detected, {current_op?.UID} {current_op?.Short_Name}");

                    // 剛剛已經讀了第一頁了, 從下一頁開始
                    current_page = 2;
                    // 按鈕機制
                    fm.FrameLoadComplete += F_Pager_LoadCompleted;
                    // 轉軌
                    log.Info($"@@ add delegate F_Pager_LoadCompleted.");
                    Thread.Sleep(100);
                    foreach (IHTMLElement a in f.getElementById("ContentPlaceHolder1_pg_gvList").all)
                    {
                        if (a.innerText == ">")
                        {
                            log.Info("[Action] 按了下一頁.");
                            log.Info($"++ Exit F_Data_LoadCompleted (2/4). Multipage, go to next page.");
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
                // 兩個出口之一, 最後一個tab, 另一個出口是, 最後一個tab, 同時沒有其他page, 另一個在F_Pager_LoadCompleted
                // 20210719: 讀完實體資料就開始解析HTML吧
                string o = $"++ Exit F_Data_LoadCompleted (3/4). The REAL END! [{current_op.UID}]";
                ParsingTables(o);

                return;
            }
            else
            {
                // 下一個tab
                current_op = QueueOperation.Dequeue();
                fm.FrameLoadComplete += F_Data_LoadCompleted;
                Thread.Sleep(100);
                log.Info($"@@ add delegate F_Data_LoadCompleted. [{current_op.UID}]");
                d.getElementById(current_op.TAB_ID).click();
                log.Info($"[Action] {current_op.TAB_ID} tab key pressed. [{current_op.UID}]");
                log.Info($"++ Exit F_Data_LoadCompleted (4/4). Go to next tab. {QueueOperation.Count + 1} tabs to go.. [{current_op.UID}]");
                return;
            }
        }
    }
}
