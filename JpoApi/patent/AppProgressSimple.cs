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
    // シンプル版特許経過情報
    public class AppProgressSimple : IDisposable
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
        public class CDocumentList  // 書類一覧
        {
            public string legalDate { get; set; }   // 受付日・発送日・作成日
            public string irirFlg { get; set; }     // IB書類フラグ
            public string availabilityFlag { get; set; }    // 書類実体有無
            public string documentCode { get; set; }        // 中間書類コード
            public string documentDescription { get; set; } // 書類名
            public string documentNumber { get; set; }      // 書類番号
            public string versionNumber { get; set; }       // バージョン番号
            public string documentSeparator { get; set; }   // 書類識別
            public string numberOfPages { get; set; }       // ページ数
            public string sizeOfDocument { get; set; }      // ドキュメントサイズ
        }
        public class CBibliographyInformation  // 番号種別
        {
            public string numberType { get; set; }  // 番号種別
            public string number { get; set; }      // 番号
            public CDocumentList[] documentList { get; set; }   // 書類一覧
        }
        public class CApplicantAttorney // 申請人（出願人・代理人）
        {
            public string applicantAttorneyCd { get; set; } // 申請人コード
            public string repeatNumber { get; set; }        // 繰返番号
            public string name { get; set; }                // 申請人氏名・名称
            public string applicantAttorneyClass { get; set; }  // 出願人・代理人識別
        }
        public class CData             // 詳細情報データ
        {
            public string applicationNumber { get; set; }   // 出願番号
            public string inventionTitle { get; set; }      // 発明名称
            public CApplicantAttorney[] applicantAttorney { get; set; } // 申請人（出願人・代理人）
            public string filingDate { get; set; }          // 出願日
            public string publicationNumber { get; set; }   // 公開番号
            public string ADPublicationNumber { get; set; } // 公開番号（西暦変換）
            public string nationalPublicationNumber { get; set; }   // 公表番号
            public string ADNationalPublicationNumber { get; set; } // 公表番号（西暦変換）
            public string publicationDate { get; set; }     // 公開日
            public string registrationNumber { get; set; }  // 登録番号
            public string registrationDate { get; set; }    // 登録日
            public string internationalApplicationNumber { get; set; }  // 国際出願番号
            public string internationalPublicationNumber { get; set; }  // 国際公開番号
            public string internationalPublicationDate { get; set; }  // 国際公開日
            public string erasureIdentifier { get; set; }   // 抹消識別
            public string expireDate { get; set; }          // 存続期間満了年月日
            public string disappearanceDate { get; set; }   // 本権利消滅日
            public CBibliographyInformation[] bibliographyInformation { get; set; } // 書類一覧（書誌）
        }
        public class CResult
        {
            public string statusCode { get; set; }      // ステータスコード
            public string errorMessage { get; set; }    // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数
            public CData data { get; set; }             // 詳細情報データ

        }
        private class CAppProgressSimple
        {
            public CResult result { get; set; }
        }
        public string m_responseFile { get; set; }
        public string m_response { get; set; }
        public AppProgressSimple(string applicationNumber, string a_access_token = "")
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
                    this.m_response = responseCache.GetJson("api/patent/v1/app_progress_simple/" + applicationNumber);
                    this.m_responseFile = responseCache.m_responseFilePath;
                    this.m_error = responseCache.m_error;

                    if (m_response.Length > 0)
                    {
                        CAppProgressSimple jsonObj = JsonConvert.DeserializeObject<CAppProgressSimple>(m_response);
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

        public CDocumentList cDocumentList(string aDocumentNumber)
        {
            foreach(CBibliographyInformation cBibliographyInformation in  m_result.data.bibliographyInformation)
            {
                foreach(CDocumentList documentList in cBibliographyInformation.documentList)
                {
                    if(documentList.documentNumber == aDocumentNumber)
                    {
                        return documentList;
                    }
                }
            }
            return null;
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
