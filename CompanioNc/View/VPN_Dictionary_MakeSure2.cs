using CompanioNc.Models;
using System.Linq;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
        private static string MakeSure_source_2_lines(string temp_source)
        {
            Com_clDataContext dc = new Com_clDataContext();
            string[] s = temp_source.Replace("<br>", "|").Split('|');
            string o_source = s[1];
            var q1 = from p1 in dc.p_source
                     where p1.source_id == o_source
                     select p1;
            if (q1.Count() == 0)
            {
                p_source new_source = new p_source()
                {
                    source_id = s[1],
                    source_name = s[0]
                };
                dc.p_source.InsertOnSubmit(new_source);
                dc.SubmitChanges();
            }
            return o_source;
        }
    }
}
