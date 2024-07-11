using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Runtime.Serialization;
using System.Linq;
using System.Xml.Linq;
using OpenXmlPowerTools;


namespace JpoApi
{
    public class DocContOA : IDisposable
    {
        private bool disposedValue;
        public DocContOA(string szXml)
        {
            //XElement xml = XElement.Parse(szXml);
            /*
            //メンバー情報のタグ内の情報を取得する
            foreach (XElement element in xml.Descendants("{http://www.jpo.go.jp}name"))
            {
                Console.WriteLine(element.Value);
            }
            */
            /*
            foreach (XElement element in xml.Descendants())
            {
                Console.WriteLine("Element Name: {0}, Value: {1}", element.Name.LocalName, element.Value);
            }
            */
            // ルート要素を選択する
            XDocument doc = XDocument.Parse(szXml);
            XElement root = doc.Root;
            Traverse(root, "");
        }
        public void Traverse(XElement element, string indent)
        {
            //Console.WriteLine("{0}Element Name: {1}, Value: {2}", indent, element.Name.LocalName, element.Value);
            switch(element.Name.LocalName)
            {
                case "pat-rspns":
                    html_pat_rspns(element);
                    break;

            }

        }
        public void html_pat_rspns(XElement element)
        {
            foreach (XElement child in element.Elements())
            {
                switch(child.Name.LocalName)
                {
                    case "response-a53":
                        html_response_a53(child);
                        break;
                }
            }
        }
        public void html_response_a53(XElement element)
        {
            foreach (XElement child in element.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "file-reference-id":
                        Console.WriteLine("【整理番号】{0}", child.Value);
                        break;
                    case "submission-date":
                        html_submission_date(child);
                        //Console.WriteLine("【出願日】{0}", child.Value);
                        break;
                }
            }
        }
        public void html_submission_date(XElement element)
        {
            foreach (XElement child in element.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "date":
                        Console.WriteLine("【出願日】{0}", child.Value);
                        break;
                }
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
        // ~patRspm2()
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
