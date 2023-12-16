using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpoApi
{
    public class DesignRegistrationInfo : IDisposable
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
        public class CRightPersonInformation  // 権利者情報
        {
            public string rightPersonCd { get; set; }   // 権利者コード
            public string rightPersonName { get; set; } // 権利者氏名・名称
        }
        public class CData
        {
            public string applicationNumber { get; set; }   // 出願番号
            public string filingDate { get; set; }          // 出願日
            public string registrationNumber { get; set; }  // 登録番号
            public string registrationDate { get; set; }    // 登録日
            public string decisionDate { get; set; }        // 査定日
            public string appealTrialDecisiondDate { get; set; }    // 審決日

            public CRightPersonInformation[] rightPersonInformation;    // 権利者情報
            public string designArticle { get; set; }      // 意匠に係る物品
            public string expireDate { get; set; }          // 存続期間満了年月日
            public string nextPensionPaymentDate { get; set; }  // 次期年金納付期限
            public string lastPaymentYearly { get; set; }   // 最終納付年分
            public string erasureIdentifier { get; set; }   // 本権利抹消識別
            public string disappearanceDate { get; set; }   // 本権利抹消日
            public string updateDate { get; set; }          // 更新日付
        }
        public class CResult
        {
            public string statusCode { get; set; }          // ステータスコード
            public string errorMessage { get; set; }        // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数
            public CData data { get; set; }                 // 詳細情報データ

        }
        private class CDesignRegistrationInfo
        {
            public CResult result { get; set; }
        }

        public string m_jsonFile { get; set; }
        public string m_json { get; set; }
        public DesignRegistrationInfo(string applicationNumber, string a_access_token = "")
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
                    this.m_json = "";
                    this.m_jsonFile = "";
                    this.m_data = null;
                    this.m_result = null;
                    return;
                }
                m_error = e_NONE;
                using (Cache jsonCache = new Cache(a_access_token))
                {
                    this.m_json = jsonCache.GetJson("api/design/v1/registration_info/" + applicationNumber);
                    this.m_jsonFile = jsonCache.m_jsonFilePath;
                    this.m_error = jsonCache.m_error;

                    if (m_json.Length > 0)
                    {
                        CDesignRegistrationInfo jsonObj = JsonConvert.DeserializeObject<CDesignRegistrationInfo>(m_json);
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
        // ~designRegistrationInfo()
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
