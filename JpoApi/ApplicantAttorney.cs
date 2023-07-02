using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web;
using System.IO.Compression;
using System.Web.UI.WebControls;
using System.Web.Caching;

namespace JpoApi
{
    // 特許申請人コード
    public class ApplicantAttorney : IDisposable
    {
        private bool disposedValue;
        public int m_error;
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
        private class CApplicantAttorney
        {
            public CResult result { get; set; }
        }
        public string m_jsonFile { get; set; }
        public string m_json { get; set; }
        public ApplicantAttorney(string applicationNumber, string a_access_token)
        {
            try
            {
                m_error = e_NONE;
                Cache jsonCache = new Cache(a_access_token);
                this.m_json = jsonCache.GetJson("api/patent/v1/applicant_attorney/" + applicationNumber);
                this.m_jsonFile = jsonCache.m_jsonFilePath;
                this.m_error = jsonCache.m_error;

                CApplicantAttorney jsonObj = JsonConvert.DeserializeObject<CApplicantAttorney>(m_json);
                this.m_result = jsonObj.result;
                this.m_data = jsonObj.result.data;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                ;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                ;
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
        // ~ApplicantAttorney()
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

