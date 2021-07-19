using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompanioNc.View
{
    internal partial class VPN_Dictionary
    {
        public static async Task<List<Response_DataModel>> RunWriteSQL_Async(List<VPN_Retrieved> vrs)
        {
            log.Info($"    Enter RunWriteSQL_Async. Current ID: {vrs[0].UID}. Number of tables: {vrs.Count}");

            List<Task<Response_DataModel>> tasks = new List<Task<Response_DataModel>>();

            foreach (VPN_Retrieved vr in vrs)
            {
                log.Info($"      Task {vr.SQL_Tablename} of {vr.UID} added");
                tasks.Add(WriteSQL_Async(vr));
            }
            var output = await Task.WhenAll(tasks);

            log.Info($"    Exit RunWriteSQL_Async.Current ID: {vrs[0].UID}. Number of tables: {vrs.Count}");
            return new List<Response_DataModel>(output);
        }
    }
}
