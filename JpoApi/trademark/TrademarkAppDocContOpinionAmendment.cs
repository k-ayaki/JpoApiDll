using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web.UI.WebControls;
using System.Xml;
using DocumentFormat.OpenXml;
using System.Text.RegularExpressions;

namespace JpoApi
{
    public class TrademarkAppDocContOpinionAmendment : IDisposable
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
        public string m_zipFile { get; set; }
        public string m_extractPath { get; set; }
        public string m_response { get; set; }
        public IEnumerable<string> m_files { get; set; }

        public List<XmlDocument> xDocs = new List<XmlDocument>();
        public CResult m_result { get; set; }           // APIの結果

        private string m_result_json = "{\r\n  \"result\": {\r\n    \"statusCode\": \"\",\r\n    \"errorMessage\": \"\",\r\n    \"remainAccessCount\": \"\"\r\n  }\r\n}\r\n";
        public class CResult
        {
            public string statusCode { get; set; }
            public string errorMessage { get; set; }
            public string remainAccessCount { get; set; }
        }
        private class CJpo
        {
            public CResult result { get; set; }
        }

        public TrademarkAppDocContOpinionAmendment(string applicationNumber, string a_access_token = "")
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
                    this.m_extractPath = "";
                    this.m_zipFile = null;
                    this.m_files = new List<string>();
                    this.m_result = null;
                    return;
                }
                this.m_error = e_NONE;
                this.m_result = JsonConvert.DeserializeObject<CResult>(m_result_json);

                using (CacheDocCont docCont = new CacheDocCont(a_access_token))
                {
                    docCont.GetZipXml("api/trademark/v1/app_doc_cont_opinion_amendment/" + applicationNumber);
                    if (docCont.m_response.Length != 0)
                    {
                        this.m_response = docCont.m_response;
                        CJpo cjpo = JsonConvert.DeserializeObject<CJpo>(this.m_response);
                        this.m_result = cjpo.result;
                        this.m_error = docCont.m_error;
                    }
                    else
                    {
                        this.m_extractPath = docCont.m_extractPath;
                        this.m_zipFile = docCont.m_zipFile;
                        this.m_files = docCont.m_files;
                    }
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }
        public string documentNumber(string filePath)
        {
            string[] dirs = filePath.Split('\\');

            Regex rx0 = new Regex(@"([0-9]{11,11})", RegexOptions.None);
            Match w_match0 = rx0.Match(dirs[dirs.Length - 2]);
            if (w_match0.Success)
            {
                return w_match0.Groups[1].Value;
            }
            return "";
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
        // ~AppDocContOpinionAmendment()
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
