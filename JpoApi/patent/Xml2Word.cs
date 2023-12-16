using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace JpoApi
{
    public class Xml2Word : IDisposable
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
        public readonly int e_WORDFILE = 0x00000040;
        public string m_wordFilePath { get; set; }
        public string m_outFileName { get; set; }
        public string m_DocumentName { get; set; }
        public string m_DocNumber { get; set; } // 出願番号
        public string m_htmlFilePath { get; set; }
        public string m_title { get; set; }
        public string m_Date { get; set; }
        public string m_provisions { get; set; }
        public Xml2Word(string xmlFilePath, string docNumber, string outFilePath="", double arMargin=20.0, double alMargin=15.0, double abMargin=20.0, double atMargin=15.0)
        {

            try
            {
                this.m_error = e_NONE;
                this.m_DocumentName = "";
                this.m_DocNumber = "";
                this.m_wordFilePath = "";
                this.m_htmlFilePath = "";
                this.m_outFileName = "";
                this.m_title = "";
                this.m_Date = "";
                this.m_provisions = "";

                // html に変換
                using (Xml2Html xml2Html = new Xml2Html(xmlFilePath, docNumber))
                {
                    this.m_error = xml2Html.m_error;

                    if (xml2Html.m_error == xml2Html.e_NONE)
                    {
                        this.m_htmlFilePath = xml2Html.m_htmlPath;

                        if (xml2Html.m_title != null)
                        {
                            this.m_title = xml2Html.m_title;

                            this.m_DocumentName = xml2Html.m_DocumentName;
                            this.m_Date = xml2Html.m_Date;
                            this.m_DocNumber = xml2Html.m_DocNumber;
                            this.m_provisions = xml2Html.m_provisions;
                            Html2Word html2Word = new Html2Word(xml2Html.m_htmlPath, arMargin, alMargin, abMargin, atMargin);
                            if (html2Word.m_error != html2Word.e_NONE)
                            {
                                this.m_error = html2Word.m_error;
                                this.m_wordFilePath = "";
                            }
                            else
                            {
                                this.m_wordFilePath = html2Word.m_wordFilePath;
                                this.m_outFileName = xml2Html.m_title + ".docx";
                            }
                        }
                        else
                        {
                            this.m_error = this.e_CACHE;
                        }
                    }
                }
                if (outFilePath.Length > 0 && this.m_outFileName.Length > 0)
                {
                    if (System.IO.Directory.Exists(outFilePath) == false)
                    {
                        Directory.CreateDirectory(outFilePath);
                    }
                    System.IO.File.Copy(this.m_wordFilePath, outFilePath + "\\" + this.m_outFileName, true);
                }
            }
            catch (Exception ex)
            {
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
        // ~Xml2Word()
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
