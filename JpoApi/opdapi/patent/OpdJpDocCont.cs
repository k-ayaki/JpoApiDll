using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpoApi
{
    public class OpdJpDocCont : IDisposable
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
        public OpdJpDocCont(string szNumber, string szDocumentId, string a_access_token = "")
        {
            try
            {
                this.m_response = "";
                this.m_response = "";
                this.m_zipFile = "";
                this.m_extractPath = "";
                CJpo cjpo2 = JsonConvert.DeserializeObject<CJpo>(this.m_result_json);
                this.m_result = cjpo2.result;

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
                    this.m_extractPath = "";
                    this.m_zipFile = null;
                    this.m_files = new List<string>();
                    return;
                }
                this.m_error = e_NONE;
                using (CacheDocCont docCont = new CacheDocCont(a_access_token))
                {
                    docCont.GetZipXml("opdapi/patent/v1/jp_doc_cont/" + szNumber + "/" + szDocumentId);
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
        // ~JpDocCont()
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
