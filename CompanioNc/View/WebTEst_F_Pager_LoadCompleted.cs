using mshtml;
using System;
using System.Windows;

namespace CompanioNc.View
{
    public partial class WebTEst : Window
    {
        private void F_Pager_LoadCompleted(object sender, FrameLoadCompleteEventArgs e)
        {
            log.Info($"From {e.Message}, this is F_Pager_LoadCompleted.");
            if (e.Message == FrameLoadStates.DocumentLoadCompletedButNotFrame) return;
            log.Info($"Entered F_Pager_LoadCompleted");
            log.Info($"  delete delegate F_Pager_LoadComplated. [{current_op?.UID}]");
            fm.FrameLoadComplete -= F_Pager_LoadCompleted;

            // 每當刷新後都要重新讀一次
            // d 是parent HTML document
            HTMLDocument d = (HTMLDocument)g.Document;
            // f 是frame(0) HTML document
            HTMLDocument f = d.frames.item(0).document.body.document;

            // 讀取資料
            foreach (Target_Table tt in current_op.Target)
            {
                // 這裡不用管多table, 因為多table只發生在管制藥那裏
                ListRetrieved.Add(new VPN_Retrieved(tt.Short_Name, tt.Header_Want, f.getElementById(tt.TargetID).outerHTML, current_op.UID, current_op.QDate));

                SaveHTML($"{tt.Short_Name}_{current_op.UID}", f.getElementById(tt.TargetID).outerHTML);

            }
            log.Info($"  1. Reading operation: {current_op.Short_Name}, [{current_op.UID}]. page: {current_page}/{total_pages}");

            // 判斷是否最後一頁, 最後一tab
            if ((current_page == total_pages) && (QueueOperation.Count == 0))
            {
                // 最後一頁
                // 處理index
                current_page = total_pages = 0;

                // 20200506: current_op 歸零似乎是不行的
                //current_op = null;
                // 20210719: 將QDATE設定為1901/1/1, 是傳達這是最後一頁了, 設定在F_Pager_LoadCompleted
                current_op.QDate = DateTime.Parse("1901/01/01");

                // 這是沒有用的add delegate, 但是為了平衡, 避免可能的錯誤
                fm.FrameLoadComplete += F_Data_LoadCompleted;
                log.Info("  add delegate F_Data_LoadCompleted.");

                log.Info($"Exit F_Pager_LoadCompleted (1/3). last page, last tab, go to finalization directly. [{current_op.UID}]");

                // 沒有按鍵無法直接觸發, 只好直接呼叫
                FrameLoadCompleteEventArgs ex = new FrameLoadCompleteEventArgs()
                {
                    Message = FrameLoadStates.DirectCall
                };
                F_Data_LoadCompleted(this, ex);
                // 此段程式的一個出口點
                return;
            }
            else if (current_page == total_pages)
            {
                // 最後一頁
                // 處理index
                current_page = total_pages = 0;
                // 轉軌
                fm.FrameLoadComplete += F_Data_LoadCompleted;
                log.Info($"  add delegate F_Data_LoadCompleted. [{current_op.UID}]");

                // 下一個tab
                log.Info($"  next operation loaded.");
                current_op = QueueOperation.Dequeue();
                log.Info($"Exit F_Pager_LoadCompleted (2/3). last page, go to next tab by clicking. [{current_op.UID}]");
                d.getElementById(current_op.TAB_ID).click();
                // 此段程式的一個出口點
                return;
            }
            else
            {
                current_page++;
                // 按鍵到下一頁, 此段程式到此結束
                // HOW TO ?????????????????????????????????????????
                // 如何下一頁, 可能要用invokescript
                // 按鈕機制
                log.Info($"  add delegate F_Pager_LoadCompleted. [{current_op.UID}]");
                log.Info($"Exit F_Pager_LoadCompleted (3/3). go to next page by clicking. [{current_op.UID}]");
                fm.FrameLoadComplete += F_Pager_LoadCompleted;
                foreach (IHTMLElement a in f.getElementById("ContentPlaceHolder1_pg_gvList").all)
                {
                    if (a.innerText == ">")
                    {
                        log.Info($"下一頁按下去了.(多頁) [{current_op.UID}]");
                        a.click();
                    }
                }
                // 此段程式的一個出口點
                return;
            }
        }

    }
}
