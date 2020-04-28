using CompanioNc.Models;
using GlobalHotKey;
using Hardcodet.Wpf.TaskbarNotification;
using mshtml;
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Globalization;
using Microsoft.VisualBasic;
using System.Windows.Navigation;

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
        private HotKeyManager hotKeyManager;
        private const string VPN_URL = @"https://medcloud.nhi.gov.tw/imme0008/IMME0008S01.aspx";

        private TaskbarIcon tb = new TaskbarIcon();

        private readonly MainWindow m;
        private FrameMonitor f;

        public WebTEst(MainWindow mw)
        {
            // 把Caller傳遞過來
            m = mw;
            f = new FrameMonitor(this, 100);
            InitializeComponent();
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
            m.VPNwindow.IsChecked = false;
            f.Dispose();
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
            this.g.Navigate(VPN_URL);
            // Handle hotkey presses.
            hotKeyManager.KeyPressed += HotKeyManagerPressed;
        }

        private void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            if ((e.HotKey.Key == Key.Y) && (e.HotKey.Modifiers == ModifierKeys.Control))
            {
                /// 目的: 更新雲端資料, 讀寫雲端資料
                /// 現在可以合併兩個步驟為一個步驟
                /// 想到一個複雜的方式, 不斷利用LoadCompleted
                f.FrameLoadComplete -= F_LoadCompleted;
                f.FrameLoadComplete += F_LoadCompleted;
                this.g.Navigate(VPN_URL);
            }
        }

        #region LoadCompleted methods
        private void F_LoadCompleted(object sender, FrameLoadCompleteEventArgs e)
        {
            HTMLDocument d;
            string temp_uid, strUID;
            /// 會有兩次
            /// 第一次讀完檔案會再執行javascript, 在local用來認證健保卡, 沒過就不會有第二次
            /// 第二次如果認證OK, 會再透過javascript下載個案雲端資料

            d = (HTMLDocument)g.Document;
            if (d != null)
            {
                // System.Windows.Forms.MessageBox.Show(d.documentElement.outerHTML);
                // documentElement是整個html;
                // 如果是空值就離開
                if (d.getElementById("ContentPlaceHolder1_lblmsg") != null)
                {
                    // 1. 卡片讀取中
                    // 無法抓取身分證字號
                    temp_uid = d.getElementById("ContentPlaceHolder1_lblmsg").innerText;
                    if (temp_uid == "卡片讀取中")
                    {
                        /// Do nothing, 這是第一次完成
                        /// 兩種情況, 如果插入正確健保卡, javascript會驅動再去讀資料, 這樣會觸發第二次完成
                        /// 第二種情況, 沒有健保卡, javascript雖然會去改動ContentPlaceHolder1_lblmsg
                        /// 但因為沒有再去讀資料, 也就不會觸發第二次完成
                        /// 所以就沒有機會this.g.LoadCompleted -= G_LoadCompleted;
                        /// 這樣會造成一直加上去, 有多個LoadCompleted
                        return;
                    }
                }
                else if (d.getElementById("ContentPlaceHolder1_lbluserID") != null)
                {
                    temp_uid = d.getElementById("ContentPlaceHolder1_lbluserID").innerText;
                    // 確定身分證字號
                    strUID = MakeSure_UID(temp_uid);
                    // if (strUID = string.Empty) 離開
                    if (strUID == string.Empty)
                    {
                        tb.ShowBalloonTip("醫療系統資料庫查無此人", "請與杏翔系統連動, 或放棄操作", BalloonIcon.Warning);
                        return;
                    }
                    else
                    {
                        // show balloon with built-in icon
                        tb.ShowBalloonTip("讀卡成功", $"身分證號: {strUID}", BalloonIcon.Info);
                        f.FrameLoadComplete -= F_LoadCompleted;
                        f.FrameLoadComplete -= F_Data_LoadCompleted;
                        f.FrameLoadComplete += F_Data_LoadCompleted;
                        //this.g.LoadCompleted -= G_LoadCompleted;
                        //this.g.LoadCompleted -= Data_LoadCompleted;
                        //this.g.LoadCompleted += Data_LoadCompleted;
                        /// 在外面的這個frame, 在下一步之前, 除了確定身分證字號外還有三件事情要做:
                        /// 1. 讀取特殊註記, 如果有的話
                        /// 2. 讀取提醒, 如果有的話
                        /// 3. 讀取所有要讀的tab

                        /// 1. 讀取特殊註記, 如果有的話
                        ///    這是在ContentPlaceHolder1_tab02
                        ///    是個table

                        /// 2. 讀取提醒, 如果有的話
                        ///    這是在ContentPlaceHolder1_GV
                        ///    也是個table

                        /// 3. 讀取所有要讀的tab
                        ///    這是在ContentPlaceHolder1_tab
                        ///    是個list
                        ///    ContentPlaceHolder1_a_0008 是雲端藥歷
                        ///    ContentPlaceHolder1_a_0009 是特定管制藥品用藥資訊
                        ///    ContentPlaceHolder1_a_0060 是檢查檢驗結果
                        ///    ContentPlaceHolder1_a_0020 是手術明細紀錄
                        ///    ContentPlaceHolder1_a_0070 是出院病歷摘要
                        ///    ContentPlaceHolder1_a_0080 是復健醫療
                        ///    ContentPlaceHolder1_a_0030 是牙科處置及手術
                        ///    ContentPlaceHolder1_a_0090 是中醫用藥
                        ///    ContentPlaceHolder1_a_0040 是過敏藥
                        ///    我想應該放在一個list
                        /// 每當按一次鍵就會觸發一次LoadCompleted
                        /// 但是下一頁不會觸發LoadCompleted
                        /// 如何使用遞歸方式?
                        /// 各頁應該使用sequential方式讀取
                        /// Parsing及寫入資料庫則可以用async await技術
                        /// 用index來看處理的位置
                        /// 分工:
                        /// 1. 準備: 初始化, 欄位基礎資料/位置, 可在windows生成時就完成
                        /// 2. 流程控制: LoadCompleted, 遞歸, iterator, 每按一次tab就會回到從頭, 
                        ///     2-1. 取得UID  -done
                        ///     2-2. 取得LIST<ToDo>
                        ///     2-3. 判斷位置, index property
                        ///     2-4. 判斷現在該做什麼, by index and LIST<ToDo>
                        /// 3. 網頁操弄與擷取資料: sequential
                        ///     3-1. 判斷分頁, 有幾頁, 現在在第幾頁, 換頁不會觸發LoadCompleted; 疑問會不會來不及?
                        ///     3-2. 要先排序, 排序也不會觸發LoadCompleted; 疑問會不會來不及?
                        ///     3-2. 都放在記憶體裡, 快速, in the LIST
                        /// 4. Parsing & Saving to SQL: async
                        ///     4-1. 多工同時處理, 快速
                        ///     4-2. 依照欄位資料/位置 Parsing
                        ///     4-3. 存入SQL
                        ///     4-4. 製作Query
                        /// 查核機制?
                    }
                }
            }
        }

        private void F_Data_LoadCompleted(object sender, FrameLoadCompleteEventArgs e)
        {
            IHTMLElement htmlgvList;

            // d 是parent HTML document
            HTMLDocument d = (HTMLDocument)g.Document;
            // f 是frame(0) HTML document
            HTMLDocument f = d.frames.item(0).document.body.document;

            /// 20200426 我竟然神奇地找到了新的路徑
            /// 新舊比較
            /// 新: htmlgvList = d.frames.item(0).document.body.document.getElementById("ContentPlaceHolder1_gvList");
            /// 舊: htmlgvList = d.Window.Frames(0).Document.GetElementById("ContentPlaceHolder1_gvList")
            htmlgvList = f.getElementById("ContentPlaceHolder1_gvList");

            tb.ShowBalloonTip("Hi", "Hello!", BalloonIcon.None);
        }
        #endregion
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
                // 第一, 第二種狀況
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
                            qu += $"{i+1}. {q.ToList()[i].uid} {q.ToList()[i].cname} \r\n";
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
                            int result;
                            if (!(int.TryParse(answer, out result))) answer = "0";
                        }
                        o = q.ToList()[int.Parse(answer)-1].uid;
                        break;
                }
            }
            return o;
        }
    }
}