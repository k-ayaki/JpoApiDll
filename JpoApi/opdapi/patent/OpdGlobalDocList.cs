using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.Office.Interop.Word;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
//using static JpoApi.DocumentListData;

namespace JpoApi
{
    public class OpdGlobalDocList : IDisposable
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

        /*
        private class XCitationAndClassificationData
        {

        }
        */
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

            [XmlElement("document-list-data", IsNullable = true)]
            public DocumentListData documentListData { get; set; }
        }

        public class DocumentListData
        {
            [XmlElement("bibliographic", IsNullable = true)]
            public Bibliographic bibliographic { get; set; }

            [XmlArray("document-lists")]
            [XmlArrayItem("document-list")]
            public List<DocumentList> documentLists { get; set; }
        }

        public class Bibliographic
        {
            [XmlElement("original", IsNullable = true)]
            public Original original { get; set; }

            [XmlElement("translated", IsNullable = true)]
            public Translated translated { get; set; }
        }
        public class Original
        {
            [XmlElement("invention-title", IsNullable = true)]
            public string inventionTitle { get; set; }

            [XmlElement("applicant", IsNullable = true)]
            public Applicant applicant { get; set; }
        }
        public class Translated
        {
            [XmlElement("invention-title", IsNullable = true)]
            public string inventionTitle { get; set; }

            [XmlElement("applicant", IsNullable = true)]
            public Applicant applicant { get; set; }
        }

        public class Applicant
        {
            [XmlElement("last-name", IsNullable = true)]
            public string lastName { get; set; }
        }

        public class DocumentList
        {
            [XmlAttribute("group")]
            public string group { get; set; }

            [XmlElement("legal-date", IsNullable = true)]
            public string legalDate { get; set; }

            [XmlElement("original", IsNullable = true)]
            public Original2 original { get; set; }

            [XmlElement("translated", IsNullable = true)]
            public Translated2 translated { get; set; }
        }

        public class Original2
        {
            [XmlAttribute("id")]
            public string id { get; set; }

            [XmlElement("document-description", IsNullable = true)]
            public string documentDescription { get; set; }
        }
        public class Translated2
        {
            [XmlAttribute("id")]
            public string id { get; set; }

            [XmlElement("document-description", IsNullable = true)]
            public string documentDescription { get; set; }
        }

        public XApiData m_resultXML { get; set; }           // APIの結果
        public string m_responseFile { get; set; }
        public string m_response { get; set; }

        public OpdGlobalDocList(string applicationNumber, string a_access_token = "")
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
                    this.m_response = responseCache.GetXml("opdapi/patent/v1/global_doc_list/" + applicationNumber);
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
        // ~global_doc_list()
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
