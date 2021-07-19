using System;
using System.Linq;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
        public static VPN_Operation Making_new_operation(string tab_id, string uid, DateTime qdate)
        {
            VPN_Operation o = (from p in Operation_Dictionary
                               where p.TAB_ID == tab_id
                               select p).Single();
            o.UID = uid;
            o.QDate = qdate;
            return o;
        }
    }
}
