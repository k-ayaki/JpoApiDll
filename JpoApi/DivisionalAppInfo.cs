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

namespace JpoApi
{
    // 特許分割出願情報
    public class DivisionalAppInfo : IDisposable
    {
        public int m_error;
        private bool disposedValue;
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
        public class CDivisionalApplicationInformation  // 分割出願群情報
        {
            public string applicationNumber { get; set; }   // 出願番号
            public string publicationNumber { get; set; }   // 公開番号
            public string ADPublicationNumber { get; set; } // 公開番号（西暦変換）
            public string nationalPublicationNumber { get; set; }   // 公表番号
            public string ADNationalPublicationNumber { get; set; } // 公表番号（西暦変換）
            public string registrationNumber { get; set; }  // 登録番号
            public string internationalApplicationNumber { get; set; }  // 国際出願番号
            public string internationalPublicationNumber { get; set; }  // 国際公開番号
            public string erasureIdentifier { get; set; }   // 抹消識別
            public string expireDate { get; set; }          // 存続期間満了年月日
            public string disappearanceDate { get; set; }   // 本権利消滅日
            public string divisionalGeneration { get; set; }    // 分割出願の世代
        }
        public class CParentApplicationInformation  // 原出願情報
        {
            public string parentApplicationNumber { get; set; } // 原出願番号
            public string filingDate { get; set; }      // 出願日
        }
        public class CData  // 詳細情報データ
        {
            public string applicationNumber { get; set; }   // 出願番号
            public CParentApplicationInformation parentApplicationInformation { get; set; } // 原出願情報
            public CDivisionalApplicationInformation[] divisionalApplicationInformation { get; set; } // 分割出願群情報
        }
        public class CResult
        {
            public string statusCode { get; set; }  // ステータスコード	
            public string errorMessage { get; set; }    // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数
            public CData data { get; set; } // 詳細情報データ

        }
        private class CDivisionalAppInfo
        {
            public CResult result { get; set; }
        }
        public string m_jsonFile { get; set; }
        public string m_json { get; set; }
        public DivisionalAppInfo(string applicationNumber, string a_access_token)
        {
            try
            {
                m_error = e_NONE;
                Cache jsonCache = new Cache(a_access_token);
                this.m_json = jsonCache.GetJson("api/patent/v1/divisional_app_info/" + applicationNumber);
                this.m_jsonFile = jsonCache.m_jsonFilePath;
                this.m_error = jsonCache.m_error;

                CDivisionalAppInfo jsonObj = JsonConvert.DeserializeObject<CDivisionalAppInfo>(m_json);
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
        // ~AppProgress()
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
