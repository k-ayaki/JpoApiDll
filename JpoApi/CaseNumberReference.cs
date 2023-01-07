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
    // 特許番号参照
    public class CaseNumberReference : IDisposable
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
        public CData m_data { get; set; }
        public CResult m_result { get; set; }
        public class CData
        {
            public string applicationNumber { get; set; }       // 出願番号
            public string publicationNumber { get; set; }       // 公開番号（西暦）
            public string nationalPublicationNumber { get; set; }   // 公表番号（西暦）
            public string registrationNumber { get; set; }      // 登録番号
        }
        public class CResult
        {
            public string statusCode { get; set; }          // ステータスコード
            public string errorMessage { get; set; }        // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数
            public CData data { get; set; }                 // 詳細情報データ

        }
        private class CCaseNumberReference
        {
            public CResult result { get; set; }
        }
        public string m_jsonFile { get; set; }
        public CaseNumberReference(string szType, string szNumber,string a_access_token, string cacheDir = null)
        {
            JpoHttp jpo = new JpoHttp();
            try
            {
                if (cacheDir == null)
                {
                    cacheDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\ayaki\jpoapi\case_number_reference\" + szType;
                    Directory.CreateDirectory(cacheDir);
                }
                m_jsonFile = cacheDir + @"\" + szNumber + ".json";
                if (File.Exists(m_jsonFile))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(m_jsonFile);
                    DateTime dt = DateTime.Now;
                    Account ac = new Account();
                    if (dt.AddDays(-ac.m_cacheEffective).Date <= fi.LastWriteTime.Date)
                    {
                        jpo.m_json = File.ReadAllText(m_jsonFile);
                        CCaseNumberReference applicant = JsonConvert.DeserializeObject<CCaseNumberReference>(jpo.m_json);
                        m_result = applicant.result;
                        m_data = applicant.result.data;

                        switch (m_result.statusCode)
                        {
                            case "100":
                                m_error = e_NONE;
                                return;
                            case "107": // 該当するデータがありません。
                            case "108": // 該当する書類実体がありません。
                            case "111": // 提供対象外の案件番号のため取得できませんでした。
                            case "204": // パラメーターの入力された値に問題があります。
                            case "208": // 「タブ文字、,、 :、|」の文字は利用できません。
                            case "301": // 指定された特許情報取得APIのURLは存在しません。
                                m_error = e_CONTENT;
                                return;
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
                    }
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
            try
            {
                NetworkState networkState = new NetworkState();
                while (DateTime.Now.Second % 6 != networkState.m_i64macaddress % 6)
                {
                    System.Threading.Thread.Sleep(100);
                }
                jpo.get(Properties.Settings.Default.url + @"/case_number_reference/" + szType + "/" + szNumber, a_access_token);
                while (DateTime.Now.Second % 6 == networkState.m_i64macaddress % 6)
                {
                    System.Threading.Thread.Sleep(100);
                }


                if (jpo.m_error == jpo.e_NONE)
                {
                    CCaseNumberReference applicant = JsonConvert.DeserializeObject<CCaseNumberReference>(jpo.m_json);
                    m_result = applicant.result;
                    m_data = applicant.result.data;
                    File.WriteAllText(m_jsonFile, jpo.m_json);
                    /*
                    if (m_result.statusCode == "100")
                    {
                        using (FileStream inFile = new FileStream(m_jsonFile, FileMode.Create, FileAccess.Write))
                        {
                            inFile.Write(jpo.m_content, 0, jpo.m_content.Length);
                        }
                    }
                    */
                    switch (m_result.statusCode)
                    {
                        case "100":
                            m_error = e_NONE;
                            break;
                        case "107": // 該当するデータがありません。
                        case "108": // 該当する書類実体がありません。
                        case "111": // 提供対象外の案件番号のため取得できませんでした。
                        case "203": // 1日のアクセス上限を超過したため閲覧を制限します。
                        case "204": // パラメーターの入力された値に問題があります。
                        case "208": // 「タブ文字、,、 :、|」の文字は利用できません。
                        case "301": // 指定された特許情報取得APIのURLは存在しません。
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

