using System;
using System.Windows;
using Microsoft.VisualBasic;
using System.Globalization;
using CompanioNc.Models;
using System.Linq;

namespace CompanioNc.View
{
    public partial class WebTEst : Window
    {
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
                string[] vs = s.m.strID.Content.ToString().Split(' ');
                log.Info($"    [{s.m.strID.Content}] being processed.");
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
    }
}
