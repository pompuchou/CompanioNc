using CompanioNc.Models;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CompanioNc.ViewModels
{
    public class Autocomplete_ViewModel : INotifyPropertyChanged
    {
        private List<string> _WaitMessage = new List<string>() { "Please Wait..." };
        public IEnumerable WaitMessage { get { return _WaitMessage; } }

        private string _QueryText;
        public string QueryText
        {
            get { return _QueryText; }
            set
            {
                if (_QueryText != value)
                {
                    _QueryText = value;
                    OnPropertyChanged("QueryText");
                    _QueryCollection = null;
                    OnPropertyChanged("QueryCollection");
                    Debug.Print("QueryText: " + value);
                }
            }
        }

        public IEnumerable _QueryCollection = null;
        public IEnumerable QueryCollection
        {
            get
            {
                Debug.Print("---" + _QueryCollection);
                QueryGoogle(QueryText);
                return _QueryCollection;
            }
        }

        private void QueryGoogle(string SearchTerm)
        {
            //Debug.Print("Query: " + SearchTerm);
            //string sanitized = HttpUtility.HtmlEncode(SearchTerm);
            //string url = @"http://google.com/complete/search?output=toolbar&q=" + sanitized;
            //WebRequest httpWebRequest = HttpWebRequest.Create(url);
            //var webResponse = httpWebRequest.GetResponse();
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.Load(webResponse.GetResponseStream());
            //var result = xmlDoc.SelectNodes("//CompleteSuggestion");
            //_QueryCollection = result;

            Com_clDataContext dc = new Com_clDataContext();
            XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement("Toplevel",
                from p in dc.sp_finduid_by_info(SearchTerm)
                select new XElement("CompleteSuggestion",
               new XElement("suggestion", new XAttribute("uid", p.uid), new XAttribute("cname", p.cname), 
               new XAttribute("key", p.key)))));
            //XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement("Toplevel", from p in dc.sp_finduid_by_info(SearchTerm)
            //                                                                                                select new XElement("CompleteSuggestion",
            //                                                                                               new XElement("suggestion", new XAttribute("data", p.cname)))));
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(doc.Document.ToString());
            var result = xmlDoc.SelectNodes("//CompleteSuggestion");
            //var result = doc.XPathSelectElements("//suggestion");
            _QueryCollection = result;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
