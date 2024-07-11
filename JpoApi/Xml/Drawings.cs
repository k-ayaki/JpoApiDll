using DocumentFormat.OpenXml.EMMA;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static JpoApi.Claims;
using static JpoApi.Drawings;
using static JpoApi.PatRspns;

namespace JpoApi
{
    public class Drawings : IDisposable
    {
        private bool disposedValue;

        // XmlRoot属性でルート要素の名前を指定
        [XmlRoot("drawings")]
        public class CDrawings
        {
            // XmlElement属性で子要素の名前を指定
            [XmlElement("figure")]
            public List<CFigure> Figures { get; set; }
            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    wlines += "【書類名】図面<br />\r\n";
                    foreach(CFigure figure in Figures)
                    {
                        wlines += figure.line;
                    }
                    return wlines;
                }
            }
        }
        // XmlRoot属性でルート要素の名前を指定
        [XmlRoot("figure")]
        public class CFigure
        {
            [XmlAttribute("num")]
            public string Num { get; set; }

            // XmlElement属性で子要素の名前を指定
            [XmlElement("img")]
            public Img Img { get; set; }
            public string line
            {
                get
                {
                    string wlines = "【図" + Strings.StrConv(this.Num, VbStrConv.Wide, 0x411) + "】<br />\r\n";
                    wlines += Img.line;
                    return wlines;
                }
            }
        }
        public class Img
        {
            [XmlAttribute("he")]
            public double He { get; set; }

            [XmlAttribute("wi")]
            public double Wi { get; set; }

            [XmlAttribute("file")]
            public string File { get; set; }

            [XmlAttribute("img-format")]
            public string ImgFormat { get; set; }
            public string line
            {
                get
                {
                    string wlines = string.Empty;
                    int height = (int)(3.777 * this.He);
                    int width = (int)(3.777 * this.Wi);
                    string w_src_png = Path.GetFileNameWithoutExtension(this.File) + ".png";
                    string w_src1 = System.IO.Path.GetDirectoryName(m_s_xmlPath) + @"\" + w_src_png;

                    string w_src0 = System.IO.Path.GetDirectoryName(m_s_xmlPath) + @"\" + this.File;
                    System.Drawing.Image img = System.Drawing.Bitmap.FromFile(w_src0);
                    img.Save(w_src1, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] dataPng = System.IO.File.ReadAllBytes(w_src1);
                    string base64Png = Convert.ToBase64String(dataPng);
                    wlines += "<img height=" + height.ToString() + " width=" + width.ToString() + " src=\"data:image/png;base64," + base64Png + "\"><br />\r\n";
                    return wlines;
                }
            }

        }
        public CDrawings m_drawings { get; set; }
        public CFigure m_figure { get; set; }
        public string m_xmlPath { get; set; }
        public Drawings(string szXml, string szXmlPath)
        {
            try
            {
                this.m_xmlPath = szXmlPath;
                this.m_drawings = null;
                XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(CDrawings));
                using (TextReader reader = new StringReader(szXml))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    //settings.CheckCharacters = false;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreComments = true;
                    settings.DtdProcessing = DtdProcessing.Parse;

                    XmlReader xmlReader = XmlReader.Create(reader, settings);
                    m_drawings = (CDrawings)serializer.Deserialize(xmlReader);
                    return;
                }
            }
            catch (Exception ex)
            {
                this.m_drawings = null;
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
        // ~Drawings()
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
