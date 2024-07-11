using DocumentFormat.OpenXml.Bibliography;
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
using static JpoApi.PatAmnd;

namespace JpoApi
{
    public class PatAppDoc : IDisposable
    {
        private bool disposedValue;

        [XmlRoot(ElementName = "pat-app-doc", Namespace = "http://www.jpo.go.jp")]
        public class CPatAppDoc
        {
            [XmlElement("application-a63", Namespace = "http://www.jpo.go.jp")]
            public ApplicationA63 ApplicationA63 { get; set; }

            public string line
            {
                get
                {
                    if (this.ApplicationA63 != null)
                    {
                        return this.ApplicationA63.line;
                    }
                    return null;
                }
            }
        }

        public class ApplicationA63
        {
            // 【書類名】
            [XmlElement(ElementName = "document-code", Namespace = "http://www.jpo.go.jp")]
            public string DocumentCode { get; set; }

            // 【整理番号】
            [XmlElement(ElementName = "file-reference-id", Namespace = "http://www.jpo.go.jp")]
            public string FileReferenceId { get; set; }

            // 【提出日】
            [XmlElement(ElementName = "submission-date", Namespace = "http://www.jpo.go.jp")]
            public SubmissionDate SubmissionDate { get; set; }

            // 【あて先】
            [XmlElement(ElementName = "addressed-to-person", Namespace = "http://www.jpo.go.jp")]
            public string AddressedToPerson { get; set; }

            // 【国際特許分類】
            [XmlElement(ElementName = "ipc-article", Namespace = "http://www.jpo.go.jp")]
            public IpcArticle IpcArticle { get; set; }

            // 【発明者】
            [XmlElement(ElementName = "inventors", Namespace = "http://www.jpo.go.jp")]
            public Inventors Inventors { get; set; }

            // 【特許出願人】
            [XmlElement(ElementName = "applicants", Namespace = "http://www.jpo.go.jp")]
            public Applicants applicants { get; set; }

            // 【代理人】
            [XmlElement(ElementName = "agents", Namespace = "http://www.jpo.go.jp")]
            public Agents agents { get; set; }

            // 【選任した代理人】
            [XmlElement(ElementName = "attorney-change-article", Namespace = "http://www.jpo.go.jp")]
            public AttorneyChangeArticle attorneyChangeArticle { get; set; }

            [XmlElement(ElementName = "charge-article")]
            public ChargeArticle ChargeArticle { get; set; }

            [XmlElement(ElementName = "submission-object-list-article")]
            public SubmissionObjectListArticle SubmissionObjectListArticle { get; set; }

            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    if (this.DocumentCode != null)
                    {
                        wlines += "【書類名】　　　　　　" + document_code2desc(this.DocumentCode) + "<br />\r\n";
                    }
                    if (this.FileReferenceId != null)
                    {
                        wlines += "【整理番号】　　　　　" + Strings.StrConv(this.FileReferenceId, VbStrConv.Wide, 0x411) + "<br />\r\n";
                    }
                    if (this.SubmissionDate != null)
                    {
                        wlines += this.SubmissionDate.line;
                    }
                    if (this.AddressedToPerson != null)
                    {
                        wlines += "【あて先】　　　　　　" + this.AddressedToPerson + "<br />\r\n";
                    }
                    if (this.IpcArticle != null)
                    {
                        wlines += this.IpcArticle.line;
                    }
                    if (this.Inventors != null)
                    {
                        wlines += this.Inventors.line;
                    }
                    if (this.applicants != null)
                    {
                        wlines += this.applicants.line;
                    }
                    if (this.agents != null)
                    {
                        wlines += this.agents.line;
                    }
                    if (this.attorneyChangeArticle != null)
                    {
                        wlines += this.attorneyChangeArticle.line;
                    }
                    if (this.ChargeArticle != null)
                    {
                        wlines += this.ChargeArticle.line;
                    }
                    if (this.SubmissionObjectListArticle != null)
                    {
                        wlines += this.SubmissionObjectListArticle.line;
                    }
                    return wlines;
                }
            }
        }
        public class IpcArticle
        {
            [XmlElement("ipc", Namespace = "http://www.jpo.go.jp")]
            public List<string> Ipc { get; set; }

            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    string elementTitle = "【国際特許分類】　　　";
                    foreach (string wIpc in this.Ipc)
                    {
                        string wIpc2 = wIpc.Replace('\u00A0', '　');
                        wlines += elementTitle + Strings.StrConv(wIpc2, VbStrConv.Wide, 0x411) + "<br />\r\n";
                        elementTitle = "　　　　　　　　　　　";
                    }
                    return wlines;
                }
            }
        }

        // 【発明者】（複数）
        public class Inventors
        {
            [XmlElement(ElementName = "inventor")]
            public List<Inventor> inventor { get; set; }
            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    foreach (Inventor wInventor in this.inventor)
                    {
                        wlines += "【発明者】<br />\r\n";
                        wlines += wInventor.line;
                    }
                    return wlines;
                }
            }
        }

        // 【発明者】
        public class Inventor
        {
            [XmlElement(ElementName = "addressbook")]
            public Addressbook addressbook { get; set; }
            public string line
            {
                get
                {
                    return addressbook.line;
                }
            }
        }

        // 【選任した代理人】（複数）
        public class AttorneyChangeArticle
        {
            [XmlElement(ElementName = "agent")]
            public List<Agent> Agent { get; set; }

            public string _JpElementName { get; set; }
            public string JpElementName
            {
                set
                {
                    foreach (Agent wAgent in this.Agent)
                    {
                        _JpElementName = value;
                    }
                }
            }
            public string _m_xml { get; set; }
            public string m_xml
            {
                get { return _m_xml; }
                set
                {
                    string _m_xml = value;
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(_m_xml);
                    _pTag = doc.DocumentElement; // <p>タグを取得
                    int i = 0;
                    foreach (XmlNode child in _pTag.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            switch (child.LocalName)
                            {
                                case "applicant":  // 【事件の表示】
                                    this.Agent[i].m_xml = child.OuterXml;
                                    i++;
                                    break;
                            }
                        }
                    }
                }
            }
            public XmlNode _pTag { get; set; }
            public string line
            {
                get
                {
                    string wlines = "";
                    if (this.Agent != null
                    && this.Agent.Count > 0)
                    {
                        foreach (Agent wAgent in this.Agent)
                        {
                            wlines += "【選任した代理人】<br />\r\n";
                            wlines += wAgent.line;
                        }
                    }
                    return wlines;
                }
            }
        }

        // 【特許出願人】
        /*
        public class Applicant
        {
            [XmlElement(ElementName = "addressbook")]   // 氏名及び住所情報
            public Addressbook Addressbook { get; set; }

            [XmlElement(ElementName = "share")]  // 持分
            public string Share { get; set; }
            public string _JpElementName { get; set; }
            public string JpElementName
            {
                get { return _JpElementName; }
                set { this._JpElementName = value; }
            }
            public string _m_xml { get; set; }
            public string m_xml
            {
                get { return _m_xml; }
                set
                {
                    string _m_xml = value;
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(_m_xml);
                    _pTag = doc.DocumentElement; // <p>タグを取得
                    foreach (XmlNode child in _pTag.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            switch (child.LocalName)
                            {
                                case "applicant":  // 【事件の表示】
                                    this.Addressbook.m_xml = child.OuterXml;
                                    break;
                            }
                        }
                    }
                }
            }
            public XmlNode _pTag { get; set; }
            public string line
            {
                get
                {
                    string wlines = "";
                    if (this.Addressbook != null)
                    {
                        if (this.JpElementName == null)
                        {
                            wlines += "【特許出願人】<br />\r\n";
                        }
                        else
                        {
                            wlines += JpElementName + "<br />\r\n";
                        }

                        if (this.Addressbook.RegisteredNumber != null)
                        {
                            wlines += "　　【識別番号】　　　" + Strings.StrConv(this.Addressbook.RegisteredNumber, VbStrConv.Wide, 0x411) + "<br />\r\n";
                        }
                        if (this.Addressbook.kana != null)
                        {
                            wlines += "　　【フリガナ】　　　" + this.Addressbook.kana + "<br />\r\n";
                        }
                        if (this.Addressbook.Name != null)
                        {
                            wlines += "　　【氏名又は名称】　" + this.Addressbook.Name + "<br />\r\n";
                        }
                        if (this.Addressbook.OriginalLanguageOfName != null)
                        {
                            wlines += "　　【住所又は居所原語表記】" + this.Addressbook.OriginalLanguageOfName + "<br />\r\n";
                        }
                        if (this.Addressbook.Address != null)
                        {
                            wlines += "　　【住所又は居所】　" + this.Addressbook.Address + "<br />\r\n";
                        }
                        if (this.Addressbook.phone != null)
                        {
                            wlines += "　　【電話番号】　　　" + Strings.StrConv(this.Addressbook.phone, VbStrConv.Wide, 0x411) + "<br />\r\n";
                        }
                        if (this.Addressbook.fax != null)
                        {
                            wlines += "　　【ファクシミリ番号】" + Strings.StrConv(this.Addressbook.fax, VbStrConv.Wide, 0x411) + "<br />\r\n";
                        }
                        if (this.Addressbook.contact != null)
                        {
                            wlines += "　　【連絡先】　　　　" + this.Addressbook.contact + "<br />\r\n";
                        }
                        if (this.Addressbook.RelationAttorneySpecialMatter != null)
                        {
                            wlines += "　　【代理関係の特記事項】" + this.Addressbook.RelationAttorneySpecialMatter + "<br />\r\n";
                        }
                    }
                    return wlines;
                }
            }
        }
        */

        // 【提出物件の目録】
        [XmlRoot(ElementName = "submission-object-list-article", Namespace = "http://www.jpo.go.jp")]
        public class SubmissionObjectListArticle
        {
            [XmlElement(ElementName = "list-group")]
            public List<ListGroup> ListGroup { get; set; }

            public string line
            {
                get
                {
                    string wlines = "【提出物件の目録】<br />\r\n";
                    if (this.ListGroup != null
                    && this.ListGroup.Count > 0)
                    {
                        foreach (ListGroup wListGroup in ListGroup)
                        {
                            wlines += wListGroup.line;
                        }
                    }
                    return wlines;
                }
            }
        }

        // 【目録】
        [XmlRoot(ElementName = "list-group", Namespace = "http://www.jpo.go.jp")]
        public class ListGroup
        {
            // 物件名
            [XmlElement(ElementName = "document-name")]
            public string DocumentName { get; set; }

            // 通数又は個数
            [XmlElement(ElementName = "number-of-object")]
            public string NumberOfObject { get; set; }

            // 援用の表示
            [XmlElement(ElementName = "citation")]
            public string Citation { get; set; }

            // 返還の申出
            [XmlElement(ElementName = "return-request")]
            public string ReturnRequest { get; set; }

            // 包括委任状番号
            [XmlElement(ElementName = "general-power-of-attorney-id")]
            public string generalPowerOfAttorneyId { get; set; }

            // 提出物件の特記事項
            [XmlElement(ElementName = "dtext")]
            public string dtext { get; set; }

            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    if (this.DocumentName != null && this.NumberOfObject != null)
                    {
                            wlines += "　　【物件名】　" + this.DocumentName + "　" + Strings.StrConv(this.NumberOfObject, VbStrConv.Wide, 0x411) + "<br />\r\n";
                    }
                    if (this.generalPowerOfAttorneyId != null)
                    {
                        wlines += "　　【包括委任状番号】　" + Strings.StrConv(this.generalPowerOfAttorneyId, VbStrConv.Wide, 0x411) + "<br />\r\n";
                    }
                    return wlines;
                }
            }
        }
        public CPatAppDoc m_patAppDoc { get; set; }
        public string m_xmlPath { get; set; }

        public string m_title { get; set; }
        public PatAppDoc(string szXml, string szXmlPath, string aLegalDate = "")
        {
            try
            {
                this.m_xmlPath = szXmlPath;
                this.m_patAppDoc = null;
                this.m_title = "提出日" + aLegalDate + "_明細書";
                XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(CPatAppDoc));
                using (TextReader reader = new StringReader(szXml))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    //settings.CheckCharacters = false;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreComments = true;
                    settings.DtdProcessing = DtdProcessing.Parse;
                    XmlReader xmlReader = XmlReader.Create(reader, settings);
                    this.m_patAppDoc = (CPatAppDoc)serializer.Deserialize(xmlReader);
                }
            }
            catch (Exception ex)
            {
                this.m_patAppDoc = null;
            }

        }
        public string htmlAll()
        {
            try
            {
                Text2html text2html = new Text2html(this.m_xmlPath);
                if (this.m_patAppDoc != null)
                {
                    text2html.setTitle("特許願：タイトルです");
                    if (this.m_patAppDoc.line != null)
                    {
                        text2html.addP(this.m_patAppDoc.line);
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
        // ~PatAppDoc()
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
