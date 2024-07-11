using DocumentFormat.OpenXml.EMMA;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static JpoApi.PatRspns;

namespace JpoApi
{
    public class Description : IDisposable
    {
        private bool disposedValue;

        public string line
        {
            get
            {
                if (this.m_description != null)
                {
                    return this.m_description.line;
                }
                return null;
            }
        }

        [XmlRoot("description")]
        public class CDescription
        {
            // 【発明の名称】
            [XmlElement("invention-title")]
            public string InventionTitle { get; set; }

            // 【技術分野】
            [XmlElement("technical-field")]
            public Paragraphs TechnicalField { get; set; }

            // 【背景技術】
            [XmlElement("background-art")]
            public Paragraphs BackgroundArt { get; set; }

            // 【先行技術文献】
            [XmlElement("citation-list")]
            public CitationList CitationList { get; set; }

            // 【参考文献】
            [XmlElement("cited-others")]
            public CitationList CitedOthers { get; set; }

            // 【発明の概要】
            [XmlElement("summary-of-invention")]
            public SummaryOfInvention SummaryOfInvention { get; set; }

            // 【図面の簡単な説明】
            [XmlElement("description-of-drawings")]
            public Paragraphs DescriptionOfDrawings { get; set; }

            // 【発明を実施するための形態】
            [XmlElement("description-of-embodiments")]
            public Paragraphs DescriptionOfEmbodiments { get; set; }
            
            // 【発明を実施するための最良の形態】
            [XmlElement("best-mode")]
            public Paragraphs BestMode { get; set; }

            // 【実施例ｎ】
            [XmlElement("embodiments-example")]
            public Paragraphs EmbodimentsExample { get; set; }

            // 【産業上の利用可能性】
            [XmlElement("industrial-applicability")]
            public Paragraphs IndustrialApplicability { get; set; }

            // 【発明の開示】
            [XmlElement("disclosure")]
            public Paragraphs disclosure { get; set; }

            // 【受託番号】
            [XmlElement("reference-to-deposited-biological-material")]
            public Paragraphs referenceToDepositedBiologicalMaterial { get; set; }

            // 【符号の説明】
            [XmlElement("reference-signs-list")]
            public Paragraphs ReferenceSignsList { get; set; }
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
                    string wlines = "【書類名】明細書<br />\r\n";

                    foreach (XmlNode child in _pTag.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            // タグの場合、タグ名を出力
                            switch (child.LocalName)
                            {
                                case "invention-title":
                                    wlines += "【発明の名称】" + this.InventionTitle + "<br />\r\n";
                                    break;
                                case "technical-field":
                                    wlines += "【技術分野】<br />\r\n";
                                    wlines += this.TechnicalField.line;
                                    break;
                                case "background-art":
                                    wlines += "【背景技術】<br />\r\n";
                                    wlines += this.BackgroundArt.line;
                                    break;
                                case "citation-list":
                                    wlines += "【先行技術文献】<br />\r\n";
                                    this.CitationList.m_xml = child.OuterXml;
                                    wlines += this.CitationList.line;
                                    break;
                                case "cited-others":
                                    wlines += "【参考文献】<br />\r\n";
                                    wlines += this.CitedOthers.line;
                                    break;
                                case "summary-of-invention":
                                    wlines += "【発明の概要】<br />\r\n";
                                    this.SummaryOfInvention.m_xml = child.OuterXml;
                                    wlines += this.SummaryOfInvention.line;
                                    break;
                                case "description-of-drawings":
                                    wlines += "【図面の簡単な説明】<br />\r\n";
                                    wlines += this.DescriptionOfDrawings.line;
                                    break;
                                case "description-of-embodiments":
                                    wlines += "【発明を実施するための形態】<br />\r\n";
                                    wlines += this.DescriptionOfEmbodiments.line;
                                    break;
                                case "best-mode":
                                    wlines += "【発明を実施するための最良の形態】<br />\r\n";
                                    wlines += this.BestMode.line;
                                    break;
                                case "embodiments-example":
                                    if (this.EmbodimentsExample.Num == null)
                                    {
                                        wlines += "【実施例】<br />\r\n";
                                    }
                                    else
                                    {
                                        wlines += "【実施例" + Strings.StrConv(EmbodimentsExample.Num, VbStrConv.Wide, 0x411) + "】<br />\r\n";
                                    }
                                    wlines += this.EmbodimentsExample.line;
                                    break;
                                case "industrial-applicability":
                                    wlines += "【産業上の利用可能性】<br />\r\n";
                                    wlines += this.IndustrialApplicability.line;
                                    break;
                                case "disclosure":
                                    wlines += "【発明の開示】<br />\r\n";
                                    wlines += this.disclosure.line;
                                    break;
                                case "reference-to-deposited-biological-material":
                                    wlines += "【受託番号】<br />\r\n";
                                    wlines += this.referenceToDepositedBiologicalMaterial.line;
                                    break;
                                case "reference-signs-list":
                                    wlines += "【符号の説明】<br />\r\n";
                                    wlines += this.ReferenceSignsList.line;
                                    break;
                                case "heading":
                                    wlines += "【" + child.InnerText + "】<br />\r\n";
                                    break;

                            }
                        }
                    }
                    return wlines;
                }
            }
        }

        [XmlRoot("invention-title")]
        public class CInventionTitle
        {
            // 【発明の名称】
            [XmlElement("invention-title")]
            public string InventionTitle { get; set; }

            public string line
            {
                get
                {
                    if (this.InventionTitle != null)
                    {
                        return this.InventionTitle;
                    }
                    return null;
                }
            }
        }

        // 【先行技術文献】
        public class CitationList
        {
            // 【特許文献】
            [XmlElement("patent-literature")]
            public Paragraphs PatentLiterature { get; set; }

            // 【非特許文献】
            [XmlElement("non-patent-literature")]
            public Paragraphs NonPatentLiterature { get; set; }
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
                                case "patent-literature":
                                    wlines += "【特許文献】<br />\r\n";
                                    wlines += this.PatentLiterature.line;
                                    break;
                                case "non-patent-literature":
                                    wlines += "【非特許文献】<br />\r\n";
                                    wlines += this.NonPatentLiterature.line;
                                    break;
                                case "heading":
                                    wlines += "【" + child.InnerText + "】<br />\r\n";
                                    break;
                            }
                        }
                    }
                    return wlines;
                }
            }
        }

        // 【発明の概要】
        public class SummaryOfInvention
        {
            // 【発明が解決しようとする課題】
            [XmlElement("tech-problem")]
            public Paragraphs TechProblem { get; set; }

            // 【課題を解決するための手段】
            [XmlElement("tech-solution")]
            public Paragraphs TechSolution { get; set; }

            // 【発明の効果】
            [XmlElement("advantageous-effects")]
            public Paragraphs AdvantageousEffects { get; set; }

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
                                case "tech-problem":
                                    wlines += "【発明が解決しようとする課題】<br />\r\n";
                                    wlines += this.TechProblem.line;
                                    break;
                                case "tech-solution":
                                    wlines += "【課題を解決するための手段】<br />\r\n";
                                    wlines += this.TechSolution.line;
                                    break;
                                case "advantageous-effects":
                                    wlines += "【発明の効果】<br />\r\n";
                                    wlines += this.AdvantageousEffects.line;
                                    break;
                                case "heading":
                                    wlines += "【" + child.InnerText + "】<br />\r\n";
                                    break;
                            }
                        }
                    }
                    return wlines;
                }
            }
        }

        public class Paragraphs
        {
            [XmlAttribute("num")]
            public string Num { get; set; }

            [XmlAnyElement("p")]
            public List<XmlElement> P { get; set; }

            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    if (this.P != null && this.P.Count > 0)
                    {
                        foreach(XmlElement p in P)
                        {
                            wlines += "【" + Strings.StrConv(p.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】<br />\r\n";
                            wlines += element2html(p) + "<br />\r\n";
                        }
                        return wlines;
                    }
                    return null;
                }
            }
        }
        public class P
        {
            [XmlAttribute("num")]
            public string Num { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        public CInventionTitle m_inventionTitle { get; set; }
        public CDescription m_description { get; set; }

        public string m_xmlPath { get; set; }

        private Text2html m_text2html { get; set; }
        public Description(string szXml, string szXmlPath)
        {
            try
            {
                this.m_xmlPath = szXmlPath;
                this.m_description = null;
                XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(CDescription));
                using (TextReader reader = new StringReader(szXml))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    //settings.CheckCharacters = false;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreComments = true;
                    settings.DtdProcessing = DtdProcessing.Parse;

                    XmlReader xmlReader = XmlReader.Create(reader, settings);
                    m_description = (CDescription)serializer.Deserialize(xmlReader);
                    m_description.m_xml = szXml;
                    return;
                }

            }
            catch (Exception ex)
            {
                this.m_description = null;
            }
        }
        public string htmlAll()
        {
            try
            {
                this.m_text2html = new Text2html(this.m_xmlPath);
                if (this.m_description != null)
                {
                    this.m_text2html.setTitle("明細書：タイトルです");
                    this.m_text2html.addP(this.m_description.line);
                }
                return this.m_text2html.htmlAll();
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
                this.m_text2html = new Text2html(this.m_xmlPath);
                if (this.m_description != null)
                {
                    this.m_text2html.setTitle("明細書：タイトルです");
                    this.m_text2html.addP("【書類名】明細書");
                    if (this.m_description.InventionTitle != null)
                    {
                        this.m_text2html.addP("【発明の名称】" + this.m_description.InventionTitle);
                    }
                    if (this.m_description.TechnicalField != null
                    && this.m_description.TechnicalField.P.Count > 0)
                    {
                        this.m_text2html.addP("【技術分野】");
                        addParagraphs(this.m_description.TechnicalField.P);
                    }
                    if (this.m_description.BackgroundArt != null
                    && this.m_description.BackgroundArt.P.Count > 0)
                    {
                        this.m_text2html.addP("【背景技術】");
                        addParagraphs(this.m_description.BackgroundArt.P);
                    }
                    if (this.m_description.CitationList != null)
                    {
                        this.m_text2html.addP("【先行技術文献】");
                        if (this.m_description.CitationList.PatentLiterature != null)
                        {
                            this.m_text2html.addP("【特許文献】");
                            addParagraphs(this.m_description.CitationList.PatentLiterature.P);
                        }
                        if (this.m_description.CitationList.NonPatentLiterature != null)
                        {
                            this.m_text2html.addP("【非特許文献】");
                            addParagraphs(this.m_description.CitationList.NonPatentLiterature.P);
                        }
                    }
                    if (this.m_description.SummaryOfInvention != null)
                    {
                        this.m_text2html.addP("【発明の概要】");
                        if (this.m_description.SummaryOfInvention.TechProblem != null)
                        {
                            this.m_text2html.addP("【発明が解決しようとする課題】");
                            addParagraphs(this.m_description.SummaryOfInvention.TechProblem.P);
                        }
                        if (this.m_description.SummaryOfInvention.TechProblem != null)
                        {
                            this.m_text2html.addP("【課題を解決するための手段】");
                            addParagraphs(this.m_description.SummaryOfInvention.TechSolution.P);
                        }
                        if (this.m_description.SummaryOfInvention.TechProblem != null)
                        {
                            this.m_text2html.addP("【発明の効果】");
                            addParagraphs(this.m_description.SummaryOfInvention.AdvantageousEffects.P);
                        }
                    }
                    if (this.m_description.DescriptionOfDrawings != null)
                    {
                        this.m_text2html.addP("【図面の簡単な説明】");
                        addParagraphs(this.m_description.DescriptionOfDrawings.P);
                    }
                    if (this.m_description.DescriptionOfEmbodiments != null)
                    {
                        this.m_text2html.addP("【発明を実施するための形態】");
                        addParagraphs(this.m_description.DescriptionOfEmbodiments.P);
                    }
                    if (this.m_description.BestMode != null)
                    {
                        this.m_text2html.addP("【発明を実施するための最良の形態】");
                        addParagraphs(this.m_description.BestMode.P);
                    }
                    if (this.m_description.IndustrialApplicability != null)
                    {
                        this.m_text2html.addP("【産業上の利用可能性】");
                        addParagraphs(this.m_description.IndustrialApplicability.P);
                    }
                    if (this.m_description.disclosure != null)
                    {
                        this.m_text2html.addP("【発明の開示】");
                        addParagraphs(this.m_description.disclosure.P);
                    }
                    if (this.m_description.referenceToDepositedBiologicalMaterial != null)
                    {
                        this.m_text2html.addP("【受託番号】");
                        addParagraphs(this.m_description.referenceToDepositedBiologicalMaterial.P);
                    }
                    if (this.m_description.ReferenceSignsList != null)
                    {
                        this.m_text2html.addP("【符号の説明】");
                        addParagraphs(this.m_description.ReferenceSignsList.P);
                    }
                }
                return this.m_text2html.htmlAll();
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private void addParagraphs(List<XmlElement> paragraphs)
        {
            foreach (XmlElement p in paragraphs)
            {
                this.m_text2html.addP("【" + Strings.StrConv(p.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】");
                //this.m_text2html.addInnerXml(p.InnerXml);
                this.m_text2html.addElement(p);
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
        // ~Description()
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
