﻿using System;
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
    // 特許優先基礎出願情報
    public class PriorityRightAppInfo : IDisposable
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
        public class CPriorityRightInformation  // 優先権基礎情報
        {
            public string parisPriorityApplicationNumber { get; set; }  // パリ条約に基づく優先権出願番号
            public string parisPriorityDate { get; set; }               // パリ条約に基づく優先権主張日
            public string parisPriorityCountryCd { get; set; }          // パリ条約に基づく優先権国コード
            public string nationalPriorityLawCd { get; set; }           // 国内優先権四法コード
            public string nationalPriorityApplicationNumber { get; set; }   // 国内優先権出願番号
            public string nationalPriorityInternationalApplicationNumber { get; set; }  // 国内優先権国際出願番号
            public string nationalPriorityDate { get; set; }            // 国内優先権主張日
        }
        public class CData  // 詳細情報データ
        {
            public string applicationNumber { get; set; }   // 願番号
            public CPriorityRightInformation[] priorityRightInformation { get; set; }   // 優先権基礎情報
        }
        public class CResult
        {
            public string statusCode { get; set; }  // ステータスコード
            public string errorMessage { get; set; }    // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数
            public CData data { get; set; }         // 詳細情報データ

        }
        private class CPriorityRightAppInfo
        {
            public CResult result { get; set; }
        }
        public string m_responseFile { get; set; }

        public string m_response { get; set; }
        public PriorityRightAppInfo(string applicationNumber, string a_access_token = "")
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
                using (Cache responseCache = new Cache(a_access_token))
                {
                    this.m_response = responseCache.GetJson("api/patent/v1/priority_right_app_info/" + applicationNumber);
                    this.m_responseFile = responseCache.m_responseFilePath;
                    this.m_error = responseCache.m_error;

                    if (m_response.Length > 0)
                    {
                        CPriorityRightAppInfo jsonObj = JsonConvert.DeserializeObject<CPriorityRightAppInfo>(m_response);
                        this.m_result = jsonObj.result;
                        if (jsonObj.result != null)
                        {
                            switch (jsonObj.result.statusCode)
                            {
                                case "100": // 正常終了
                                    this.m_data = jsonObj.result.data;
                                    break;
                                case "107": // 該当するデータがありません。
                                case "108": // 該当する書類実体がありません。
                                case "111": // 提供対象外の案件番号のため取得できませんでした。
                                    this.m_error = this.e_CONTENT;
                                    break;
                                case "203": // 1日のアクセス上限を超過したため閲覧を制限します。
                                    this.m_error = this.e_SERVER;
                                    break;
                                case "204": // パラメーターの入力された値に問題があります。
                                case "208": // 「タブ文字、,、 :、|」の文字は利用できません。
                                case "210": // 無効なトークンです。
                                    this.m_error = this.e_CONTENT;
                                    break;
                                case "301": // 指定された特許情報取得APIのURLは存在しません。
                                    this.m_error = this.e_NETWORK;
                                    break;
                                case "302": // 処理が時間内に終了しないため、タイムアウトになりました。
                                    this.m_error = this.e_TIMEOVER;
                                    break;
                                case "303": // アクセスが集中しています。
                                    this.m_error = this.e_SERVER;
                                    break;
                                case "400": // 無効なリクエストです
                                case "999": // 想定外のエラーが発生しました。
                                    this.m_error = this.e_NETWORK;
                                    break;
                            }

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
