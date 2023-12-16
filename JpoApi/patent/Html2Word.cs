using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Office.Interop.Word;
using Word = Microsoft.Office.Interop.Word;
using System.Xml.Linq;
using System.IO.Compression;
using System.Drawing.Printing;
using DocumentFormat.OpenXml.Office2013.Word;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools;
using DocumentFormat.OpenXml.VariantTypes;

namespace JpoApi
{
    public class Html2Word : IDisposable
    {
        public int m_error { get; set; }
        public readonly int e_NONE = 0x00000000;
        public readonly int e_NETWORK = 0x00000001;
        public readonly int e_SERVER = 0x00000002;
        public readonly int e_TIMEOVER = 0x00000004;
        public readonly int e_CONTENT = 0x00000008;
        public readonly int e_ZIPFILE = 0x00000010;
        public readonly int e_CACHE = 0x00000020;
        public readonly int e_WORDFILE = 0x00000040;
        public string m_htmlName { get; set; }  //
        public string m_wordFilePath { get; set; }  // 
        private string m_htmlPath { get; set; } //
        private string m_docxName { get; set; }  // 

        private Word.Document m_wordDoc = null;
        private Word.Application m_oWord = null;
        private bool disposedValue;
        public Html2Word(string htmlPath, double arMargin = 20.0, double alMargin=15.0, double abMargin=20.0, double atMargin=15.0)
        {
            try
            {
                this.m_error = this.e_NONE;
                this.m_htmlPath = htmlPath;
                // htmlファイルと同一パス・同一名称で拡張子のみ変更
                this.m_wordFilePath = Path.GetDirectoryName(htmlPath) + @"\" + Path.GetFileNameWithoutExtension(htmlPath) + ".docx";
                if (File.Exists(this.m_wordFilePath))
                {
                    File.Delete(this.m_wordFilePath);
                }
                ConvertDOCX(htmlPath, this.m_wordFilePath, false, arMargin, alMargin, abMargin, atMargin);
                if(File.Exists(this.m_wordFilePath))
                {
                    File.SetLastWriteTime(this.m_wordFilePath, File.GetLastWriteTime(this.m_htmlPath));
                    File.SetCreationTime(this.m_wordFilePath, File.GetCreationTime(this.m_htmlPath));
                    File.SetLastAccessTime(this.m_wordFilePath, File.GetLastAccessTime(this.m_htmlPath));
                }
                else
                {
                    this.m_error = this.e_WORDFILE;
                }
            }
            catch (Exception ex)
            {
                this.m_error = this.e_WORDFILE;
                return;
            }
        }

        // DocumentFormat.OpenXML が必要
        private static void ConvertDOCX(string htmlPath, string docxPath, bool isLandScape, double arMargin, double alMargin, double abMargin, double atMargin)
        {
            try
            {
                string htmlSectionID = "Sect1";
                //Creating a word document using the the Open XML SDK 2.0
                using (WordprocessingDocument document = WordprocessingDocument.Create(docxPath, WordprocessingDocumentType.Document))
                {
                    //create a paragraph
                    MainDocumentPart mainDocumenPart = document.AddMainDocumentPart();
                    mainDocumenPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    Body documentBody = new Body();
                    mainDocumenPart.Document.Append(documentBody);
                    string htmlText = File.ReadAllText(htmlPath, System.Text.Encoding.GetEncoding("shift_jis"));

                    MemoryStream ms = new MemoryStream(Encoding.GetEncoding("shift_jis").GetBytes(htmlText));

                    // Create alternative format import part.
                    AlternativeFormatImportPart formatImportPart = mainDocumenPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.Html, htmlSectionID);

                    //ms.Seek(0, SeekOrigin.Begin);

                    // Feed HTML data into format import part (chunk).
                    formatImportPart.FeedData(ms);
                    AltChunk altChunk = new AltChunk();
                    altChunk.Id = htmlSectionID;
                    mainDocumenPart.Document.Body.Append(altChunk);
                    /*
                     inch equiv = 1440 (1 inch margin)
                     */
                    double width = 210.0 * 1440.0 / 25.4;   // A4 width
                    double height = 297.0 * 1440.0 / 25.4;   // A4 height

                    SectionProperties sectionProps = new SectionProperties();
                    PageSize pageSize;
                    if (isLandScape)
                    {
                        pageSize = new PageSize() { Width = (UInt32Value)height, Height = (UInt32Value)width, Orient = PageOrientationValues.Landscape };
                    }
                    else
                    {
                        pageSize = new PageSize() { Width = (UInt32Value)width, Height = (UInt32Value)height, Orient = PageOrientationValues.Portrait };
                    }

                    double rMargin = arMargin * 1440.0 / 25.4;
                    double lMargin = alMargin * 1440.0 / 25.4;
                    double bMargin = abMargin * 1440.0 / 25.4;
                    double tMargin = atMargin * 1440.0 / 25.4;

                    PageMargin pageMargin = new PageMargin() { Top = (Int32)tMargin, Right = (UInt32Value)rMargin, Bottom = (Int32)bMargin, Left = (UInt32Value)lMargin, Header = (UInt32Value)360U, Footer = (UInt32Value)360U, Gutter = (UInt32Value)0U };
                    sectionProps.Append(pageSize);
                    sectionProps.Append(pageMargin);
                    mainDocumenPart.Document.Body.Append(sectionProps);

                    //Saving/Disposing of the created word Document
                    document.MainDocumentPart.Document.Save();
                    document.Dispose();
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        private void _Html2Word(object oHtmlPath, object odocxPath)
        {
            try
            {
                this.m_wordDoc = null;
                this.m_oWord = null;

                // Word アプリケーションオブジェクトを作成
                this.m_oWord = new Word.Application();
                this.m_oWord.Visible = false;
                this.m_wordDoc = new Word.Document();
                object oMissing = System.Reflection.Missing.Value;

                object confirmconversion = System.Reflection.Missing.Value;
                object readOnly = false;
                object oallowsubstitution = System.Reflection.Missing.Value;
                // 絶対パスを与える必要あり
                this.m_wordDoc = this.m_oWord.Documents.Open(ref oHtmlPath, ref confirmconversion, ref readOnly, ref oMissing,
                                              ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                              ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                              ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                if(this.m_docxName.IndexOf("拒絶理由通知書") >= 0
                || this.m_docxName.IndexOf("特許査定") >= 0
                || this.m_docxName.IndexOf("拒絶査定") >= 0
                || this.m_docxName.IndexOf("補正の却下の決定") >= 0)
                {
                    RefusalReasonページ設定();
                } else
                {
                    OpinionAmendmentページ設定();
                }
                /*
                foreach (Paragraph paragraph in m_wordDoc.Paragraphs)
                {
                    if (paragraph.Range.OMaths.Count == 0 && paragraph.Range.Tables.Count == 0 && paragraph.Range.InlineShapes.Count == 0)
                    {
                        フォント設定(paragraph.Range.Font);
                        行設定(paragraph.Range.ParagraphFormat);
                    }
                }
                */
                object fileFormat = WdSaveFormat.wdFormatDocumentDefault;
                this.m_wordDoc.SaveAs2(ref odocxPath, ref fileFormat);
                // 文書を閉じる
                this.m_wordDoc.Close();
                this.m_wordDoc = null;
                this.m_oWord.Quit();
                this.m_oWord = null;
            }
            catch (Exception ex)
            {
                if (this.m_wordDoc != null)
                {
                    this.m_wordDoc.Close();
                    this.m_wordDoc = null;
                }
                if (this.m_oWord != null)
                {
                    this.m_oWord.Quit();
                    this.m_oWord = null;
                }
                return;
            }
        }

        public void フォント設定(Word.Font font)
        {
            if (this.m_wordDoc == null
            || this.m_wordDoc.TrackRevisions)
            {
                return;
            }
            font.NameFarEast = "ＭＳ 明朝";
            font.NameAscii = "ＭＳ 明朝";
            font.NameOther = "ＭＳ 明朝";
            font.Name = "ＭＳ 明朝";
            font.Size = (float)(12.0);

            font.Bold = 0;    // 太字
            font.Italic = 0; // 斜体
            font.StrikeThrough = 0;   // 取り消し線
            font.DoubleStrikeThrough = 0; //    ' 二重取り消し線
            font.Outline = 0; // アウトライン
            font.Emboss = 0;  // エンボス
            font.Shadow = 0;  // 影
            font.Hidden = 0;  // 隠し文字
            font.SmallCaps = 0;   // 大文字化
            font.AllCaps = 0;
            font.Color = WdColor.wdColorAutomatic;    // 文字色
            font.Engrave = 0;
            font.Spacing = 0;     // 文字間隔
            font.Scaling = 100;   // 文字スケール
            font.Position = 0;    // 文字位置
            //font.Borders[1].LineStyle = WdLineStyle.wdLineStyleNone;  //
        }

        public void RefusalReasonページ設定()
        {
            Word.Style NormalStyle = this.m_wordDoc.Styles[Word.WdBuiltinStyle.wdStyleNormal];
            NormalStyle.Font.Color = Word.WdColor.wdColorBlack;
            NormalStyle.Font.Name = "ＭＳ 明朝";
            NormalStyle.Font.NameAscii = "ＭＳ 明朝";
            NormalStyle.Font.NameFarEast = "ＭＳ 明朝";
            NormalStyle.Font.NameOther = "ＭＳ 明朝";
            NormalStyle.Font.Size = (float)(12.0);

            Word.PageSetup pageSetup = this.m_wordDoc.PageSetup;
            pageSetup.TextColumns.SetCount(1);
            pageSetup.TextColumns.EvenlySpaced = -1;
            pageSetup.TextColumns.LineBetween = 0;

            pageSetup.LineNumbering.Active = 0;
            pageSetup.Orientation = WdOrientation.wdOrientPortrait;
            pageSetup.TopMargin = m_wordDoc.Application.MillimetersToPoints(20);
            pageSetup.BottomMargin = m_wordDoc.Application.MillimetersToPoints(15);
            pageSetup.LeftMargin = m_wordDoc.Application.MillimetersToPoints(30);       // 拒絶理由通知
            pageSetup.RightMargin = m_wordDoc.Application.MillimetersToPoints(25);      // 拒絶理由通知
            pageSetup.Gutter = m_wordDoc.Application.MillimetersToPoints(0);
            pageSetup.HeaderDistance = m_wordDoc.Application.MillimetersToPoints(10);
            pageSetup.FooterDistance = m_wordDoc.Application.MillimetersToPoints(10);
            pageSetup.PageWidth = m_wordDoc.Application.MillimetersToPoints(210);
            pageSetup.PageHeight = m_wordDoc.Application.MillimetersToPoints(297);
            pageSetup.FirstPageTray = WdPaperTray.wdPrinterDefaultBin;
            pageSetup.OtherPagesTray = WdPaperTray.wdPrinterDefaultBin;
            pageSetup.SectionStart = WdSectionStart.wdSectionContinuous;
            pageSetup.OddAndEvenPagesHeaderFooter = 0;
            pageSetup.DifferentFirstPageHeaderFooter = 0;
            pageSetup.VerticalAlignment = WdVerticalAlignment.wdAlignVerticalTop;

            pageSetup.SuppressEndnotes = -1;
            pageSetup.MirrorMargins = 0;
            pageSetup.TwoPagesOnOne = false;
            pageSetup.BookFoldPrinting = false;
            pageSetup.BookFoldRevPrinting = false;
            pageSetup.BookFoldPrintingSheets = 1;
            pageSetup.GutterPos = WdGutterStyle.wdGutterPosLeft;
            pageSetup.CharsLine = 36;                               // 拒絶理由通知
            pageSetup.LayoutMode = WdLayoutMode.wdLayoutModeGrid;
        }
        public void OpinionAmendmentページ設定()
        {
            Word.Style NormalStyle = this.m_wordDoc.Styles[Word.WdBuiltinStyle.wdStyleNormal];
            NormalStyle.Font.Color = Word.WdColor.wdColorBlack;
            NormalStyle.Font.Name = "ＭＳ 明朝";
            NormalStyle.Font.NameAscii = "ＭＳ 明朝";
            NormalStyle.Font.NameFarEast = "ＭＳ 明朝";
            NormalStyle.Font.NameOther = "ＭＳ 明朝";
            NormalStyle.Font.Size = (float)(12.0);

            Word.PageSetup pageSetup = this.m_wordDoc.PageSetup;
            pageSetup.TextColumns.SetCount(1);
            pageSetup.TextColumns.EvenlySpaced = -1;
            pageSetup.TextColumns.LineBetween = 0;

            pageSetup.LineNumbering.Active = 0;
            pageSetup.Orientation = WdOrientation.wdOrientPortrait;
            pageSetup.TopMargin = m_wordDoc.Application.MillimetersToPoints(20);
            pageSetup.BottomMargin = m_wordDoc.Application.MillimetersToPoints(15);
            pageSetup.LeftMargin = m_wordDoc.Application.MillimetersToPoints(20);
            pageSetup.RightMargin = m_wordDoc.Application.MillimetersToPoints(15);
            pageSetup.Gutter = m_wordDoc.Application.MillimetersToPoints(0);
            pageSetup.HeaderDistance = m_wordDoc.Application.MillimetersToPoints(10);
            pageSetup.FooterDistance = m_wordDoc.Application.MillimetersToPoints(10);
            pageSetup.PageWidth = m_wordDoc.Application.MillimetersToPoints(210);
            pageSetup.PageHeight = m_wordDoc.Application.MillimetersToPoints(297);
            pageSetup.FirstPageTray = WdPaperTray.wdPrinterDefaultBin;
            pageSetup.OtherPagesTray = WdPaperTray.wdPrinterDefaultBin;
            pageSetup.SectionStart = WdSectionStart.wdSectionContinuous;
            pageSetup.OddAndEvenPagesHeaderFooter = 0;
            pageSetup.DifferentFirstPageHeaderFooter = 0;
            pageSetup.VerticalAlignment = WdVerticalAlignment.wdAlignVerticalTop;

            pageSetup.SuppressEndnotes = -1;
            pageSetup.MirrorMargins = 0;
            pageSetup.TwoPagesOnOne = false;
            pageSetup.BookFoldPrinting = false;
            pageSetup.BookFoldRevPrinting = false;
            pageSetup.BookFoldPrintingSheets = 1;
            pageSetup.GutterPos = WdGutterStyle.wdGutterPosLeft;
            pageSetup.CharsLine = 40;
            pageSetup.LayoutMode = WdLayoutMode.wdLayoutModeGrid;
        }
        public void 行設定(Word.ParagraphFormat paragraphFormat)
        {
            if (this.m_wordDoc == null
            || this.m_wordDoc.TrackRevisions)
            {
                return;
            }
            paragraphFormat.SpaceBefore = 0;
            paragraphFormat.SpaceBeforeAuto = 0;
            paragraphFormat.SpaceAfter = 0;
            paragraphFormat.SpaceAfterAuto = 0;
            paragraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceExactly;
            paragraphFormat.LineSpacing = (float)14.8;
            paragraphFormat.WordWrap = 0;
            paragraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            paragraphFormat.WidowControl = 0;
            paragraphFormat.KeepWithNext = 0;
            paragraphFormat.KeepTogether = 0;
            paragraphFormat.PageBreakBefore = 0;
            paragraphFormat.NoLineNumber = 0;

            paragraphFormat.Hyphenation = -1;
            paragraphFormat.FirstLineIndent = m_wordDoc.Application.MillimetersToPoints(0);
            paragraphFormat.OutlineLevel = WdOutlineLevel.wdOutlineLevelBodyText;
            paragraphFormat.CharacterUnitLeftIndent = 0;
            paragraphFormat.CharacterUnitRightIndent = 0;
            paragraphFormat.CharacterUnitFirstLineIndent = 0;
            paragraphFormat.LineUnitBefore = 0;
            paragraphFormat.LineUnitAfter = 0;
            paragraphFormat.AutoAdjustRightIndent = 0;
            paragraphFormat.DisableLineHeightGrid = 0;
            paragraphFormat.FarEastLineBreakControl = 0;
            paragraphFormat.HangingPunctuation = 0;
            paragraphFormat.HalfWidthPunctuationOnTopOfLine = 0;

            paragraphFormat.AddSpaceBetweenFarEastAndAlpha = -1;
            paragraphFormat.AddSpaceBetweenFarEastAndDigit = -1;
            paragraphFormat.BaseLineAlignment = WdBaselineAlignment.wdBaselineAlignBaseline;
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
        // ~Html2Word()
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
