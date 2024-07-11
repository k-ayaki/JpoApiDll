using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static JpoApi.PatRspns;

namespace JpoApi
{
    public class Claims : IDisposable
    {
        private bool disposedValue;

        public string line
        {
            get
            {
                if (this.m_claims != null)
                {
                    return this.m_claims.line;
                }
                return null;
            }
        }

        [XmlRoot("claims")]
        public class CClaims
        {
            [XmlElement("claim")]
            //public List<XmlElement> Claims { get; set; }
            public List<CClaim> Claims { get; set; }
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
                    if (this.Claims != null
                    && this.Claims.Count > 0)
                    {
                        wlines += "【書類名】特許請求の範囲<br />\r\n";
                        foreach (CClaim claim in this.Claims)
                        {
                            wlines += claim.line;
                        }
                        return wlines;
                    }
                    return null;
                }
            }
        }

        [XmlRoot("claim")]
        public class CClaim
        {
            [XmlAttribute("num")]
            public string Num { get; set; }

            [XmlAnyElement("claim-text")]
            public XmlElement element { get; set; }

            public string line
            {
                get
                {
                    if (this.element != null)
                    {
                        return "【請求項" + Strings.StrConv(this.Num, VbStrConv.Wide, 0x411) + "】<br />\r\n"
                               + element2html(this.element) + "<br />\r\n";
                    }
                    return null;
                }
            }
        }
        public CClaims m_claims { get; set; }
        public CClaim m_claim { get; set; }
        public string m_xmlPath { get; set; }
        public Claims(string szXml, string szXmlPath)
        {
            try
            {
                this.m_xmlPath = szXmlPath;
                this.m_claims = null;
                XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(CClaims));
                using (TextReader reader = new StringReader(szXml))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    //settings.CheckCharacters = false;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreComments = true;
                    settings.DtdProcessing = DtdProcessing.Parse;

                    XmlReader xmlReader = XmlReader.Create(reader, settings);
                    m_claims = (CClaims)serializer.Deserialize(xmlReader);
                    return;
                }
            }
            catch (Exception ex)
            {
                this.m_claims = null;
            }
            try
            {
                this.m_xmlPath = szXmlPath;
                this.m_claim = null;
                XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(CClaim));
                using (TextReader reader = new StringReader(szXml))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    //settings.CheckCharacters = false;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreComments = true;
                    settings.DtdProcessing = DtdProcessing.Parse;

                    XmlReader xmlReader = XmlReader.Create(reader, settings);
                    m_claim = (CClaim)serializer.Deserialize(xmlReader);
                    return;
                }
            }
            catch (Exception ex)
            {
                this.m_claims = null;
            }
        }

        public string htmlAll()
        {
            try
            {
                Text2html text2html = new Text2html(this.m_xmlPath);
                if (this.m_claims != null)
                {
                    text2html.setTitle("特許請求の範囲：タイトルです");
                    if (this.m_claims.line != null)
                    {
                        text2html.addP(this.m_claims.line);
                    }
                }
                return text2html.htmlAll();
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public string htmlAll2()
        {
            try
            {
                Text2html text2html = new Text2html(this.m_xmlPath);
                if (this.m_claims != null)
                {
                    text2html.setTitle("特許請求の範囲：タイトルです");
                    text2html.addP("【書類名】特許請求の範囲");
                    if (this.m_claims.Claims.Count > 0)
                    {
                        foreach(CClaim claim in this.m_claims.Claims)
                        {
                            text2html.addP("【請求項" + Strings.StrConv(claim.Num, VbStrConv.Wide, 0x411) + "】");
                            //text2html.addInnerXml(claim.ClaimNode.InnerXml);
                            text2html.addElement(claim.element);
                        }
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
        // ~Claims()
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
