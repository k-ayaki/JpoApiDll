﻿using System;
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
        public Xml2Html(string xmlPath, string a_DocNumber)
        {
            try
            {
                m_Date = "";
                m_error = e_NONE;
                m_dirName = System.IO.Path.GetDirectoryName(xmlPath);
                m_dirNames = xmlPath.Split('\\');
                m_htmlPath = m_dirName + @"\" + Path.GetFileNameWithoutExtension(xmlPath) + ".html";
                m_xmlPath = xmlPath;
                m_DocNumber2 = a_DocNumber;

                string wHtmlbody = _Xml2Html();
                if (File.Exists(m_htmlPath))
                {
                    if (File.GetLastWriteTime(m_htmlPath) == File.GetLastWriteTime(m_xmlPath))
                    {
                        return;
                    }
                    File.Delete(m_htmlPath);
                } 
                File.WriteAllText(m_htmlPath, wHtmlbody, Encoding.GetEncoding("shift_jis"));
                File.SetLastWriteTime(m_htmlPath, File.GetLastWriteTime(m_xmlPath));
                File.SetCreationTime(m_htmlPath, File.GetCreationTime(m_xmlPath));
                File.SetLastAccessTime(m_htmlPath, File.GetLastAccessTime(m_xmlPath));
                if (File.Exists(m_htmlPath) == false)
                {
                    m_error = e_CACHE;
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                m_error = e_CACHE;
                return;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                m_error = e_CACHE;
                return;
            }
        }

        private string _Xml2Html()
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                string wHtmlbody = null;
                m_error = e_NONE;
                XmlNamespaceManager xmlNsManager = new XmlNamespaceManager(xDoc.NameTable);
                xmlNsManager.AddNamespace("jp", "http://www.jpo.go.jp");

                xDoc.XmlResolver = null;
                if (File.Exists(m_xmlPath))
                {
                    //string szXml = File.ReadAllText(xmlPath, Encoding.GetEncoding("shift_jis"));
                    //xDoc.LoadXml(szXml);
                    xDoc.Load(m_xmlPath);
                }
                else
                {
                    m_error = e_CACHE;
                    return wHtmlbody;
                }
                // 拒絶理由通知書・特許査定
                XmlNode node_notice_pat_exam = xDoc.SelectSingleNode("//jp:notice-pat-exam", xmlNsManager);
                if (node_notice_pat_exam != null)
                {
                    wHtmlbody = html_notice_pat_exam(node_notice_pat_exam, xmlNsManager);
                }
                // 手続補正書
                XmlNode node_pat_amnd = xDoc.SelectSingleNode("//jp:pat-amnd", xmlNsManager);
                if (node_pat_amnd != null)
                {
                    wHtmlbody = html_pat_amnd(node_pat_amnd, xmlNsManager);
                }
                // 意見書
                XmlNode node_pat_rspns = xDoc.SelectSingleNode("//jp:pat-rspns", xmlNsManager);
                if (node_pat_rspns != null)
                {
                    wHtmlbody = html_pat_rspns(node_pat_rspns, xmlNsManager);
                }
                // 添付書類
                XmlNode node_attaching_document = xDoc.SelectSingleNode("//jp:attaching-document", xmlNsManager);
                if (node_attaching_document != null)
                {
                    wHtmlbody = html_attaching_document(node_attaching_document, xmlNsManager);
                }
                return wHtmlbody;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                m_error = e_CACHE;
                return "";
            }
            catch (System.UnauthorizedAccessException ex)
            {
                m_error = e_CACHE;
                return "";
            }
        }
        // 手続補正書への変換
        private string html_pat_amnd(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                m_title = title_amendment(node, xmlNsManager);
                string wHtmlbody = "<html><head>";
                wHtmlbody += "<title>" + m_title + "</title>\r\n";
                wHtmlbody += "<meta charset=\"shift_jis\"></head><body><div><font face=\"ＭＳ明朝\"><p>\r\n";

                foreach (XmlNode node2 in node.ChildNodes)
                {
                    wHtmlbody += html_amendment(node2, xmlNsManager);
                }
                wHtmlbody += "</p></font></div></body></html>\r\n";
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        // 意見書への変換
        private string html_pat_rspns(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                m_title = title_rspns(node, xmlNsManager);
                string wHtmlbody = "<html><head>";
                wHtmlbody += "<title>" + m_title + "</title>\r\n";
                wHtmlbody += "<meta charset=\"shift_jis\"></head><body><div><font face=\"ＭＳ明朝\"><p>\r\n";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    wHtmlbody += html_rspns(node2, xmlNsManager);
                }
                wHtmlbody += "</p></font></div></body></html>\r\n";
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
                m_title = title_attaching_document(node, xmlNsManager);
                string wHtmlbody = "<html><head>";
                wHtmlbody += "<title>" + m_title + "</title>\r\n";
                wHtmlbody += "<meta charset=\"shift_jis\"></head><body><div><font face=\"ＭＳ明朝\"><p>\r\n";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    wHtmlbody += attaching_document(node2, xmlNsManager);
                }
                wHtmlbody += "</p></font></div></body></html>\r\n";
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
                    m_DocumentName = document_code2desc(node_document_code.InnerText);
                } else
                {
                    m_DocumentName = "";
                }

                // 提出日
                XmlNode node_date = node.SelectSingleNode("//jp:submission-date/jp:date", xmlNsManager);
                if (node_date != null)
                {
                    m_Date = node_date.InnerText;
                } else
                {
                    m_Date = "";
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
                            m_DocNumber = node_doc_number.InnerText;
                        }
                    }
                }
                if(m_DocNumber != null && m_DocNumber.Length > 0)
                {
                    wTitle = "特願" + m_DocNumber + "_起案日" + m_Date + "_" + m_DocumentName;
                }
                else
                {
                    wTitle = "起案日" + m_Date + "_" + m_DocumentName;
                }
                if (m_dirNames.Length >= 2)
                {
                    wTitle += m_dirNames[m_dirNames.Length - 2];
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
                            wHtmlbody += "【あて先】　　　　　　" + node2.InnerText + "<br />\r\n";
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
                            wHtmlbody += "【発送番号】　　　　　" + Strings.StrConv(node2.InnerText, VbStrConv.Wide, 0x411) + "<br />\r\n";
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
                wHtmlbody += "【書類名】　　　　　　" + document_code2desc(node.InnerText) + "<br />\r\n";
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
                wHtmlbody += "【整理番号】　　　　　" + Strings.StrConv(node.InnerText, VbStrConv.Wide, 0x411) + "<br />\r\n";
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
                wHtmlbody += "【事件の表示】<br />\r\n";
                XmlNode node_appeal_reference = node.SelectSingleNode("jp:appeal-reference/jp:doc-number", xmlNsManager);
                if (node_appeal_reference != null)
                {
                    string doc_number = Microsoft.VisualBasic.Strings.StrConv(node_appeal_reference.InnerText, VbStrConv.Wide, 0x411);
                    wHtmlbody += "　　【審判番号】　　　不服" + doc_number.Substring(0, 4) + "－" + doc_number.Substring(4) + "<br />\r\n";
                }
                XmlNode node_applicaton_reference = node.SelectSingleNode("jp:application-reference", xmlNsManager);
                XmlNode node_doc_number = node.SelectSingleNode("jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                if (node_doc_number != null)
                {
                    switch (node_applicaton_reference.Attributes["appl-type"].Value)
                    {
                        case "international-application":
                            string international_application_number = node_doc_number.InnerText;
                            wHtmlbody += "　　【国際出願番号】　PCT/" + international_application_number.Substring(0, 6) + "/" + international_application_number.Substring(6) + "<br />\r\n";
                            wHtmlbody += "　　【出願の区分】　　特許<br />\r\n";
                            break;
                        case "application":
                            string doc_number = Microsoft.VisualBasic.Strings.StrConv(node_doc_number.InnerText, VbStrConv.Wide, 0x411);
                            wHtmlbody += "　　【出願番号】　　　特願" + doc_number.Substring(0, 4) + "－" + doc_number.Substring(4) + "<br />\r\n";
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
                    wHtmlbody += "【手数料の表示】<br />\r\n";
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
                        wHtmlbody += "　　【指定立替納付】<br />\r\n";
                        break;
                    case "transfer":
                        wHtmlbody += "　　【振替番号】　　　" + Strings.StrConv(number, VbStrConv.Wide, 0x411) + "<br />\r\n";
                        break;
                }
                if (amount.Length > 0)
                {
                    wHtmlbody += "　　【納付金額】　　　" + Strings.StrConv(amount, VbStrConv.Wide, 0x411) + "<br />\r\n";
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
                    wHtmlbody += "【手続補正" + Strings.StrConv(node2.Attributes["jp:serial-number"].Value, VbStrConv.Wide, 0x411) + "】<br />\r\n";
                    foreach(XmlNode node3 in node2.ChildNodes)
                    {
                        switch (node3.LocalName)
                        {
                            case "document-code":
                                wHtmlbody += "　【補正対象書類名】　" + document_code2desc(node3.InnerText) + "<br />\r\n";
                                break;
                            case "item-of-amendment":
                                wHtmlbody += "　【補正対象項目名】　" + node3.InnerText + "<br />\r\n";
                                w_item_of_amendment = node3.InnerText;
                                break;
                            case "way-of-amendment":
                                wHtmlbody += "　【補正方法】　　　　" + way_of_amendment(node3.InnerText) + "<br />\r\n";
                                break;
                            case "contents-of-amendment":
                                wHtmlbody += "　【補正の内容】<br />\r\n";
                                switch(node3.Attributes["jp:kind-of-document"].Value)
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
                            wHtmlbody += "　　【書類名】特許請求の範囲<br />\r\n";
                            wHtmlbody += claims(node2, xmlNsManager);
                            break;
                        case "claim":
                            wHtmlbody += "　　【請求項" + Strings.StrConv(node2.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】<br />\r\n";
                            wHtmlbody += claims(node2, xmlNsManager);
                            break;
                        case "claim-text":
                            wHtmlbody += "" + p2html(node2) + "<br />\r\n";
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
                            wHtmlbody += "【書類名】明細書<br />\r\n";
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "p":
                            wHtmlbody += "　　【" + Strings.StrConv(node2.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】<br />\r\n";
                            wHtmlbody += "" + p2html(node2) + "<br />\r\n";
                            break;
                        case "invention-title":
                            wHtmlbody += "【発明の名称】" + p2html(node2) + "<br />\r\n";
                            break;
                        case "technical-field":
                            wHtmlbody += "【技術分野】<br />\r\n";
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "background-art":
                            wHtmlbody += "【背景技術】<br />\r\n";
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "citation-list":
                            wHtmlbody += "【先行技術文献】<br />\r\n";
                            foreach (XmlNode node3 in node2.ChildNodes)
                            {
                                switch (node3.Name)
                                {
                                    case "patent-literature":
                                        wHtmlbody += "【特許文献】<br />\r\n";
                                        wHtmlbody += description(node3, xmlNsManager);
                                        break;
                                    case "non-patent-literature":
                                        wHtmlbody += "【非特許文献】<br />\r\n";
                                        wHtmlbody += description(node3, xmlNsManager);
                                        break;
                                    case "heading":
                                        wHtmlbody += "【" + node2.InnerText + "】<br />\r\n";
                                        break;
                                }
                            }
                            break;
                        case "cited-others":
                            wHtmlbody += "【参考文献】<br />\r\n";
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "summary-of-invention":
                            wHtmlbody += "【発明の概要】<br />\r\n";
                            foreach (XmlNode node3 in node2.ChildNodes)
                            {
                                switch (node3.Name)
                                {
                                    case "tech-problem":
                                        wHtmlbody += "【発明が解決しようとする課題】<br />\r\n";
                                        wHtmlbody += description(node3, xmlNsManager);
                                        break;
                                    case "tech-solution":
                                        wHtmlbody += "【課題を解決するための手段】<br />\r\n";
                                        wHtmlbody += description(node3, xmlNsManager);
                                        break;
                                    case "advantageous-effects":
                                        wHtmlbody += "【発明の効果】<br />\r\n";
                                        wHtmlbody += description(node3, xmlNsManager);
                                        break;
                                    case "heading":
                                        wHtmlbody += "【" + node2.InnerText + "】<br />\r\n";
                                        break;
                                }
                            }
                            break;
                        case "description-of-drawings":
                            wHtmlbody += "【図面の簡単な説明】<br />\r\n";
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "description-of-embodiments":
                            wHtmlbody += "【発明を実施するための形態】<br />\r\n";
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "embodiments-example":
                            if (node2.Attributes["ex-num"].Value == null)
                            {
                                wHtmlbody += "【実施例】<br />\r\n";
                            }
                            else
                            {
                                wHtmlbody += "【実施例" + Strings.StrConv(node2.Attributes["ex-num"].Value, VbStrConv.Wide, 0x411) + "】<br />\r\n";
                            }
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "best-mode":
                            wHtmlbody += "【発明を実施するための最良の形態】<br />\r\n";
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "industrial-applicability":
                            wHtmlbody += "【産業上の利用可能性】<br />\r\n";
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "disclosure":
                            wHtmlbody += "【発明の開示】<br />\r\n";
                            break;
                        case "reference-to-deposited-biological-material":
                            wHtmlbody += "【受託番号】<br />\r\n";
                            break;
                        case "reference-signs-list":
                            wHtmlbody += "【符号の説明】<br />\r\n";
                            wHtmlbody += description(node2, xmlNsManager);
                            break;
                        case "heading":
                            wHtmlbody += "【" + node2.InnerText + "】<br />\r\n";
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
                    wHtmlbody += "　　【書類名】要約書<br />\r\n";
                }
                wHtmlbody += p2html(node) + "<br />\r\n";
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
                            wHtmlbody += "【あて先】　　　　　　" + node2.InnerText + "<br />\r\n";
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
                            wHtmlbody += "　　【" + a_item_of_amendment + "】" + p2html(node2) + "<br />\r\n";
                            break;
                            /*
                        case "drawings":        // 図面
                            wHtmlbody += drawings(node2, xmlNsManager, a_item_of_amendment);
                            break;
                            */
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
                string wHtmlbody = "【提出日】　　　　　　" + ad2jpCalender(node.InnerText) + "<br />\r\n";
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
                            wHtmlbody += "" + wItemName + Strings.StrConv(node2.InnerText, VbStrConv.Wide, 0x411) + "<br />\r\n";
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
                        wHtmlbody += "　　【" + a_item_of_amendment + "】<br />\r\n";
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
                            wHtmlbody += "【" + a_item_of_amendment + "】<br />\r\n";
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
                            wHtmlbody += "【" + a_item_of_amendment + "】<br />\r\n";
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
                    wHtmlbody += "　　【" + a_item_of_amendment + "】<br />\r\n";
                    XmlNode node_document_name = node_list_group.SelectSingleNode("jp:document-name", xmlNsManager);
                    XmlNode node_number_of_object = node_list_group.SelectSingleNode("jp:number-of-object", xmlNsManager);
                    if (node_document_name != null
                    && node_number_of_object != null)
                    {
                        wHtmlbody += "　　【物件名】　　　　" + node_document_name.InnerText + "　" + Strings.StrConv(node_number_of_object.InnerText, VbStrConv.Wide, 0x411) + "<br />\r\n";
                    }
                    XmlNode node_citation = node_list_group.SelectSingleNode("jp:citation", xmlNsManager);
                    if (node_citation != null)
                    {
                        wHtmlbody += "　　【援用の表示】　　" + p2html(node_citation) + "<br />\r\n";
                    }
                    XmlNode node_general_power_of_attorney_id = node_list_group.SelectSingleNode("jp:general-power-of-attorney-id", xmlNsManager);
                    if (node_general_power_of_attorney_id != null)
                    {
                        wHtmlbody += "　　【包括委任状番号】" + Strings.StrConv(node_general_power_of_attorney_id.InnerText, VbStrConv.Wide, 0x411) + "<br />\r\n";
                    }
                    XmlNode node_dtext = node_list_group.SelectSingleNode("jp:dtext", xmlNsManager);
                    if (node_dtext != null)
                    {
                        wHtmlbody += "　　【提出物件の特記事項】" + p2html(node_dtext) + "<br />\r\n";
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
                    wHtmlbody += "【手数料補正】<br />\r\n";
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        switch (node2.LocalName)
                        {
                            case "document-code":
                                wHtmlbody += "　【補正対象書類名】　" + document_code2desc(node2.InnerText) + "<br />\r\n";
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
                                            wHtmlbody += "　【指定立替納付】<br />\r\n";
                                        }
                                        else
                                        {
                                            wHtmlbody += "　【振替番号】　　　　" + Strings.StrConv(node_account.Attributes["number"].Value, VbStrConv.Wide, 0x411) + "<br />\r\n";
                                        }
                                    }
                                    XmlNode node_fee = node_payment.SelectSingleNode("jp:fee", xmlNsManager);
                                    if (node_fee != null)
                                    {
                                        wHtmlbody += "　　【納付金額】　　　" + Strings.StrConv(node_fee.Attributes["amount"].Value, VbStrConv.Wide, 0x411) + "<br />\r\n";
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
                                wHtmlbody += "　　【書類名】図面<br />\r\n";
                                a_item_of_amendment = "";
                            }
                            wHtmlbody += drawings(node2, xmlNsManager, a_item_of_amendment);
                            break;
                        case "figure":
                            wHtmlbody += "　　【図" + Strings.StrConv(node2.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】<br />\r\n";
                            wHtmlbody += "" + p2html(node2) + " <br />\r\n";
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
                    m_DocumentName = document_code2desc(node_document_code.InnerText);
                } else
                {
                    m_DocumentName = "";
                }
                XmlNode node_date = node_rspns.SelectSingleNode("//jp:submission-date/jp:date", xmlNsManager);
                if (node_date != null)
                {
                    m_Date = node_date.InnerText;
                }
                else
                {
                    m_Date = "";
                }
                // 出願番号
                m_DocNumber = m_DocNumber2;
                XmlNode node_application_reference = node_rspns.SelectSingleNode("//jp:indication-of-case-article/jp:application-reference", xmlNsManager);
                if (node_application_reference != null)
                {
                    if (node_application_reference.Attributes["appl-type"].Value == "application")
                    {
                        XmlNode node_doc_number = node_rspns.SelectSingleNode("//jp:indication-of-case-article/jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                        if (node_doc_number != null)
                        {
                            m_DocNumber = node_doc_number.InnerText;
                        }
                    }
                }
                if (m_DocNumber != null && m_DocNumber.Length > 0)
                {
                    wTitle = "特願" + m_DocNumber + "_起案日" + m_Date + "_" + m_DocumentName;
                }
                else
                {
                    wTitle = "起案日" + m_Date + "_" + m_DocumentName;
                }
                if (m_dirNames.Length >= 2)
                {
                    wTitle += m_dirNames[m_dirNames.Length - 2];
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
                            wHtmlbody += "【あて先】　　　　　　" + node.InnerText + "<br />\r\n";
                            break;
                        case "indication-of-case-article":
                            wHtmlbody += indication_of_case_article(node, xmlNsManager);
                            break;
                        case "applicants":
                            wHtmlbody += applicants(node, xmlNsManager, "出願人");
                            break;
                        case "agents":
                            wHtmlbody += agents(node, xmlNsManager, "代理人");
                            break;
                        case "dispatch-number":
                            wHtmlbody += "【発送番号】　　　　　" + Strings.StrConv(node.InnerText, VbStrConv.Wide, 0x411) + "<br />\r\n";
                            break;
                        case "opinion-contents-article":
                            wHtmlbody += opinion_contents_article(node, xmlNsManager);
                            /*
                            wHtmlbody += "【意見の内容】<br />\r\n";
                            foreach (XmlNode node_p in node.SelectNodes("p", xmlNsManager))
                            {
                                wHtmlbody += "" + p2html(node_p) + "<br />\r\n";
                            }
                            */
                            break;
                        case "proof-means":
                            wHtmlbody += proof_means(node, xmlNsManager);
                            //wHtmlbody += "【証拠方法】　　　　　" + p2html(node) + "<br />\r\n";
                            break;
                        case "dtext":
                            wHtmlbody += dtext(node, xmlNsManager);
                            //wHtmlbody += "【その他】　　　　　　" + p2html(node) + "<br />\r\n";
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
                string wHtmlbody = "【意見の内容】<br />\r\n";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.LocalName)
                    {
                        case "p":
                            wHtmlbody += p2html(node2) + " <br />\r\n";
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
                            wHtmlbody += wItemName + p2html(node2) + " <br />\r\n";
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
                            wHtmlbody += wItemName + p2html(node2) + " <br />\r\n";
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
            string[] 元号名 = { "明治", "大正", "昭和", "平成", "令和" };
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
                    wHtmlbody += "　　【識別番号】　　　" + Strings.StrConv(node_registered_number.InnerText, VbStrConv.Wide, 0x411) + "<br />\r\n";
                }
                XmlNode attorney = node_addressbook.ParentNode.SelectSingleNode("jp:attorney", xmlNsManager);
                if (attorney != null)
                {
                    wHtmlbody += "　　【弁理士】<br />\r\n";
                }
                XmlNode lawyer = node_addressbook.ParentNode.SelectSingleNode("jp:lawyer", xmlNsManager);
                if (lawyer != null)
                {
                    wHtmlbody += "　　【弁護士】<br />\r\n";
                }
                XmlNode node_address_text = node_addressbook.SelectSingleNode("jp:address/jp:text", xmlNsManager);
                if (node_address_text != null)
                {
                    wHtmlbody += "　　【住所又は居所】　" + node_address_text.InnerText + "<br />\r\n";
                }
                XmlNode node_name = node_addressbook.SelectSingleNode("jp:name", xmlNsManager);
                if (node_name != null)
                {
                    wHtmlbody += "　　【氏名又は名称】　" + node_name.InnerText + "<br />\r\n";
                }
                XmlNode node_phone = node_addressbook.SelectSingleNode("jp:phone", xmlNsManager);
                if (node_phone != null)
                {
                    wHtmlbody += "　　【電話番号】　　　" + Strings.StrConv(node_phone.InnerText, VbStrConv.Wide, 0x411) + "<br />\r\n";
                }
                XmlNode node_fax = node_addressbook.SelectSingleNode("jp:fax", xmlNsManager);
                if (node_fax != null)
                {
                    wHtmlbody += "　　【ファクシミリ番号】　　　" + Strings.StrConv(node_fax.InnerText, VbStrConv.Wide, 0x411) + "<br />\r\n";
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
                string wTitle = "特願" + m_DocNumber2;
                XmlNode node_document_name = node_attaching_document.SelectSingleNode("//jp:document-name", xmlNsManager);
                if (node_document_name != null)
                {
                    m_DocumentName = node_document_name.InnerText;
                }
                else
                {
                    m_DocumentName = "";
                }
                wTitle += "_" + m_DocumentName;
                if (m_dirNames.Length >= 2)
                {
                    wTitle += "_" + m_dirNames[m_dirNames.Length - 2];
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
                            wHtmlbody += "　【書類名】　　　　" + node.InnerText + "<br />\r\n";
                            break;
                        case "p":
                            wHtmlbody += "" + p2html(node) + "<br />\r\n";
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
                m_title = title_notice_pat_exam(node_notice_pat_exam, xmlNsManager);
                string wHtmlbody = "<html><head>";
                wHtmlbody += "<title>" + m_title + "</title>";
                wHtmlbody += "<meta charset=\"shift_jis\"></head><body><div><font face=\"ＭＳ明朝\"><p>\r\n";
                foreach (XmlNode node2 in node_notice_pat_exam.ChildNodes)
                {
                    wHtmlbody += notice_pat_exam(node2, xmlNsManager);
                }
                wHtmlbody += "</p></font></div></body></html>\r\n";
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
                    m_DocumentName = node_document_name.InnerText;
                }
                XmlNode node_doc_number = node_notice_pat_exam.SelectSingleNode("//jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                if(node_doc_number != null)
                {
                    m_DocNumber = node_doc_number.InnerText;
                }
                XmlNode node_drafting_date = node_notice_pat_exam.SelectSingleNode("//jp:drafting-date/jp:date", xmlNsManager);
                if(node_drafting_date != null)
                {
                    m_Date = node_drafting_date.InnerText;
                }
                wTitle = "特願" + m_DocNumber + "_起案日" + m_Date + "_" + m_DocumentName;
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
                            wHtmlbody += "" + centering(node.InnerText) + "<br />\r\n";
                            break;
                        case "bibliog-in-ntc-pat-exam":
                            wHtmlbody += "<br />\r\n<br />\r\n<br /><br />\r\n";
                            wHtmlbody += bibliog_in_ntc_pat_exam(node, xmlNsManager);
                            break;
                        case "conclusion-part-article":
                            {
                                wHtmlbody += "<br />\r\n<br />\r\n<br /><br />\r\n";
                                foreach (XmlNode node2 in node.SelectNodes("p", xmlNsManager))
                                {
                                    wHtmlbody += p2html(node2) + "<br />\r\n";
                                }
                            }
                            break;
                        case "drafting-body":
                            wHtmlbody += "<br />\r\n<br />\r\n<br /><br />\r\n";
                            wHtmlbody += p2html(node) + "<br />\r\n";
                            break;
                        case "footer-article":
                            wHtmlbody += "<br />\r\n<br />\r\n<br /><br />\r\n";
                            wHtmlbody += footer_article(node, xmlNsManager);
                            break;
                        case "final-decision-group":
                            wHtmlbody += "<br />\r\n<br />\r\n<br /><br />\r\n";
                            wHtmlbody += final_decision_group(node, xmlNsManager);
                            break;
                        case "final-decision-memo":
                            wHtmlbody += "<br />\r\n<br />\r\n<br /><br />\r\n";
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
                        wHtmlbody += "　　　　　　　　　　　　" + node.InnerText + "<br />\r\n";
                        break;
                    case "final-decision-bibliog":
                        XmlNode node_doc_number = node.SelectSingleNode("//jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                        if (node_doc_number != null)
                        {
                            string docNumber = node_doc_number.InnerText.Substring(0, 4) + "-" + node_doc_number.InnerText.Substring(4, 6);
                            docNumber = Strings.StrConv(docNumber, VbStrConv.Wide, 0x411);

                            wHtmlbody += "<br />\r\n<br />\r\n<br /><br />\r\n";
                            wHtmlbody += "　特許出願の番号　　　　　　特願" + docNumber + "<br />\r\n";
                        }
                        break;
                    case "final-decision-body":
                        wHtmlbody += "<br />\r\n<br />\r\n<br /><br />\r\n";
                        wHtmlbody += "１．調査した分野（ＩＰＣ，ＤＢ名）<br />\r\n";
                        wHtmlbody += "<br />\r\n<br />\r\n";
                        foreach (XmlNode node2 in node.ChildNodes)
                        {
                            switch(node2.LocalName)
                            {
                                case "field-of-search-article":
                                    foreach (XmlNode node3 in node2.ChildNodes)
                                    {
                                        if (node3.LocalName == "field-of-search")
                                        {
                                            wHtmlbody += "　" + node3.InnerText + "<br />\r\n";
                                        }
                                    }
                                    break;
                                case "patent-reference-article":
                                    wHtmlbody += "<br />\r\n<br />\r\n<br /><br />\r\n";
                                    wHtmlbody += "２．参考特許文献<br />\r\n";
                                    wHtmlbody += "<br />\r\n<br />\r\n";
                                    foreach (XmlNode node3 in node2.ChildNodes)
                                    {
                                        if (node3.LocalName == "patent-reference-group")
                                        {
                                            foreach (XmlNode node4 in node3.ChildNodes)
                                            {
                                                switch (node4.LocalName)
                                                {
                                                    case "document-number":
                                                        wHtmlbody += "　" + (node4.InnerText + "　　　　　　　　　　　　　　　　　　　　　　　　　　").Substring(0, 26);
                                                        break;
                                                    case "kind-of-document":
                                                        wHtmlbody += node4.InnerText + "<br />\r\n";
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
                        wHtmlbody += "<br />\r\n<br />\r\n<br /><br />\r\n";
                        wHtmlbody += "３．参考図書雑誌<br />\r\n";
                        wHtmlbody += "<br />\r\n";
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

            foreach (XmlNode node in node_footer_article.ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "kind-of-application":
                        wHtmlbody += "１．出願種別　　　　　　　　" + node.InnerText + "<br />\r\n";
                        break;
                    case "exist-of-reference-doc":
                        wHtmlbody += "２．参考文献　　　　　　　　";
                        if (node.Attributes["jp:true-or-false"].Value == "true")
                        {
                            wHtmlbody += "有<br />\r\n";
                        }
                        else
                        {
                            wHtmlbody += "無<br />\r\n";
                        }
                        break;
                    case "patent-law-section30":
                        wHtmlbody += "３．特許法第３０条適用　　　";
                        if (node.Attributes["jp:true-or-false"].Value == "true")
                        {
                            wHtmlbody += "有<br />\r\n";
                        }
                        else
                        {
                            wHtmlbody += "無<br />\r\n";
                        }
                        break;
                    case "change-flag-invention-title":
                        wHtmlbody += "４．発明の名称の変更　　　　";
                        if (node.Attributes["jp:true-or-false"].Value == "true")
                        {
                            wHtmlbody += "有<br />\r\n";
                        }
                        else
                        {
                            wHtmlbody += "無<br />\r\n";
                        }
                        break;
                    case "ipc-article":
                        wHtmlbody += "<br />\r\n<br />\r\n";
                        wHtmlbody += "５．国際特許分類（ＩＰＣ）<br />\r\n";
                        wHtmlbody += "<br />\r\n<br />\r\n";
                        foreach (XmlNode node2 in node.ChildNodes)
                        {
                            switch (node2.LocalName)
                            {
                                case "ipc":
                                    string ipc = node2.InnerText.Replace("\xA0", " ");
                                    ipc = Strings.StrConv(ipc, VbStrConv.Wide, 0x411);
                                    wHtmlbody += "　　　　　　　　　　　　　　　" + ipc + "<br />\r\n";
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
                footer[i] += "<br />\r\n";
                wHtmlbody += footer[i];
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
                                wHtmlbody += "　特許出願の番号　　　　　　特願" + doc_number.Substring(0, 4) + "－" + doc_number.Substring(4) + "<br />\r\n";
                            }
                            break;
                        case "drafting-date":
                            XmlNode node_drafting_date = node.SelectSingleNode("jp:date", xmlNsManager);
                            {
                                wHtmlbody += "　起案日　　　　　　　　　　" + ad2jpCalender(node_drafting_date.InnerText) + "<br />\r\n";
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
                                    wHtmlbody += "　特許庁審査官　　　　　　　" + name + "　　　　　　　　" + staff_code + "　" + office_code + "<br />\r\n";
                                }
                            }
                            break;
                        case "invention-title":
                            wHtmlbody += "　発明の名称　　　　　　　　" + node.InnerText + "<br />\r\n";
                            break;
                        case "number-of-claim":
                            string numberOfClaim = node.InnerText.Replace("\xA0"," ");
                            numberOfClaim = Strings.StrConv(numberOfClaim, VbStrConv.Wide, 0x411);
                            wHtmlbody += "　請求項の数　　　　　　　　" + numberOfClaim + "<br />\r\n";
                            break;
                        case "addressed-to-person-group":
                            if(node.Attributes["jp:kind-of-person"].Value == "applicant")
                            {
                                XmlNode node_name = node.SelectSingleNode("jp:addressbook/jp:name", xmlNsManager);
                                if(node_name != null)
                                {
                                    wHtmlbody += "　特許出願人　　　　　　　　" + node_name.InnerText + "<br />\r\n";
                                }
                            } else
                            if (node.Attributes["jp:kind-of-person"].Value == "attorney")
                            {
                                XmlNode node_name = node.SelectSingleNode("jp:addressbook/jp:name", xmlNsManager);
                                if (node_name != null)
                                {
                                    wHtmlbody += "　代理人　　　　　　　　　　" + node_name.InnerText + "<br />\r\n";
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
                            wHtmlbody += "　適用条文　　　　　　　　　" + article + "<br />\r\n";
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
                        wHtmlbody += HttpUtility.HtmlEncode(node.OuterXml);
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
            //wHtmlbody += "<img height=" + height.ToString() + " width=" + width.ToString() + " src=\"" + w_src_png + "\"><br />\r\n";
            byte[] dataPng = File.ReadAllBytes(w_src1);
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
