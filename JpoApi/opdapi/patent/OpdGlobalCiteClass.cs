using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using static JpoApi.OpdGlobalDocList;

namespace JpoApi
{
    public class OpdGlobalCiteClass : IDisposable
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
        public readonly int e_ACCOUNT = 0x00000040;

        public CData m_data { get; set; }
        public CResult m_result { get; set; }           // APIの結果

        private string m_result_json = "{\r\n  \"result\": {\r\n    \"statusCode\": \"\",\r\n    \"errorMessage\": \"\",\r\n    \"remainAccessCount\": \"\"\r\n  }\r\n}\r\n";

        public class CApplicantAttorneyCd   // 申請人
        {
            public string applicantAttorneyCd { get; set; } // 申請人コード
            public string name { get; set; }                // 申請人氏名・名称
        }
        public class CData
        {
            public CApplicantAttorneyCd[] applicantAttorney;    // 申請人
        }
        public class CResult
        {
            public string statusCode { get; set; }      // ステータスコード
            public string errorMessage { get; set; }    // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数
            public CData data { get; set; }             // 詳細情報データ

        }
        private class CGlobalDocList
        {
            public CResult result { get; set; }
        }

        private string xmlString = "<?xml version=\"1.0\" encoding=\"utf-8\"?><api-data xmlns=\"https://www.jpo.go.jp\"><statusCode></statusCode><errorMessage /><remainAccessCount></remainAccessCount></api-data>";

        [XmlRoot("api-data", Namespace = "https://www.jpo.go.jp")]
        public class XApiData
        {
            [XmlElement("statusCode", IsNullable = true)]
            public string statusCode { get; set; }

            [XmlElement("errorMessage", IsNullable = true)]
            public string errorMessage { get; set; }

            [XmlElement("remainAccessCount", IsNullable = true)]
            public string remainAccessCount { get; set; }

            [XmlElement("citation-and-classification-data", IsNullable = true)]
            public CitationAndClassificationData citationAndClassificationData { get; set; }
        }
        public class CitationAndClassificationData
        {
            [XmlElement("reference", IsNullable = true)]
            public Reference reference { get; set; }

            [XmlElement("classification", IsNullable = true)]
            public Classification classification { get; set; }

            [XmlElement("citation", IsNullable = true)]
            public Citation citation { get; set; }
        }

        public class Reference
        {
            [XmlElement("application-number", IsNullable = true)]
            public ApplicationNumber applicationNumber { get; set; }

            [XmlElement("publication-number", IsNullable = true)]
            public PublicationNumber publicationNumber { get; set; }

            [XmlElement("registration-number", IsNullable = true)]
            public RegistrationNumber registrationNumber { get; set; }
        }

        public class ApplicationNumber
        {
            [XmlElement("document-number", IsNullable = true)]
            public string documentNumber { get; set; }

            [XmlElement("date", IsNullable = true)]
            public string date { get; set; }
        }
        public class PublicationNumber
        {
            [XmlElement("document-number", IsNullable = true)]
            public string documentNumber { get; set; }

            [XmlElement("date", IsNullable = true)]
            public string date { get; set; }
        }
        public class RegistrationNumber
        {
            [XmlElement("document-number", IsNullable = true)]
            public string documentNumber { get; set; }

            [XmlElement("date", IsNullable = true)]
            public string date { get; set; }
        }

        public class Classification
        {
            [XmlArray("ipcs", IsNullable = true)]
            [XmlArrayItem("ipc", IsNullable = true)]
            public List<string> ipc { get; set; }

            [XmlElement("originals")]
            public Originals originals { get; set; }

        }
        public class Originals
        {
            [XmlElement("original")]
            public List<Original> OriginalList { get; set; }
        }
        public class Original
        {
            [XmlAttribute("scheme")]
            public string Scheme { get; set; }

            [XmlText]
            public string Value { get; set; }
        }
        public class Citation
        {
            [XmlArray("patent-literatures", IsNullable = true)]
            [XmlArrayItem("patent-literature", IsNullable = true)]
            public List<PatentLiteratureLists> patentLiteratureLists { get; set; }

            [XmlArray("non-patent-literatures", IsNullable = true)]
            [XmlArrayItem("non-patent-literature", IsNullable = true)]
            public List<NonPatentLiteratureLists> nonPatentLiteratureLists { get; set; }
        }
        public class PatentLiteratureLists
        {
            [XmlElement("draft-date", IsNullable = true)]
            public string draftDate { get; set; }

            [XmlElement("cited-in", IsNullable = true)]
            public string citedIn { get; set; }

            [XmlElement("publication-number", IsNullable = true)]
            public PublicationNumber2 publicationNumber { get; set; }
        }
        public class PublicationNumber2
        {
            [XmlAttribute("format")]
            public string format { get; set; }

            [XmlElement("document-number", IsNullable = true)]
            public string documentNumber { get; set; }
        }
        public class NonPatentLiteratureLists
        {
            [XmlElement("draft-date", IsNullable = true)]
            public string draftDate { get; set; }

            [XmlElement("cited-in", IsNullable = true)]
            public string citedIn { get; set; }

            [XmlElement("text", IsNullable = true)]
            public string text { get; set; }
        }
        public XApiData m_resultXML { get; set; }           // APIの結果
        public string m_responseFile { get; set; }
        public string m_response { get; set; }

        public OpdGlobalCiteClass(string fileNumber, string a_access_token = "")
        {
            try
            {
                if (a_access_token.Length == 0)
                {
                    using (Account ac = new Account())
                    {
                        using (AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path))
                        {
                            a_access_token = at.m_access_token.access_token;
                        }
                    }
                }
                if (a_access_token.Length == 0)
                {
                    this.m_error = this.e_ACCOUNT;
                    this.m_response = "";
                    this.m_responseFile = "";
                    this.m_data = null;
                    this.m_result = null;
                    return;
                }
                this.m_result = JsonConvert.DeserializeObject<CResult>(this.m_result_json);
                this.m_error = e_NONE;

                using (Cache responseCache = new Cache(a_access_token))
                {
                    this.m_response = responseCache.GetXml("opdapi/patent/v1/global_cite_class/" + fileNumber);
                    this.m_responseFile = responseCache.m_responseFilePath;
                    this.m_error = responseCache.m_error;

                    if (this.m_response.Length > 0)
                    {
                        XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(XApiData));
                        TextReader reader = new StringReader(this.m_response);
                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.IgnoreWhitespace = true;
                        settings.IgnoreProcessingInstructions = true;
                        settings.IgnoreComments = true;
                        XmlReader xmlReader = XmlReader.Create(reader, settings);
                        this.m_resultXML = (XApiData)serializer.Deserialize(xmlReader);

                        this.m_result.statusCode = this.m_resultXML.statusCode;
                        this.m_result.errorMessage = this.m_resultXML.errorMessage;
                        this.m_result.remainAccessCount = this.m_resultXML.remainAccessCount;

                        this.m_error = responseCache.m_error;
                    }
                    else
                    {
                        this.m_error = this.e_ACCOUNT;
                    }
                }
            }
            catch (Exception ex)
            {
                this.m_error = this.e_ACCOUNT;
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
        // ~OpdGlobalCiteClass()
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
