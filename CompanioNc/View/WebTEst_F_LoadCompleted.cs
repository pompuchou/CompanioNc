using CompanioNc.Models;
using Hardcodet.Wpf.TaskbarNotification;
using HtmlAgilityPack;
using mshtml;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace CompanioNc.View
{
    public partial class WebTEst : Window
    {
        private void F_LoadCompleted(object sender, FrameLoadCompleteEventArgs e)
        {
            log.Info($"From {e.Message}, this is F_LoadCompleted.");
            // 20200509 因為沒有先 -= F_LoadComplted, 造成了幽靈, 因此一定要 -= F_LoadComplted 在任何Exit之前.
            // 每次作業F_LoadCompleted只會被呼叫一次,沒有被洗掉的情形
            fm.FrameLoadComplete -= F_LoadCompleted;
            log.Info($"Delete delegate F_LoadCompleted.");
            if (e.Message == FrameLoadStates.DocumentLoadCompletedButNotFrame)
            {
                // 沒有lbluserID, 例如沒插健保卡
                // activate hotkeys 1
                Activate_Hotkeys();
                log.Info($"Exit F_LoadCompleted (1/5). No NHI card inserted.");
                return;
            }
            /// 會有兩次
            /// 第一次讀完檔案會再執行javascript, 在local用來認證健保卡, 沒過就不會有第二次
            /// 如果認證OK, 會再透過javascript下載個案雲端資料, 觸發第二次
            /// 這段程式的目的:
            /// 1. 取得身分證字號
            /// 2. 讀取有哪些tab

            log.Info($"Entered F_LoadCompleted");

            HTMLDocument d = (HTMLDocument)g.Document;
            // 確定身分證字號
            string strUID = MakeSure_UID(d.getElementById("ContentPlaceHolder1_lbluserID").innerText);
            // if (strUID = string.Empty) 離開
            if (strUID == string.Empty)
            {
                tb.ShowBalloonTip("醫療系統資料庫查無此人", "請與杏翔系統連動, 或放棄操作", BalloonIcon.Warning);

                // activate hotkeys 2
                Activate_Hotkeys();
                log.Info($"Exit F_LoadCompleted (2/5). NHI card inserted but no such person.");
                return;
            }
            else
            {
                /// 表示讀卡成功
                /// show balloon with built-in icon
                tb.ShowBalloonTip("讀卡成功", $"身分證號: {strUID}", BalloonIcon.Info);
                log.Info($"  Successful NHI card read, VPN id: {strUID}.");

                /// 讀卡成功後做三件事: 讀特殊註記, 讀提醒, 開始準備讀所有資料

                /// 2. 讀取提醒, 如果有的話
                ///    這是在ContentPlaceHolder1_GV
                ///    也是個table
                IHTMLElement List_NHI_lab_reminder = d?.getElementById("ContentPlaceHolder1_GV");
                if (List_NHI_lab_reminder != null)
                {
                    HtmlDocument Html_NHI_lab_reminder = new HtmlDocument();
                    Html_NHI_lab_reminder.LoadHtml(List_NHI_lab_reminder?.outerHTML);

                    // 寫入資料庫
                    foreach (HtmlNode tr in Html_NHI_lab_reminder.DocumentNode.SelectNodes("//table/tbody/tr"))
                    {
                        HtmlDocument h_ = new HtmlDocument();
                        h_.LoadHtml(tr.InnerHtml);
                        HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                        if ((tds == null) || (tds.Count == 0)) continue;
                        string Lab_Name = string.Empty, Last_Date = string.Empty;
                        // tds[0] 是檢查(驗)項目類別名稱
                        Lab_Name = tds[0]?.InnerText;
                        // tds[1] 是最近1次檢查日期
                        Last_Date = tds[1]?.InnerText;

                        using (Com_clDataContext dc = new Com_clDataContext())
                        {
                            var q1 = from p1 in dc.tbl_NHI_lab_reminder
                                     where (p1.uid == strUID) && (p1.lab_name == Lab_Name) && (p1.last_date == Last_Date)
                                     select p1;
                            if (q1.Count() == 0)
                            {
                                tbl_NHI_lab_reminder newReminder = new tbl_NHI_lab_reminder()
                                {
                                    uid = strUID,
                                    QDATE = DateTime.Now,
                                    lab_name = Lab_Name,
                                    last_date = Last_Date
                                };
                                dc.tbl_NHI_lab_reminder.InsertOnSubmit(newReminder);
                                dc.SubmitChanges();
                            }
                        };
                    }
                }

                /// 準備: 初始化, 欄位基礎資料/位置, 可在windows生成時就完成

                #region 準備 - 製造QueueOperation

                // Initialization
                QueueOperation?.Clear();
                ListRetrieved?.Clear();
                current_page = total_pages = 0;

                log.Info($"  start reading Operation(s).");
                // 讀取所有要讀的tab, 這些資料位於"ContentPlaceHolder1_tab"
                // IHTMLElement 無法轉型為 HTMLDocument
                // 20200429 tested successful
                IHTMLElement List_under_investigation = d?.getElementById("ContentPlaceHolder1_tab");
                // li 之下就是 a
                List<string> balloonstring = new List<string>();
                string BalloonTip = string.Empty;
                foreach (IHTMLElement hTML in List_under_investigation?.all)
                {
                    if (tab_id_wanted.Contains(hTML.id))
                    {
                        // 僅將要讀入的排入, 並沒有真的讀取資料
                        VPN_Operation vOP = VPN_Dictionary.Making_new_operation(hTML.id, strUID, DateTime.Now);
                        QueueOperation.Enqueue(vOP);
                        log.Info($"    讀入operation: {vOP.Short_Name}, [{strUID}]");
                        balloonstring.Add(vOP.Short_Name);
                    }
                }

                for (int i = 0; i < balloonstring.Count; i++)
                {
                    if (i == 0)
                    {
                        BalloonTip = balloonstring[i];
                    }
                    else if ((i % 3) == 0)
                    {
                        BalloonTip += $";\r\n{balloonstring[i]}";
                    }
                    else
                    {
                        BalloonTip += $"; {balloonstring[i]}";
                    }
                }
                log.Info($"  end of Reading Operation(s), 共{QueueOperation?.Count}個Operation(s).");

                #endregion 準備 - 製造QueueOperation

                #region 執行

                if (QueueOperation.Count > 0)
                {
                    // 流程控制, fm = framemonitor

                    // 載入第一個operation
                    log.Info($"  the first operation loaded.");
                    current_op = QueueOperation.Dequeue();
                    tb.ShowBalloonTip($"開始讀取 [{current_op.UID}]", BalloonTip, BalloonIcon.Info);

                    // 判斷第一個operation是否active, (小心起見, 其實應該不可能不active)
                    // 不active就要按鍵
                    // 要注意class這個attribute是在上一層li層, 需把它改過來
                    if (d.getElementById(current_op.TAB_ID.Replace("_a_", "_li_")).className == "active")
                    {
                        // 由於此時沒有按鍵, 因此無法觸發LoadComplete, 必須人工觸發
                        log.Info($"  add delegate F_Data_LoadCompleted.");
                        fm.FrameLoadComplete += F_Data_LoadCompleted;
                        FrameLoadCompleteEventArgs ex = new FrameLoadCompleteEventArgs()
                        {
                            Message = FrameLoadStates.DirectCall
                        };
                        log.Info($"Exit F_LoadCompleted (3/5). Goto F_Data_LoadCompleted by direct call.");
                        F_Data_LoadCompleted(this, ex);
                        return;
                    }
                    else
                    {
                        // 不active反而可以用按鍵, 自動會觸發F_Data_LoadCompleted
                        log.Info($"  push TAB {current_op.TAB_ID} Button.");
                        log.Info($"  add delegate F_Data_LoadCompleted.");
                        fm.FrameLoadComplete += F_Data_LoadCompleted;
                        log.Info($"Exit F_LoadCompleted (4/5). Go to next tab by pushing tab button.");
                        d.getElementById(current_op.TAB_ID).click();
                        return;
                    }
                }
                else
                {
                    /// 做個紀錄
                    /// 一個都沒有
                    log.Info($"  no record at all.");
                    tb.ShowBalloonTip("沒有資料", "健保資料庫裡沒有資料可讀取", BalloonIcon.Info);
                    // 製作紀錄by rd
                    tbl_Query2 q = new tbl_Query2()
                    {
                        uid = strUID,
                        QDATE = DateTime.Now,
                        cloudmed_N = 0,
                        cloudlab_N = 0,
                        schedule_N = 0,
                        op_N = 0,
                        dental_N = 0,
                        allergy_N = 0,
                        discharge_N = 0,
                        rehab_N = 0,
                        tcm_N = 0
                    };
                    using (Com_clDataContext dc = new Com_clDataContext())
                    {
                        dc.tbl_Query2.InsertOnSubmit(q);
                        dc.SubmitChanges();
                    };

                    // activate hotkeys 3
                    Activate_Hotkeys();
                    log.Info($"Exit F_LoadCompleted (5/5). NHI inserted and verified but completey no data.");
                }

                #endregion 執行
            }
        }
    }
}
