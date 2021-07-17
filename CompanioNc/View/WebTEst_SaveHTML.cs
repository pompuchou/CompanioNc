using System;
using System.Windows;

namespace CompanioNc.View
{
    public partial class WebTEst : Window
    {
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

    }
}
