using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JpoApi
{
    public class Xmlfolder2Html
    {
        public string m_dirName { get; set; }
        public string m_outdirName { get; set; }
        public string m_xmlPath { get; set; }
        public Xmlfolder2Html(string xmlPath)
        {

            try
            {
                if (xmlPath.Substring(0, 1) == ".")
                {
                    m_dirName = Directory.GetCurrentDirectory();
                    Uri u1 = new Uri(m_dirName + @"\");
                    Uri u2 = new Uri(u1, xmlPath);
                    m_xmlPath = u2.LocalPath;   // 絶対パス
                }
                else
                {
                    m_dirName = Path.GetDirectoryName(xmlPath);
                    m_xmlPath = xmlPath;
                }
                Uri u3 = new Uri(m_xmlPath + @"\..");
                m_outdirName = u3.LocalPath;   // １つ上の絶対パス
                //IEnumerable<string> files = Directory.EnumerateFiles(
                //        m_xmlPath, "*.xml", SearchOption.AllDirectories);

                Xml2Html xml2Html;
                Html2Word html2Word;

                string w_Date = "";
                string w_DocNumber = "";
                string wordPath = "";

                xml2Html = new Xml2Html(xmlPath);
                if (xml2Html.m_title != null)
                {
                    if (xml2Html.m_Date != null)
                    {
                        w_Date = xml2Html.m_Date;
                    }
                    if (xml2Html.m_DocNumber != null)
                    {
                        w_DocNumber = xml2Html.m_DocNumber;
                        wordPath = m_outdirName + Path.GetFileNameWithoutExtension(xml2Html.m_title) + ".docx";
                    }
                    else
                    {
                        wordPath = m_outdirName + "特願" + w_DocNumber + "_起案日" + w_Date + "_" + xml2Html.m_DocumentName + ".docx";
                    }
                    html2Word = new Html2Word(xml2Html.m_htmlName, wordPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
