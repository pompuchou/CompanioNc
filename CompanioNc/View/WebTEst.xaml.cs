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

        public WebTEst(MainWindow mw)
        {
            // 把Caller傳遞過來
            m = mw;
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
            // Handle hotkey presses.
            hotKeyManager.KeyPressed += HotKeyManagerPressed;
        }

        private void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            if ((e.HotKey.Key == Key.Y) && (e.HotKey.Modifiers == ModifierKeys.Control))
            {
                // 更新雲端資料
                // 想到一個複雜的方式, 不斷利用LoadCompleted
                this.g.LoadCompleted -= G_LoadCompleted;
                this.g.LoadCompleted += G_LoadCompleted;
                this.g.Navigate(VPN_URL);
            }
            if ((e.HotKey.Key == Key.G) && (e.HotKey.Modifiers == ModifierKeys.Control))
            {
                //// 目的: 讀寫雲端資料
                //HTMLDocument d;
                //string temp_uid;

                //// 第一步找到身分證字號
                //d = (HTMLDocument)g.Document;
                //// 如果是空值就離開
                //if (d.getElementById("ContentPlaceHolder1_lbluserID") == null)
                //{
                //    // 無法抓取身分證字號
                //    tb.ShowBalloonTip("讀卡失敗", "無法抓取", BalloonIcon.Error);
                //    return;
                //}
                //else
                //{
                //    temp_uid = d.getElementById("ContentPlaceHolder1_lbluserID").innerText;
                //}

                //// 第二步找到正確的身分證號
                //// 新病人一定要連動, 不然不做了
                //System.Windows.Forms.MessageBox.Show($"ID: {temp_uid}");
            }
        }

        private void G_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            HTMLDocument d;
            string temp_uid, strUID;
            // 會有兩次
            // 第一次讀完檔案會再執行javascript
            // 第二次

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
                        tb.ShowBalloonTip("醫療系統資料庫查無此人", $"請與杏翔系統連動, 或放棄操作", BalloonIcon.Warning);
                        return;
                    }
                    else
                    {
                        // show balloon with built-in icon
                        tb.ShowBalloonTip("讀卡成功", $"身分證號: {strUID}", BalloonIcon.Info);
                        this.g.LoadCompleted -= G_LoadCompleted;
                    }
                    // 讀取特殊註記, 如果有的話

                    // 讀取提醒, 如果有的話

                    // 讀取所有要讀的tab
                }
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