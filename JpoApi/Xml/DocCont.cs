using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Office.Interop.Word;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.VisualBasic;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Web.Caching;
using System.Security;
using System.Xml.Linq;
using System.Web;
using System.Text.RegularExpressions;
using static JpoApi.ApplicationBody;

namespace JpoApi
{
    public class DocCont : IDisposable
    {
        private bool disposedValue;
        public int m_error { get; set; }
        public readonly int e_NONE = 0x00000000;
        public readonly int e_NETWORK = 0x00000001;
        public readonly int e_SERVER = 0x00000002;
        public readonly int e_TIMEOVER = 0x00000004;
        public readonly int e_CONTENT = 0x00000008;
        public readonly int e_ZIPFILE = 0x00000010;
        public readonly int e_CACHE = 0x00000020;
        private string m_xmlPath { get; set; }
        public string m_htmlPath { get; set; }
        public string m_dirName { get; set; }
        public string m_legalDate { get; set; }      // 提出日・起案日
        public string m_DocNumber { get; set; } // 出願番号
        public string m_DocumentName { get; set; }  // 文書名
        public string m_DocNumber2 { get; set; } // 出願番号 （外部指定）
        public string m_title { get; set; }     // htmlのタイトル

        //private string[] m_dirNames;
        private XmlDocument m_xDoc { get; set; }
        private XmlNamespaceManager m_xmlNsManager { get; set; }
        public string m_provisions { get; set; }
        public string m_xml { get; set; }
        public DocCont(string a_xmlPath, string a_DocNumber, string aLegalDate = "") 
        {
            try
            {
                this.m_legalDate = aLegalDate;
                this.m_error = e_NONE;
                this.m_dirName = System.IO.Path.GetDirectoryName(a_xmlPath);
                //this.m_dirNames = a_xmlPath.Split('\\');
                this.m_htmlPath = this.m_dirName + @"\" + Path.GetFileNameWithoutExtension(a_xmlPath) + ".html";
                this.m_xmlPath = a_xmlPath;
                this.m_DocNumber2 = a_DocNumber;
                this.m_provisions = "";
                this.m_legalDate = aLegalDate;

                this.m_xml = File.ReadAllText(this.m_xmlPath, Encoding.GetEncoding("shift_jis"));
                PatRspns patrspns = new PatRspns(this.m_xml,this.m_xmlPath, this.m_legalDate);
                if (patrspns.m_patRepns != null)
                {
                    string wHtmlbody = patrspns.htmlAll();
                    if (System.IO.File.Exists(this.m_htmlPath))
                    {
                        if (System.IO.File.GetLastWriteTime(m_htmlPath) == System.IO.File.GetLastWriteTime(m_xmlPath))
                        {
                            return;
                        }
                        System.IO.File.Delete(this.m_htmlPath);
                    }
                    System.IO.File.WriteAllText(this.m_htmlPath, wHtmlbody, Encoding.GetEncoding("shift_jis"));
                    if (System.IO.File.Exists(this.m_htmlPath))
                    {
                        System.IO.File.SetLastWriteTime(this.m_htmlPath, System.IO.File.GetLastWriteTime(this.m_xmlPath));
                        System.IO.File.SetCreationTime(this.m_htmlPath, System.IO.File.GetCreationTime(this.m_xmlPath));
                        System.IO.File.SetLastAccessTime(this.m_htmlPath, System.IO.File.GetLastAccessTime(this.m_xmlPath));
                    }
                    else
                    {
                        this.m_error = this.e_CACHE;
                    }
                }
                PatAmnd patamnd = new PatAmnd(this.m_xml, this.m_xmlPath, this.m_legalDate);
                if (patamnd.m_patAmnd != null)
                {
                    string wHtmlbody = patamnd.htmlAll();
                    if (System.IO.File.Exists(this.m_htmlPath))
                    {
                        if (System.IO.File.GetLastWriteTime(m_htmlPath) == System.IO.File.GetLastWriteTime(m_xmlPath))
                        {
                            return;
                        }
                        System.IO.File.Delete(this.m_htmlPath);
                    }
                    System.IO.File.WriteAllText(this.m_htmlPath, wHtmlbody, Encoding.GetEncoding("shift_jis"));
                    if (System.IO.File.Exists(this.m_htmlPath))
                    {
                        System.IO.File.SetLastWriteTime(this.m_htmlPath, System.IO.File.GetLastWriteTime(this.m_xmlPath));
                        System.IO.File.SetCreationTime(this.m_htmlPath, System.IO.File.GetCreationTime(this.m_xmlPath));
                        System.IO.File.SetLastAccessTime(this.m_htmlPath, System.IO.File.GetLastAccessTime(this.m_xmlPath));
                    }
                    else
                    {
                        this.m_error = this.e_CACHE;
                    }
                }
                ApplicationBody applicationBody = new ApplicationBody(this.m_xml, this.m_xmlPath, this.m_legalDate);
                if (applicationBody.m_applicationBody != null)
                {
                    string wHtmlbody = applicationBody.htmlAll();
                    if (System.IO.File.Exists(this.m_htmlPath))
                    {
                        if (System.IO.File.GetLastWriteTime(m_htmlPath) == System.IO.File.GetLastWriteTime(m_xmlPath))
                        {
                            return;
                        }
                        System.IO.File.Delete(this.m_htmlPath);
                    }
                    System.IO.File.WriteAllText(this.m_htmlPath, wHtmlbody, Encoding.GetEncoding("shift_jis"));
                    if (System.IO.File.Exists(this.m_htmlPath))
                    {
                        System.IO.File.SetLastWriteTime(this.m_htmlPath, System.IO.File.GetLastWriteTime(this.m_xmlPath));
                        System.IO.File.SetCreationTime(this.m_htmlPath, System.IO.File.GetCreationTime(this.m_xmlPath));
                        System.IO.File.SetLastAccessTime(this.m_htmlPath, System.IO.File.GetLastAccessTime(this.m_xmlPath));
                    }
                    else
                    {
                        this.m_error = this.e_CACHE;
                    }
                }
                PatAppDoc patAppDoc = new PatAppDoc(this.m_xml, this.m_xmlPath, this.m_legalDate);
                if (patAppDoc.m_patAppDoc != null)
                {
                    string wHtmlbody = patAppDoc.htmlAll();
                    if (System.IO.File.Exists(this.m_htmlPath))
                    {
                        if (System.IO.File.GetLastWriteTime(m_htmlPath) == System.IO.File.GetLastWriteTime(m_xmlPath))
                        {
                            return;
                        }
                        System.IO.File.Delete(this.m_htmlPath);
                    }
                    System.IO.File.WriteAllText(this.m_htmlPath, wHtmlbody, Encoding.GetEncoding("shift_jis"));
                    if (System.IO.File.Exists(this.m_htmlPath))
                    {
                        System.IO.File.SetLastWriteTime(this.m_htmlPath, System.IO.File.GetLastWriteTime(this.m_xmlPath));
                        System.IO.File.SetCreationTime(this.m_htmlPath, System.IO.File.GetCreationTime(this.m_xmlPath));
                        System.IO.File.SetLastAccessTime(this.m_htmlPath, System.IO.File.GetLastAccessTime(this.m_xmlPath));
                    }
                    else
                    {
                        this.m_error = this.e_CACHE;
                    }
                }

            }
            catch (Exception ex)
            {
                this.m_error = this.e_CACHE;
                return;
            }
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
        // ~DocCont()
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
