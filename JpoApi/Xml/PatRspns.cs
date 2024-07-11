using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JpoApi.OpdGlobalDocList;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools;
using System.Web.UI.WebControls;
using System.Management.Automation;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Net;
using static JpoApi.OpdGlobalCiteClass;
using DocumentFormat.OpenXml.EMMA;
using static JpoApi.PatRspns;
using Microsoft.VisualBasic;
using System.Xml.Linq;
using System.Web;
using DocumentFormat.OpenXml;
using Microsoft.Office.Interop.Word;
using System.Globalization;

namespace JpoApi
{
    public class PatRspns : IDisposable
    {
        private bool disposedValue;

        // 意見書クラス
        [XmlRoot(ElementName = "pat-rspns", Namespace = "http://www.jpo.go.jp")]
        public class CPatRspns
        {
            [XmlElement(ElementName = "response-a53")]
            public ResponseA53 ResponseA53 { get; set; }

            [XmlAttribute(AttributeName = "lang")]
            public string Lang { get; set; }

            [XmlAttribute(AttributeName = "dtd-version")]
            public string DtdVersion { get; set; }
            public string _m_xml { get; set; }
            public string m_xml
            {
                get { return _m_xml; }
                set
                {
                    string _m_xml = value;
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(_m_xml);
                    _pTag = doc.DocumentElement;
                    foreach (XmlNode child in _pTag.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            switch (child.LocalName)
                            {
                                case "response-a53":  // 【事件の表示】
                                    this.ResponseA53.m_xml = child.OuterXml;
                                    break;
                            }
                        }
                    }
                }
            }
            public XmlNode _pTag { get; set; }
        }

        [XmlRoot(ElementName = "response-a53", Namespace = "http://www.jpo.go.jp")]
        public class ResponseA53
        {
            // 【書類名】
            [XmlElement(ElementName = "document-code")]
            public string DocumentCode { get; set; }

            // 【整理番号】
            [XmlElement(ElementName = "file-reference-id", Namespace = "http://www.jpo.go.jp")]
            public string FileReferenceId { get; set; }

            // 【提出日】
            [XmlElement(ElementName = "submission-date", Namespace = "http://www.jpo.go.jp")]
            public SubmissionDate SubmissionDate { get; set; }

            // 【あて先】
            [XmlElement(ElementName = "addressed-to-person")]
            public string AddressedToPerson { get; set; }

            // 【事件の表示】
            [XmlElement(ElementName = "indication-of-case-article")]
            public IndicationOfCaseArticle IndicationOfCaseArticle { get; set; }

            // 【特許出願人】
            [XmlElement(ElementName = "applicants")]
            public Applicants Applicants { get; set; }

            // 【代理人】
            [XmlElement(ElementName = "agents")]
            public Agents Agents { get; set; }

            // 【発送番号】
            [XmlElement(ElementName = "dispatch-number")]
            public string DispatchNumber { get; set; }

            // 【意見の内容】
            [XmlElement("opinion-contents-article")]
            public XmlElement OpinionContentsArticle { get; set; }

            // 【証拠方法】
            [XmlElement("proof-means")]
            public string ProofMeans { get; set; }

            // 【その他】
            [XmlElement("dtext")]
            public string Dtext { get; set; }

            // 【提出物件の目録】
            [XmlElement("submission-object-list-article")]
            public SubmissionObjectListArticle SubmissionObjectListArticle { get; set; }


            [XmlAttribute(AttributeName = "kind-of-law")]
            public string KindOfLaw { get; set; }

            public string _m_xml { get; set; }
            public string m_xml
            {
                get { return _m_xml; }
                set
                {
                    string _m_xml = value;
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(_m_xml);
                    _pTag = doc.DocumentElement; // タグを取得
                    foreach (XmlNode child in _pTag.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            switch (child.LocalName)
                            {
                                case "indication-of-case-article":  // 【事件の表示】
                                    this.IndicationOfCaseArticle.m_xml = child.OuterXml;
                                    break;
                                case "applicants":  // 【特許出願人】
                                    this.Applicants.m_xml = child.OuterXml;
                                    if (this.IndicationOfCaseArticle.AppealReference != null)
                                    {
                                        this.Applicants.JpElementName = "【審判請求人】";
                                    }
                                    break;
                                case "agents":      // 【代理人】
                                    this.Agents.m_xml = child.OuterXml;
                                    break;
                            }
                        }
                    }
                }
            }
            public XmlNode _pTag { get; set; }
            public XmlNode pTag
            {
                get { return _pTag; }
                set { _pTag = value; }
            }
            public string line
            {
                get {
                    string wlines = "【書類名】　　　　　　意見書<br />\r\n";
                    foreach (XmlNode child in _pTag.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            // タグの場合、タグ名を出力
                            switch (child.LocalName)
                            {
                                case "file-reference-id":
                                    wlines += "【整理番号】　　　　　" +
                                        Strings.StrConv(this.FileReferenceId, VbStrConv.Wide, 0x411) 
                                        + this.FileReferenceId + "<br />\r\n";
                                    break;
                                case "submission-date":
                                    wlines += this.SubmissionDate.line;
                                    break;
                                case "addressed-to-person":
                                    wlines += "【あて先】　　　　　　" + this.AddressedToPerson + "<br />\r\n";
                                    break;
                                case "indication-of-case-article":  // 【事件の表示】
                                    this.IndicationOfCaseArticle.m_xml = child.OuterXml;
                                    wlines += this.IndicationOfCaseArticle.line;
                                    break;
                                case "applicants":  // 【特許出願人】
                                    wlines += this.Applicants.line;
                                    break;
                                case "agents":      // 【代理人】
                                    wlines += this.Agents.line;
                                    break;
                                case "dispatch-number":      // 【発送番号】
                                    wlines += "【発送番号】　　　　　" + Strings.StrConv(this.DispatchNumber, VbStrConv.Wide, 0x411) + "<br />\r\n";
                                    break;
                                case "opinion-contents-article":  //
                                    wlines += "【意見の内容】<br />\r\n";
                                    wlines += element2html(this.OpinionContentsArticle);
                                    break;
                                case "proof-means": // 【証拠方法】
                                    wlines += this.ProofMeans;
                                    break;
                                case "dtext":       // 【その他】
                                    wlines += this.Dtext;
                                    break;
                                case "submission-object-list-article":// 【提出物件の目録】
                                    wlines += this.SubmissionObjectListArticle.line;
                                    break;
                            }
                        }
                    }
                    return wlines;
                }
            }
        }

        // 【提出日】
        [XmlRoot(ElementName = "submission-date", Namespace = "http://www.jpo.go.jp")]
        public class SubmissionDate
        {
            [XmlElement(ElementName = "date", Namespace = "http://www.jpo.go.jp")]
            public string Date { get; set; }

            public string line {
                get {   return "【提出日】　　　　　　" + Get和暦(this.Date) + "<br />\r\n"; } }

        }

        // 【事件の表示】
        [XmlRoot(ElementName = "indication-of-case-article")]
        public class IndicationOfCaseArticle
        {
            // 出願番号
            [XmlElement(ElementName = "application-reference")]
            public ApplicationReference ApplicationReference { get; set; }

            // 審判番号
            [XmlElement(ElementName = "appeal-reference")]
            public AppealReference AppealReference { get; set; }
            public string _m_xml { get; set; }
            public string m_xml
            {
                get { return _m_xml; }
                set
                {
                    string _m_xml = value;
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(_m_xml);
                    _pTag = doc.DocumentElement;
                }
            }
            public XmlNode _pTag { get; set; }

            public XmlNode pTag
            {
                get { return _pTag; }
                set { 
                    _pTag = value; 
                }
            }
            public string line
            {
                get
                {
                    if (this.ApplicationReference != null && this.ApplicationReference.line != null)
                    {
                        return "【事件の表示】<br />\r\n"
                             + this.ApplicationReference.line;
                    }
                    if (this.AppealReference != null && this.AppealReference.line != null)
                    {
                        return "【事件の表示】<br />\r\n"
                             + this.AppealReference.line;
                    }
                    return null;
                }
            }
        }

        // 【出願番号】
        [XmlRoot(ElementName = "application-reference", Namespace = "http://www.jpo.go.jp")]
        public class ApplicationReference
        {
            [XmlElement(ElementName = "document-id")]
            public DocumentId DocumentId { get; set; }

            [XmlAttribute(AttributeName = "appl-type")]
            public string ApplType { get; set; }

            [XmlAttribute(AttributeName = "kind-of-law")]
            public string KindOfLaw { get; set; }
            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    if (this.DocumentId != null
                    && this.DocumentId.line != null)
                    {
                        switch(this.ApplType)
                        {
                            case "application":
                                wlines += "　　【出願番号】　　　"
                                    + "特願" + Strings.StrConv(this.DocumentId.line.Substring(0, 4), VbStrConv.Wide, 0x411) +
                                    "－" + Strings.StrConv(this.DocumentId.line.Substring(4, 6), VbStrConv.Wide, 0x411) + "<br />\r\n";
                                break;
                            case "international-application":
                                wlines += "　　【国際出願番号】　"
                                    + "ＰＣＴ／ＪＰ" + Strings.StrConv(this.DocumentId.line.Substring(0, 4), VbStrConv.Wide, 0x411) +
                                    "／" + Strings.StrConv(this.DocumentId.line.Substring(4, 6), VbStrConv.Wide, 0x411) + "<br />\r\n";
                                break;
                            case "registration":
                                wlines += "　　【登録番号】　　　"
                                    + "特許" + Strings.StrConv(this.DocumentId.line, VbStrConv.Wide, 0x411) + "<br />\r\n";
                                break;
                        }
                    }
                    return wlines;
                }
            }
        }
        // 【審判番号】
        [XmlRoot(ElementName = "appeal-reference", Namespace = "http://www.jpo.go.jp")]
        public class AppealReference
        {
            [XmlElement(ElementName = "doc-number")]
            public string DocNumber { get; set; }

            [XmlElement(ElementName = "date")]
            public string Date { get; set; }

            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    if (this.DocNumber != null)
                    {
                        wlines += "　　【審判番号】　　　"
                                + "不服" + Strings.StrConv(this.DocNumber.Substring(0, 4), VbStrConv.Wide, 0x411)
                                + "－" + Strings.StrConv(this.DocNumber.Substring(4, 6), VbStrConv.Wide, 0x411) + "<br />\r\n";

                    }
                    if (this.DocNumber != null)
                    {
                        wlines += "　　【審判請求日】　　" + Get和暦(this.Date) + "<br />\r\n";

                    }
                    return wlines;
                }
            }
        }
        // 番号
        [XmlRoot(ElementName = "document-id", Namespace = "http://www.jpo.go.jp")]
        public class DocumentId
        {
            [XmlElement(ElementName = "doc-number")]
            public string DocNumber { get; set; }

            public string line
            {
                get
                {
                    return this.DocNumber;
                }
            }
        }

        // 【特許出願人】（複数）
        public class Applicants
        {
            [XmlElement(ElementName = "applicant")]
            public List<Applicant> Applicant { get; set; }
            
            public string _JpElementName { get; set; }
            public string JpElementName {
                set {
                    _JpElementName = value;
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
                                    this.Applicant[i].m_xml = child.OuterXml;
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
                    if (_JpElementName == null || _JpElementName.Length == 0)
                    {
                        _JpElementName = "【特許出願人】";
                    }
                    string wlines = "";
                    if (this.Applicant != null
                    && this.Applicant.Count > 0)
                    {
                        foreach(Applicant wApplicant in this.Applicant)
                        {
                            wlines += _JpElementName + "<br />\r\n";
                            wlines += wApplicant.line;
                        }
                    }
                    return wlines;
                }
            }
        }

        // 【特許出願人】または【審判請求人】または【補正をする者】
        public class Applicant
        {
            [XmlElement(ElementName = "addressbook")]   // 氏名及び住所情報
            public Addressbook Addressbook { get; set; }

            [XmlElement(ElementName = "share")]  // 持分
            public string Share { get; set; }
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
        public class Addressbook
        {

            [XmlElement(ElementName = "kana")]  // 　【フリガナ】
            public string kana { get; set; }

            [XmlElement(ElementName = "name")]  // 氏名又は名称
            public string Name { get; set; }

            [XmlElement(ElementName = "original-language-of-name")]  // 　【住所又は居所原語表記】
            public string OriginalLanguageOfName { get; set; }

            [XmlElement(ElementName = "registered-number")] // 識別番号
            public string RegisteredNumber { get; set; }

            [XmlElement(ElementName = "address")]    // 住所又は居所
            public Address Address { get; set; }

            [XmlElement(ElementName = "phone")] // 電話番号
            public string phone { get; set; }

            [XmlElement(ElementName = "fax")]   // ファクシミリ番号
            public string fax { get; set; }

            [XmlElement(ElementName = "contact")]   // 連絡先
            public string contact { get; set; }

            [XmlElement(ElementName = "relation-attorney-special-matter")]   // 　【代理関係の特記事項】
            public string RelationAttorneySpecialMatter { get; set; }

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
                }
            }
            public XmlNode _pTag { get; set; }
            public string line
            {
                get
                {
                    string wlines = "";
                    if (this.Name != null)
                    {
                        wlines += "　　【氏名又は名称】　" + this.Name + "<br />\r\n";
                    }
                    if (this.RegisteredNumber != null)
                    {
                        wlines += "　　【識別番号】　　　" + Strings.StrConv(this.RegisteredNumber, VbStrConv.Wide, 0x411) + "<br />\r\n";
                    }
                    if (this.Address != null && this.Address.line != null)
                    {
                        wlines += "　　【住所又は居所】　" + this.Address.line + "<br />\r\n";
                    }
                    if (this.phone != null)
                    {
                        wlines += "　　【電話番号】　　　" + Strings.StrConv(this.phone, VbStrConv.Wide, 0x411) + "<br />\r\n";
                    }
                    if (this.fax != null)
                    {
                        wlines += "　　【ファクシミリ番号】" + Strings.StrConv(this.fax, VbStrConv.Wide, 0x411)  + "<br />\r\n";
                    }
                    return wlines;
                }
            }
        }

        // 【住所又は居所】
        public class Address
        {
            [XmlElement(ElementName = "text")]
            public string text { get; set; }

            public string line
            {
                get
                {
                    return this.text;
                }
            }
        }

        // 【代理人】（複数）
        public class Agents
        {
            [XmlElement(ElementName = "agent")]
            public List<Agent> Agent { get; set; }

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
                                case "agent":  // 【事件の表示】
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
                            wlines += "【代理人】<br />\r\n";
                            wlines += wAgent.line;
                        }
                    }
                    return wlines;
                }
            }
        }
        // 【代理人】
        public class Agent
        {
            [XmlElement(ElementName = "addressbook")]
            public Addressbook Addressbook { get; set; }

            [XmlElement(ElementName = "attorney")]  // 弁理士
            public string attorney { get; set; }

            [XmlElement(ElementName = "lawyer")]    // 弁護士
            public string lawyer { get; set; }

            [XmlAttribute(AttributeName = "kind-of-agent")]
            public string KindOfAgent { get; set; }

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
                                case "addressbook":  // 【事件の表示】
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
                            if (this.attorney != null)
                            {
                                wlines += "　　【弁理士】<br />\r\n";
                            }
                            if (this.lawyer != null)
                            {
                                wlines += "　　【弁護士】<br />\r\n";
                            }
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
                        foreach(ListGroup wListGroup in ListGroup)
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
                    string wlines = "　【目録】<br />\r\n";
                    if (this.DocumentName != null)
                    {
                        wlines += "　　【物件名】　" + this.DocumentName + "<br />\r\n";
                    }
                    if (this.NumberOfObject != null)
                    {
                        wlines += "　　【通数又は個数】　" + this.NumberOfObject + "<br />\r\n";
                    }
                    if (this.Citation != null)
                    {
                        wlines += "　　【援用の表示】　" + this.Citation + "<br />\r\n";
                    }
                    if (this.ReturnRequest != null)
                    {
                        wlines += "　　【返還の申出】　" + this.ReturnRequest + "<br />\r\n";
                    }
                    if (this.generalPowerOfAttorneyId != null)
                    {
                        wlines += "　　【包括委任状番号】" + this.generalPowerOfAttorneyId + "<br />\r\n";
                    }
                    if (this.dtext != null)
                    {
                        wlines += "　　【提出物件の特記事項】" + this.dtext + "<br />\r\n";
                    }
                    return wlines;
                }
            }
        }
        public CPatRspns m_patRepns { get; set; }
        public string m_xmlPath { get; set; }   // 元となるxmlのファイル名
        public string m_Date { get; set; }      // 提出日・起案日
        public string m_DocNumber { get; set; } // 出願番号
        public string m_DocumentName { get; set; }  // 文書名
        public string m_DocNumber2 { get; set; } // 出願番号 （外部指定）
        public string m_title { get; set; }     // htmlのタイトル
        public static string m_s_xmlPath { get; set; }
        public PatRspns(string szXml, string szXmlPath, string aLegalDate = "")
        {
            try
            {
                //this.tmp();
                //this.tmp2();
                //this.tmp3();
                this.m_xmlPath = szXmlPath;
                m_s_xmlPath = szXmlPath;
                this.m_patRepns = null;
                this.m_Date = aLegalDate;
                this.m_DocNumber = string.Empty;
                this.m_DocumentName = string.Empty;
                this.m_DocNumber2 = string.Empty;
                this.m_title = "提出日" + aLegalDate + "_意見書";
                XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(CPatRspns));
                using (TextReader reader = new StringReader(szXml))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    //settings.CheckCharacters = false;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreComments = true;
                    settings.DtdProcessing = DtdProcessing.Parse;

                    XmlReader xmlReader = XmlReader.Create(reader, settings);
                    m_patRepns = (CPatRspns)serializer.Deserialize(xmlReader);
                    m_patRepns.m_xml = szXml;
                }
                if (this.m_patRepns != null
                && this.m_patRepns.ResponseA53 != null)
                {
                    if (this.m_patRepns.ResponseA53.SubmissionDate != null
                    && this.m_patRepns.ResponseA53.SubmissionDate.Date != null)
                    {
                        this.m_Date = this.m_patRepns.ResponseA53.SubmissionDate.Date;
                    }
                    if (this.m_patRepns.ResponseA53.IndicationOfCaseArticle != null
                    && this.m_patRepns.ResponseA53.IndicationOfCaseArticle.ApplicationReference != null
                    && this.m_patRepns.ResponseA53.IndicationOfCaseArticle.ApplicationReference.DocumentId != null
                    && this.m_patRepns.ResponseA53.IndicationOfCaseArticle.ApplicationReference.DocumentId.DocNumber != null)
                    {
                        this.m_DocNumber = this.m_patRepns.ResponseA53.IndicationOfCaseArticle.ApplicationReference.DocumentId.DocNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                this.m_patRepns = null;
            }
        }

        public string htmlAll()
        {
            try
            {
                Text2html text2html = new Text2html(this.m_xmlPath);
                if (this.m_patRepns != null)
                {
                    text2html.setTitle(m_title);
                    text2html.addP(this.m_patRepns.ResponseA53.line);
                    return text2html.htmlAll();
                }
                return "";
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
                if (this.m_patRepns != null)
                {
                    text2html.setTitle(m_title);
                    text2html.addP("【書類名】　　　　　　意見書");
                    if (this.m_patRepns.ResponseA53 != null)
                    {
                        if (this.m_patRepns.ResponseA53.SubmissionDate != null
                        && this.m_patRepns.ResponseA53.SubmissionDate.Date != null)
                        {
                            text2html.addP("【提出日】　　　　　　" + this.m_patRepns.ResponseA53.SubmissionDate.Date);
                        }
                        if (this.m_patRepns.ResponseA53.FileReferenceId != null)
                        {
                            text2html.addP("【整理番号】　　　　　" + Strings.StrConv(this.m_patRepns.ResponseA53.FileReferenceId, VbStrConv.Wide, 0x411));
                        }
                        if (this.m_patRepns.ResponseA53.AddressedToPerson != null)
                        {
                            text2html.addP("【あて先】　　　　　　" + this.m_patRepns.ResponseA53.AddressedToPerson);
                        }
                        if (this.m_patRepns.ResponseA53.IndicationOfCaseArticle != null
                        && this.m_patRepns.ResponseA53.IndicationOfCaseArticle.ApplicationReference != null
                        && this.m_patRepns.ResponseA53.IndicationOfCaseArticle.ApplicationReference.DocumentId != null
                        && this.m_patRepns.ResponseA53.IndicationOfCaseArticle.ApplicationReference.DocumentId.DocNumber != null)
                        {
                            text2html.addP("【事件の表示】");
                            text2html.addP("　　【出願番号】　　　" + this.m_patRepns.ResponseA53.IndicationOfCaseArticle.ApplicationReference.DocumentId.DocNumber);
                        }
                        if (this.m_patRepns.ResponseA53.Applicants != null
                        && this.m_patRepns.ResponseA53.Applicants.Applicant.Count > 0)
                        {
                            foreach (Applicant applicant in this.m_patRepns.ResponseA53.Applicants.Applicant)
                            {
                                if (applicant.Addressbook != null)
                                {
                                    text2html.addP("【特許出願人】");
                                    if (applicant.Addressbook.Name != null)
                                    {
                                        text2html.addP("　　【氏名又は名称】　" + applicant.Addressbook.Name);
                                    }
                                    if (applicant.Addressbook.RegisteredNumber != null)
                                    {
                                        text2html.addP("　　【識別番号】　　　" + Strings.StrConv(applicant.Addressbook.RegisteredNumber, VbStrConv.Wide, 0x411));
                                    }
                                    if (applicant.Addressbook.Address != null)
                                    {
                                        text2html.addP("　　【住所又は居所】　" + applicant.Addressbook.Address);
                                    }
                                }
                            }
                        }
                        if (this.m_patRepns.ResponseA53.Agents != null
                        && this.m_patRepns.ResponseA53.Agents.Agent.Count > 0)
                        {
                            foreach (Agent agent in this.m_patRepns.ResponseA53.Agents.Agent)
                            {
                                if (agent.Addressbook != null)
                                {
                                    text2html.addP("【代理人】");
                                    if (agent.Addressbook.RegisteredNumber != null)
                                    {
                                        text2html.addP("　　【識別番号】　　　" + Strings.StrConv(agent.Addressbook.RegisteredNumber, VbStrConv.Wide, 0x411));
                                    }
                                    if (agent.Addressbook.Name != null)
                                    {
                                        text2html.addP("　　【弁理士】");
                                        text2html.addP("　　【氏名又は名称】　" + agent.Addressbook.Name);
                                    }
                                    /*
                                    if (agent.Addressbook.lawyer != null)
                                    {
                                        text2html.addP("　　【弁護士】");
                                        text2html.addP("　　【氏名又は名称】　" + agent.Addressbook.lawyer);
                                    }
                                    */
                                    if (agent.Addressbook.Address != null)
                                    {
                                        text2html.addP("　　【住所又は居所】　" + agent.Addressbook.Address);
                                    }
                                    if (agent.Addressbook.phone != null)
                                    {
                                        text2html.addP("　　【電話番号】　　　" + Strings.StrConv(agent.Addressbook.phone, VbStrConv.Wide, 0x411));
                                    }
                                    if (agent.Addressbook.fax != null)
                                    {
                                        text2html.addP("　　【ファクシミリ番号】" + Strings.StrConv(agent.Addressbook.fax, VbStrConv.Wide, 0x411));
                                    }
                                }
                            }
                        }
                        if (this.m_patRepns.ResponseA53.DispatchNumber != null)
                        {
                            text2html.addP("【発送番号】　　　　　" + Strings.StrConv(this.m_patRepns.ResponseA53.DispatchNumber, VbStrConv.Wide, 0x411));
                        }
                        if (this.m_patRepns.ResponseA53.OpinionContentsArticle != null)
                        {
                            text2html.addP("【意見の内容】");
                            //text2html.addOuterXml(m_patRepns.ResponseA53.OpinionContentsArticle.OuterXml);
                            text2html.addElement(m_patRepns.ResponseA53.OpinionContentsArticle);
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

        public static string element2html(XmlElement element)
        {
            string wHtmlbody = node2html((XmlNode)element);
            return wHtmlbody;
        }

        // タグの再帰処理
        public static string node2html(XmlNode node)
        {
            string wHtmlbody = "";

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text)
                {
                    // テキストノードの場合、テキストを出力
                    wHtmlbody += child.Value;
                }
                else if (child.NodeType == XmlNodeType.Element)
                {
                    // タグの場合、タグ名を出力
                    switch (child.LocalName)
                    {
                        case "img":
                            if (wHtmlbody.Length > 0) wHtmlbody += "<br />\r\n";
                            wHtmlbody += node_img(child);
                            break;
                        case "chemistry":
                            if (wHtmlbody.Length > 0) wHtmlbody += "<br />\r\n";
                            wHtmlbody += "【化" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】\r\n";
                            wHtmlbody += node2html(child);
                            break;
                        case "tables":
                            if (wHtmlbody.Length > 0) wHtmlbody += "<br />\r\n";
                            wHtmlbody += "【表" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】\r\n";
                            wHtmlbody += node2html(child);
                            break;
                        case "maths":
                            if (wHtmlbody.Length > 0) wHtmlbody += "<br />\r\n";
                            wHtmlbody += "【数" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】\r\n";
                            wHtmlbody += node2html(child);
                            break;
                        case "patcit":
                            if (wHtmlbody.Length > 0) wHtmlbody += "<br />\r\n";
                            wHtmlbody += "　　【特許文献" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】" + child.OuterXml + "\r\n";
                            break;
                        case "nplcit":
                            if (wHtmlbody.Length > 0) wHtmlbody += "<br />\r\n";
                            wHtmlbody += "　　【非特許文献" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】" + child.OuterXml + "\r\n";
                            break;
                        case "figref":
                            if (wHtmlbody.Length > 0) wHtmlbody += "<br />\r\n";
                            wHtmlbody += "　　【図" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】" + child.OuterXml + "\r\n";
                            break;
                        case "#text":
                            string szText = HttpUtility.HtmlEncode(child.OuterXml);
                            szText = szText.Replace("&#160;", "&#32;");
                            wHtmlbody += szText;
                            break;
                        case "br":
                            if (child.InnerText == null)
                            {
                                wHtmlbody += "<br />\r\n";
                            }
                            else
                            {
                                wHtmlbody += "<br />\r\n";
                                wHtmlbody += node2html(child);
                            }
                            break;
                        case "u":
                        case "sup":
                        case "sub":
                        default:
                            if (child.InnerText == null)
                            {
                                wHtmlbody += "<" + child.Name + " />";
                            }
                            else
                            {
                                wHtmlbody += "<" + child.Name + ">";
                                wHtmlbody += node2html(child);
                                wHtmlbody += "</" + child.Name + ">";
                            }
                            break;
                    }
                }
            }
            return wHtmlbody;
        }
        private static string node_img(XmlNode node)
        {
            string wHtmlbody = "";
            int height = (int)(3.777 * double.Parse(node.Attributes["he"].Value));
            int width = (int)(3.777 * double.Parse(node.Attributes["wi"].Value));
            string w_src_png = Path.GetFileNameWithoutExtension(node.Attributes["file"].Value) + ".png";
            string w_src1 = System.IO.Path.GetDirectoryName(m_s_xmlPath) + @"\" + w_src_png;

            string w_src0 = System.IO.Path.GetDirectoryName(m_s_xmlPath) + @"\" + node.Attributes["file"].Value;
            System.Drawing.Image img = System.Drawing.Bitmap.FromFile(w_src0);
            img.Save(w_src1, System.Drawing.Imaging.ImageFormat.Png);
            byte[] dataPng = System.IO.File.ReadAllBytes(w_src1);
            string base64Png = Convert.ToBase64String(dataPng);
            wHtmlbody += "<img height=" + height.ToString() + " width=" + width.ToString() + " src=\"data:image/png;base64," + base64Png + "\"><br />\r\n";
            return wHtmlbody;
        }
        public static string Get西暦(string legalDate)
        {
            try
            {
                string format = "yyyyMMdd";
                DateTime dTime = DateTime.ParseExact(legalDate, format, null);
                return Get西暦(dTime);
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static string Get西暦(DateTime date)
        {
            CultureInfo english = new CultureInfo("en");
            english.DateTimeFormat.Calendar = new GregorianCalendar();
            string wareki_date = date.ToString("yyyy年MM月dd日", english);
            wareki_date = Strings.StrConv(wareki_date, VbStrConv.Wide);
            return wareki_date;
        }

        public static string Get和暦(string legalDate)
        {
            try
            {
                string format = "yyyyMMdd";
                DateTime dTime = DateTime.ParseExact(legalDate, format, null);
                return Get和暦(dTime);
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static string Get和暦(DateTime date)
        {
            CultureInfo Japanese = new CultureInfo("ja-JP");
            Japanese.DateTimeFormat.Calendar = new JapaneseCalendar();
            string wareki_date = date.ToString("ggyy年MM月dd日", Japanese);
            wareki_date = Strings.StrConv(wareki_date, VbStrConv.Wide);
            return wareki_date;
        }

        private void tmp()
        {
            string xmlString = @"<MyClass><MyField><p>Hello, <br />world!</p></MyField></MyClass>"; // あなたのXML文字列

            XmlSerializer serializer = new XmlSerializer(typeof(MyClass));

            using (TextReader reader = new StringReader(xmlString))
            {
                MyClass obj = (MyClass)serializer.Deserialize(reader);
                Console.WriteLine(obj.MyField); // 出力: <p>Hello, world!</p>
            }
        }
        [XmlRoot("MyClass")]
        public class MyClass
        {
            [XmlElement("MyField")]
            public XmlElement MyField { get; set; }
        }
        private void tmp2()
        {
            string xmlString = @"<MyClass><MyField><p>Hello, <br />world!</p></MyField></MyClass>"; // あなたのXML文字列

            XmlSerializer serializer = new XmlSerializer(typeof(MyClass));

            using (TextReader reader = new StringReader(xmlString))
            {
                MyClass obj = (MyClass)serializer.Deserialize(reader);
                Console.WriteLine(obj.MyField.InnerXml); // 出力: <p>Hello, <br />world!</p>
            }
        }

        [XmlRoot("p")]
        public class Para
        {
            public XmlElement MyField { get; set; }
        }
        private void tmp3()
        {
            string xmlString = @"<tmp><p num=""0100"">He<u>llo, </u><br />world!</p><p num=""0101"">こんにちは<br />世界</p></tmp>"; // あなたのXML文字列

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);

            XmlNode pTag = doc.DocumentElement; // <p>タグを取得

            foreach (XmlNode child in pTag.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.InnerText.Length == 0)
                    {
                        Console.WriteLine("<" + child.Name + " />"); // 出力:
                    }
                    else
                    {
                        string wHtmlBody = convHtml(child);
                        Console.WriteLine("<p>" + wHtmlBody + "</p>"); // 出力:
                    }
                }
            }
        }
        public string convHtml(XmlNode node)
        {
            string wHtmlBody = "";

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text)
                {
                    // テキストノードの場合、テキストを出力
                    wHtmlBody += child.Value;
                }
                else if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.InnerText.Length == 0)
                    {
                        wHtmlBody += "<" + child.Name + " />";
                    }
                    else
                    {
                        // タグの場合、タグ名を出力
                        wHtmlBody += "<" + child.Name + ">";
                        wHtmlBody += this.convHtml(child);
                        wHtmlBody += "</" + child.Name + ">";
                    }
                }
            }
            return wHtmlBody;
        }
        public static string document_code2desc(string code)
        {
            switch (code.Substring(0, 1) + code.Substring(2))
            {
                case "A50": return "誤記訂正書";
                case "A51": return "手続補正書（方式）";
                case "A511": return "手続補完書";
                case "A521": return "手続補正書";
                case "A5210": return "特許協力条約第３４条補正の翻訳文提出書（職権）";
                case "A5211": return "特許協力条約第３４条補正の写し提出書";
                case "A5212": return "特許協力条約第３４条補正の写し提出書（職権）";
                case "A522": return "手続補正書";
                case "A523": return "手続補正書";
                case "A524": return "誤訳訂正書";
                case "A525": return "特許協力条約第１９条補正の翻訳文提出書";
                case "A526": return "特許協力条約第１９条補正の翻訳文提出書（職権）";
                case "A527": return "特許協力条約第１９条補正の写し提出書";
                case "A528": return "特許協力条約第１９条補正の写し提出書（職権）";
                case "A529": return "特許協力条約第３４条補正の翻訳文提出書";
                case "A53": return "意見書";
                case "A531": return "意見書（要約）";
                case "A541": return "ＰＣＴ１９条補正書";
                case "A542": return "ＰＣＴ３４条補正書";
                case "A56": return "異議補正書";
                case "A57": return "答弁書";
                case "A58": return "弁駁書";
                case "A59": return "弁明書";
                case "A601": return "期間延長請求書";
                case "A603": return "期間延長請求書（期間徒過）";
                case "A61": return "登録料納付";
                case "A621": return "出願審査請求書";
                case "A623": return "実用新案技術評価請求書";
                case "A624": return "実用新案技術評価請求書（他人）";
                case "A625": return "出願審査請求書（他人）";
                case "A626": return "国内処理請求書";
                case "A627": return "出願公開請求書";
                case "A63": return "特許願";
                //case "A63": return "実用新案登録願";
                //case "A63": return "意匠登録願";
                //case "A63": return "商標登録願";
                case "A631": return "翻訳文提出書";
                //case "A632": return "防護標章登録願";
                case "A632": return "国内書面";
                //case "A633": return "防護標章登録に基づく権利存続期間更新登録願";
                case "A633": return "図面の提出書";
                case "A6330": return "明細書";
                case "A6331": return "図面";
                case "A6332": return "要約書";
                case "A6333": return "特許請求の範囲";
                //case "A6333": return "実用新案登録請求の範囲";
                case "A634": return "書換登録申請書";
                //case "A634": return "国際出願翻訳文提出書";
                case "A6340": return "外国語明細書";
                case "A6341": return "外国語図面";
                case "A6342": return "外国語要約";
                case "A6343": return "外国語特許請求の範囲";
                case "A635": return "防護標章登録に基づく権利書換登録申請書";
                //case "A635": return "国際出願翻訳文提出書（職権）";
                case "A636": return "類似意匠登録願";
                case "A637": return "重複登録商標に係る商標権存続期間更新登録願";
                case "A638": return "地域団体商標登録願";
                case "A639": return "団体商標登録願";
                case "A641": return "異議申立書";
                case "A642": return "意見申立書";
                case "A651": return "異議理由補充書";
                case "A652": return "証拠調申請書";
                case "A653": return "証拠保全申立書";
                case "A654": return "証人尋問申請書";
                case "A655": return "検証申請書";
                case "A656": return "当事者尋問申立書";
                case "A657": return "鑑定申立書";
                case "A658": return "証拠保全申立取消書";
                case "A659": return "証拠調期日変更願書";
                case "A6591": return "証拠調申請取下書";
                case "A6592": return "証人不参届";
                case "A661": return "異議取下書";
                case "A662": return "異議一部放棄書";
                case "A67": return "受託番号変更届";
                case "A681": return "代表者選定届";
                case "A685": return "代表者選定届（申立人）";
                case "A691": return "雑書類";
                case "A695": return "雑書類（申立人第三者）";
                case "A701": return "組織変更届（出願人）";
                case "A702": return "組織変更届（申立人）";
                case "A711": return "出願人名義変更届";
                case "A712": return "出願人名義変更届（一般承継）";
                case "A713": return "出願人名義変更届（特例商標登録出願）";
                case "A714": return "出願人名義変更届（特例商標登録出願）（一般承継）";
                case "A715": return "書換登録申請者名義変更届 ";
                case "A721": return "名称（氏名）変更届（出願人）";
                case "A722": return "名称（氏名）変更届（代理人）";
                case "A723": return "名称（氏名）変更届（復代理人）";
                case "A724": return "名称（氏名）変更届（指定代理人）";
                case "A725": return "名称（氏名）変更届（申立人）";
                case "A726": return "名称（氏名）変更届（申立人代理人）";
                case "A727": return "名称（氏名）変更届（申立人復代理人）";
                case "A728": return "名称（氏名）変更届（申立人指定代理人）";
                case "A731": return "住所変更届（出願人）";
                case "A732": return "住所変更届（代理人）";
                case "A733": return "住所変更届（復代理人）";
                case "A734": return "住所変更届（指定代理人）";
                case "A735": return "住所変更届（申立人）";
                case "A736": return "住所変更届（申立人代理人）";
                case "A737": return "住所変更届（申立人復代理人）";
                case "A738": return "住所変更届（申立人指定代理人）";
                case "A7421": return "代理人変更届";
                case "A7422": return "代理人受任届";
                case "A7423": return "代理人選任届";
                case "A7424": return "代理人辞任届";
                case "A7425": return "代理人解任届";
                case "A7426": return "代理権変更届";
                case "A7427": return "代理権消滅届";
                case "A7431": return "復代理人変更届";
                case "A7432": return "復代理人受任届";
                case "A7433": return "復代理人選任届";
                case "A7434": return "復代理人辞任届";
                case "A7435": return "復代理人解任届";
                case "A7436": return "復代理権変更届";
                case "A7437": return "復代理権消滅届";
                case "A7461": return "代理人変更届（申立人）";
                case "A7462": return "代理人受任届（申立人）";
                case "A7463": return "代理人選任届（申立人）";
                case "A7464": return "代理人辞任届（申立人）";
                case "A7465": return "代理人解任届（申立人）";
                case "A7466": return "代理権変更届（申立人）";
                case "A7467": return "代理権消滅届（申立人）";
                case "A7468": return "包括委任状援用制限届（申立人）";
                case "A7471": return "復代理人変更届（申立人）";
                case "A7472": return "復代理人受任届（申立人）";
                case "A7473": return "復代理人選任届（申立人）";
                case "A7474": return "復代理人辞任届（申立人）";
                case "A7475": return "復代理人解任届（申立人）";
                case "A7476": return "復代理権変更届（申立人）";
                case "A7477": return "復代理権消滅届（申立人）";
                case "A751": return "印鑑変更届（出願人）";
                case "A752": return "印鑑変更届（代理人）";
                case "A753": return "印鑑変更届（復代理人）";
                case "A754": return "印鑑変更届（指定代理人）";
                case "A755": return "印鑑変更届（申立人）";
                case "A756": return "印鑑変更届（申立人代理人）";
                case "A757": return "印鑑変更届（申立人復代理人）";
                case "A758": return "印鑑変更届（申立人指定代理人）";
                case "A761": return "出願取下書";
                case "A762": return "出願放棄書";
                case "A763": return "指定商品一部放棄書";
                case "A764": return "先の出願に基づく優先権主張取下書";
                case "A765": return "パリ条約による優先権主張放棄書";
                case "A766": return "書換登録申請取下書";
                case "A768": return "使用に基づく特例の適用の主張取下書";
                case "A7731": return "出願変更届（独立→類似）";
                case "A7732": return "出願変更届（類似→独立）";
                case "A7741": return "出願変更届（独立→連合）";
                case "A7742": return "出願変更届（連合→独立）";
                case "A781": return "上申書";
                case "A785": return "上申書（申立人）";
                case "A79": return "優先権証明書提出書";
                case "A7A1": return "一括組織変更届（出願人）";
                case "A7A5": return "一括組織変更届（申立人）";
                case "A7B": return "一括名義変更届（一般承継）";
                case "A7C1": return "一括名称（氏名）変更届（出願人）";
                case "A7C2": return "一括名称（氏名）変更届（代理人）";
                case "A7C3": return "一括名称（氏名）変更届（復代理人）";
                case "A7C4": return "一括名称（氏名）変更届（指定代理人）";
                case "A7C5": return "一括名称（氏名）変更届（申立人）";
                case "A7C6": return "一括名称（氏名）変更届（申立人代理人）";
                case "A7C7": return "一括名称（氏名）変更届（申立人復代理人）";
                case "A7C8": return "一括名称（氏名）変更届（申立人指定代理人）";
                case "A7D1": return "一括住所変更届（出願人）";
                case "A7D2": return "一括住所変更届（代理人）";
                case "A7D3": return "一括住所変更届（復代理人）";
                case "A7D4": return "一括住所変更届（指定代理人）";
                case "A7D5": return "一括住所変更届（申立人）";
                case "A7D6": return "一括住所変更届（申立人代理人）";
                case "A7D7": return "一括住所変更届（申立人復代理人）";
                case "A7D8": return "一括住所変更届（申立人指定代理人）";
                case "A80": return "新規性の喪失の例外証明書提出書";
                case "A801": return "新規性喪失の例外適用申請書";
                case "A81": return "出願日証明書提出書";
                case "A82": return "物件提出書";
                case "A821": return "手続補足書";
                case "A822": return "証明書類提出書";
                case "A824": return "ひな形又は見本補足書";
                case "A826": return "協議の結果届";
                case "A831": return "刊行物等提出書";
                case "A832": return "情報提供書";
                case "A833": return "特徴記載書";
                case "A84": return "証明請求書";
                case "A841": return "優先権証明請求書";
                case "A842": return "証明請求書";
                case "A843": return "優先権証明請求（電子データ交換協定）";
                case "A8431": return "優先権証明書類請求（電子データ交換協定）";
                case "A845": return "優先権証明応答（電子データ交換協定）";
                case "A8451": return "優先権証明書類応答（電子データ交換協定）";
                case "A85": return "謄本請求書";
                case "A851": return "ﾌｧｲﾙ記録事項記載書類の交付請求書";
                case "A852": return "認証付ﾌｧｲﾙ記録事項記載書類の交付請求書";
                case "A86": return "閲覧請求書";
                case "A861": return "ファイル記録事項の閲覧（縦覧）請求書";
                case "A87": return "優先審査に関する事情説明書";
                case "A871": return "早期審査に関する事情説明書";
                case "A872": return "早期審査に関する事情説明補充書";
                case "A8911": return "就業先届（出願人）";
                case "A8912": return "就業先届（代理人）";
                case "A8915": return "就業先届（申立人）";
                case "A8916": return "就業先届（申立人代理人）";
                case "A8921": return "就業先変更届（出願人）";
                case "A8922": return "就業先変更届（代理人）";
                case "A8925": return "就業先変更届（申立人）";
                case "A8926": return "就業先変更届（申立人代理人）";
                case "A8931": return "就業先消滅届（出願人）";
                case "A8932": return "就業先消滅届（代理人）";
                case "A8935": return "就業先消滅届（申立人）";
                case "A8936": return "就業先消滅届（申立人代理人）";
                case "A907": return "秘密意匠期間変更請求書";
                case "A908": return "協議の結果届";
                case "A916": return "世界知的所有権機関へのアクセスコード付与請求書";
                case "A917": return "回復理由書";
                case "C50": return "期間延長願";
                case "C51": return "手続補正書";
                case "C511": return "手数料補正書";
                case "C512": return "手数料補正書";
                case "C53": return "意見書";
                case "C54": return "回答書";

                case "C541": return "釈明書";
                case "C56": return "異議申立書";
                case "C561": return "異議申立書";
                case "C565": return "異議理由補充書";
                case "C569": return "異議取下書";

                case "C57": return "答弁書";
                case "C58": return "弁駁書";
                case "C60": return "審判請求書";
                case "C605": return "審判請求理由補充書";
                case "C609": return "請求取下書";
                case "C6091": return "一部請求取下書";

                case "C61": return "審判事件答弁書";
                case "C611": return "訂正請求書";
                case "C619": return "訂正取下書";
                case "C62": return "審判事件弁駁書";
                case "C63": return "参加申請書";
                case "C635": return "参加申請書";
                case "C636": return "補助参加申請書";
                case "C638": return "補助参加取下書";
                case "C639": return "参加取下書";
                case "C64": return "審理再開申立書";
                case "C641": return "特許異議申立期間満了前審理の上申書";

                case "C65": return "口頭審理・証拠調";
                case "C6511": return "書面審理申立書";
                case "C6512": return "口頭審理申立書";
                case "C6513": return "口頭審理陳述要領書";
                case "C6514": return "証拠申出書";
                case "C6515": return "証拠説明書";
                case "C6516": return "録音テープ等の書面化申出書";
                case "C6517": return "録音テープ等の内容説明書";
                case "C6518": return "録音テープ等の内容説明書に対する意見書";
                case "C6519": return "費用の額の決定請求書";
                case "C652": return "証拠調申立書";
                case "C6520": return "催告に対する意見書";
                case "C654": return "証人尋問申出書";
                case "C6541": return "尋問事項書";
                case "C6542": return "回答希望事項記載書面";
                case "C6543": return "尋問に代わる書面の提出書";
                case "C6544": return "書証の申出書";
                case "C6545": return "文書提出命令の申立書";
                case "C6546": return "文書特定の申出書";
                case "C6547": return "文書提出命令に対する意見書";
                case "C655": return "検証申出書";
                case "C657": return "鑑定の申出書";
                case "C6571": return "鑑定の申出に対する意見書";
                case "C6572": return "鑑定事項書";
                case "C6573": return "鑑定書";
                case "C659": return "期日変更請求書";
                case "C6591": return "証拠取下書";
                case "C6592": return "不出頭の届出書";

                case "C66": return "審理再開申立書";
                case "C661": return "異議取下書";
                case "C662": return "一部異議取下書";
                case "C665": return "訴状等";
                case "C67": return "参加取下書";
                case "C68": return "行政不服申立書";
                case "C681": return "行政不服の決定書";
                case "C69": return "郵便送達報告書";
                case "C70": return "代表者選定（変更）届";
                case "C701": return "組織変更届";
                case "C71": return "名義変更届";
                case "C712": return "受継届";
                case "C72": return "名称（氏名）変更届";
                case "C721": return "名称（氏名）変更届";
                case "C73": return "住所変更届";
                case "C731": return "住所変更届";
                case "C74": return "代理人変更届";
                case "C7427": return "代理権消滅届";
                case "C7428": return "包括委任状援用制限届";
                case "C7431": return "復代理人変更届";
                case "C75": return "改印届";
                case "C751": return "印鑑変更届";
                case "C76": return "出願取下書";
                case "C761": return "出願放棄書";
                case "C77": return "出願変更届";
                case "C78": return "上申書";
                case "C781": return "伺書";
                case "C7A1": return "一括組織変更届";

                case "C7B": return "一括名義変更届";
                case "C7C1": return "一括名称（氏名）変更届";
                case "C7D1": return "住所変更届";
                case "C80": return "証拠（物件）提出書";
                case "C84": return "営業秘密に関する申出書";
                case "C87": return "優先審理事情説明書";
                case "C875": return "優先審理に関する事情説明書";
                case "C876": return "早期審理に関する事情説明書";
                case "C877": return "早期審理に関する事情説明補充書";
                case "C88": return "早期審理に関する事情説明書";
                case "C90": return "包袋引継・借用";
                case "C99": return "行政不服申立";

                default:
                    return code;
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
        // ~patRspns()
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
