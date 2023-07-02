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

namespace JpoApi
{
    // 特許登録情報取得
    public class RegistrationInfo : IDisposable
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
            public string inventionTitle { get; set; }      // 発明の名称
            public string numberOfClaims { get; set; }      // 請求項の数
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
        private class CRegistrationInfo
        {
            public CResult result { get; set; }
        }

        public string m_jsonFile { get; set; }
        public string m_json { get; set; }
        public RegistrationInfo(string applicationNumber, string a_access_token)
        {
            try
            {
                m_error = e_NONE;
                Cache jsonCache = new Cache(a_access_token);
                this.m_json = jsonCache.GetJson("api/patent/v1/registration_info/" + applicationNumber);
                this.m_jsonFile = jsonCache.m_jsonFilePath;
                this.m_error = jsonCache.m_error;

                CRegistrationInfo jsonObj = JsonConvert.DeserializeObject<CRegistrationInfo>(m_json);
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

