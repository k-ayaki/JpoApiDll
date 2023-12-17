using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Office.Interop.Word;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.VisualBasic;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Web.Caching;
using System.Security;
using System.Xml.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace JpoApi
{
    public class Xml2Html : IDisposable
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

        private string m_xmlPath { get; set; }
        public string m_htmlPath { get; set; }
        public string m_dirName { get; set; }
        public string m_Date { get; set; }      // 提出日・起案日
        public string m_DocNumber { get; set; } // 出願番号
        public string m_DocumentName { get; set; }  // 文書名
        public string m_DocNumber2 { get; set; } // 出願番号 （外部指定）
        public string m_title { get; set; }     // htmlのタイトル

        private string[] m_dirNames;
        private XmlDocument m_xDoc { get; set; }
        private XmlNamespaceManager m_xmlNsManager { get; set; }
        public string m_provisions { get; set; }

        public Xml2Html(string a_xmlPath, string a_DocNumber)
        {
            try
            {
                this.m_Date = "";
                this.m_error = e_NONE;
                this.m_dirName = System.IO.Path.GetDirectoryName(a_xmlPath);
                this.m_dirNames = a_xmlPath.Split('\\');
                this.m_htmlPath = this.m_dirName + @"\" + Path.GetFileNameWithoutExtension(a_xmlPath) + ".html";
                this.m_xmlPath = a_xmlPath;
                this.m_DocNumber2 = a_DocNumber;
                this.m_provisions = "";

                string wHtmlbody = _Xml2Html();
                if (System.IO.File.Exists(this.m_htmlPath))
                {
                    /*
                    if (System.IO.File.GetLastWriteTime(m_htmlPath) == System.IO.File.GetLastWriteTime(m_xmlPath))
                    {
                        return;
                    }
                    */
                    System.IO.File.Delete(this.m_htmlPath);
                }
                System.IO.File.WriteAllText(this.m_htmlPath, wHtmlbody, Encoding.GetEncoding("shift_jis"));
                if (System.IO.File.Exists(this.m_htmlPath))
                {
                    System.IO.File.SetLastWriteTime(this.m_htmlPath, System.IO.File.GetLastWriteTime(this.m_xmlPath));
                    System.IO.File.SetCreationTime(this.m_htmlPath, System.IO.File.GetCreationTime(this.m_xmlPath));
                    System.IO.File.SetLastAccessTime(this.m_htmlPath, System.IO.File.GetLastAccessTime(this.m_xmlPath));
                }
                else
                {
                    this.m_error = this.e_CACHE;
                }
            }
            catch (Exception ex)
            {
                this.m_error = this.e_CACHE;
                return;
            }
        }

        private string _Xml2Html()
        {
            try
            {
                this.m_xDoc = new XmlDocument();
                string wHtmlbody = null;
                this.m_error = e_NONE;
                this.m_xmlNsManager = new XmlNamespaceManager(this.m_xDoc.NameTable);
                this.m_xmlNsManager.AddNamespace("jp", "http://www.jpo.go.jp");

                this.m_xDoc.XmlResolver = null;
                if (System.IO.File.Exists(this.m_xmlPath))
                {
                    this.m_xDoc.Load(this.m_xmlPath);
                }
                else
                {
                    this.m_error = e_CACHE;
                    return wHtmlbody;
                }
                // 拒絶理由通知書・特許査定
                XmlNode node_notice_pat_exam = m_xDoc.SelectSingleNode("//jp:notice-pat-exam", m_xmlNsManager);
                if (node_notice_pat_exam != null)
                {
                    wHtmlbody = html_notice_pat_exam(node_notice_pat_exam, m_xmlNsManager);
                    // 条文の取得
                    XmlNode node_drafting_body = m_xDoc.SelectSingleNode("//jp:drafting-body", m_xmlNsManager);
                    if (node_drafting_body != null)
                    {
                        this.m_provisions = this.provisions(node_drafting_body, m_xmlNsManager);
                    }
                }
                XmlNode node_notice_pat_exam_m = m_xDoc.SelectSingleNode("//jp:notice-pat-exam-rn", m_xmlNsManager);
                if (node_notice_pat_exam_m != null)
                {
                    wHtmlbody = html_notice_pat_exam(node_notice_pat_exam_m, m_xmlNsManager);
                    // 条文の取得
                    XmlNode node_drafting_body = m_xDoc.SelectSingleNode("//jp:drafting-body", m_xmlNsManager);
                    if (node_drafting_body != null)
                    {
                        this.m_provisions = this.provisions(node_drafting_body, m_xmlNsManager);
                    }
                }
                // 手続補正書
                XmlNode node_pat_amnd = m_xDoc.SelectSingleNode("//jp:pat-amnd", m_xmlNsManager);
                if (node_pat_amnd != null)
                {
                    wHtmlbody = html_pat_amnd(node_pat_amnd, m_xmlNsManager);
                }
                // 意見書
                XmlNode node_pat_rspns = m_xDoc.SelectSingleNode("//jp:pat-rspns", m_xmlNsManager);
                if (node_pat_rspns != null)
                {
                    wHtmlbody = html_pat_rspns(node_pat_rspns, m_xmlNsManager);
                }
                // 添付書類
                XmlNode node_attaching_document = m_xDoc.SelectSingleNode("//jp:attaching-document", m_xmlNsManager);
                if (node_attaching_document != null)
                {
                    wHtmlbody = html_attaching_document(node_attaching_document, m_xmlNsManager);
                }
                return wHtmlbody;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                this.m_error = this.e_CACHE;
                return "";
            }
            catch (System.UnauthorizedAccessException ex)
            {
                this.m_error = this.e_CACHE;
                return "";
            }
        }
        // 手続補正書への変換
        private string html_pat_amnd(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                this.m_title = title_amendment(node, xmlNsManager);
                string wHtmlbody = "<html>";
                wHtmlbody += html_head(this.m_title);
                string wHtmlbody2 = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    wHtmlbody2 += html_amendment(node2, xmlNsManager);
                }
                wHtmlbody += html_body(wHtmlbody2);
                wHtmlbody += "</html>\r\n";
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private string html_head(string szTitle)
        {
            string head = "<html>\r\n"
                + "\r\n"
                + "<head>\r\n"
                + "<title>" + szTitle + "</title>\r\n"
                + "<meta http-equiv=Content-Type content=\"text/html; charset=shift_jis\">\r\n" 
                + "<meta name=Generator content=\"Microsoft Word 15 (filtered)\">\r\n"
                + "<style>\r\n"
                + "<!--\r\n"
                + " /* Font Definitions */\r\n"
                + "@font-face\r\n"
                + "\t{font-family:\"ＭＳ 明朝\";\r\n"
                + "\tpanose-1:2 2 6 9 4 2 5 8 3 4;}\r\n"
                + "@font-face\r\n"
                + "\t{font-family:\"Cambria Math\";\r\n"
                + "\tpanose-1:2 4 5 3 5 4 6 3 2 4;}\r\n"
                + "@font-face\r\n"
                + "\t{font-family:\"ＭＳ Ｐゴシック\";\r\n"
                + "\tpanose-1:2 11 6 0 7 2 5 8 2 4;}\r\n"
                + "@font-face\r\n"
                + "\t{font-family:ＭＳ明朝;\r\n"
                + "\tpanose-1:0 0 0 0 0 0 0 0 0 0;}\r\n"
                + "@font-face\r\n"
                + "\t{font-family:\"\\@ＭＳ Ｐゴシック\";\r\n"
                + "\tpanose-1:2 11 6 0 7 2 5 8 2 4;}\r\n"
                + "@font-face\r\n"
                + "\t{font-family:\"\\@ＭＳ 明朝\";\r\n"
                + "\tpanose-1:2 2 6 9 4 2 5 8 3 4;}\r\n"
                + "@font-face\r\n"
                + "\t{font-family:\"\\@ＭＳ明朝\";\r\n"
                + "\tpanose-1:0 0 0 0 0 0 0 0 0 0;}\r\n"
                + " /* Style Definitions */\r\n"
                + " p\r\n"
                + "\t{margin-right:0mm;\r\n"
                + "\tmargin-left:0mm;\r\n"
                + "\tfont-size:12.0pt;\r\n"
                + "\tfont-family:\"ＭＳ Ｐゴシック\";\r\n"
                + "\tcolor:black;}\r\n"
                + ".MsoChpDefault\r\n"
                + "\t{font-family:\"游明朝\",serif;}\r\n"
                + " /* Page Definitions */\r\n"
                + " @page WordSection1\r\n"
                + "\t{size:595.25pt 841.85pt;\r\n"
                + "\tmargin:20.0mm 20.0mm 42.5pt 20.0mm;\r\n"
                + "\tlayout-grid:16.3pt .05pt;}\r\n"
                + "div.WordSection1\r\n"
                + "\t{page:WordSection1;}\r\n"
                + "-->\r\n"
                + "</style>\r\n"
                + "\r\n"
                + "</head>\r\n\r\n";
            return head;
        }
        private string html_body(string contents)
        {
            string body = "<body lang=JA style='word-wrap:break-word'>\r\n"
                        + "\r\n"
                        + "<div class=WordSection1 style='layout-grid:16.3pt .05pt'>\r\n"
                        + "\r\n"
                        + "<div>\r\n";
            body += contents;
            body += "</div>\r\n"
                    + "\r\n"
                    + "</div>\r\n"
                    + "\r\n"
                    + "</body>\r\n\r\n";
            return body;
        }

        private string html_p(string szText)
        {
            szText = szText.Replace("\r\n", "");
            string[] separatingStrings = { "<br />" };
            string[] lines = szText.Split(separatingStrings, System.StringSplitOptions.None);
            string p = "";
            foreach(string line in lines)
            {
                p += "<p style='margin:0mm;line-height:14.8pt;punctuation-wrap:simple;vertical-align:\r\n"
                   + "baseline;word-break:break-all'>";
                if (line.Length > 0)
                {
                    p += "<span style='font-family:\"ＭＳ 明朝\",serif'>";
                    p += line;
                }
                else
                {
                    p += "<span lang=EN-US style='color:windowtext'>&nbsp;";
                }
                p += "</span></p>\r\n\r\n";
            }
            return p;
        }
        private string html_img(string szImg)
        {
            string p = "<p style='punctuation-wrap:simple;word-break:break-all'><span lang=X-NONE\r\n"
                    + "style='font-size:10.5pt;font-family:\"Arial\",sans-serif;border:none black 1.0pt;\r\n"
                    + "padding:0mm'>";
            p += szImg;
            p += "</span></p>\r\n";
            return p;
        }

        // 意見書への変換
        private string html_pat_rspns(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                this.m_title = title_rspns(node, xmlNsManager);
                string wHtmlbody = "<html>";
                wHtmlbody += html_head(this.m_title);

                string wHtmlbody2 = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    wHtmlbody2 += html_rspns(node2, xmlNsManager);
                }
                wHtmlbody += html_body(wHtmlbody2);
                wHtmlbody += "</html>\r\n";
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 添付書類への変換
        private string html_attaching_document(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                this.m_title = title_attaching_document(node, xmlNsManager);
                string wHtmlbody = "<html>" + html_head(this.m_title);
                string wHtmlbody2 = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    wHtmlbody2 += attaching_document(node2, xmlNsManager);
                }
                wHtmlbody += html_body(wHtmlbody2) + "</html>";
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        // 手続補正書の名称
        private string title_amendment(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wTitle = "";
                if (node == null)
                {
                    return wTitle;
                }
                // ドキュメント名称
                XmlNode node_document_code = node.SelectSingleNode("//jp:document-code", xmlNsManager);
                if (node_document_code != null)
                {
                    this.m_DocumentName = document_code2desc(node_document_code.InnerText);
                } else
                {
                    this.m_DocumentName = "";
                }

                // 提出日
                XmlNode node_date = node.SelectSingleNode("//jp:submission-date/jp:date", xmlNsManager);
                if (node_date != null)
                {
                    this.m_Date = node_date.InnerText;
                } else
                {
                    this.m_Date = "";
                }
                // 出願番号
                m_DocNumber = m_DocNumber2;
                XmlNode node_application_reference = node.SelectSingleNode("//jp:indication-of-case-article/jp:application-reference", xmlNsManager);
                if (node_application_reference != null)
                {
                    if (node_application_reference.Attributes["appl-type"].Value == "application")
                    {
                        XmlNode node_doc_number = node.SelectSingleNode("//jp:indication-of-case-article/jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                        if (node_doc_number != null)
                        {
                            this.m_DocNumber = node_doc_number.InnerText;
                        }
                    }
                }
                if (this.m_DocNumber != null && this.m_DocNumber.Length > 0)
                {
                    wTitle = "特願" + this.m_DocNumber + "_起案日" + this.m_Date + "_" + this.m_DocumentName;
                }
                else
                {
                    wTitle = "起案日" + this.m_Date + "_" + this.m_DocumentName;
                }
                if (this.m_dirNames.Length >= 2)
                {
                    wTitle += this.m_dirNames[this.m_dirNames.Length - 2];
                }
                return wTitle;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                return "";
            }
        }

        // 手続補正書htmlへの変換
        private string html_amendment(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                if(node == null)
                {
                    return wHtmlbody;
                }
                foreach(XmlNode node2 in node.ChildNodes)
                {
                    switch(node2.LocalName)
                    {
                        case "document-code":   // 書類名
                            wHtmlbody += document_code(node2, xmlNsManager);
                            break;
                        case "file-reference-id":   // 整理番号
                            wHtmlbody += file_reference_id(node2, xmlNsManager);
                            break;
                        case "submission-date":     // 提出日
                            wHtmlbody += submission_date(node2, xmlNsManager);
                            break;
                        case "addressed-to-person":
                            wHtmlbody += html_p("【あて先】　　　　　　" + node2.InnerText);
                            break;
                        case "indication-of-case-article":  // 事件名
                            wHtmlbody += indication_of_case_article(node2, xmlNsManager);
                            break;
                        case "applicants":  // 補正をする者
                            wHtmlbody += applicants(node2, xmlNsManager, "補正をする者");
                            break;
                        case "agents":  // 代理人
                            wHtmlbody += agents(node2, xmlNsManager, "代理人");
                            break;
                        case "dispatch-number":
                            wHtmlbody += html_p("【発送番号】　　　　　" + Strings.StrConv(node2.InnerText, VbStrConv.Wide, 0x411));
                            break;
                        case "amendment-article":
                            wHtmlbody += amendment_article(node2, xmlNsManager);
                            break;
                        case "amendment-charge-article":
                            wHtmlbody += amendment_charge_article(node2, xmlNsManager);
                            break;
                        case "charge-article":
                            wHtmlbody += charge_article(node2, xmlNsManager);
                            break;
                        case "dtext":
                            wHtmlbody += dtext(node2, xmlNsManager);
                            break;
                        case "proof-means":
                            wHtmlbody += proof_means(node2, xmlNsManager);
                            break;
                        default:
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private string document_code(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment = "")
        {
            try
            {
                string wHtmlbody = "";
                wHtmlbody += html_p("【書類名】　　　　　　" + document_code2desc(node.InnerText));
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private string file_reference_id(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment = "")
        {
            try
            {
                string wHtmlbody = "";
                wHtmlbody += html_p("【整理番号】　　　　　" + Strings.StrConv(node.InnerText, VbStrConv.Wide, 0x411));
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private string indication_of_case_article(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment = "")
        {
            try
            {
                string wHtmlbody = "";
                wHtmlbody += html_p("【事件の表示】");
                XmlNode node_appeal_reference = node.SelectSingleNode("jp:appeal-reference/jp:doc-number", xmlNsManager);
                if (node_appeal_reference != null)
                {
                    string doc_number = Microsoft.VisualBasic.Strings.StrConv(node_appeal_reference.InnerText, VbStrConv.Wide, 0x411);
                    wHtmlbody += html_p("　　【審判番号】　　　不服" + doc_number.Substring(0, 4) + "－" + doc_number.Substring(4));
                }
                XmlNode node_applicaton_reference = node.SelectSingleNode("jp:application-reference", xmlNsManager);
                XmlNode node_doc_number = node.SelectSingleNode("jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                if (node_doc_number != null)
                {
                    switch (node_applicaton_reference.Attributes["appl-type"].Value)
                    {
                        case "international-application":
                            string international_application_number = node_doc_number.InnerText;
                            wHtmlbody += html_p("　　【国際出願番号】　PCT/" + international_application_number.Substring(0, 6) + "/" + international_application_number.Substring(6));
                            wHtmlbody += html_p("　　【出願の区分】　　特許");
                            break;
                        case "application":
                            string doc_number = Microsoft.VisualBasic.Strings.StrConv(node_doc_number.InnerText, VbStrConv.Wide, 0x411);
                            wHtmlbody += html_p("　　【出願番号】　　　特願" + doc_number.Substring(0, 4) + "－" + doc_number.Substring(4));
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private string charge_article(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                XmlNode node_payment = node.SelectSingleNode("jp:charge-article/jp:payment", xmlNsManager);
                if (node_payment != null)
                {
                    wHtmlbody += html_p("【手数料の表示】");
                    wHtmlbody += payment(node_payment, xmlNsManager);
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private string payment(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                string amount = "";
                string number = "";
                string account_type = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.LocalName)
                    {
                        case "fee":
                            amount = node2.Attributes["amount"].Value;
                            break;
                        case "account":
                            number = node2.Attributes["number"].Value;
                            account_type = node2.Attributes["account-type"].Value;
                            break;
                        default:
                            break;
                    }
                }
                switch (account_type)
                {
                    case "credit-card":
                        wHtmlbody += html_p("　　【指定立替納付】");
                        break;
                    case "transfer":
                        wHtmlbody += html_p("　　【振替番号】　　　" + Strings.StrConv(number, VbStrConv.Wide, 0x411));
                        break;
                }
                if (amount.Length > 0)
                {
                    wHtmlbody += html_p("　　【納付金額】　　　" + Strings.StrConv(amount, VbStrConv.Wide, 0x411));
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 手続補正書・【手続補正N】 のhtmlへの変換
        private string amendment_article(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                string w_item_of_amendment = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    wHtmlbody += html_p("【手続補正" + Strings.StrConv(node2.Attributes["jp:serial-number"].Value, VbStrConv.Wide, 0x411) + "】");
                    foreach (XmlNode node3 in node2.ChildNodes)
                    {
                        switch (node3.LocalName)
                        {
                            case "document-code":
                                wHtmlbody += html_p("　【補正対象書類名】　" + document_code2desc(node3.InnerText));
                                break;
                            case "item-of-amendment":
                                wHtmlbody += html_p("　【補正対象項目名】　" + node3.InnerText);
                                w_item_of_amendment = node3.InnerText;
                                break;
                            case "way-of-amendment":
                                wHtmlbody += html_p("　【補正方法】　　　　" + way_of_amendment(node3.InnerText));
                                break;
                            case "contents-of-amendment":
                                wHtmlbody += html_p("　【補正の内容】");
                                switch (node3.Attributes["jp:kind-of-document"].Value)
                                {
                                    case "claims":
                                        wHtmlbody += claims(node3, xmlNsManager);
                                        break;
                                    case "description":
                                        wHtmlbody += description(node3, xmlNsManager);
                                        break;
                                    case "drawings":
                                        wHtmlbody += drawings(node3, xmlNsManager, w_item_of_amendment);
                                        break;
                                    case "abstract":
                                        wHtmlbody += amd_abstract(node3, xmlNsManager, w_item_of_amendment);
                                        break;
                                    default:
                                        wHtmlbody += contents_of_amendment(node3, xmlNsManager, w_item_of_amendment);
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private string claims(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                foreach(XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.Name)
                    {
                        case "claims":
                            wHtmlbody += html_p("　　【書類名】特許請求の範囲");
                            wHtmlbody += claims(node2, xmlNsManager);
                            break;
                        case "claim":
                            wHtmlbody += html_p("　　【請求項" + Strings.StrConv(node2.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】");
                            wHtmlbody += claims(node2, xmlNsManager);
                            break;
                        case "claim-text":
                            wHtmlbody += html_p(p2html(node2));
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private string description(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.Name)
                    {
                        case "description":
                            wHtmlbody += html_p("【書類名】明細書");
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "p":
                            wHtmlbody += html_p("　　【" + Strings.StrConv(node2.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】");
                            wHtmlbody += html_p(p2html(node2));
                            break;
                        case "invention-title":
                            wHtmlbody += html_p("【発明の名称】" + p2html(node2));
                            break;
                        case "technical-field":
                            wHtmlbody += html_p("【技術分野】");
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "background-art":
                            wHtmlbody += html_p("【背景技術】");
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "citation-list":
                            wHtmlbody += html_p("【先行技術文献】");
                            foreach (XmlNode node3 in node2.ChildNodes)
                            {
                                switch (node3.Name)
                                {
                                    case "patent-literature":
                                        wHtmlbody += html_p("【特許文献】");
                                        wHtmlbody += description(node3, xmlNsManager);
                                        break;
                                    case "non-patent-literature":
                                        wHtmlbody += html_p("【非特許文献】");
                                        wHtmlbody += description(node3, xmlNsManager);
                                        break;
                                    case "heading":
                                        wHtmlbody += html_p("【" + node2.InnerText + "】");
                                        break;
                                }
                            }
                            break;
                        case "cited-others":
                            wHtmlbody += html_p("【参考文献】");
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "summary-of-invention":
                            wHtmlbody += html_p("【発明の概要】");
                            foreach (XmlNode node3 in node2.ChildNodes)
                            {
                                switch (node3.Name)
                                {
                                    case "tech-problem":
                                        wHtmlbody += html_p("【発明が解決しようとする課題】");
                                        wHtmlbody += description(node3, xmlNsManager);
                                        break;
                                    case "tech-solution":
                                        wHtmlbody += html_p("【課題を解決するための手段】");
                                        wHtmlbody += description(node3, xmlNsManager);
                                        break;
                                    case "advantageous-effects":
                                        wHtmlbody += html_p("【発明の効果】");
                                        wHtmlbody += description(node3, xmlNsManager);
                                        break;
                                    case "heading":
                                        wHtmlbody += html_p("【" + node2.InnerText + "】");
                                        break;
                                }
                            }
                            break;
                        case "description-of-drawings":
                            wHtmlbody += html_p("【図面の簡単な説明】");
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "description-of-embodiments":
                            wHtmlbody += html_p("【発明を実施するための形態】");
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "embodiments-example":
                            if (node2.Attributes.Count == 0 
                            || node2.Attributes["ex-num"].Value == null)
                            {
                                wHtmlbody += html_p("【実施例】");
                            }
                            else
                            {
                                wHtmlbody += html_p("【実施例" + Strings.StrConv(node2.Attributes["ex-num"].Value, VbStrConv.Wide, 0x411) + "】");
                            }
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "best-mode":
                            wHtmlbody += html_p("【発明を実施するための最良の形態】");
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "industrial-applicability":
                            wHtmlbody += html_p("【産業上の利用可能性】");
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "disclosure":
                            wHtmlbody += html_p("【発明の開示】");
                            break;
                        case "reference-to-deposited-biological-material":
                            wHtmlbody += html_p("【受託番号】");
                            break;
                        case "reference-signs-list":
                            wHtmlbody += html_p("【符号の説明】");
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "heading":
                            wHtmlbody += html_p("【" + node2.InnerText + "】");
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private string amd_abstract(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment = "")
        {
            try
            {
                string wHtmlbody = "";
                if (a_item_of_amendment == "全文")
                {
                    wHtmlbody += html_p("　　【書類名】要約書");
                }
                wHtmlbody += html_p(p2html(node));
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        // 補正の内容
        private string contents_of_amendment(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment)
        {
            try
            {
                string wHtmlbody = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.LocalName)
                    {
                        case "file-reference-id":   // 整理番号
                            wHtmlbody += file_reference_id(node2, xmlNsManager, a_item_of_amendment);
                            break;
                        case "submission-date": // 提出日
                            wHtmlbody += submission_date(node2, xmlNsManager, a_item_of_amendment);
                            break;
                        case "ipc-article":     //　国際特許分類
                            wHtmlbody += ipc_article(node2, xmlNsManager, a_item_of_amendment);
                            break;
                        case "addressed-to-person":     // あて先
                            wHtmlbody += html_p("【あて先】　　　　　　" + node2.InnerText);
                            break;
                        case "indication-of-case-article":  // 事件名
                            wHtmlbody += indication_of_case_article(node2, xmlNsManager);
                            break;
                        case "inventors":       // 発明者
                            wHtmlbody += inventors(node2, xmlNsManager, a_item_of_amendment);
                            break;
                        case "applicants":      // 出願人・補正をする者
                            wHtmlbody += applicants(node2, xmlNsManager, a_item_of_amendment);
                            break;
                        case "agents":          // 代理人
                            wHtmlbody += agents(node2, xmlNsManager, a_item_of_amendment);
                            break;
                        case "submission-object-list-article":  // 物件名
                            wHtmlbody += submission_object_list_article(node2, xmlNsManager, a_item_of_amendment);
                            break;
                        case "law-of-industrial-regenerate":
                            wHtmlbody += html_p("　　【" + a_item_of_amendment + "】" + p2html(node2));
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 提出日
        private string submission_date(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment="")
        {
            try
            {
                string wHtmlbody = html_p("【提出日】　　　　　　" + ad2jpCalender(node.InnerText));
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 国際特許分類
        private string ipc_article(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment="")
        {
            try
            {
                string wHtmlbody = "";
                string wItemName = "【国際特許分類】";

                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.LocalName)
                    {
                        case "ipc":
                            wHtmlbody += html_p(wItemName + Strings.StrConv(node2.InnerText, VbStrConv.Wide, 0x411));
                            wItemName = "　　　　　　　　";
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 発明者
        private string inventors(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment = "発明者")
        {
            string wHtmlbody = "";

            foreach (XmlNode node2 in node.ChildNodes)
            {
                switch (node2.LocalName)
                {
                    case "inventor":
                        wHtmlbody += html_p("　　【" + a_item_of_amendment + "】");
                        foreach (XmlNode node3 in node2.ChildNodes)
                        {
                            switch(node3.LocalName)
                            {
                                case "addressbook":
                                    wHtmlbody += addressbook(node3, xmlNsManager);
                                    break;
                            }
                        }
                        break;
                }
            }
            return wHtmlbody;
        }

        // 出願人／権利者／補正をする者
        private string applicants(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment)
        {
            try
            {
                string wHtmlbody = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.LocalName)
                    {
                        case "applicant":
                            wHtmlbody += html_p("【" + a_item_of_amendment + "】");
                            foreach (XmlNode node3 in node2.ChildNodes)
                            {
                                switch (node3.LocalName)
                                {
                                    case "addressbook":
                                        wHtmlbody += addressbook(node3, xmlNsManager);
                                        break;
                                }
                            }
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        // 代理人
        private string agents(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment = "代理人")
        {
            try
            {
                string wHtmlbody = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.LocalName)
                    {
                        case "agent":
                            wHtmlbody += html_p("【" + a_item_of_amendment + "】");
                            foreach (XmlNode node3 in node2.ChildNodes)
                            {
                                switch (node3.LocalName)
                                {
                                    case "addressbook":
                                        wHtmlbody += addressbook(node3, xmlNsManager);
                                        break;
                                }
                            }
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private string submission_object_list_article(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment)
        {
            try
            {
                string wHtmlbody = "";
                XmlNode node_list_group = node.SelectSingleNode("jp:list-group", xmlNsManager);
                if (node_list_group != null)
                {
                    wHtmlbody += html_p("　　【" + a_item_of_amendment + "】");
                    XmlNode node_document_name = node_list_group.SelectSingleNode("jp:document-name", xmlNsManager);
                    XmlNode node_number_of_object = node_list_group.SelectSingleNode("jp:number-of-object", xmlNsManager);
                    if (node_document_name != null
                    && node_number_of_object != null)
                    {
                        wHtmlbody += html_p("　　【物件名】　　　　" + node_document_name.InnerText + "　" + Strings.StrConv(node_number_of_object.InnerText, VbStrConv.Wide, 0x411));
                    }
                    XmlNode node_citation = node_list_group.SelectSingleNode("jp:citation", xmlNsManager);
                    if (node_citation != null)
                    {
                        wHtmlbody += html_p("　　【援用の表示】　　" + p2html(node_citation));
                    }
                    XmlNode node_general_power_of_attorney_id = node_list_group.SelectSingleNode("jp:general-power-of-attorney-id", xmlNsManager);
                    if (node_general_power_of_attorney_id != null)
                    {
                        wHtmlbody += html_p("　　【包括委任状番号】" + Strings.StrConv(node_general_power_of_attorney_id.InnerText, VbStrConv.Wide, 0x411));
                    }
                    XmlNode node_dtext = node_list_group.SelectSingleNode("jp:dtext", xmlNsManager);
                    if (node_dtext != null)
                    {
                        wHtmlbody += html_p("　　【提出物件の特記事項】" + p2html(node_dtext));
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 手数料補正
        private string amendment_charge_article(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                if (node != null)
                {
                    wHtmlbody += html_p("【手数料補正】");
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        switch (node2.LocalName)
                        {
                            case "document-code":
                                wHtmlbody += html_p("　【補正対象書類名】　" + document_code2desc(node2.InnerText));
                                break;
                            case "charge-article":
                                XmlNode node_payment = node2.SelectSingleNode("jp:payment", xmlNsManager);
                                if (node_payment != null)
                                {
                                    XmlNode node_account = node_payment.SelectSingleNode("jp:account", xmlNsManager);
                                    if (node_account != null)
                                    {
                                        if (node_account.Attributes["account-type"].Value == "credit-card")
                                        {
                                            wHtmlbody += html_p("　【指定立替納付】");
                                        }
                                        else
                                        {
                                            wHtmlbody += html_p("　【振替番号】　　　　" + Strings.StrConv(node_account.Attributes["number"].Value, VbStrConv.Wide, 0x411));
                                        }
                                    }
                                    XmlNode node_fee = node_payment.SelectSingleNode("jp:fee", xmlNsManager);
                                    if (node_fee != null)
                                    {
                                        wHtmlbody += html_p("　　【納付金額】　　　" + Strings.StrConv(node_fee.Attributes["amount"].Value, VbStrConv.Wide, 0x411));
                                    }
                                }
                                break;
                        }
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        // 図
        private string drawings(XmlNode node, XmlNamespaceManager xmlNsManager, string a_item_of_amendment = "")
        {
            try
            {
                string wHtmlbody = "";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch(node2.LocalName)
                    {
                        case "drawings":
                            if (a_item_of_amendment == "全図")
                            {
                                wHtmlbody += html_p("　　【書類名】図面");
                                a_item_of_amendment = "";
                            }
                            wHtmlbody += drawings(node2, xmlNsManager, a_item_of_amendment);
                            break;
                        case "figure":
                            wHtmlbody += html_p("　　【図" + Strings.StrConv(node2.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】");
                            wHtmlbody += html_p(p2html(node2));
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 意見書の名称
        private string title_rspns(XmlNode node_rspns, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wTitle = "";
                if (node_rspns == null)
                {
                    return wTitle;
                }
                XmlNode node_document_code = node_rspns.SelectSingleNode("//jp:document-code", xmlNsManager);
                if (node_document_code != null)
                {
                    this.m_DocumentName = document_code2desc(node_document_code.InnerText);
                } else
                {
                    this.m_DocumentName = "";
                }
                XmlNode node_date = node_rspns.SelectSingleNode("//jp:submission-date/jp:date", xmlNsManager);
                if (node_date != null)
                {
                    this.m_Date = node_date.InnerText;
                }
                else
                {
                    this.m_Date = "";
                }
                // 出願番号
                this.m_DocNumber = this.m_DocNumber2;
                XmlNode node_application_reference = node_rspns.SelectSingleNode("//jp:indication-of-case-article/jp:application-reference", xmlNsManager);
                if (node_application_reference != null)
                {
                    if (node_application_reference.Attributes["appl-type"].Value == "application")
                    {
                        XmlNode node_doc_number = node_rspns.SelectSingleNode("//jp:indication-of-case-article/jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                        if (node_doc_number != null)
                        {
                            this.m_DocNumber = node_doc_number.InnerText;
                        }
                    }
                }
                if (this.m_DocNumber != null && this.m_DocNumber.Length > 0)
                {
                    wTitle = "特願" + this.m_DocNumber + "_起案日" + this.m_Date + "_" + this.m_DocumentName;
                }
                else
                {
                    wTitle = "起案日" + this.m_Date + "_" + this.m_DocumentName;
                }
                if (this.m_dirNames.Length >= 2)
                {
                    wTitle += this.m_dirNames[this.m_dirNames.Length - 2];
                }
                return wTitle;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 意見書htmlの生成
        private string html_rspns(XmlNode node_rspns, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                if(node_rspns==null)
                {
                    return wHtmlbody;
                }
                foreach (XmlNode node in node_rspns.ChildNodes)
                {
                    switch (node.LocalName)
                    {
                        case "document-code":
                            wHtmlbody += document_code(node, xmlNsManager);
                            break;
                        case "file-reference-id":
                            wHtmlbody += file_reference_id(node, xmlNsManager);
                            break;

                        case "submission-date":
                            wHtmlbody += submission_date(node, xmlNsManager);
                            break;
                        case "addressed-to-person":
                            wHtmlbody += html_p("【あて先】　　　　　　" + node.InnerText);
                            break;
                        case "indication-of-case-article":
                            wHtmlbody += indication_of_case_article(node, xmlNsManager);
                            break;
                        case "applicants":
                            wHtmlbody += applicants(node, xmlNsManager, "特許出願人");
                            break;
                        case "agents":
                            wHtmlbody += agents(node, xmlNsManager, "代理人");
                            break;
                        case "dispatch-number":
                            wHtmlbody += html_p("【発送番号】　　　　　" + Strings.StrConv(node.InnerText, VbStrConv.Wide, 0x411));
                            break;
                        case "opinion-contents-article":
                            wHtmlbody += opinion_contents_article(node, xmlNsManager);
                            break;
                        case "proof-means":
                            wHtmlbody += proof_means(node, xmlNsManager);
                            break;
                        case "dtext":
                            wHtmlbody += dtext(node, xmlNsManager);
                            break;

                        default:
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 意見の内容
        private string opinion_contents_article(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = html_p("【意見の内容】");
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.LocalName)
                    {
                        case "p":
                            wHtmlbody += html_p(p2html(node2));
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        // 証拠方法
        private string proof_means(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                string wItemName = "【証拠方法】";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.LocalName)
                    {
                        case "p":
                            wHtmlbody += html_p(wItemName + p2html(node2));
                            wItemName = "";
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        // その他
        private string dtext(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                string wItemName = "【その他】";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.LocalName)
                    {
                        case "p":
                            wHtmlbody += html_p(wItemName + p2html(node2));
                            wItemName = "";
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private string ad2jpCalender(string ymdstr)
        {
            DateTime thisDate = new DateTime(int.Parse(ymdstr.Substring(0, 4)), int.Parse(ymdstr.Substring(4, 2)), int.Parse(ymdstr.Substring(6, 2)));
            JapaneseCalendar カレンダー = new JapaneseCalendar();
            string[] 元号名 = { "明治", "大正", "昭和", "平成", "令和","","","" };
            string jymd = 元号名[カレンダー.GetEra(thisDate) - 1] + Strings.StrConv(カレンダー.GetYear(thisDate).ToString(), VbStrConv.Wide, 0x411) + "年";
            jymd += Strings.StrConv(カレンダー.GetMonth(thisDate).ToString(), VbStrConv.Wide, 0x411) + "月";
            jymd += Strings.StrConv(カレンダー.GetDayOfMonth(thisDate).ToString(), VbStrConv.Wide, 0x411) + "日";
            return jymd;
        }
        private string addressbook(XmlNode node_addressbook, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";

                XmlNode node_registered_number = node_addressbook.SelectSingleNode("jp:registered-number", xmlNsManager);
                if(node_registered_number != null)
                {
                    wHtmlbody += html_p("　　【識別番号】　　　" + Strings.StrConv(node_registered_number.InnerText, VbStrConv.Wide, 0x411));
                }
                XmlNode attorney = node_addressbook.ParentNode.SelectSingleNode("jp:attorney", xmlNsManager);
                if (attorney != null)
                {
                    wHtmlbody += html_p("　　【弁理士】");
                }
                XmlNode lawyer = node_addressbook.ParentNode.SelectSingleNode("jp:lawyer", xmlNsManager);
                if (lawyer != null)
                {
                    wHtmlbody += html_p("　　【弁護士】");
                }
                XmlNode node_address_text = node_addressbook.SelectSingleNode("jp:address/jp:text", xmlNsManager);
                if (node_address_text != null)
                {
                    wHtmlbody += html_p("　　【住所又は居所】　" + node_address_text.InnerText);
                }
                XmlNode node_name = node_addressbook.SelectSingleNode("jp:name", xmlNsManager);
                if (node_name != null)
                {
                    wHtmlbody += html_p("　　【氏名又は名称】　" + node_name.InnerText);
                }
                XmlNode node_phone = node_addressbook.SelectSingleNode("jp:phone", xmlNsManager);
                if (node_phone != null)
                {
                    wHtmlbody += html_p("　　【電話番号】　　　" + Strings.StrConv(node_phone.InnerText, VbStrConv.Wide, 0x411));
                }
                XmlNode node_fax = node_addressbook.SelectSingleNode("jp:fax", xmlNsManager);
                if (node_fax != null)
                {
                    wHtmlbody += html_p("　　【ファクシミリ番号】　　　" + Strings.StrConv(node_fax.InnerText, VbStrConv.Wide, 0x411));
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 添付書類の名称
        private string title_attaching_document(XmlNode node_attaching_document, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wTitle = "特願" + this.m_DocNumber2;
                XmlNode node_document_name = node_attaching_document.SelectSingleNode("//jp:document-name", xmlNsManager);
                if (node_document_name != null)
                {
                    this.m_DocumentName = node_document_name.InnerText;
                }
                else
                {
                    this.m_DocumentName = "";
                }
                wTitle += "_" + m_DocumentName;
                if (this.m_dirNames.Length >= 2)
                {
                    wTitle += "_" + this.m_dirNames[this.m_dirNames.Length - 2];
                }
                return wTitle;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 添付書類hmlの生成
        private string attaching_document(XmlNode node_attaching_document, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                foreach (XmlNode node in node_attaching_document.ChildNodes)
                {
                    switch (node.LocalName)
                    {
                        case "document-name":
                            wHtmlbody += html_p("　【書類名】　　　　" + node.InnerText);
                            break;
                        case "p":
                            wHtmlbody += html_p(p2html(node));
                            break;
                        default:
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 補正の方法
        private string way_of_amendment(string code)
        {
            switch(code)
            {
                case "1":
                    return "追加";
                case "2":
                    return "削除";
                case "3":
                    return "変更";
                default:
                    return code;
            }
        }
        private string document_code2desc(string code)
        {
            switch (code.Substring(0,1)+code.Substring(2))
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


        // 拒絶理由通知書・特許査定
        private string html_notice_pat_exam(XmlNode node_notice_pat_exam, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                this.m_title = title_notice_pat_exam(node_notice_pat_exam, xmlNsManager);
                string wHtmlbody = "<html>";
                wHtmlbody += html_head(m_title);

                //wHtmlbody += "<title>" + m_title + "</title>";
                //wHtmlbody += "<meta charset=\"shift_jis\"></head><body><div><font face=\"ＭＳ明朝\"><p>\r\n";
                string wHtmlbody2 = "";
                foreach (XmlNode node2 in node_notice_pat_exam.ChildNodes)
                {
                    wHtmlbody2 += notice_pat_exam(node2, xmlNsManager);
                }
                //wHtmlbody += "</p></font></div></body></html>\r\n";
                wHtmlbody += html_body(wHtmlbody2);
                wHtmlbody += "</html>\r\n";
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 拒絶理由通知書のタイトル
        private string title_notice_pat_exam(XmlNode node_notice_pat_exam, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wTitle = "";
                XmlNode node_document_name = node_notice_pat_exam.SelectSingleNode("//jp:document-name", xmlNsManager);
                if(node_document_name != null)
                {
                    this.m_DocumentName = node_document_name.InnerText;
                }
                XmlNode node_doc_number = node_notice_pat_exam.SelectSingleNode("//jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                if(node_doc_number != null)
                {
                    this.m_DocNumber = node_doc_number.InnerText;
                }
                XmlNode node_drafting_date = node_notice_pat_exam.SelectSingleNode("//jp:drafting-date/jp:date", xmlNsManager);
                if(node_drafting_date != null)
                {
                    this.m_Date = node_drafting_date.InnerText;
                }
                wTitle = "特願" + this.m_DocNumber + "_起案日" + this.m_Date + "_" + this.m_DocumentName;
                return wTitle;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 拒絶理由通知書のhtml
        private string notice_pat_exam(XmlNode node_notice_of_rejection, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                foreach (XmlNode node in node_notice_of_rejection.ChildNodes)
                {
                    switch (node.LocalName)
                    {
                        case "document-name":
                            wHtmlbody += html_p(centering(node.InnerText));
                            break;
                        case "bibliog-in-ntc-pat-exam":
                        case "bibliog-in-ntc-pat-exam-rn":
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += bibliog_in_ntc_pat_exam(node, xmlNsManager);
                            break;
                        case "conclusion-part-article":
                            {
                                wHtmlbody += html_p("");
                                wHtmlbody += html_p("");
                                wHtmlbody += html_p("");
                                wHtmlbody += html_p("");
                                foreach (XmlNode node2 in node.SelectNodes("p", xmlNsManager))
                                {
                                    wHtmlbody += html_p(p2html(node2));
                                }
                            }
                            break;
                        case "drafting-body":
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            foreach (XmlNode node2 in node.SelectNodes("p", xmlNsManager))
                            {
                                wHtmlbody += html_p(p2html(node2));
                            }
                            break;
                        case "footer-article":
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += footer_article(node, xmlNsManager);
                            break;
                        case "final-decision-group":
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += final_decision_group(node, xmlNsManager);
                            break;
                        case "final-decision-memo":
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += final_decision_memo(node, xmlNsManager);
                            break;
                        default:
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 特許査定の種別等
        private string final_decision_memo(XmlNode node_final_decision_memo, XmlNamespaceManager xmlNsManager)
        {
            string wHtmlbody = "";

            foreach (XmlNode node in node_final_decision_memo.ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "document-name":
                        wHtmlbody += html_p("　　　　　　　　　　　　" + node.InnerText);
                        break;
                    case "final-decision-bibliog":
                        XmlNode node_doc_number = node.SelectSingleNode("//jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                        if (node_doc_number != null)
                        {
                            string docNumber = node_doc_number.InnerText.Substring(0, 4) + "-" + node_doc_number.InnerText.Substring(4, 6);
                            docNumber = Strings.StrConv(docNumber, VbStrConv.Wide, 0x411);
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("");
                            wHtmlbody += html_p("　特許出願の番号　　　　　　特願" + docNumber);
                        }
                        break;
                    case "final-decision-body":
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("１．調査した分野（ＩＰＣ，ＤＢ名）");
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("");
                        foreach (XmlNode node2 in node.ChildNodes)
                        {
                            switch(node2.LocalName)
                            {
                                case "field-of-search-article":
                                    foreach (XmlNode node3 in node2.ChildNodes)
                                    {
                                        if (node3.LocalName == "field-of-search")
                                        {
                                            wHtmlbody += html_p("　" + node3.InnerText);
                                        }
                                    }
                                    break;
                                case "patent-reference-article":
                                    wHtmlbody += html_p("");
                                    wHtmlbody += html_p("");
                                    wHtmlbody += html_p("");
                                    wHtmlbody += html_p("");
                                    wHtmlbody += html_p("２．参考特許文献");
                                    wHtmlbody += html_p("");
                                    wHtmlbody += html_p("");
                                    foreach (XmlNode node3 in node2.ChildNodes)
                                    {
                                        if (node3.LocalName == "patent-reference-group")
                                        {
                                            string szLine = "";
                                            foreach (XmlNode node4 in node3.ChildNodes)
                                            {
                                                switch (node4.LocalName)
                                                {
                                                    case "document-number":
                                                        szLine = "　" + (node4.InnerText + "　　　　　　　　　　　　　　　　　　　　　　　　　　").Substring(0, 26);
                                                        break;
                                                    case "kind-of-document":
                                                        szLine += node4.InnerText;
                                                        wHtmlbody += html_p(szLine);
                                                        szLine = "";
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    case "reference-books-article":
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("３．参考図書雑誌");
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("");
                        break;
                    default:
                        break;
                }
            }
            return wHtmlbody;
        }

        // 特許査定の種別等
        private string final_decision_group(XmlNode node_footer_article, XmlNamespaceManager xmlNsManager)
        {
            string wHtmlbody = "";
            string szLine = "";

            foreach (XmlNode node in node_footer_article.ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "kind-of-application":
                        wHtmlbody += html_p("１．出願種別　　　　　　　　" + node.InnerText);
                        break;
                    case "exist-of-reference-doc":
                        szLine += "２．参考文献　　　　　　　　";
                        if (node.Attributes["jp:true-or-false"].Value == "true")
                        {
                            szLine += "有";
                        }
                        else
                        {
                            szLine += "無";
                        }
                        wHtmlbody += html_p(szLine);
                        break;
                    case "patent-law-section30":
                        szLine += "３．特許法第３０条適用　　　";
                        if (node.Attributes["jp:true-or-false"].Value == "true")
                        {
                            szLine += "有";
                        }
                        else
                        {
                            szLine += "無";
                        }
                        wHtmlbody += html_p(szLine);
                        break;
                    case "change-flag-invention-title":
                        szLine += "４．発明の名称の変更　　　　";
                        if (node.Attributes["jp:true-or-false"].Value == "true")
                        {
                            szLine += "有";
                        }
                        else
                        {
                            szLine += "無";
                        }
                        wHtmlbody += html_p(szLine);
                        break;
                    case "ipc-article":
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("５．国際特許分類（ＩＰＣ）");
                        wHtmlbody += html_p("");
                        wHtmlbody += html_p("");
                        foreach (XmlNode node2 in node.ChildNodes)
                        {
                            switch (node2.LocalName)
                            {
                                case "ipc":
                                    string ipc = node2.InnerText.Replace("\xA0", " ");
                                    ipc = Strings.StrConv(ipc, VbStrConv.Wide, 0x411);
                                    wHtmlbody += html_p("　　　　　　　　　　　　　　　" + ipc);
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return wHtmlbody;
        }

        // 拒絶理由通知書のフッター
        private string footer_article(XmlNode node_footer_article, XmlNamespaceManager xmlNsManager)
        {
            string wHtmlbody = "";
            string[] footer = new string[] { "　", "　", "　", "　" };

            foreach (XmlNode node in node_footer_article.ChildNodes)
            {
                switch(node.LocalName)
                {
                    case "approval-column-article":
                        foreach (XmlNode node2 in node.ChildNodes)
                        {
                            switch(node2.LocalName)
                            {
                                case "staff1-group":
                                case "staff2-group":
                                case "staff3-group":
                                case "staff4-group":
                                    foreach (XmlNode node3 in node2.ChildNodes)
                                    {
                                        switch (node3.LocalName)
                                        {
                                            case "official-title":
                                                footer[0] += "　　　　　　　";
                                                footer[1] += (node3.InnerText + "　　　　　　　").Substring(0, 7);
                                                break;
                                            case "name":
                                                footer[2] += (node3.InnerText + "　　　　　　　").Substring(0, 7);
                                                break;
                                            case "staff-code":
                                                footer[3] += (Strings.StrConv(node3.InnerText, VbStrConv.Wide, 0x411) + "　　　　　　　").Substring(0, 7);
                                                break;
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case "devider":
                        for (int i = 0; i < 4; i++)
                        {
                            footer[i] += "　";
                        }
                        foreach (XmlNode node2 in node.ChildNodes)
                        {
                            switch (node2.LocalName)
                            {
                                case "official-title":
                                    footer[0] += "　　　　　　";
                                    footer[1] += (node2.InnerText + "　　　　　　").Substring(0, 6);
                                    break;
                                case "name":
                                    footer[2] += (node2.InnerText + "　　　　　　").Substring(0, 6);
                                    break;
                                case "staff-code":
                                    footer[3] += (Strings.StrConv(node2.InnerText, VbStrConv.Wide, 0x411) + "　　　　　　").Substring(0, 6);
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            for (int i = 0; i< 4; i++)
            {
                wHtmlbody += html_p(footer[i]);
            }
            return wHtmlbody;
        }

        // 拒絶理由通知書　書誌事項
        private string bibliog_in_ntc_pat_exam(XmlNode node_bibliog_in_ntc_pat_exam, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                foreach (XmlNode node in node_bibliog_in_ntc_pat_exam.ChildNodes)
                {
                    switch (node.LocalName)
                    {
                        case "application-reference":
                            XmlNode node_doc_number = node.SelectSingleNode("jp:document-id/jp:doc-number", xmlNsManager);
                            {
                                string doc_number = Strings.StrConv(node_doc_number.InnerText, VbStrConv.Wide, 0x411);
                                wHtmlbody += html_p("　特許出願の番号　　　　　　特願" + doc_number.Substring(0, 4) + "－" + doc_number.Substring(4));
                            }
                            break;
                        case "drafting-date":
                            XmlNode node_drafting_date = node.SelectSingleNode("jp:date", xmlNsManager);
                            {
                                wHtmlbody += html_p("　起案日　　　　　　　　　　" + ad2jpCalender(node_drafting_date.InnerText));
                            }
                            break;
                        case "draft-person-group":
                            {
                                XmlNode node_name = node.SelectSingleNode("jp:name", xmlNsManager);
                                if(node_name!=null)
                                {
                                    string name = Strings.StrConv(node_name.InnerText, VbStrConv.Wide, 0x411);
                                    string staff_code = "";
                                    string office_code = "";
                                    XmlNode node_staff_code = node.SelectSingleNode("jp:staff-code", xmlNsManager);
                                    if(node_staff_code!=null)
                                    {
                                        staff_code = Strings.StrConv(node_staff_code.InnerText, VbStrConv.Wide, 0x411);
                                    }
                                    XmlNode node_office_code = node.SelectSingleNode("jp:office-code", xmlNsManager);
                                    if (node_office_code != null)
                                    {
                                        office_code = Strings.StrConv(node_office_code.InnerText, VbStrConv.Wide, 0x411);
                                    }
                                    wHtmlbody += html_p("　特許庁審査官　　　　　　　" + name + "　　　　　　　　" + staff_code + "　" + office_code);
                                }
                            }
                            break;
                        case "invention-title":
                            wHtmlbody += html_p("　発明の名称　　　　　　　　" + node.InnerText);
                            break;
                        case "number-of-claim":
                            string numberOfClaim = node.InnerText.Replace("\xA0"," ");
                            numberOfClaim = Strings.StrConv(numberOfClaim, VbStrConv.Wide, 0x411);
                            wHtmlbody += html_p("　請求項の数　　　　　　　　" + numberOfClaim);
                            break;
                        case "addressed-to-person-group":
                            if(node.Attributes["jp:kind-of-person"].Value == "applicant")
                            {
                                XmlNode node_name = node.SelectSingleNode("jp:addressbook/jp:name", xmlNsManager);
                                if(node_name != null)
                                {
                                    wHtmlbody += html_p("　特許出願人　　　　　　　　" + node_name.InnerText);
                                }
                            } else
                            if (node.Attributes["jp:kind-of-person"].Value == "attorney")
                            {
                                XmlNode node_name = node.SelectSingleNode("jp:addressbook/jp:name", xmlNsManager);
                                if (node_name != null)
                                {
                                    wHtmlbody += html_p("　代理人　　　　　　　　　　" + node_name.InnerText);
                                }
                            }
                            break;
                        case "article-group":
                            string article = "";
                            foreach (XmlNode node_article in node.SelectNodes("jp:article", xmlNsManager))
                            {
                                if (article.Length == 0)
                                {
                                    article = node_article.InnerText;
                                }
                                else
                                {
                                    article += "、" + node_article.InnerText;
                                }
                            }
                            wHtmlbody += html_p("　適用条文　　　　　　　　　" + article);
                            break;
                        default:
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private string p2html(XmlNode nodeP)
        {
            string wHtmlbody = "";
            foreach(XmlNode node in nodeP.ChildNodes)
            {
                switch(node.LocalName)
                {
                    case "img":
                        wHtmlbody += "\r\n" + node_img(node);
                        break;
                    case "chemistry":
                        wHtmlbody += "\r\n【化" + Strings.StrConv(node.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】\r\n";
                        wHtmlbody += p2html(node);
                        break;
                    case "tables":
                        wHtmlbody += "\r\n【表" + Strings.StrConv(node.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】\r\n";
                        wHtmlbody += p2html(node);
                        break;
                    case "maths":
                        wHtmlbody += "\r\n【数" + Strings.StrConv(node.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】\r\n";
                        wHtmlbody += p2html(node);
                        break;
                    case "patcit":
                        wHtmlbody += "\r\n　　【特許文献" + Strings.StrConv(node.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】" + node.OuterXml + "<br />\r\n";
                        break;
                    case "nplcit":
                        wHtmlbody += "\r\n　　【非特許文献" + Strings.StrConv(node.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】" + node.OuterXml + "<br />\r\n";
                        break;
                    case "figref":
                        wHtmlbody += "\r\n　　【図" + Strings.StrConv(node.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】" + node.OuterXml + "<br />\r\n";
                        break;
                    case "#text":
                        string szText = HttpUtility.HtmlEncode(node.OuterXml);
                        szText = szText.Replace("&#160;", "&#32;");
                        wHtmlbody += szText;
                        break;
                    default:
                        wHtmlbody += node.OuterXml;
                        break;

                }
            }
            wHtmlbody += "\r\n";
            return wHtmlbody;
        }
        private string node_img(XmlNode node)
        {
            string wHtmlbody = "";
            int height = (int)(3.777 * double.Parse(node.Attributes["he"].Value));
            int width = (int)(3.777 * double.Parse(node.Attributes["wi"].Value));
            string w_src_png = Path.GetFileNameWithoutExtension(node.Attributes["file"].Value) + ".png";
            string w_src1 = m_dirName + @"\" + w_src_png;

            string w_src0 = m_dirName + @"\" + node.Attributes["file"].Value;
            System.Drawing.Image img = System.Drawing.Bitmap.FromFile(w_src0);
            img.Save(w_src1, System.Drawing.Imaging.ImageFormat.Png);
            byte[] dataPng = System.IO.File.ReadAllBytes(w_src1);
            string base64Png = Convert.ToBase64String(dataPng);
            wHtmlbody += "<img height=" + height.ToString() + " width=" + width.ToString() + " src=\"data:image/png;base64," + base64Png + "\"><br />\r\n";
            return wHtmlbody;
        }

        // センタリング処理
        private string centering(string text)
        {
            string repeatedString = "";
            if (text.Length * 2 < 72)
            {
                repeatedString = new string('　', (72 - text.Length * 2) / 4);
            }
            return repeatedString + text;
        }

        // 条文
        private string provisions(XmlNode node_drafting_body, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string provisionsDetail = "";

                string drafting_body = p2html(node_drafting_body);

                // xmlのdrafting-body を改行ごとに区分
                string[] del = { "\r\n" };
                string[] sentences = drafting_body.Split(del, StringSplitOptions.None);

                // 条文列挙部分の接続
                for (int i = 0; i + 1 < sentences.Length; i++)
                {
                    if (sentences[i].IndexOf(@"<br />") == 36
                    && sentences[i].Substring(35, 1) != "。")
                    {
                        sentences[i] = sentences[i].Substring(0, 36);
                        sentences[i] += sentences[i + 1];
                        sentences[i] = sentences[i].Replace("　", "");
                        sentences[i + 1] = "";
                    }
                    if (sentences[i].IndexOf("　記") >= 0)
                    {
                        break;
                    }
                }

                foreach (string sentence in sentences)
                {
                    string sentence2 = sentence.Replace(@"<br />", "");
                    sentence2 = sentence2.Replace("　", "");
                    sentence2 = Strings.StrConv(sentence2, VbStrConv.Wide, 0x411);

                    // 条文列挙部分の抽出
                    if (Regex.IsMatch(sentence2, "この出願(の|は[、，]?)(下記の請求項|請求項[０-９，、]+|特許請求の範囲|発明の詳細な説明|特許請求の範囲又は発明の詳細な説明|明細書|下記)")
                    || Regex.IsMatch(sentence2, "その出願の日前の(日本語)?特許出願であって、")
                    || Regex.IsMatch(sentence2, "[０-９]+年[０-９]+月[０-９]+日付けでした手続補正は[、，]")
                    || sentence2.IndexOf("特許を受けることができない") >= 0
                    || (sentence2.IndexOf("要件を") >= 0 && sentence2.IndexOf("満たしていない") >= 0))
                    {
                        // 括弧部分の除去
                        string sentence3 = "";
                        foreach (Match match0 in Regex.Matches(sentence2, "(?<lv0>[^（]*)?(?<lv1>（[^）]*）)?"))
                        {
                            sentence3 += match0.Groups["lv0"].Value;
                        }

                        string prov1 = "";
                        string lv1 = "";
                        string lv2 = "";
                        string lv3 = "";
                        foreach (Match match in Regex.Matches(sentence3, "(?<prov1>(?<lv1>特許法第?(?<lv11>[０-９]+条(の[０-９]+)?))?(?<lv2>第[０-９]+項(柱書)?)?(?<lv3>(?<lv31>第[０-９]+)(、|，)?(?<lv32>[０-９]+)?号)?)(および|及び|または|又は|亦は|叉は|ならびに?|並びに?|、|，|[のにで](規定|該当))"))
                        {
                            prov1 = match.Groups["prov1"].Value;
                            if (prov1.Length > 0)
                            {
                                if (match.Groups["lv1"].Value.Length > 0)
                                {
                                    lv1 = "特許法第" + match.Groups["lv11"].Value;
                                    lv2 = match.Groups["lv2"].Value;
                                }
                                else
                                if (lv1.Length > 0)
                                {
                                    if (match.Groups["lv2"].Value.Length > 0)
                                    {
                                        lv2 = match.Groups["lv2"].Value;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                                if (match.Groups["lv32"].Value.Length > 0)
                                {
                                    lv3 = match.Groups["lv31"].Value + "号";
                                }
                                else
                                {
                                    lv3 = match.Groups["lv3"].Value;
                                }
                                prov1 = lv1 + lv2 + lv3;
                                if (lv1 != "特許法第４１条")
                                {
                                    if (provisionsDetail.IndexOf(prov1) == -1)
                                    {
                                        if (provisionsDetail.Length > 0)
                                        {
                                            provisionsDetail += ",";
                                        }
                                        provisionsDetail += prov1;
                                    }
                                }
                                if (match.Groups["lv32"].Value.Length > 0
                                && lv1 != "特許法第４１条")
                                {
                                    lv3 = "第" + match.Groups["lv32"].Value + "号";
                                    prov1 = lv1 + lv2 + lv3;
                                    if (provisionsDetail.IndexOf(prov1) == -1)
                                    {
                                        if (provisionsDetail.Length > 0)
                                        {
                                            provisionsDetail += ",";
                                        }
                                        provisionsDetail += prov1;
                                    }
                                }
                            }
                        }
                    }
                }
                return provisionsDetail;
            }
            catch (Exception e)
            {
                return "ErrorProvision";
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
        // ~Xml2Html()
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
