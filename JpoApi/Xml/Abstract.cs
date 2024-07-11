using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static JpoApi.PatRspns;

namespace JpoApi
{
    public class Abstract : IDisposable
    {
        private bool disposedValue;

        // XmlRoot属性でルート要素の名前を指定
        [XmlRoot("abstract")]
        public class CAbstract
        {
            // XmlElement属性で子要素の名前を指定
            [XmlAnyElement("p")]
            public XmlElement P { get; set; }
            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    wlines += "【書類名】要約書<br />\r\n";
                    wlines += element2html(P) + "<br />\r\n";
                    return wlines;
                }
            }
        }


        public Abstract(string szXml, string szXmlPath) 
        {
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
        // ~Abstract()
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
