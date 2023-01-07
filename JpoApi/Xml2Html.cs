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
using static System.Net.Mime.MediaTypeNames;

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
                wHtmlbody += "<meta charset=\"shift_jis\"></head><body><div>\r\n";

                foreach (XmlNode node2 in node.ChildNodes)
                {
                    wHtmlbody += html_amendment(node2, xmlNsManager);
                }
                wHtmlbody += "</div></body></html>\r\n";
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
                wHtmlbody += "<meta charset=\"shift_jis\"></head><body><div>\r\n";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    wHtmlbody += html_rspns(node2, xmlNsManager);
                }
                wHtmlbody += "</div></body></html>\r\n";
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
                wHtmlbody += "<meta charset=\"shift_jis\"></head><body><div>\r\n";
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    wHtmlbody += attaching_document(node2, xmlNsManager);
                }
                wHtmlbody += "</div></body></html>\r\n";
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
                            wHtmlbody += "<p>【書類名】　　　　　　" + document_code2desc(node2.InnerText) + "</p>\r\n";
                            break;
                        case "file-reference-id":
                            wHtmlbody += "<p>【整理番号】　　　　　" + Strings.StrConv(node2.InnerText, VbStrConv.Wide, 0x411) + "</p>\r\n";
                            break;
                        case "submission-date":
                            wHtmlbody += "<p>【提出日】　　　　　　" + ad2jpCalender(node2.InnerText) + "</p>\r\n";
                            break;
                        case "addressed-to-person":
                            wHtmlbody += "<p>【あて先】　　　　　　" + node2.InnerText + "</p>\r\n";
                            break;
                        case "indication-of-case-article":  // 事件名
                            {
                                XmlNode node_applicaton_reference = node2.SelectSingleNode("jp:application-reference", xmlNsManager);
                                XmlNode node_doc_number = node2.SelectSingleNode("jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                                if (node_doc_number != null)
                                {
                                    wHtmlbody += "<p>【事件の表示】</p>\r\n";
                                    switch(node_applicaton_reference.Attributes["appl-type"].Value)
                                    {
                                        case "international-application":
                                            string international_application_number = node_doc_number.InnerText;
                                            wHtmlbody += "<p>　　【国際出願番号】　PCT/" + international_application_number.Substring(0, 6) + "/" + international_application_number.Substring(6) + "</p>\r\n";
                                            wHtmlbody += "<p>　　【出願の区分】　　特許</p>\r\n";
                                            break;
                                        case "application":
                                        default:
                                            string doc_number = Microsoft.VisualBasic.Strings.StrConv(node_doc_number.InnerText, VbStrConv.Wide, 0x411);
                                            wHtmlbody += "<p>　　【出願番号】　　　特願" + doc_number.Substring(0, 4) + "－" + doc_number.Substring(4) + "</p>\r\n";
                                            break;
                                    }
                                }
                            }
                            break;
                        case "applicants":  // 出願人
                            foreach(XmlNode node3 in node2.SelectNodes("jp:applicant", xmlNsManager))
                            {
                                wHtmlbody += "<p>【補正をする者】</p>\r\n";
                                wHtmlbody += applicant(node3, xmlNsManager);
                            }
                            break;
                        case "agents":  // 代理人
                            foreach(XmlNode node3 in node2.SelectNodes("jp:agent", xmlNsManager))
                            {
                                wHtmlbody += "<p>【代理人】</p>\r\n";
                                wHtmlbody += agent(node3, xmlNsManager);
                            }
                            break;
                        case "dispatch-number":
                            wHtmlbody += "<p>【発送番号】　　　　　" + Strings.StrConv(node2.InnerText, VbStrConv.Wide, 0x411) + "</p>\r\n";
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
                            wHtmlbody += "<p>【その他】　　　　　　" + p2html(node2) + "</p>\r\n";
                            break;
                        case "jp:proof-means":
                            wHtmlbody += "<p>【証拠方法】　　　　　" + p2html(node2) + "</p>\r\n";
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

        private string charge_article(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                XmlNode node_payment = node.SelectSingleNode("jp:charge-article/jp:payment", xmlNsManager);
                if (node_payment != null)
                {
                    wHtmlbody += "<p>【手数料の表示】</p>\r\n";
                    string amount = "";
                    string number = "";
                    foreach (XmlNode node3 in node_payment)
                    {
                        switch (node3.LocalName)
                        {
                            case "fee":
                                amount = node3.Attributes["amount"].Value;
                                break;
                            case "account":
                                number = node3.Attributes["number"].Value;
                                break;
                            default:
                                break;
                        }
                    }
                    if (number.Length > 0)
                    {
                        wHtmlbody += "<p>　　【振替番号】　　　" + Strings.StrConv(number, VbStrConv.Wide, 0x411) + "</p>\r\n";
                    }
                    if (amount.Length > 0)
                    {
                        wHtmlbody += "<p>　　【納付金額】　　　" + Strings.StrConv(amount, VbStrConv.Wide, 0x411) + "円</p>\r\n";
                    }
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
                    wHtmlbody += "<p>【手続補正" + Strings.StrConv(node2.Attributes["jp:serial-number"].Value, VbStrConv.Wide, 0x411) + "】</p>\r\n";
                    foreach(XmlNode node3 in node2.ChildNodes)
                    {
                        switch (node3.LocalName)
                        {
                            case "document-code":
                                wHtmlbody += "<p>　【補正対象書類名】　" + document_code2desc(node3.InnerText) + "</p>\r\n";
                                break;
                            case "item-of-amendment":
                                wHtmlbody += "<p>　【補正対象項目名】　" + node3.InnerText + "</p>\r\n";
                                w_item_of_amendment = node3.InnerText;
                                break;
                            case "way-of-amendment":
                                wHtmlbody += "<p>　【補正方法】　　　　" + way_of_amendment(node3.InnerText) + "</p>\r\n";
                                break;
                            case "contents-of-amendment":
                                wHtmlbody += "<p>　【補正の内容】</p>\r\n";
                                switch(node3.Attributes["jp:kind-of-document"].Value)
                                {
                                    case "claims":
                                        XmlNode node_claims = node3.SelectSingleNode("claims", xmlNsManager);
                                        if (node_claims != null)
                                        {
                                            wHtmlbody += "<p>　　【書類名】特許請求の範囲</p>\r\n";
                                            foreach (XmlNode node_claim in node_claims.SelectNodes("claim", xmlNsManager))
                                            {
                                                wHtmlbody += "<p>　　【請求項" + Strings.StrConv(node_claim.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】</p>\r\n";
                                                XmlNode node_claim_text = node_claim.SelectSingleNode("claim-text", xmlNsManager);
                                                wHtmlbody += "<p>" + p2html(node_claim_text) + "</p>\r\n";
                                            }
                                        }
                                        else
                                        {
                                            XmlNode node_claim = node3.SelectSingleNode("claim", xmlNsManager);
                                            wHtmlbody += "<p>　　【請求項" + Strings.StrConv(node_claim.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】</p>\r\n";
                                            XmlNode node_claim_text = node_claim.SelectSingleNode("claim-text", xmlNsManager);
                                            wHtmlbody += "<p>" + p2html(node_claim_text) + "</p>\r\n";
                                        }
                                        break;
                                    case "description":
                                        foreach (XmlNode node4 in node3.ChildNodes)
                                        {
                                            switch (node4.Name)
                                            {
                                                case "p":
                                                    wHtmlbody += "<p>　　【" + Strings.StrConv(node4.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】</p>\r\n";
                                                    wHtmlbody += "<p>" + p2html(node4) + "</p>\r\n";
                                                    break;
                                                case "invention-title":
                                                    wHtmlbody += "<p>　　【発明の名称】" + p2html(node4) + "</p>\r\n";
                                                    break;
                                            }
                                        }
                                        break;
                                    default:
                                        wHtmlbody += inventors(node3, w_item_of_amendment, xmlNsManager);
                                        wHtmlbody += amendment_applicants(node3, w_item_of_amendment, xmlNsManager);
                                        wHtmlbody += amendment_submission_object_list_article(node3, w_item_of_amendment, xmlNsManager);
                                        wHtmlbody += law_of_industrial_regenerate(node3, w_item_of_amendment, xmlNsManager);
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
        private string inventors(XmlNode node, string a_item_of_amendment, XmlNamespaceManager xmlNsManager)
        {
            string wHtmlbody = "";
            XmlNode node_applicants = node.SelectSingleNode("jp:inventors", xmlNsManager);
            if (node_applicants != null)
            {
                foreach (XmlNode node_applicant in node_applicants.SelectNodes("jp:inventor", xmlNsManager))
                {
                    wHtmlbody += "<p>　　【" + a_item_of_amendment + "】</p>\r\n";
                    XmlNode node_name = node_applicant.SelectSingleNode("jp:addressbook/jp:name", xmlNsManager);
                    if (node_name != null)
                    {
                        wHtmlbody += "<p>　　【氏名又は名称】　" + p2html(node_name) + " </p>\r\n";
                    }
                    XmlNode node_address_text = node_applicant.SelectSingleNode("jp:addressbook/jp:address/jp:text", xmlNsManager);
                    if (node_address_text != null)
                    {
                        wHtmlbody += "<p>　　【住所又は居所】　" + p2html(node_address_text) + "</p>\r\n";
                    }
                }
            }
            return wHtmlbody;
        }
        private string amendment_applicants(XmlNode node, string a_item_of_amendment, XmlNamespaceManager xmlNsManager)
        {
            string wHtmlbody = "";
            XmlNode node_applicants = node.SelectSingleNode("jp:applicants", xmlNsManager);
            if (node_applicants != null)
            {
                foreach (XmlNode node_applicant in node_applicants.SelectNodes("jp:applicant", xmlNsManager))
                {
                    wHtmlbody += "<p>　　【" + a_item_of_amendment + "】</p>\r\n";
                    XmlNode node_registered_number = node_applicant.SelectSingleNode("jp:addressbook/jp:registered-number", xmlNsManager);
                    if (node_registered_number != null)
                    {
                        wHtmlbody += "<p>　　【識別番号】　　　" + p2html(node_registered_number) + "</p>\r\n";
                    }
                    XmlNode node_name = node_applicant.SelectSingleNode("jp:addressbook/jp:name", xmlNsManager);
                    if (node_name != null)
                    {
                        wHtmlbody += "<p>　　【氏名又は名称】　" + p2html(node_name) + " </p>\r\n";
                    }
                }
            }
            return wHtmlbody;
        }
        private string amendment_submission_object_list_article(XmlNode node, string a_item_of_amendment, XmlNamespaceManager xmlNsManager)
        {
            string wHtmlbody = "";
            XmlNode node_list_group = node.SelectSingleNode("jp:submission-object-list-article/jp:list-group", xmlNsManager);
            if (node_list_group != null)
            {
                wHtmlbody += "<p>　　【" + a_item_of_amendment + "】</p>\r\n";
                XmlNode node_document_name = node_list_group.SelectSingleNode("jp:document-name", xmlNsManager);
                XmlNode node_number_of_object = node_list_group.SelectSingleNode("jp:number-of-object", xmlNsManager);
                if(node_document_name != null
                && node_number_of_object != null)
                {
                    wHtmlbody += "<p>　　【物件名】　　　　" + node_document_name.InnerText + "　" + node_number_of_object.InnerText + "</p>\r\n";
                }
                XmlNode node_citation = node_list_group.SelectSingleNode("jp:citation", xmlNsManager);
                if(node_citation != null)
                {
                    wHtmlbody += "<p>　　【援用の表示】　　" + node_citation.InnerText + "</p>\r\n";
                }
                XmlNode node_general_power_of_attorney_id = node_list_group.SelectSingleNode("jp:general-power-of-attorney-id", xmlNsManager);
                if (node_general_power_of_attorney_id != null)
                {
                    wHtmlbody += "<p>　　【包括委任状番号】" + node_general_power_of_attorney_id.InnerText + "</p>\r\n";
                }
                XmlNode node_dtext = node_list_group.SelectSingleNode("jp:dtext", xmlNsManager);
                if (node_dtext != null)
                {
                    wHtmlbody += "<p>　　【提出物件の特記事項】" + node_dtext.InnerText + "</p>\r\n";
                }
            }
            return wHtmlbody;
        }
        private string law_of_industrial_regenerate(XmlNode node, string a_item_of_amendment, XmlNamespaceManager xmlNsManager)
        {
            string wHtmlbody = "";
            XmlNode node_law_of_industrial_regenerate = node.SelectSingleNode("jp:law-of-industrial-regenerate", xmlNsManager);
            if (node_law_of_industrial_regenerate != null)
            {
                wHtmlbody += "<p>　　【" + a_item_of_amendment + "】" + p2html(node_law_of_industrial_regenerate) + "</p>\r\n";
            }
            return wHtmlbody;
        }

        // 手数料補正
        private string amendment_charge_article(XmlNode node, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                if (node != null)
                {
                    wHtmlbody += "<p>【手数料補正】</p>\r\n";
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        switch (node2.LocalName)
                        {
                            case "document-code":
                                wHtmlbody += "<p>　【補正対象書類名】　" + document_code2desc(node2.InnerText) + "</p>\r\n";
                                break;
                            case "charge-article":
                                XmlNode node_payment = node2.SelectSingleNode("jp:payment", xmlNsManager);
                                if (node_payment != null)
                                {
                                    XmlNode node_account = node_payment.SelectSingleNode("jp:account", xmlNsManager);
                                    if (node_account != null)
                                    {
                                        wHtmlbody += "<p>　【振替番号】　　　　" + node_account.Attributes["number"].Value + "</p>\r\n";
                                    }
                                    XmlNode node_fee = node_payment.SelectSingleNode("jp:fee", xmlNsManager);
                                    if (node_fee != null)
                                    {
                                        wHtmlbody += "<p>　【納付金額】　　　　" + node_fee.Attributes["amount"].Value + "円</p>\r\n";
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
                //Console.WriteLine(ex.Message);
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
                            wHtmlbody += "<p>【書類名】　　　　　　" + document_code2desc(node.InnerText) + "</p>\r\n";
                            break;
                        case "file-reference-id":
                            wHtmlbody += "<p>【整理番号】　　　　　" + Strings.StrConv(node.InnerText, VbStrConv.Wide, 0x411) + "</p>\r\n";
                            break;

                        case "submission-date":
                            {
                                wHtmlbody += "<p>【提出日】　　　　　　" + ad2jpCalender(node.InnerText) + "</p>\r\n";
                            }
                            break;
                        case "addressed-to-person":
                            wHtmlbody += "<p>【あて先】　　　　　　" + node.InnerText + "</p>\r\n";
                            break;
                        case "indication-of-case-article":
                            {
                                XmlNode node_applicaton_reference = node.SelectSingleNode("jp:application-reference", xmlNsManager);
                                XmlNode node_doc_number = node.SelectSingleNode("jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                                if (node_doc_number != null)
                                {
                                    wHtmlbody += "<p>【事件の表示】</p>\r\n";
                                    switch (node_applicaton_reference.Attributes["appl-type"].Value)
                                    {
                                        case "international-application":
                                            string international_application_number = node_doc_number.InnerText;
                                            wHtmlbody += "<p>　　【国際出願番号】　PCT/" + international_application_number.Substring(0, 6) + "/" + international_application_number.Substring(6) + "</p>\r\n";
                                            wHtmlbody += "<p>　　【出願の区分】　　特許</p>\r\n";
                                            break;
                                        case "application":
                                        default:
                                            string doc_number = Microsoft.VisualBasic.Strings.StrConv(node_doc_number.InnerText, VbStrConv.Wide, 0x411);
                                            wHtmlbody += "<p>　　【出願番号】　　　特願" + doc_number.Substring(0, 4) + "－" + doc_number.Substring(4) + "</p>\r\n";
                                            break;
                                    }
                                }
                            }
                            break;
                        case "applicants":
                            foreach (XmlNode node2 in node.SelectNodes("//jp:applicant", xmlNsManager))
                            {
                                wHtmlbody += "<p>【出願人】</p>\r\n";
                                wHtmlbody += applicant(node2, xmlNsManager);
                            }
                            break;
                        case "agents":
                            foreach (XmlNode node2 in node.SelectNodes("//jp:agent", xmlNsManager))
                            {
                                wHtmlbody += "<p>【代理人】</p>\r\n";
                                wHtmlbody += agent(node2, xmlNsManager);
                            }
                            break;
                        case "dispatch-number":
                            wHtmlbody += "<p>【発送番号】　　　　　" + Strings.StrConv(node.InnerText, VbStrConv.Wide, 0x411) + "</p>\r\n";
                            break;
                        case "opinion-contents-article":
                            {
                                wHtmlbody += "<p>【意見の内容】</p>\r\n";
                                foreach (XmlNode node_p in node.SelectNodes("p", xmlNsManager))
                                {
                                    wHtmlbody += "<p>" + p2html(node_p) + "</p>\r\n";
                                }
                            }
                            break;
                        case "jp:proof-means":
                            wHtmlbody += "<p>【証拠方法】　　　　　" + p2html(node) + "</p>\r\n";
                            break;
                        case "dtext":
                            wHtmlbody += "<p>【その他】　　　　　　" + p2html(node) + "</p>\r\n";
                            break;

                        default:
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
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

        private string applicant(XmlNode node_applicant, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";

                XmlNode node_addressbook = node_applicant.SelectSingleNode("jp:addressbook", xmlNsManager);
                if(node_addressbook != null)
                {
                    XmlNode node_registered_number = node_addressbook.SelectSingleNode("jp:registered-number", xmlNsManager);
                    if (node_registered_number != null)
                    {
                        wHtmlbody += "<p>　　【識別番号】　　　" + Strings.StrConv(node_registered_number.InnerText, VbStrConv.Wide, 0x411) + "</p>\r\n";
                    }
                    XmlNode node_address_text = node_addressbook.SelectSingleNode("jp:address/jp:text", xmlNsManager);
                    if (node_address_text != null)
                    {
                        wHtmlbody += "<p>　　【住所又は居所】　" + node_address_text.InnerText + "</p>\r\n";
                    }

                    XmlNode node_name = node_addressbook.SelectSingleNode("jp:name", xmlNsManager);
                    if (node_name != null)
                    {
                        wHtmlbody += "<p>　　【氏名又は名称】　" + node_name.InnerText + "</p>\r\n";
                    }

                    XmlNode node_phone = node_addressbook.SelectSingleNode("jp:phone", xmlNsManager);
                    if (node_phone != null)
                    {
                        wHtmlbody += "<p>　　【電話番号】　　　" + node_phone.InnerText + "</p>\r\n";
                    }
                    XmlNode node_fax = node_addressbook.SelectSingleNode("jp:fax", xmlNsManager);
                    if (node_fax != null)
                    {
                        wHtmlbody += "<p>　　【ファクシミリ番号】　　　" + node_fax.InnerText + "</p>\r\n";
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                return "";
            }
        }
        private string agent(XmlNode node_agent, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";

                XmlNode node_addressbook = node_agent.SelectSingleNode("jp:addressbook", xmlNsManager);
                if (node_addressbook != null)
                {
                    XmlNode node_registered_number = node_addressbook.SelectSingleNode("jp:registered-number", xmlNsManager);
                    if (node_registered_number != null)
                    {
                        wHtmlbody += "<p>　　【識別番号】　　　" + Strings.StrConv(node_registered_number.InnerText, VbStrConv.Wide, 0x411) + "</p>\r\n";
                    }
                    XmlNode attorney = node_agent.SelectSingleNode("jp:attorney", xmlNsManager);
                    if (attorney != null)
                    {
                        wHtmlbody += "<p>　　【弁理士】</p>\r\n";
                    }
                    XmlNode lawyer = node_agent.SelectSingleNode("jp:lawyer", xmlNsManager);
                    if (lawyer != null)
                    {
                        wHtmlbody += "<p>　　【弁護士】</p>\r\n";
                    }
                    XmlNode node_name = node_addressbook.SelectSingleNode("jp:name", xmlNsManager);
                    if (node_name != null)
                    {
                        wHtmlbody += "<p>　　【氏名又は名称】　" + node_name.InnerText + "</p>\r\n";
                    }

                    XmlNode node_phone = node_addressbook.SelectSingleNode("jp:phone", xmlNsManager);
                    if (node_phone != null)
                    {
                        wHtmlbody += "<p>　　【電話番号】　　　" + node_phone.InnerText + "</p>\r\n";
                    }
                    XmlNode node_fax = node_addressbook.SelectSingleNode("jp:fax", xmlNsManager);
                    if (node_fax != null)
                    {
                        wHtmlbody += "<p>　　【ファクシミリ番号】　　　" + node_fax.InnerText + "</p>\r\n";
                    }
                }
                XmlNode node_representative_group = node_agent.SelectSingleNode("jp:representative-group", xmlNsManager);
                wHtmlbody += representative_group(node_representative_group, xmlNsManager);

                XmlNode node_contact = node_agent.SelectSingleNode("jp:contact", xmlNsManager);
                if (node_contact != null)
                {
                    wHtmlbody += "<p>　　【連絡先】　　　　" + node_contact.InnerText + "</p>\r\n";
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                return "";
            }
        }
        private string addressbook(XmlNode node_addressbook, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";

                XmlNode node_registered_number = node_addressbook.SelectSingleNode("jp:registered-number", xmlNsManager);
                if(node_registered_number != null)
                {
                    wHtmlbody += "<p>　　【識別番号】　　　" + Strings.StrConv(node_registered_number.InnerText, VbStrConv.Wide, 0x411) + "</p>\r\n";
                }
                XmlNode node_name = node_addressbook.SelectSingleNode("jp:name", xmlNsManager);
                if (node_name != null)
                {
                    wHtmlbody += "<p>　　【氏名又は名称】　" + node_name.InnerText + "</p>\r\n";
                }

                XmlNode node_phone = node_addressbook.SelectSingleNode("jp:phone", xmlNsManager);
                if (node_phone != null)
                {
                    wHtmlbody += "<p>　　【電話番号】　　　" + node_phone.InnerText + "</p>\r\n";
                }
                XmlNode node_fax = node_addressbook.SelectSingleNode("jp:fax", xmlNsManager);
                if (node_fax != null)
                {
                    wHtmlbody += "<p>　　【ファクシミリ番号】　　　" + node_fax.InnerText + "</p>\r\n";
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                return "";
            }
        }

        private string representative_group(XmlNode node_representative_group, XmlNamespaceManager xmlNsManager)
        {
            try
            {
                string wHtmlbody = "";
                if(node_representative_group != null)
                {
                    XmlNode node_representative_identification = node_representative_group.SelectSingleNode("jp:representative/jp:representative-identification", xmlNsManager);
                    XmlNode node_name = node_representative_group.SelectSingleNode("jp:representative/jp:name", xmlNsManager);
                    if (node_representative_identification != null
                    && node_name != null)
                    {
                        wHtmlbody += "<p>　　【" + node_representative_identification.InnerText + "】　　　　" + node_name.InnerText + " </p>\r\n";
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
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
                            wHtmlbody += "<p>　【書類名】　　　　" + node.InnerText + "</p>\r\n";
                            break;
                        case "p":
                            wHtmlbody += "<p>" + p2html(node) + "</p>\r\n";
                            break;
                        default:
                            break;
                    }
                }
                return wHtmlbody;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
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
                wHtmlbody += "<meta charset=\"shift_jis\"></head><body><div>\r\n";
                foreach (XmlNode node2 in node_notice_pat_exam.ChildNodes)
                {
                    wHtmlbody += notice_pat_exam(node2, xmlNsManager);
                }
                wHtmlbody += "</div></body></html>\r\n";
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
                            wHtmlbody += "<p>" + centering(node.InnerText) + "</p>\r\n";
                            break;
                        case "bibliog-in-ntc-pat-exam":
                            wHtmlbody += "<p><br />\r\n<br />\r\n<br /></p>\r\n";
                            wHtmlbody += bibliog_in_ntc_pat_exam(node, xmlNsManager);
                            break;
                        case "conclusion-part-article":
                            {
                                wHtmlbody += "<p><br />\r\n<br />\r\n<br /></p>\r\n";
                                foreach (XmlNode node2 in node.SelectNodes("p", xmlNsManager))
                                {
                                    wHtmlbody += "<p>" + p2html(node2) + "</p>\r\n";
                                }
                            }
                            break;
                        case "drafting-body":
                            wHtmlbody += "<p><br />\r\n<br />\r\n<br /></p>\r\n";
                            wHtmlbody += "<p>" + p2html(node) + "</p>\r\n";
                            break;
                        case "footer-article":
                            wHtmlbody += "<p><br />\r\n<br />\r\n<br /></p>\r\n";
                            wHtmlbody += footer_article(node, xmlNsManager);
                            break;
                        case "final-decision-group":
                            wHtmlbody += "<p><br />\r\n<br />\r\n<br /></p>\r\n";
                            wHtmlbody += final_decision_group(node, xmlNsManager);
                            break;
                        case "final-decision-memo":
                            wHtmlbody += "<p><br />\r\n<br />\r\n<br /></p>\r\n";
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
                        wHtmlbody += "<p>　　　　　　　　　　　　" + node.InnerText + "</p>\r\n";
                        break;
                    case "final-decision-bibliog":
                        XmlNode node_doc_number = node.SelectSingleNode("//jp:application-reference/jp:document-id/jp:doc-number", xmlNsManager);
                        if (node_doc_number != null)
                        {
                            string docNumber = node_doc_number.InnerText.Substring(0, 4) + "-" + node_doc_number.InnerText.Substring(4, 6);
                            docNumber = Strings.StrConv(docNumber, VbStrConv.Wide, 0x411);

                            wHtmlbody += "<p><br />\r\n<br />\r\n<br /></p>\r\n";
                            wHtmlbody += "<p>　特許出願の番号　　　　　　特願" + docNumber + "</p>\r\n";
                        }
                        break;
                    case "final-decision-body":
                        wHtmlbody += "<p><br />\r\n<br />\r\n<br /></p>\r\n";
                        wHtmlbody += "<p>１．調査した分野（ＩＰＣ，ＤＢ名）</p>\r\n";
                        wHtmlbody += "<p><br />\r\n</p>\r\n";
                        foreach (XmlNode node2 in node.ChildNodes)
                        {
                            switch(node2.LocalName)
                            {
                                case "field-of-search-article":
                                    foreach (XmlNode node3 in node2.ChildNodes)
                                    {
                                        if (node3.LocalName == "field-of-search")
                                        {
                                            wHtmlbody += "<p>　" + node3.InnerText + "</p>\r\n";
                                        }
                                    }
                                    break;
                                case "patent-reference-article":
                                    wHtmlbody += "<p><br />\r\n<br />\r\n<br /></p>\r\n";
                                    wHtmlbody += "<p>２．参考特許文献</p>\r\n";
                                    wHtmlbody += "<p><br />\r\n</p>\r\n";
                                    foreach (XmlNode node3 in node2.ChildNodes)
                                    {
                                        if (node3.LocalName == "patent-reference-group")
                                        {
                                            foreach (XmlNode node4 in node3.ChildNodes)
                                            {
                                                switch (node4.LocalName)
                                                {
                                                    case "document-number":
                                                        wHtmlbody += "<p>　" + (node4.InnerText + "　　　　　　　　　　　　　　　　　　　　　　　　　　").Substring(0, 26);
                                                        break;
                                                    case "kind-of-document":
                                                        wHtmlbody += node4.InnerText + "</p>\r\n";
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
                        wHtmlbody += "<p><br />\r\n<br />\r\n<br /></p>\r\n";
                        wHtmlbody += "<p>３．参考図書雑誌</p>\r\n";
                        wHtmlbody += "<p></p>\r\n";
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
                        wHtmlbody += "<p>１．出願種別　　　　　　　　" + node.InnerText + "</p>\r\n";
                        break;
                    case "exist-of-reference-doc":
                        wHtmlbody += "<p>２．参考文献　　　　　　　　";
                        if (node.Attributes["jp:true-or-false"].Value == "true")
                        {
                            wHtmlbody += "有</p>\r\n";
                        }
                        else
                        {
                            wHtmlbody += "無</p>\r\n";
                        }
                        break;
                    case "patent-law-section30":
                        wHtmlbody += "<p>３．特許法第３０条適用　　　";
                        if (node.Attributes["jp:true-or-false"].Value == "true")
                        {
                            wHtmlbody += "有</p>\r\n";
                        }
                        else
                        {
                            wHtmlbody += "無</p>\r\n";
                        }
                        break;
                    case "change-flag-invention-title":
                        wHtmlbody += "<p>４．発明の名称の変更　　　　";
                        if (node.Attributes["jp:true-or-false"].Value == "true")
                        {
                            wHtmlbody += "有</p>\r\n";
                        }
                        else
                        {
                            wHtmlbody += "無</p>\r\n";
                        }
                        break;
                    case "ipc-article":
                        wHtmlbody += "<p><br />\r\n</p>\r\n";
                        wHtmlbody += "<p>５．国際特許分類（ＩＰＣ）</p>\r\n";
                        wHtmlbody += "<p><br />\r\n</p>\r\n";
                        foreach (XmlNode node2 in node.ChildNodes)
                        {
                            switch (node2.LocalName)
                            {
                                case "ipc":
                                    string ipc = node2.InnerText.Replace("\xA0", " ");
                                    ipc = Strings.StrConv(ipc, VbStrConv.Wide, 0x411);
                                    wHtmlbody += "<p>　　　　　　　　　　　　　　　" + ipc + "</p>\r\n";
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
            string[] footer = new string[] { "<p>　", "<p>　", "<p>　", "<p>　" };

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
                footer[i] += "</p>\r\n";
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
                                wHtmlbody += "<p>　特許出願の番号　　　　　　特願" + doc_number.Substring(0, 4) + "－" + doc_number.Substring(4) + "</p>\r\n";
                            }
                            break;
                        case "drafting-date":
                            XmlNode node_drafting_date = node.SelectSingleNode("jp:date", xmlNsManager);
                            {
                                wHtmlbody += "<p>　起案日　　　　　　　　　　" + ad2jpCalender(node_drafting_date.InnerText) + "</p>\r\n";
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
                                    wHtmlbody += "<p>　特許庁審査官　　　　　　　" + name + "　　　　　　　　" + staff_code + "　" + office_code + "</p>\r\n";
                                }
                            }
                            break;
                        case "invention-title":
                            wHtmlbody += "<p>　発明の名称　　　　　　　　" + node.InnerText + "</p>\r\n";
                            break;
                        case "number-of-claim":
                            string numberOfClaim = node.InnerText.Replace("\xA0"," ");
                            numberOfClaim = Strings.StrConv(numberOfClaim, VbStrConv.Wide, 0x411);
                            wHtmlbody += "<p>　請求項の数　　　　　　　　" + numberOfClaim + "</p>\r\n";
                            break;
                        case "addressed-to-person-group":
                            if(node.Attributes["jp:kind-of-person"].Value == "applicant")
                            {
                                XmlNode node_name = node.SelectSingleNode("jp:addressbook/jp:name", xmlNsManager);
                                if(node_name != null)
                                {
                                    wHtmlbody += "<p>　特許出願人　　　　　　　　" + node_name.InnerText + "</p>\r\n";
                                }
                            } else
                            if (node.Attributes["jp:kind-of-person"].Value == "attorney")
                            {
                                XmlNode node_name = node.SelectSingleNode("jp:addressbook/jp:name", xmlNsManager);
                                if (node_name != null)
                                {
                                    wHtmlbody += "<p>　代理人　　　　　　　　　　" + node_name.InnerText + "</p>\r\n";
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
                            wHtmlbody += "<p>　適用条文　　　　　　　　　" + article + "</p>\r\n";
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
                if(node.LocalName == "img")
                {
                    wHtmlbody += "\r\n" + node_img(node);
                }
                else
                if(node.LocalName == "chemistry")
                {
                    wHtmlbody += "\r\n【化" + Strings.StrConv(node.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】\r\n";
                    wHtmlbody += p2html(node);
                }
                else
                if (node.LocalName == "tables")
                {
                    wHtmlbody += "\r\n【表" + Strings.StrConv(node.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】\r\n";
                    wHtmlbody += p2html(node);
                }
                else
                if (node.LocalName == "maths")
                {
                    wHtmlbody += "\r\n【数" + Strings.StrConv(node.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】\r\n";
                    wHtmlbody += p2html(node);
                }
                else
                if (node.LocalName == "#text")
                {
                    wHtmlbody += HttpUtility.HtmlEncode(node.OuterXml);
                }
                else
                {
                    wHtmlbody += node.OuterXml;
                }
            }
            wHtmlbody += "\r\n";
            return wHtmlbody;
        }
        private string node_img(XmlNode node)
        {
            string wHtmlbody = "<p>";
            int height = (int)(3.777 * double.Parse(node.Attributes["he"].Value));
            int width = (int)(3.777 * double.Parse(node.Attributes["wi"].Value));
            string w_src_png = Path.GetFileNameWithoutExtension(node.Attributes["file"].Value) + ".png";
            string w_src1 = m_dirName + @"\" + w_src_png;

            string w_src0 = m_dirName + @"\" + node.Attributes["file"].Value;
            System.Drawing.Image img = System.Drawing.Bitmap.FromFile(w_src0);
            img.Save(w_src1, System.Drawing.Imaging.ImageFormat.Png);
            wHtmlbody += "<img height=" + height.ToString() + " width=" + width.ToString() + " src=\"" + w_src_png + "\"></p>\r\n";
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
