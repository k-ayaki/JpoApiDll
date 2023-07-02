using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpoApi
{
    public class JppFixedAddress : IDisposable
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
        public class CData  // 詳細情報データ
        {
            public string url { get; set; }   // JPlatPat固定アドレス
        }
        public class CResult
        {
            public string statusCode { get; set; }  // ステータスコード
            public string errorMessage { get; set; }    // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数
            public CData data { get; set; }     // 詳細情報データ

        }
        private class CJppFixedAddress
        {
            public CResult result { get; set; }
        }
        public string m_jsonFile { get; set; }
        public string m_json { get; set; }

        public JppFixedAddress(string applicationNumber, string a_access_token)
        {
            try
            {
                m_error = e_NONE;
                Cache jsonCache = new Cache(a_access_token);
                this.m_json = jsonCache.GetJson("api/patent/v1/jpp_fixed_address/" + applicationNumber);
                this.m_jsonFile = jsonCache.m_jsonFilePath;
                this.m_error = jsonCache.m_error;

                CJppFixedAddress jsonObj = JsonConvert.DeserializeObject<CJppFixedAddress>(m_json);
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
        // ~JppFixedAddress()
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
