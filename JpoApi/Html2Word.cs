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

namespace JpoApi
{
    public class Html2Word : IDisposable
    {
        public string m_htmlName { get; set; }  // 
        public string m_docxPath { get; set; }  // 
        private string m_htmlPath { get; set; }
        private string m_docxName { get; set; }  // 

        private Word.Document m_wordDoc = null;
        private Word.Application m_oWord = null;
        private bool disposedValue;
        public Html2Word(string htmlPath, string docxPath = null)
        {
            try
            {
                m_htmlPath = htmlPath;
                object odocxPath;
                if (docxPath == null)
                {
                    // htmlファイルと同一パス・同一名称で拡張子のみ変更
                    m_docxPath = Path.GetDirectoryName(htmlPath) + @"\" + Path.GetFileNameWithoutExtension(htmlPath) + ".docx";
                }
                else
                {
                    // htmlファイルと同一パス・同一名称で拡張子のみ変更
                    m_docxPath = docxPath;
                }
                m_docxName = Path.GetFileNameWithoutExtension(m_docxPath);

                odocxPath = (object)m_docxPath;
                if (File.Exists(m_docxPath))
                {
                    if(File.GetLastWriteTime(m_htmlPath) == File.GetLastWriteTime(m_docxPath))
                    {
                        return;
                    }
                    File.Delete(m_docxPath);
                }
                object oHtmlPath = (object)htmlPath;
                _Html2Word(oHtmlPath, odocxPath);
                if(File.Exists(m_docxPath))
                {
                    File.SetLastWriteTime(m_docxPath, File.GetLastWriteTime(m_htmlPath));
                    File.SetCreationTime(m_docxPath, File.GetCreationTime(m_htmlPath));
                    File.SetLastAccessTime(m_docxPath, File.GetLastAccessTime(m_htmlPath));
                }
                else
                {
                    m_docxPath = "";
                }
            }
            catch (Exception ex)
            {
                m_docxPath = "";
                return;
            }
        }

        private void _Html2Word(object oHtmlPath, object odocxPath)
        {
            try
            {
                m_wordDoc = null;
                m_oWord = null;

                // Word アプリケーションオブジェクトを作成
                m_oWord = new Word.Application();
                m_oWord.Visible = false;
                m_wordDoc = new Word.Document();
                object oMissing = System.Reflection.Missing.Value;

                object confirmconversion = System.Reflection.Missing.Value;
                object readOnly = false;
                object oallowsubstitution = System.Reflection.Missing.Value;
                // 絶対パスを与える必要あり
                m_wordDoc = m_oWord.Documents.Open(ref oHtmlPath, ref confirmconversion, ref readOnly, ref oMissing,
                                              ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                              ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                              ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                if(m_docxName.IndexOf("拒絶理由通知書") >= 0
                || m_docxName.IndexOf("特許査定") >= 0
                || m_docxName.IndexOf("拒絶査定") >= 0
                || m_docxName.IndexOf("補正の却下の決定") >= 0)
                {
                    RefusalReasonページ設定();
                } else
                {
                    OpinionAmendmentページ設定();
                }
                foreach (Paragraph paragraph in m_wordDoc.Paragraphs)
                {
                    if (paragraph.Range.OMaths.Count == 0 && paragraph.Range.Tables.Count == 0 && paragraph.Range.InlineShapes.Count == 0)
                    {
                        フォント設定(paragraph.Range.Font);
                        行設定(paragraph.Range.ParagraphFormat);
                    }
                }
                object fileFormat = WdSaveFormat.wdFormatDocumentDefault;
                m_wordDoc.SaveAs2(ref odocxPath, ref fileFormat);
                // 文書を閉じる
                m_wordDoc.Close();
                m_wordDoc = null;
                m_oWord.Quit();
                m_oWord = null;
            }
            catch (Exception ex)
            {
                if (m_wordDoc != null)
                {
                    m_wordDoc.Close();
                    m_wordDoc = null;
                }
                if (m_oWord != null)
                {
                    m_oWord.Quit();
                    m_oWord = null;
                }
                return;
            }
        }

        public void フォント設定(Word.Font font)
        {
            if (m_wordDoc == null
            || m_wordDoc.TrackRevisions)
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
            Word.Style NormalStyle = m_wordDoc.Styles[Word.WdBuiltinStyle.wdStyleNormal];
            NormalStyle.Font.Color = Word.WdColor.wdColorBlack;
            NormalStyle.Font.Name = "ＭＳ 明朝";
            NormalStyle.Font.NameAscii = "ＭＳ 明朝";
            NormalStyle.Font.NameFarEast = "ＭＳ 明朝";
            NormalStyle.Font.NameOther = "ＭＳ 明朝";
            NormalStyle.Font.Size = (float)(12.0);

            Word.PageSetup pageSetup = m_wordDoc.PageSetup;
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
            Word.Style NormalStyle = m_wordDoc.Styles[Word.WdBuiltinStyle.wdStyleNormal];
            NormalStyle.Font.Color = Word.WdColor.wdColorBlack;
            NormalStyle.Font.Name = "ＭＳ 明朝";
            NormalStyle.Font.NameAscii = "ＭＳ 明朝";
            NormalStyle.Font.NameFarEast = "ＭＳ 明朝";
            NormalStyle.Font.NameOther = "ＭＳ 明朝";
            NormalStyle.Font.Size = (float)(12.0);

            Word.PageSetup pageSetup = m_wordDoc.PageSetup;
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
            if (m_wordDoc == null
            || m_wordDoc.TrackRevisions)
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
