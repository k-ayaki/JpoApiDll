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
using static JpoApi.ApplicantAttorney;

namespace JpoApi
{
    public class CiteDocInfo : IDisposable
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

        public string m_cacheDir { get; set; }
        public CData m_data { get; set; }
        public CResult m_cache_result { get; set; }     // APIキャッシュの結果
        public CResult m_result { get; set; }           // APIの結果

        private string m_result_json = "{\r\n  \"result\": {\r\n    \"statusCode\": \"\",\r\n    \"errorMessage\": \"\",\r\n    \"remainAccessCount\": \"\"\r\n  }\r\n}\r\n";
        public class CNonPatentDoc  // 非特許文献情報
        {
            public string draftDate { get; set; }       // 起案日
            public string citationType { get; set; }    // 種別
            public string documentType { get; set; }    // 文献分類
            public string authorName { get; set; }      // 著者/翻訳者名
            public string paperTitle { get; set; }      // 論文名/タイトル
            public string publicationName { get; set; } // 刊行物名
            public string issueCountryCd { get; set; }  // 発行国コード
            public string publisher { get; set; }       // 発行所／発行者
            public string issueDate { get; set; }       // 発行／受入年月日日
            public string issueDateType { get; set; }   // 年月日フラグ
            public string issueNumber { get; set; }     // 版数／巻／号数
            public string citationPages { get; set; }   // 引用頁
        }
        public class CPatentDoc // 特許文献情報
        {
            public string draftDate { get; set; }       // 起案日
            public string citationType { get; set; }    // 種別
            public string documentNumber { get; set; }  // 文献番号
        }
        public class CData
        {
            public string applicationNumber { get; set; }   // 出願番号
            public CPatentDoc[] patentDoc { get; set; }     // 特許文献情報
            public CNonPatentDoc[] nonPatentDoc { get; set; }   // 非特許文献情報
        }
        public class CResult
        {
            public string statusCode { get; set; }  // ステータスコード
            public string errorMessage { get; set; }    // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数
            public CData data { get; set; }     // 詳細情報データ

        }
        private class CCitedDocInfo
        {
            public CResult result { get; set; }
        }
        public string m_jsonFile { get; set; }
        public CiteDocInfo(string applicationNumber,string a_access_token, string cacheDir = null)
        {
            try
            {
                m_error = e_NONE;
                CResult m_result = JsonConvert.DeserializeObject<CResult>(m_result_json);
                CResult m_cache_result = JsonConvert.DeserializeObject<CResult>(m_result_json);

                m_cacheDir = "";
                if (cacheDir != null)
                {
                    if (Directory.Exists(cacheDir))
                    {
                        m_cacheDir = cacheDir;
                    }
                }
                if (m_cacheDir.Length == 0)
                {
                    m_cacheDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\ayaki\jpoapi\cite_doc_info";
                    if (Directory.Exists(m_cacheDir) == false)
                    {
                        Directory.CreateDirectory(m_cacheDir);
                    }
                }
                m_jsonFile = m_cacheDir + @"\" + applicationNumber + ".json";
                if (isCache() == e_CONTENT)
                {
                    return;
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                ;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                ;
            }
            read(applicationNumber, a_access_token);
        }

        // キャッシュの存在チェック
        private int isCache()
        {
            if (File.Exists(m_jsonFile))
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(m_jsonFile);
                DateTime dt = DateTime.Now;
                Account ac = new Account();
                if (dt.AddDays(-ac.m_cacheEffective).Date <= fi.LastWriteTime.Date)
                {
                    using (JpoHttp jpo = new JpoHttp())
                    {
                        jpo.m_json = File.ReadAllText(m_jsonFile);
                        CCitedDocInfo applicant = JsonConvert.DeserializeObject<CCitedDocInfo>(jpo.m_json);
                        m_result = applicant.result;
                        m_cache_result = applicant.result;
                        m_data = applicant.result.data;

                        switch (m_cache_result.statusCode)
                        {
                            case "100":
                                m_error = e_NONE;
                                break;
                            case "107": // 該当するデータがありません。
                            case "108": // 該当する書類実体がありません。
                            case "111": // 提供対象外の案件番号のため取得できませんでした。
                            case "204": // パラメーターの入力された値に問題があります。
                            case "208": // 「タブ文字、,、 :、|」の文字は利用できません。
                            case "301": // 指定された特許情報取得APIのURLは存在しません。
                                m_error = e_CONTENT;
                                break;
                            case "203": // 1日のアクセス上限を超過したため閲覧を制限します。
                            case "210": // 無効なトークンです。
                            case "212": // 無効な認証情報です。
                            case "302": // 処理が時間内に終了しないため、タイムアウトになりました。
                            case "303": // アクセスが集中しています。
                            case "400": // 無効なリクエストです。
                            case "999": // 想定外のエラーが発生しました。
                            default:
                                break;
                        }
                        jpo.Dispose();
                        return m_error;
                    }
                }
                else
                {
                    m_error = e_CACHE;
                }
            }
            else
            {
                m_error = e_CACHE;
            }
            return m_error;
        }

        private void read(string applicationNumber, string a_access_token)
        {
            try
            {
                using (JpoHttp jpo = new JpoHttp())
                {
                    NetworkState networkState = new NetworkState();
                    while (DateTime.Now.Second % 6 != networkState.m_i64macaddress % 6)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    jpo.get(Properties.Settings.Default.url + @"/cite_doc_info/" + applicationNumber, a_access_token);
                    while (DateTime.Now.Second % 6 == networkState.m_i64macaddress % 6)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    if (jpo.m_error == jpo.e_NONE)
                    {
                        CCitedDocInfo applicant = JsonConvert.DeserializeObject<CCitedDocInfo>(jpo.m_json);
                        m_result = applicant.result;
                        m_data = applicant.result.data;
                        File.WriteAllText(m_jsonFile, jpo.m_json);
                        switch (m_result.statusCode)
                        {
                            case "100":
                                m_error = e_NONE;
                                break;
                            case "107": // 該当するデータがありません。
                            case "108": // 該当する書類実体がありません。
                            case "111": // 提供対象外の案件番号のため取得できませんでした。
                            case "204": // パラメーターの入力された値に問題があります。
                            case "208": // 「タブ文字、,、 :、|」の文字は利用できません。
                            case "301": // 指定された特許情報取得APIのURLは存在しません。
                            case "203": // 1日のアクセス上限を超過したため閲覧を制限します。
                            case "210": // 無効なトークンです。
                            case "212": // 無効な認証情報です。
                            case "302": // 処理が時間内に終了しないため、タイムアウトになりました。
                            case "303": // アクセスが集中しています。
                            case "400": // 無効なリクエストです。
                            case "999": // 想定外のエラーが発生しました。
                            default:
                                m_error = e_CONTENT;
                                break;
                        }
                    }
                    else
                    {
                        m_error = jpo.m_error;
                    }
                    jpo.Dispose();
                    return;
                }
                m_error = e_NETWORK;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                m_error = e_CACHE;
                return;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                m_error = e_CACHE;
                return;
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

