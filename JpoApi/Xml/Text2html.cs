using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.IO;


namespace JpoApi
{
    public class Text2html : IDisposable
    {
        private bool disposedValue;
        public string m_htmlBody { get; set; }
        public string m_Title { get; set; }
        public class Html
        {
            public string head { get; set; }
            public List<string> p {  get; set; }
        }
        public Html m_html {  get; set; }
        private string m_xmlPath {  get; set; }
        private string m_dirName { get; set; }
        public Text2html(string a_xmlPath)
        {
            this.m_xmlPath = a_xmlPath;
            this.m_dirName = System.IO.Path.GetDirectoryName(a_xmlPath);
            this.m_html = new Html();
            this.m_html.p = new List<string>();
            this.m_htmlBody = "";
            this.m_Title = "";
        }
        public void setTitle(string szTitle)
        {
            this.m_html.head = "<head>\r\n"
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
            return;
        }
        public void addP(string szText)
        {
            szText = szText.Replace("\r\n", "");
            string[] separatingStrings = { "<br />" };
            string[] lines = szText.Split(separatingStrings, System.StringSplitOptions.None);
            foreach (string line in lines)
            {
                string p = "<p style='margin:0mm;line-height:14.8pt;punctuation-wrap:simple;vertical-align:\r\n"
                   + "baseline;word-break:break-all'>";
                if (line.Length > 0)
                {
                    p += "<span style='font-family:\"ＭＳ 明朝\",serif'>";
                    p += line.Replace(Convert.ToChar(0xA0).ToString(), @"&#160;");
                }
                else
                {
                    p += "<span lang=EN-US style='color:windowtext'>&nbsp;";
                }
                p += "</span></p>\r\n\r\n";
                this.m_html.p.Add(p);
            }
        }
        public void addHtml(string szText)
        {
            this.m_html.p.Add(szText);
        }

        // "claim-text" の処理
        public void addOuterXml(string outerXml)
        {
            outerXml = @"<root>" + outerXml + @"</root>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(outerXml);

            XmlNode pTag = doc.DocumentElement; // <p>タグを取得

            foreach (XmlNode child in pTag.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element) // <p>タグを取得
                {
                    if (child.InnerText.Length == 0)
                    {
                        this.addP("");
                    }
                    else
                    {
                        string wHtmlbody = this.convHtml(child);
                        if(wHtmlbody.Length > 0)
                        {
                            this.addP(wHtmlbody);
                        }
                    }
                }
            }
        }

        // Paragraph の処理
        public void addInnerXml(string paragraph)
        {
            paragraph = @"<root>" + paragraph + @"</root>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(paragraph);

            XmlNode pTag = doc.DocumentElement; // <p>タグを取得
            string wHtmlbody = this.convHtml(pTag);
            if (wHtmlbody.Length > 0)
            {
                this.addP(wHtmlbody);
            }
        }

        // タグの再帰処理
        public string convHtml(XmlNode node)
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
                            if (wHtmlbody.Length > 0) this.addP(wHtmlbody);
                            wHtmlbody = string.Empty;
                            this.addP(node_img(child));
                            wHtmlbody = string.Empty;
                            break;
                        case "chemistry":
                            if (wHtmlbody.Length > 0) this.addP(wHtmlbody);
                            wHtmlbody = string.Empty;
                            this.addP("【化" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】");
                            wHtmlbody += convHtml(child);
                            break;
                        case "tables":
                            if (wHtmlbody.Length > 0) this.addP(wHtmlbody);
                            wHtmlbody = string.Empty;
                            this.addP("【表" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】");
                            wHtmlbody += convHtml(child);
                            break;
                        case "maths":
                            if (wHtmlbody.Length > 0) this.addP(wHtmlbody);
                            wHtmlbody = string.Empty;
                            this.addP("【数" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】");
                            wHtmlbody += convHtml(child);
                            break;
                        case "patcit":
                            if (wHtmlbody.Length > 0) this.addP(wHtmlbody);
                            wHtmlbody = string.Empty;
                            this.addP("　　【特許文献" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】" + child.OuterXml);
                            break;
                        case "nplcit":
                            if (wHtmlbody.Length > 0) this.addP(wHtmlbody);
                            wHtmlbody = string.Empty;
                            this.addP("　　【非特許文献" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】" + child.OuterXml);
                            break;
                        case "figref":
                            if (wHtmlbody.Length > 0) this.addP(wHtmlbody);
                            wHtmlbody = string.Empty;
                            this.addP("　　【図" + Strings.StrConv(child.Attributes["num"].Value, VbStrConv.Wide, 0x411) + "】" + child.OuterXml);
                            break;
                        case "#text":
                            string szText = HttpUtility.HtmlEncode(child.OuterXml);
                            szText = szText.Replace("&#160;", "&#32;");
                            wHtmlbody += szText;
                            break;
                        case "br":
                            if (child.InnerText == null)
                            {
                                this.addP(wHtmlbody);
                            }
                            else
                            {
                                wHtmlbody += this.convHtml(child);
                                this.addP(wHtmlbody);
                            }
                            wHtmlbody = string.Empty;
                            break;
                        case "u":
                        default:
                            if (child.InnerText == null)
                            {
                                wHtmlbody += "<" + child.Name + " />";
                            }
                            else
                            {
                                wHtmlbody += "<" + child.Name + ">";
                                wHtmlbody += this.convHtml(child);
                                wHtmlbody += "</" + child.Name + ">";
                            }
                            break;
                    }
                }
            }
            return wHtmlbody;
        }
        public void addElement(XmlElement element)
        {
            string wHtmlbody = this.convHtml((XmlNode)element);
            if (wHtmlbody.Length > 0)
            {
                this.addP(wHtmlbody);
            }
        }

        public string htmlAll()
        {
            string wHtmlBody = "<html>\r\n";
            wHtmlBody += this.m_html.head;
            wHtmlBody += "<body lang=JA style='word-wrap:break-word'>\r\n"
                + "\r\n"
                + "<div class=WordSection1 style='layout-grid:16.3pt .05pt'>\r\n"
                + "\r\n"
                + "<div>\r\n";
            foreach (string w_p in this.m_html.p)
            {
                wHtmlBody += w_p;
            }
            wHtmlBody += "</div>\r\n"
                    + "\r\n"
                    + "</div>\r\n"
                    + "\r\n"
                    + "</body>\r\n\r\n";
            wHtmlBody += "</html>";
            return wHtmlBody;
        }
        public string html_body(string contents)
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
        // ~text2html()
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
