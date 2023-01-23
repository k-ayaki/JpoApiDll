using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpoApi
{
    public class ConvertXml2Word : IDisposable
    {
        private bool disposedValue;
        public string m_wordFilePath { get; set; }  // 
        public string m_DocumentName { get; set; }  // 

        private ConvertWord m_convertWord { get; set; }

        public ConvertXml2Word()
        {
            m_convertWord = new ConvertWord();
        }

        public void DoConvert(string xmlFilePath, string docNumber = null, string wordPath = null)
        {
            try
            {
                m_DocumentName = "";
                m_wordFilePath = "";
                if (wordPath == null)
                {
                    wordPath = System.IO.Directory.GetCurrentDirectory();
                }
                Xml2Html xml2Html = new Xml2Html(xmlFilePath, docNumber);
                if (xml2Html.m_error == xml2Html.e_NONE
                && xml2Html.m_title != null)
                {
                    m_DocumentName = xml2Html.m_DocumentName;
                    m_wordFilePath = wordPath + @"\" + Path.GetFileNameWithoutExtension(xml2Html.m_title) + @".docx";
                    m_convertWord.Html2Word(xml2Html.m_htmlPath, m_wordFilePath);
                    if (File.Exists(m_wordFilePath) == false)
                    {
                        m_wordFilePath = "";
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        ~ConvertXml2Word()
        {
            m_convertWord.Dispose();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    m_convertWord.Dispose();

                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~ConvertHtml()
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
