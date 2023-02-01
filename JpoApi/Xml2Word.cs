using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string m_DocumentName { get; set; }

        public Xml2Word(string xmlFilePath, string docNumber, string wordPath, double arMargin, double alMargin, double abMargin, double atMargin)
        {

            try
            {
                m_error = e_NONE;
                m_DocumentName = "";
                m_wordFilePath = "";
                if (wordPath == null)
                {
                    wordPath = System.IO.Directory.GetCurrentDirectory();
                }
                Xml2Html xml2Html = new Xml2Html(xmlFilePath, docNumber);
                m_error = xml2Html.m_error;

                if (xml2Html.m_error == xml2Html.e_NONE)
                {
                    if (xml2Html.m_title != null)
                    {
                        m_DocumentName = xml2Html.m_DocumentName;
                        m_wordFilePath = wordPath + @"\" + Path.GetFileNameWithoutExtension(xml2Html.m_title) + @".docx";
                        Html2Word html2Word = new Html2Word(xml2Html.m_htmlPath, m_wordFilePath, arMargin, alMargin, abMargin, atMargin);
                        if (html2Word.m_error != html2Word.e_NONE)
                        {
                            m_error = html2Word.m_error;
                            m_wordFilePath = "";
                        }
                    }
                    else
                    {
                        m_error = e_CACHE;
                    }
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
