using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JpoApi.Description;
using System.Xml.Serialization;
using System.Xml;
using static JpoApi.Claims;
using System.IO;
using static JpoApi.Abstract;
using static JpoApi.Drawings;

namespace JpoApi
{
    public class ApplicationBody : IDisposable
    {
        private bool disposedValue;

        [XmlRoot("application-body")]
        public class CApplicationBody
        {
            [XmlElement("description")]
            public CDescription description { get; set; }

            [XmlElement("claims")]
            public CClaims claims { get; set; }

            [XmlElement("abstract")]
            public CAbstract cabstract { get; set; }

            [XmlElement("drawings")]
            public CDrawings drawings { get; set; }

            public string _m_xml { get; set; }
            public string m_xml
            {
                get
                {
                    return _m_xml;
                }
                set
                {
                    string _m_xml = value;
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(_m_xml);
                    _pTag = doc.DocumentElement; // <p>タグを取得
                }
            }

            public XmlNode _pTag { get; set; }

            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    foreach (XmlNode child in _pTag.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            // タグの場合、タグ名を出力
                            switch (child.LocalName)
                            {
                                case "description":
                                    this.description.m_xml = child.OuterXml;
                                    wlines += this.description.line;
                                    break;
                                case "claims":
                                    //this.claims.m_xml = child.OuterXml;
                                    wlines += this.claims.line;
                                    break;
                                case "abstract":
                                    wlines += this.cabstract.line;
                                    break;
                                case "drawings":
                                    wlines += this.drawings.line;
                                    break;
                            }
                        }
                    }
                    return wlines;
                }
            }
        }
        public CApplicationBody m_applicationBody { get; set; }
        public string m_xmlPath { get; set; }
        public string m_title { get; set; }
        public ApplicationBody(string szXml, string szXmlPath, string aLegalDate = "")
        {
            try
            {
                this.m_xmlPath = szXmlPath;
                this.m_applicationBody = null;
                this.m_title = "提出日" + aLegalDate + "_特許請求の範囲";
                XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(CApplicationBody));
                using (TextReader reader = new StringReader(szXml))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    //settings.CheckCharacters = false;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreComments = true;
                    settings.DtdProcessing = DtdProcessing.Parse;

                    XmlReader xmlReader = XmlReader.Create(reader, settings);
                    m_applicationBody = (CApplicationBody)serializer.Deserialize(xmlReader);
                    m_applicationBody.m_xml = szXml;
                    return;
                }
            }
            catch (Exception ex)
            {
                this.m_applicationBody = null;
            }
        }
        public string htmlAll()
        {
            try
            {
                Text2html text2html = new Text2html(this.m_xmlPath);
                if (this.m_applicationBody != null)
                {
                    text2html.setTitle(this.m_title);
                    if (this.m_applicationBody.line != null)
                    {
                        text2html.addP(this.m_applicationBody.line);
                    }
                }
                return text2html.htmlAll();
            }
            catch (Exception ex)
            {
                return "";
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
        // ~ApplicationBody()
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
