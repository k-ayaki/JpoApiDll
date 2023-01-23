using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace JpoApi
{
    public class InpitRss : IDisposable
    {
        private bool disposedValue;
        public string m_szDate { get; set; }
        public DateTime m_date { get; set; }
        public int m_statusCode { get; set; }
        public string m_url { get; set; }
        public string m_rss { get; set; }
        public InpitRss(string docNumber)
        {
            m_statusCode = 0;
            m_rss = "";
            m_szDate = "";
            m_date = DateTime.Parse("1970/01/01 00:00:00");
            Match match = Regex.Match(docNumber, "^(?<year>([0-9]{4,4}))(?<number>([0-9]{6,6}))$");
            if(match.Success)
            {
                string docYear = match.Groups["year"].Value;
                string number = (Int32.Parse(match.Groups["number"].Value) - 1).ToString("D6");
                m_url = Properties.Settings.Default.rss_url + @"/patent/" + docYear + @"/" + docYear + number.Substring(0,3) + @"001/" + docNumber + ".xml";
                using (JpoHttp jpoHttp = new JpoHttp())
                {
                    jpoHttp.get(m_url, "");
                    m_rss = jpoHttp.m_json;
                    m_statusCode = jpoHttp.m_statusCode;
                    XmlDocument xDoc = new XmlDocument();
                    if (m_rss.Length > 0)
                    {
                        xDoc.LoadXml(m_rss);
                        XmlNamespaceManager xmlNsManager = new XmlNamespaceManager(xDoc.NameTable);

                        XmlNode node_pubDate = xDoc.SelectSingleNode("//channel/pubDate", xmlNsManager);
                        if (node_pubDate != null)
                        {
                            m_szDate = date(node_pubDate);
                            if (m_szDate.Length > 0)
                            {
                                m_date = DateTime.Parse(m_szDate);
                            }
                        }
                    }
                }
            }
        }
        private string date(XmlNode node_pubDate)
        {
            string w_date = "";
            if (node_pubDate == null)
            {
                return "";
            }
            string[] dates = node_pubDate.InnerText.Split(' ');
            if (dates.Length > 4)
            {
                string month = "";
                switch (dates[2])
                {
                    case "Jan":
                        month = "1";
                        break;
                    case "Feb":
                        month = "2";
                        break;
                    case "Mar":
                        month = "3";
                        break;
                    case "Apr":
                        month = "4";
                        break;
                    case "May":
                        month = "5";
                        break;
                    case "Jun":
                        month = "6";
                        break;
                    case "Aug":
                        month = "8";
                        break;
                    case "Sep":
                        month = "9";
                        break;
                    case "Oct":
                        month = "10";
                        break;
                    case "Nov":
                        month = "11";
                        break;
                    case "Dec":
                        month = "12";
                        break;
                    default:
                        month = "1";
                        break;
                }
                string year = dates[3];
                w_date = dates[3] + "/" + month + "/" + dates[1] + " 00:00:00";
            }
            return w_date;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~inpitRss()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
