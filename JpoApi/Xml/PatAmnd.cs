using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JpoApi.PatRspns;
using System.Xml.Serialization;
using static JpoApi.PatAmnd;
using System.IO;
using System.Xml;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.VisualBasic;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.EMMA;

namespace JpoApi
{
    public class PatAmnd : IDisposable
    {
        private bool disposedValue;

        // 手続補正書root
        [XmlRoot(ElementName = "pat-amnd", Namespace = "http://www.jpo.go.jp")]
        public class CPatAmnd
        {
            [XmlElement(ElementName = "amendment-a523", Namespace = "http://www.jpo.go.jp")]
            public AmendmentA523 AmendmentA523 { get; set; }

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
                                case "amendment-a523":  // 【事件の表示】
                                    this.AmendmentA523.m_xml = child.OuterXml;
                                    break;
                            }
                        }
                    }
                }
            }
            public XmlNode _pTag { get; set; }
        }

        // 手続補正書
        [XmlRoot(ElementName = "amendment-a523", Namespace = "http://www.jpo.go.jp")]
        public class AmendmentA523
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

            // 【事件の表示】
            [XmlElement(ElementName = "indication-of-case-article", Namespace = "http://www.jpo.go.jp")]
            public IndicationOfCaseArticle IndicationOfCaseArticle { get; set;}

            // 【補正をする者】
            [XmlElement(ElementName = "applicants", Namespace = "http://www.jpo.go.jp")]
            public Applicants Applicants { get; set; }

            // 【代理人】
            [XmlElement(ElementName = "agents", Namespace = "http://www.jpo.go.jp")]
            public Agents Agents { get; set; }

            // 【発送番号】
            [XmlElement(ElementName = "dispatch-number", Namespace = "http://www.jpo.go.jp")]
            public string DispatchNumber { get; set; }

            // 【手続補正ｎ】
            [XmlElement(ElementName = "amendment-article", Namespace = "http://www.jpo.go.jp")]
            public AmendmentArticle AmendmentArticle { get; set; }

            // 【手数料の表示】
            [XmlElement(ElementName = "charge-article", Namespace = "http://www.jpo.go.jp")]
            public ChargeArticle ChargeArticle { get; set; }
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
                                case "indication-of-case-article":  // 【事件の表示】
                                    this.IndicationOfCaseArticle.m_xml = child.OuterXml;
                                    break;
                                case "applicants":  // 【補正をする者】
                                    this.Applicants.m_xml = child.OuterXml;
                                    this.Applicants.JpElementName = "【補正をする者】";
                                    break;
                                case "agents":      // 【代理人】
                                    this.Agents.m_xml = child.OuterXml;
                                    break;
                            }
                        }
                    }
                    /*
                    XmlNamespaceManager xmlNsManager = new XmlNamespaceManager(doc.NameTable);
                    xmlNsManager.AddNamespace("jp", "http://www.jpo.go.jp");
                    this.ResponseA53.pTag = doc.SelectSingleNode("//jp:response-a53", xmlNsManager);
                    */
                }
            }
            public XmlNode _pTag { get; set; }
            public string line
            {
                get
                {
                    try
                    {
                        string wlines = "【書類名】　　　　　　手続補正書<br />\r\n";
                        if (this.FileReferenceId != null)
                        {
                            wlines += "【整理番号】　　　　　" + this.FileReferenceId + "<br />\r\n";
                        }
                        if (this.SubmissionDate != null
                        && this.SubmissionDate.line != null)   // 【提出日】
                        {
                            wlines += SubmissionDate.line;
                        }
                        if (this.AddressedToPerson != null)
                        {
                            wlines += "【あて先】　　　　　　" + this.AddressedToPerson + "<br />\r\n";
                        }
                        if (this.IndicationOfCaseArticle != null
                        && this.IndicationOfCaseArticle.line != null)  // 【事件の表示】
                        {
                            wlines += this.IndicationOfCaseArticle.line;
                        }
                        if (this.Applicants != null
                        && this.Applicants.line != null)  // 【補正をする者】
                        {
                            this.Applicants.JpElementName = "【補正をする者】";
                            wlines += this.Applicants.line;
                        }
                        if (this.Agents != null
                        && this.Agents.line != null)  // 【代理人】
                        {
                            wlines += this.Agents.line;
                        }
                        if (this.DispatchNumber != null)    // 【発送番号】
                        {
                            wlines += "【発送番号】　　　　　" + Strings.StrConv(this.DispatchNumber, VbStrConv.Wide, 0x411) + "<br />\r\n";
                        }
                        if (this.AmendmentArticle != null)  // 【手続補正ｎ】
                        {
                            wlines += AmendmentArticle.line;
                        }
                        if (this.ChargeArticle != null
                        && this.ChargeArticle.line != null)     // 【手数料の表示】
                        {
                            wlines += this.ChargeArticle.line;
                        }
                        return wlines;
                    }
                    catch (Exception e)
                    {
                        return "";
                    }
                }
            }


        }

        // 手続補正ｎの複数
        [XmlRoot(ElementName = "amendment-article", Namespace = "http://www.jpo.go.jp")]
        public class AmendmentArticle
        {
            [XmlElement(ElementName = "amendment-group", Namespace = "http://www.jpo.go.jp")]
            public List<AmendmentGroup> AmendmentGroups { get; set; }
            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    int SerialNumber = 1;
                    foreach (AmendmentGroup AmendmentGroup in this.AmendmentGroups)
                    {
                        wlines += "【手続補正" + Strings.StrConv(SerialNumber.ToString(), VbStrConv.Wide, 0x411) + "】<br />\r\n";
                        wlines += AmendmentGroup.line;
                        SerialNumber++;
                    }
                    return wlines;
                }
            }
        }

        // 手続補正ｎ
        [XmlRoot(ElementName = "amendment-group", Namespace = "http://www.jpo.go.jp")]
        public class AmendmentGroup
        {
            // 【補正対象書類名】
            [XmlElement(ElementName = "document-code", Namespace = "http://www.jpo.go.jp")]
            public string DocumentCode { get; set; }

            // 【補正対象項目名】
            [XmlElement(ElementName = "item-of-amendment", Namespace = "http://www.jpo.go.jp")]
            public string ItemOfAmendment { get; set; }

            // 【補正方法】
            [XmlElement(ElementName = "way-of-amendment", Namespace = "http://www.jpo.go.jp")]
            public string WayOfAmendment { get; set; }

            // 【補正の内容】
            [XmlElement(ElementName = "contents-of-amendment", Namespace = "http://www.jpo.go.jp")]
            public XmlElement ContentsOfAmendment { get; set; }

            [System.Xml.Serialization.XmlAttribute(AttributeName = "serial-number", Namespace = "http://www.jpo.go.jp")]
            public string SerialNumber { get; set; }

            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    if (this.DocumentCode != null)
                    {
                        wlines += "　【補正対象書類名】　" + document_code2desc(this.DocumentCode) + "<br />\r\n";
                    }
                    if (this.ItemOfAmendment != null)
                    {
                        wlines += "　【補正対象項目名】　" + this.ItemOfAmendment + "<br />\r\n";
                    }
                    if (this.WayOfAmendment != null)
                    {
                        switch (this.WayOfAmendment)
                        {
                            case "1":
                                wlines += "　【補正方法】　追加<br />\r\n";
                                break;
                            case "2":
                                wlines += "　【補正方法】　削除<br />\r\n";
                                break;
                            case "3":
                                wlines += "　【補正方法】　変更<br />\r\n";
                                break;
                        }
                        if (this.WayOfAmendment != "2"
                        && this.ContentsOfAmendment != null)
                        {
                            wlines += "　【補正の内容】<br />\r\n";
                            Claims claims = new Claims(this.ContentsOfAmendment.OuterXml, m_s_xmlPath);
                            if (claims.m_claims != null)
                            {
                                wlines += claims.line;
                            }
                            else if (this.ContentsOfAmendment.LocalName == "claim")
                            {
                                wlines += "【請求項" + Strings.StrConv(this.ContentsOfAmendment.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】";
                                wlines += element2html(this.ContentsOfAmendment);
                            }
                            else
                            {
                                Description description = new Description(this.ContentsOfAmendment.OuterXml, m_s_xmlPath);
                                if (description.m_description != null)
                                {
                                    wlines += description.line;
                                }
                                else if (this.ContentsOfAmendment.LocalName == "invention-title")
                                {
                                    wlines += "【発明の名称】" + this.ContentsOfAmendment.InnerText + "<br />\r\n";
                                }
                                else if (this.ContentsOfAmendment.LocalName == "p")
                                {
                                    wlines += "【" + Strings.StrConv(this.ContentsOfAmendment.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】<br />\r\n";
                                    wlines += element2html(this.ContentsOfAmendment);
                                }
                            }
                        }
                    }
                    return wlines;
                }
            }
        }

        // 【手数料の表示】
        [XmlRoot(ElementName = "charge-article", Namespace = "http://www.jpo.go.jp")]
        public class ChargeArticle
        {
            [XmlElement(ElementName = "payment", Namespace = "http://www.jpo.go.jp")]
            public Payment Payment { get; set; }
            public string line
            {
                get
                {
                    string wlines = "【手数料の表示】<br />\r\n";
                    if (this.Payment != null)
                    {
                        wlines += this.Payment.line;
                    }
                    return wlines;
                }
            }
        }

        // 
        [XmlRoot(ElementName = "payment", Namespace = "http://www.jpo.go.jp")]
        public class Payment
        {
            [XmlElement(ElementName = "fee", Namespace = "http://www.jpo.go.jp")]
            public Fee Fee { get; set; }

            [XmlElement(ElementName = "account", Namespace = "http://www.jpo.go.jp")]
            public Account Account { get; set; }

            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    if (this.Account != null)
                    {
                        wlines += this.Account.line;
                    }
                    if (this.Fee != null)
                    {
                        wlines += this.Fee.line;
                    }
                    return wlines;
                }
            }
        }

        // 【予納台帳番号】
        [XmlRoot(ElementName = "account", Namespace = "http://www.jpo.go.jp")]
        public class Account
        {
            [XmlAttribute(AttributeName = "number")]
            public string Number { get; set; }

            [XmlAttribute(AttributeName = "account-type")]
            public string AccountType { get; set; }
            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    switch (AccountType)
                    {
                        case "credit-card":
                            wlines += "　　【指定立替納付】<br />\r\n";
                            break;
                        case "transfer":
                            wlines += "　　【振替番号】　　　" + Strings.StrConv(this.Number, VbStrConv.Wide, 0x411) + "<br />\r\n";
                            break;
                        case "deposit":
                            wlines += "　　【予納台帳番号】　" + Strings.StrConv(this.Number, VbStrConv.Wide, 0x411) + "<br />\r\n";
                            break;
                    }
                    return wlines;
                }
            }
        }
        // 【納付金額】
        [XmlRoot(ElementName = "fee", Namespace = "http://www.jpo.go.jp")]
        public class Fee
        {
            [XmlAttribute(AttributeName = "amount")]
            public string Amount { get; set; }

            [XmlAttribute(AttributeName = "currency")]
            public string Currency { get; set; }

            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    if (this.Amount != null)
                    {
                        wlines += "　　【納付金額】　　　" + Strings.StrConv(this.Amount, VbStrConv.Wide, 0x411) + "<br />\r\n";
                    }
                    return wlines;
                }
            }
        }
        public CPatAmnd m_patAmnd { get; set; }
        public string m_xmlPath { get; set; }

        public string m_title { get; set; }
        public PatAmnd(string szXml,string szXmlPath, string aLegalDate = "")
        {
            try
            {
                this.m_xmlPath = szXmlPath;
                this.m_patAmnd = null;
                this.m_title = "提出日" + aLegalDate + "手続補正書";
                XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(CPatAmnd));
                using (TextReader reader = new StringReader(szXml))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    //settings.CheckCharacters = false;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreComments = true;
                    settings.DtdProcessing = DtdProcessing.Parse;
                    XmlReader xmlReader = XmlReader.Create(reader, settings);
                    m_patAmnd = (CPatAmnd)serializer.Deserialize(xmlReader);
                }
            }
            catch (Exception ex)
            {
                this.m_patAmnd = null;
            }

        }

        public string htmlAll()
        {
            try
            {
                Text2html text2html = new Text2html(this.m_xmlPath);
                if (this.m_patAmnd != null
                && this.m_patAmnd.AmendmentA523 != null)
                {
                    text2html.setTitle(this.m_title);
                    text2html.addP(this.m_patAmnd.AmendmentA523.line);
                    return text2html.htmlAll();
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
                if (this.m_patAmnd != null)
                {
                    text2html.setTitle("手続補正書：タイトルです");
                    text2html.addP("【書類名】　　　　　　手続補正書");
                    if (this.m_patAmnd.AmendmentA523 != null)
                    {
                        if (this.m_patAmnd.AmendmentA523.SubmissionDate != null
                        && this.m_patAmnd.AmendmentA523.SubmissionDate.Date != null)
                        {
                            text2html.addP("【提出日】　　　　　　" + this.m_patAmnd.AmendmentA523.SubmissionDate.Date);
                        }
                        if (this.m_patAmnd.AmendmentA523.FileReferenceId != null)
                        {
                            text2html.addP("【整理番号】　　　　　" + Strings.StrConv(this.m_patAmnd.AmendmentA523.FileReferenceId, VbStrConv.Wide, 0x411));
                        }

                        if (this.m_patAmnd.AmendmentA523.AddressedToPerson != null)
                        {
                            text2html.addP("【あて先】　　　　　　" + this.m_patAmnd.AmendmentA523.AddressedToPerson);
                        }
                        if (this.m_patAmnd.AmendmentA523.IndicationOfCaseArticle != null
                        && this.m_patAmnd.AmendmentA523.IndicationOfCaseArticle.ApplicationReference != null
                        && this.m_patAmnd.AmendmentA523.IndicationOfCaseArticle.ApplicationReference.DocumentId != null
                        && this.m_patAmnd.AmendmentA523.IndicationOfCaseArticle.ApplicationReference.DocumentId.DocNumber != null)
                        {
                            text2html.addP("【事件の表示】");
                            text2html.addP("　　【出願番号】　　　" + this.m_patAmnd.AmendmentA523.IndicationOfCaseArticle.ApplicationReference.DocumentId.DocNumber);
                        }
                        if (this.m_patAmnd.AmendmentA523.Applicants != null
                        && this.m_patAmnd.AmendmentA523.Applicants.Applicant.Count > 0)
                        {
                            foreach (Applicant applicant in this.m_patAmnd.AmendmentA523.Applicants.Applicant)
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
                        if (this.m_patAmnd.AmendmentA523.Agents != null
                        && this.m_patAmnd.AmendmentA523.Agents.Agent.Count > 0)
                        {
                            foreach (Agent agent in this.m_patAmnd.AmendmentA523.Agents.Agent)
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
                        if (this.m_patAmnd.AmendmentA523.DispatchNumber != null)
                        {
                            text2html.addP("【発送番号】　　　　　" + Strings.StrConv(this.m_patAmnd.AmendmentA523.DispatchNumber, VbStrConv.Wide, 0x411));
                        }
                        if (this.m_patAmnd.AmendmentA523.AmendmentArticle != null
                        && this.m_patAmnd.AmendmentA523.AmendmentArticle.AmendmentGroups.Count > 0)
                        {
                            int SerialNumber = 1;
                            foreach(AmendmentGroup amendmentGroup in this.m_patAmnd.AmendmentA523.AmendmentArticle.AmendmentGroups)
                            {
                                text2html.addP("【手続補正" + Strings.StrConv(SerialNumber.ToString(), VbStrConv.Wide, 0x411) + "】");
                                text2html.addP("　【補正対象書類名】　" + amendmentGroup.DocumentCode);
                                text2html.addP("　【補正対象項目名】　" + amendmentGroup.ItemOfAmendment);
                                text2html.addP("　【補正方法】　　　　" + amendmentGroup.WayOfAmendment);
                                text2html.addP("　【補正の内容】");
                                Claims claims = new Claims(amendmentGroup.ContentsOfAmendment.OuterXml, this.m_xmlPath);
                                if (claims.m_claims != null)
                                {
                                    text2html.addP(claims.line);
                                }
                                else 
                                if(claims.m_claim != null)
                                {
                                    text2html.addP(claims.m_claim.line);
                                }
                                else
                                {
                                    Description description = new Description(amendmentGroup.ContentsOfAmendment.OuterXml, this.m_xmlPath);
                                    if (description.m_description != null)
                                    {
                                        text2html.addP(description.line);
                                    }
                                }
                                SerialNumber++;
                            }
                        }
                        if (this.m_patAmnd.AmendmentA523.ChargeArticle != null)
                        {
                            text2html.addP("【手数料の表示】");
                            if (this.m_patAmnd.AmendmentA523.ChargeArticle.Payment != null)
                            {
                                if (this.m_patAmnd.AmendmentA523.ChargeArticle.Payment.Account != null
                                && this.m_patAmnd.AmendmentA523.ChargeArticle.Payment.Account.Number != null)
                                {
                                    text2html.addP("　　【予納台帳番号】　" + Strings.StrConv(this.m_patAmnd.AmendmentA523.ChargeArticle.Payment.Account.Number, VbStrConv.Wide, 0x411));
                                }
                                if (this.m_patAmnd.AmendmentA523.ChargeArticle.Payment.Fee != null
                                && this.m_patAmnd.AmendmentA523.ChargeArticle.Payment.Fee.Amount != null)
                                {
                                    text2html.addP("　　【納付金額】　　　" + Strings.StrConv(this.m_patAmnd.AmendmentA523.ChargeArticle.Payment.Fee.Amount, VbStrConv.Wide, 0x411));
                                }
                            }
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
        // ~patAmnd()
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
