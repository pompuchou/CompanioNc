using CompanioNc.Models;
using GlobalHotKey;
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

        private HotKeyManager hotKeyManager;
        private const string VPN_URL = @"https://medcloud.nhi.gov.tw/imme0008/IMME0008S01.aspx";
        private const string DEFAULT_URL = @"https://www.googl.com";

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
            // Create the hotkey manager.
            hotKeyManager = new HotKeyManager();
            // Register Ctrl+Y, Ctrl+G hotkey. Save this variable somewhere for the further unregistering.
            try
            {
                hotKeyManager.Register(Key.Y, ModifierKeys.Control);
                hotKeyManager.Register(Key.G, ModifierKeys.Control);
            }
            catch (Exception)
            {
                throw;
            }
            // 漏了 +=, 難怪不fire
            fm.FrameLoadComplete -= F_LoadCompleted;
            fm.FrameLoadComplete += F_LoadCompleted;
            this.g.Navigate(VPN_URL);

            // Handle hotkey presses.
            hotKeyManager.KeyPressed += HotKeyManagerPressed;
        }

        private void WebTEst_Closed(object sender, EventArgs e)
        {
            try
            {
                hotKeyManager.Unregister(Key.Y, ModifierKeys.Control);
                hotKeyManager.Unregister(Key.G, ModifierKeys.Control);
            }
            catch (Exception)
            {
                throw;
            }
            hotKeyManager.Dispose();
            m.VPNwindow.IsChecked = false;
            fm.Dispose();
        }
        
        #endregion

        #region LoadCompleted methods

        private void F_LoadCompleted(object sender, FrameLoadCompleteEventArgs e)
        {
            /// 會有兩次
            /// 第一次讀完檔案會再執行javascript, 在local用來認證健保卡, 沒過就不會有第二次
            /// 第二次如果認證OK, 會再透過javascript下載個案雲端資料
            /// 這段程式的目的:
            /// 1. 取得身分證字號
            /// 2. 讀取有哪些tab

            HTMLDocument d = (HTMLDocument)g.Document;
            if (d?.getElementById("ContentPlaceHolder1_lbluserID") != null)
            {
                // 確定身分證字號
                string strUID = MakeSure_UID(d.getElementById("ContentPlaceHolder1_lbluserID").innerText);
                // if (strUID = string.Empty) 離開
                if (strUID == string.Empty)
                {
                    tb.ShowBalloonTip("醫療系統資料庫查無此人", "請與杏翔系統連動, 或放棄操作", BalloonIcon.Warning);
                    return;
                }
                else
                {
                    /// 表示讀卡成功
                    /// show balloon with built-in icon
                    tb.ShowBalloonTip("讀卡成功", $"身分證號: {strUID}", BalloonIcon.Info);

                    /// 讀卡成功後做三件事: 讀特殊註記, 讀提醒, 開始準備讀所有資料
                    /// To Do:
                    /// 1. 讀取特殊註記, 如果有的話
                    ///    這是在ContentPlaceHolder1_tab02
                    ///    是個table

                    /// To Do:
                    /// 2. 讀取提醒, 如果有的話
                    ///    這是在ContentPlaceHolder1_GV
                    ///    也是個table

                    /// 準備: 初始化, 欄位基礎資料/位置, 可在windows生成時就完成

                    #region 準備

                    // Initialization
                    QueueOperation.Clear();
                    ListRetrieved.Clear();
                    current_page = total_pages = 0;

                    // 讀取所有要讀的tab, 這些資料位於"ContentPlaceHolder1_tab"
                    // IHTMLElement 無法轉型為 HTMLDocument
                    // 20200429 tested successful
                    IHTMLElement List_under_investigation = d.getElementById("ContentPlaceHolder1_tab");
                    // li 之下就是 a
                    foreach (IHTMLElement hTML in List_under_investigation.all)
                    {
                        if (tab_id_wanted.Contains(hTML.id))
                        {
                            QueueOperation.Enqueue(VPN_Dictionary.Making_new_operation(hTML.id, strUID, DateTime.Now));
                        }
                    }

                    #endregion 準備

                    #region 執行

                    if (QueueOperation.Count > 0)
                    {
                        // 流程控制, fm = framemonitor
                        // 換軌
                        fm.FrameLoadComplete -= F_LoadCompleted;
                        fm.FrameLoadComplete -= F_Data_LoadCompleted;
                        fm.FrameLoadComplete += F_Data_LoadCompleted;

                        // 載入第一個operation
                        current_op = QueueOperation.Dequeue();

                        // 判斷第一個operation是否active, (小心起見, 其實應該不可能不active)
                        // 不active就要按鍵
                        // 要注意class這個attribute是在上一層li層, 需把它改過來
                        if (d.getElementById(current_op.TAB_ID.Replace("_a_", "_li_")).className == "active")
                        {
                            // 由於此時沒有按鍵, 因此無法觸發LoadComplete, 必須人工觸發
                            FrameLoadCompleteEventArgs args = new FrameLoadCompleteEventArgs
                            {
                                MyProperty = 1
                            };
                            F_Data_LoadCompleted(this, args);
                        }
                        else
                        {
                            // 不active反而可以用按鍵, 自動會觸發F_Data_LoadCompleted
                            d.getElementById(current_op.TAB_ID).click();
                        }
                        // 這段程式到此結束
                    }
                    else
                    {
                        /// ToDo: 做個紀錄吧!
                    }

                    #endregion 執行
                }
            }
        }

        private async void F_Data_LoadCompleted(object sender, FrameLoadCompleteEventArgs e)
        {
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

            // 讀取資料, 存入記憶體
            if (current_op != null)
            {
                foreach (Target_Table tt in current_op.Target)
                {
                    // 是否有多tables, 端看tt.Children, 除了管制藥物外, 其餘都不用
                    if (tt.Children == null)
                    {
                        ListRetrieved.Add(new VPN_Retrieved(tt.Short_Name, tt.Header_Want, f.getElementById(tt.TargetID).outerHTML, current_op.UID, current_op.QDate));
                    }
                    else
                    {
                        // 有多個table, 使用情形僅有管制藥物
                        ListRetrieved.Add(new VPN_Retrieved(tt.Short_Name, tt.Header_Want, f.getElementById(tt.TargetID).children(tt.Children).outerHTML, current_op.UID, current_op.QDate));
                    }
                }
            }

            #region 判斷多頁

            // 目前重點, 如何判斷多頁?
            // 設定total_pages = ????
            HtmlDocument p = new HtmlDocument();
            if (f.getElementById(@"ctl00$ContentPlaceHolder1$pg_gvList_input") == null)
            {
                total_pages = 1;
            }
            else
            {
                // 如果多頁, 轉換loadcomplete, 呼叫pager by click
                // 20200502: outerHTML的XPATH="//selection/option", innerHTML的XPATH="//option"
                p.LoadHtml(f.getElementById(@"ctl00$ContentPlaceHolder1$pg_gvList_input").innerHTML);
                HtmlNodeCollection o = p.DocumentNode.SelectNodes("//option");
                total_pages = o.Count;
                // 轉軌
                fm.FrameLoadComplete -= F_Data_LoadCompleted;
                fm.FrameLoadComplete -= F_Pager_LoadCompleted;
                fm.FrameLoadComplete += F_Pager_LoadCompleted;

                // 剛剛已經讀了第一頁了, 從下一頁開始
                current_page = 2;
                // 按鈕機制
                foreach (IHTMLElement a in f.getElementById("ContentPlaceHolder1_pg_gvList").all)
                {
                    if (a.innerText == ">")
                    {
                        a.click();
                    }
                }
            }

            #endregion 判斷多頁

            // 判斷是否最後一tab
            if (QueueOperation.Count == 0)
            {
                // Count = 0 代表最後一個 tab
                fm.FrameLoadComplete -= F_LoadCompleted;

                /// 確定是最後一個tab這段程式到此結束
                /// 4. Parsing & Saving to SQL: async
                ///     4-1. 多工同時處理, 快速
                ///     4-2. 依照欄位資料/位置 Parsing
                ///     4-3. 存入SQL
                ///     4-4. 製作Query
                /// 查核機制?

                List<Response_DataModel> rds = await VPN_Dictionary.RunWriteSQL_Async(ListRetrieved);

                // 製作紀錄by rd
                Com_clDataContext dc = new Com_clDataContext();
                tbl_Query2 q = new tbl_Query2()
                {
                    uid = ListRetrieved.First().UID,
                    QDATE = ListRetrieved.First().QDate
                };
                q.cloudmed_N = (short?)(from p1 in rds
                                where p1.SQL_Tablename == "med"
                               select p1.Count).Sum();
                q.cloudlab_N = (short?)(from p1 in rds
                                        where p1.SQL_Tablename == "lab"
                                        select p1.Count).Sum();
                q.schedule_N = (short?)(from p1 in rds
                                        where p1.SQL_Tablename == "sch_up"
                                        select p1.Count).Sum();
                q.op_N = (short?)(from p1 in rds
                                  where p1.SQL_Tablename == "op"
                                  select p1.Count).Sum();
                q.dental_N = (short?)(from p1 in rds
                                      where p1.SQL_Tablename == "dental"
                                      select p1.Count).Sum();
                q.allergy_N = (short?)(from p1 in rds
                                       where p1.SQL_Tablename == "all"
                                       select p1.Count).Sum();
                q.discharge_N = (short?)(from p1 in rds
                                         where p1.SQL_Tablename == "dis"
                                         select p1.Count).Sum();
                q.rehab_N = (short?)(from p1 in rds
                                     where p1.SQL_Tablename == "reh"
                                     select p1.Count).Sum();
                q.tcm_N = (short?)(from p1 in rds
                                   where p1.SQL_Tablename == "tcm"
                                   select p1.Count).Sum();
                dc.tbl_Query2.InsertOnSubmit(q);
                dc.SubmitChanges();

            }
            else
            {
                // 下一個tab
                current_op = QueueOperation.Dequeue();
                d.getElementById(current_op.TAB_ID).click();
            }
        }

        private void F_Pager_LoadCompleted(object sender, FrameLoadCompleteEventArgs e)
        {
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
            }

            // 判斷是否最後一頁, 最後一tab
            if ((current_page == total_pages) && (QueueOperation.Count == 0))
            {
                // 最後一頁
                // 處理index
                current_page = total_pages = 0;
                // 轉軌
                fm.FrameLoadComplete -= F_Pager_LoadCompleted;
                fm.FrameLoadComplete -= F_Data_LoadCompleted;
                fm.FrameLoadComplete += F_Data_LoadCompleted;

                // current_op 歸零
                current_op = null;

                // 沒有按鍵無法直接觸發, 只好直接呼叫
                FrameLoadCompleteEventArgs args = new FrameLoadCompleteEventArgs
                {
                    MyProperty = 1
                };
                F_Data_LoadCompleted(this, args);
            }
            else if (current_page == total_pages)
            {
                // 最後一頁
                // 處理index
                current_page = total_pages = 0;
                // 轉軌
                fm.FrameLoadComplete -= F_Pager_LoadCompleted;
                fm.FrameLoadComplete -= F_Data_LoadCompleted;
                fm.FrameLoadComplete += F_Data_LoadCompleted;

                // 下一個tab
                current_op = QueueOperation.Dequeue();
                d.getElementById(current_op.TAB_ID).click();
            }
            else
            {
                current_page++;
                // 按鍵到下一頁, 此段程式到此結束
                // HOW TO ?????????????????????????????????????????
                // 如何下一頁, 可能要用invokescript
                // 按鈕機制
                foreach (IHTMLElement a in f.getElementById("ContentPlaceHolder1_pg_gvList").all)
                {
                    if (a.innerText == ">")
                    {
                        a.click();
                    }
                }
            }
        }

        #endregion LoadCompleted methods

        #region Hotkeys, Functions

        private void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            if ((e.HotKey.Key == Key.Y) && (e.HotKey.Modifiers == ModifierKeys.Control))
            {
                /// 目的: 更新雲端資料, 讀寫雲端資料
                /// 現在可以合併兩個步驟為一個步驟
                /// 想到一個複雜的方式, 不斷利用LoadCompleted
                fm.FrameLoadComplete -= F_LoadCompleted;
                fm.FrameLoadComplete += F_LoadCompleted;
                this.g.Navigate(VPN_URL);
            }
            else if ((e.HotKey.Key == Key.G) && (e.HotKey.Modifiers == ModifierKeys.Control))
            {
                /// 目的: 取消讀取雲端藥歷
                fm.FrameLoadComplete -= F_LoadCompleted;
                this.g.Navigate(DEFAULT_URL);
            }
        }

        private string MakeSure_UID(string vpnUID)
        {
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
                // 身分證字號在[7], 還要去掉括弧
                thesisUID = vs[7].Substring(1, (vs[7].Length - 2));
                thesisNAME = vs[8];
            }
            catch (Exception)
            {
                /// 杏翔沒開, 或是沒連動, 反正就是抓不到
                /// thesisUID = string.Empty;
                /// thesisNAME = string.Empty;
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
                        // 如果沒有錯誤發生
                        // 此時為第一種狀況
                        /// VPN 有, 杏翔 有, 資料庫 有 => 理想狀況取的正確UID
                    }
                    catch (Exception)
                    {
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
                            cname = thesisNAME
                        };
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
                            break;

                        case 1:
                            // passed test
                            // 這是第三種狀況(1/2)
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
                        o = string.Empty;
                        break;

                    case 1:
                        // passed test
                        // 這是第三種狀況(1/2)
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
                        o = q.ToList()[int.Parse(answer) - 1].uid;
                        break;
                }
            }
            return o;
        }

        #endregion
    }
}