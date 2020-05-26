using CompanioNc.Models;
using Hardcodet.Wpf.TaskbarNotification;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using mshtml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CompanioNc.View
{
    /// <summary>
    /// WebTEstView.xaml 的互動邏輯
    /// 20200425: 決定不採用MVVM了, 要用code behind
    ///             幾個要求:
    ///             01. 分辨需要的幾個tab
    ///             02. 主動去找要的tab,而不是被動一個個看哪個有哪個沒有
    ///             03. 每個tab都確實要抓到,不能漏
    ///             04. 新個案可以直接寫入tbl_patients
    ///             05. 每次讀完卡,可以自動更新資料
    ///             06. 不再透過uid.txt直接用程式內部的變數傳遞UID
    ///             07. 儲存特殊註記
    ///             08. 儲存提醒
    ///             09. 改善找到UID, 之前怎麼新個案就找不到?
    ///             10. 新個案等於是要在開杏翔的情況下才能輸入,這樣就避免了輸入中間三位身分證號可能的錯誤
    /// </summary>
    public partial class WebTEst : Window
    {

        #region FLAGS

        //private static readonly log4net.ILog log = LogHelper.GetLogger();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string VPN_URL = @"https://medcloud.nhi.gov.tw/imme0008/IMME0008S01.aspx";
        private const string DEFAULT_URL = @"https://www.googl.com";
        private const string HTML_DIRECTORY = @"C:\vpn\html";

        private readonly string[] tab_id_wanted = new string[] { "ContentPlaceHolder1_a_0008", "ContentPlaceHolder1_a_0009", "ContentPlaceHolder1_a_0020",
                                                                 "ContentPlaceHolder1_a_0030", "ContentPlaceHolder1_a_0040", "ContentPlaceHolder1_a_0060",
                                                                 "ContentPlaceHolder1_a_0070", "ContentPlaceHolder1_a_0080", "ContentPlaceHolder1_a_0090" };

        private readonly Queue<VPN_Operation> QueueOperation = new Queue<VPN_Operation>();
        private VPN_Operation current_op;
        private readonly List<VPN_Retrieved> ListRetrieved = new List<VPN_Retrieved>();
        private int current_page, total_pages;

        private readonly TaskbarIcon tb = new TaskbarIcon();

        private readonly MainWindow m;
        private readonly FrameMonitor fm;

        #endregion FLAGS

        #region Constructor, Loaded, and Closed

        public WebTEst(MainWindow mw)
        {
            // 把Caller傳遞過來
            m = mw;
            fm = new FrameMonitor(this, 100);
            InitializeComponent();
        }

        private void WebTEst_Loaded(object sender, RoutedEventArgs e)
        {
            // 漏了 +=, 難怪不fire
            fm.FrameLoadComplete += F_LoadCompleted;
            log.Info($"  add delegate F_LoadCompleted.");

            // 20200508 加上此段, 因為如果沒有健保卡, 根本不會觸發F_LoadCompleted.
            // activate hotkeys 0
            // 如果我讓它可以觸發的話, 就不用activate hotkeys了
            // Activate_Hotkeys();

            log.Info($"Start to load {VPN_URL} not by hotkey.");
            this.g.Navigate(VPN_URL);
        }

        private void WebTEst_Closed(object sender, EventArgs e)
        {
            // deactivate hotkeys 1
            Deactivate_Hotkeys();
            log.Info($"WebTEst is being closed.");
            m.VPNwindow.IsChecked = false;
            fm.Dispose();
        }

        #endregion Constructor, Loaded, and Closed

        #region LoadCompleted methods

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
                        ListRetrieved.Add(new VPN_Retrieved(tt.Short_Name, tt.Header_Want, f.getElementById(tt.TargetID).outerHTML, current_op.UID, current_op.QDate));

                        SaveHTML($"{tt.Short_Name}_{current_op.UID}", f.getElementById(tt.TargetID).outerHTML);
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
                string tempSTR = m.Label1.Text;
                m.Label1.Text = string.Empty;
                m.Label1.Text = tempSTR;
                m.Web_refresh();

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

        #endregion LoadCompleted methods

        #region Hotkeys, Functions

        public void HotKey_Ctrl_Y()
        {
            /// 目的: 更新雲端資料, 讀寫雲端資料
            /// 現在可以合併兩個步驟為一個步驟
            /// 想到一個複雜的方式, 不斷利用LoadCompleted
            fm.FrameLoadComplete += F_LoadCompleted;
            log.Info("add delegate F_LoadCompleted.");

            // deactivate hotkeys 2
            Deactivate_Hotkeys();

            log.Info($"Start to load {VPN_URL} by hotkey.");

            this.g.Navigate(VPN_URL);
        }

        public void HotKey_Ctrl_G()
        {
            fm.FrameLoadComplete += F_LoadCompleted;
            log.Info("add delegate F_LoadCompleted.");
            //this.g.Navigate(DEFAULT_URL);

            // deactivate hotkeys 2
            Deactivate_Hotkeys();

            FrameLoadCompleteEventArgs ex = new FrameLoadCompleteEventArgs()
            {
                Message = FrameLoadStates.DirectCall
            };
            F_LoadCompleted(this, ex);
        }

        private string MakeSure_UID(string vpnUID)
        {
            log.Info($"  Begin to check UID: {vpnUID}");

            string thesisUID = string.Empty;
            string thesisNAME = string.Empty;
            string o = string.Empty;
            Com_alDataContext dc = new Com_alDataContext();

            /// 找到正確的身分證號碼, 1. 從MainViewModel中的CPatient
            /// 絕不補中間三碼
            /// 如果SQL server裡沒有資料甚至可以寫入
            /// 寫入的資料來源為杏翔系統, 接口為MainWindow的strID.Content
            /// 杏翔的key sample 如下
            /// "2020/04/25 上午 02 診 015 號 - (H223505857) 陳俞潔"
            /// 依照杏翔有無, 資料庫有無, VPN有, 應該有四種情形
            /// VPN 有, 杏翔 有, 資料庫 有 => 理想狀況取的正確UID
            /// VPN 有, 杏翔 有, 資料庫 無 => 新個案, UID寫入資料庫tbl_patients, 取得正確UID
            /// VPN 有, 杏翔 異, 資料庫 有 => 只有一筆, 直接取得UID; 若有多筆, 跳出視窗選擇正確UID
            /// VPN 有, 杏翔 異, 資料庫 無 => 新個案, 不做任何動作, 絕不補中間三碼
            /// VPN 有, 杏翔 無, 資料庫 有 => 只有一筆, 直接取得UID; 若有多筆, 跳出視窗選擇正確UID
            /// VPN 有, 杏翔 無, 資料庫 無 => 新個案, 不做任何動作, 絕不補中間三碼

            // 取得thesisUID
            try
            {
                string[] vs = this.m.strID.Content.ToString().Split(' ');
                log.Info($"    [{this.m.strID.Content}] being processed.");
                // 身分證字號在[7], 還要去掉括弧
                thesisUID = vs[7].Substring(1, (vs[7].Length - 2));
                thesisNAME = vs[8];
                log.Info($"    杏翔系統目前UID: {thesisUID}");
            }
            catch (Exception ex)
            {
                //log.Error(ex.Message);
                /// 杏翔沒開, 或是沒連動, 反正就是抓不到
                /// thesisUID = string.Empty;
                /// thesisNAME = string.Empty;
                log.Error($"    杏翔系統無法取得UID: [{thesisUID}], ERROR:{ex.Message}");
            }

            if (!string.IsNullOrEmpty(thesisUID))
            {
                // 第一, 第二種狀況, 有杏翔
                if ((thesisUID.Substring(0, 4) == vpnUID.Substring(0, 4)) &&
                                (thesisUID.Substring(7, 3) == vpnUID.Substring(7, 3)))
                {
                    /// 要確認不要確認?
                    /// 在看診情況下,這是90%的狀況
                    /// passed  first test
                    /// 在區分兩種狀況, 如果資料庫裏面沒有, 就是新個案
                    try
                    {
                        // Single() returns the item, throws an exception if there are 0 or more than one item in the sequence.
                        string sqlUID = (from p in dc.tbl_patients
                                         where p.uid == thesisUID
                                         select p.uid).Single();
                        log.Info($"    VPN 有 [{vpnUID}], 杏翔 有 [{thesisUID}], 資料庫 有資料庫裏面也也有: [{sqlUID}]");
                        // 如果沒有錯誤發生
                        // 此時為第一種狀況
                        /// VPN 有, 杏翔 有, 資料庫 有 => 理想狀況取的正確UID
                    }
                    catch (Exception ex)
                    {
                        log.Error($"    {ex.Message}: 資料庫裡沒有這個病人, 新加入tbl_patients.");
                        // Single() returns the item, throws an exception if there are 0 or more than one item in the sequence.
                        // 因為uid是primary key, 如果有錯誤只可能是沒有此uid
                        // 此時為第二種狀況
                        // VPN 有, 杏翔 有, 資料庫 無 => 新個案, UID寫入資料庫tbl_patients, 取得正確UID
                        // 接下來就是要新增一筆資料
                        CultureInfo MyCultureInfo = new CultureInfo("en-US");
                        DateTime dummyDateTime = DateTime.ParseExact("Friday, April 10, 2009", "D", MyCultureInfo);
                        tbl_patients newPt = new tbl_patients
                        {
                            cid = 0,
                            uid = thesisUID,
                            bd = dummyDateTime,
                            cname = thesisNAME,
                            QDATE = DateTime.Now
                        };
                        // 20200526 加入QDATE
                        dc.tbl_patients.InsertOnSubmit(newPt);
                        dc.SubmitChanges();
                    }
                    // 無論第一或第二種狀況, 都是回傳thesisUID
                    o = thesisUID;
                }
                else
                {
                    // 如果沒有使用companion, 或是用別人的健保卡,單獨要查詢
                    // 第三, 第四種狀況
                    var q = from p in dc.tbl_patients
                            where (p.uid.Substring(0, 4) == vpnUID.Substring(0, 4) &&
                            p.uid.Substring(7, 3) == vpnUID.Substring(7, 3))
                            select new { p.uid, p.cname };
                    switch (q.Count())
                    {
                        case 0:
                            // 這是第四種狀況
                            // VPN 有, 杏翔 無, 資料庫 無 => 新個案, 不做任何動作, 絕不補中間三碼
                            o = string.Empty;
                            log.Info("    VPN 有, 杏翔 異, 資料庫 無 => 新個案, 不做任何動作, 絕不補中間三碼");
                            break;

                        case 1:
                            // passed test
                            // 這是第三種狀況(1/2)
                            log.Info("    VPN 有, 杏翔 異, 資料庫 有, 且只有一筆 => 直接從資料庫抓");
                            o = q.Single().uid;
                            break;

                        default:
                            // 這是第三種狀況(2/2)
                            string qu = "請選擇 \r\n";
                            for (int i = 0; i < q.Count(); i++)
                            {
                                qu += $"{i + 1}. {q.ToList()[i].uid} {q.ToList()[i].cname} \r\n";
                            }
                            string answer = "0";
                            while ((int.Parse(answer) < 1) || (int.Parse(answer) > q.Count()))
                            {
                                answer = Interaction.InputBox(qu);
                                // 有逃脫機制了
                                if (answer == "q")
                                {
                                    o = string.Empty;
                                    return o;
                                }
                                if (!(int.TryParse(answer, out int result))) answer = "0";
                            }
                            log.Info("    VPN 有, 杏翔 異, 資料庫 有, 但有多筆 => 選擇後從資料庫抓");
                            o = q.ToList()[int.Parse(answer) - 1].uid;
                            break;
                    }
                }
            }
            else
            {
                // 如果沒有使用companion, 或是用別人的健保卡,單獨要查詢
                // 第三, 第四種狀況
                var q = from p in dc.tbl_patients
                        where (p.uid.Substring(0, 4) == vpnUID.Substring(0, 4) &&
                        p.uid.Substring(7, 3) == vpnUID.Substring(7, 3))
                        select new { p.uid, p.cname };
                switch (q.Count())
                {
                    case 0:
                        // 這是第四種狀況
                        // VPN 有, 杏翔 無, 資料庫 無 => 新個案, 不做任何動作, 絕不補中間三碼
                        log.Info("    VPN 有, 杏翔 無, 資料庫 無 => 新個案, 不做任何動作, 絕不補中間三碼");
                        o = string.Empty;
                        break;

                    case 1:
                        // passed test
                        // 這是第三種狀況(1/2)
                        log.Info("    VPN 有, 杏翔 無, 資料庫 有, 且只有一筆 => 直接從資料庫抓");
                        o = q.Single().uid;
                        break;

                    default:
                        // 這是第三種狀況(2/2)
                        string qu = "請選擇 \r\n";
                        for (int i = 0; i < q.Count(); i++)
                        {
                            qu += $"{i + 1}. {q.ToList()[i].uid} {q.ToList()[i].cname} \r\n";
                        }
                        string answer = "0";
                        while ((int.Parse(answer) < 1) || (int.Parse(answer) > q.Count()))
                        {
                            answer = Interaction.InputBox(qu);
                            // 有逃脫機制了
                            if (answer == "q")
                            {
                                o = string.Empty;
                                return o;
                            }
                            if (!(int.TryParse(answer, out int result))) answer = "0";
                        }
                        log.Info("    VPN 有, 杏翔 無, 資料庫 有, 但有多筆 => 選擇後從資料庫抓");
                        o = q.ToList()[int.Parse(answer) - 1].uid;
                        break;
                }
            }
            log.Info($"  End to check UID: {vpnUID}");
            return o;
        }

        private void Activate_Hotkeys()
        {
            // 20200508 已經完成了, 又開始可以有反應了
            try
            {
                m.hotKeyManager.Register(Key.Y, ModifierKeys.Control);
                m.hotKeyManager.Register(Key.G, ModifierKeys.Control);
                log.Info("Hotkey Ctrl-Y, Ctrl-G registered.");
            }
            catch (Exception ex)
            {
                log.Fatal($"Double Register Ctrl-Y, Ctrl-G. Fatal. Error: {ex.Message}");
            }
        }

        private void Deactivate_Hotkeys()
        {
            // 20200508 加上不反應期的功能
            try
            {
                m.hotKeyManager.Unregister(Key.Y, ModifierKeys.Control);
                m.hotKeyManager.Unregister(Key.G, ModifierKeys.Control);
                log.Info("Hotkey Ctrl-Y, Ctrl-G 1 unregistered.");
            }
            catch (Exception ex)
            {
                log.Fatal($"Double Unregister Ctrl-Y, Ctrl-G. Fatal. Error: {ex.Message}");
            }
        }

        private void SaveHTML(string sname, string outerHTML)
        {
            // 製作自動檔名
            string temp_filepath = HTML_DIRECTORY;
            // 存放目錄,不存在就要建立一個
            if (!System.IO.Directory.Exists(temp_filepath))
            {
                System.IO.Directory.CreateDirectory(temp_filepath);
            }
            // 自動產生名字
            temp_filepath += $"\\{sname}_{DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}";
            temp_filepath += $"_{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}{DateTime.Now.Millisecond}.html";

            // 製作html檔 writing to html
            System.IO.StreamWriter sw = new System.IO.StreamWriter(temp_filepath, true, System.Text.Encoding.Unicode);
            sw.Write(outerHTML);
            sw.Close();

        }

        #endregion Hotkeys, Functions
    
    }
}