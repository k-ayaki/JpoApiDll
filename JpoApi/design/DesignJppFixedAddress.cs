﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpoApi
{
    // 意匠J-PlatPat固定アドレス取得API
    public class DesignJppFixedAddress : IDisposable
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
        private class CDesignJppFixedAddress
        {
            public CResult result { get; set; }
        }
        public string m_responseFile { get; set; }
        public string m_response { get; set; }

        public DesignJppFixedAddress(string applicationNumber, string a_access_token = "")
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
                m_error = e_NONE;
                Cache responseCache = new Cache(a_access_token);
                this.m_response = responseCache.GetJson("api/design/v1/jpp_fixed_address/" + applicationNumber);
                this.m_responseFile = responseCache.m_responseFilePath;
                this.m_error = responseCache.m_error;

                if (m_response.Length > 0)
                {
                    CDesignJppFixedAddress jsonObj = JsonConvert.DeserializeObject<CDesignJppFixedAddress>(m_response);
                    this.m_result = jsonObj.result;
                    if (jsonObj.result != null)
                    {
                        this.m_data = jsonObj.result.data;
                    }
                    else
                    {
                        this.m_error = this.e_NETWORK;
                    }
                }
                else
                {
                    this.m_error = this.e_ACCOUNT;
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
        // ~designJppFixedAddress()
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
