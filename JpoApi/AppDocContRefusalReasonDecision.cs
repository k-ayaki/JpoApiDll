using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web.UI.WebControls;
using System.Web.Caching;

namespace JpoApi
{
    // 特許発送書類
    public class AppDocContRefusalReasonDecision : IDisposable
    {
        private bool disposedValue;
        public int m_error { get; set; }
        public readonly int e_NONE = 0x00000000;
        public readonly int e_NETWORK = 0x00000001;
        public readonly int e_SERVER = 0x00000002;
        public readonly int e_TIMEOVER = 0x00000004;
        public readonly int e_CONTENT = 0x00000008;
        public readonly int e_ZIPFILE = 0x00000010;
        public readonly int e_CACHE = 0x00000020;
        public string m_zipFile { get; set; }
        public string m_jsonFile { get; set; }
        public string m_extraPath { get; set; }
        public IEnumerable<string> m_files { get; set; }
        public CResult m_result { get; set; }

        public class CResult
        {
            public string statusCode { get; set; }      // ステータスコード
            public string errorMessage { get; set; }        // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数
        }
        private class CAppDocContRefusalReasonDecision
        {
            public CResult result { get; set; }
        }
        public AppDocContRefusalReasonDecision(string requestNumber, string a_access_token, string cacheDir = null)
        {
            try
            {
                m_error = e_NONE;
                if (cacheDir == null)
                {
                    cacheDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\ayaki\jpoapi\app_doc_cont_refusal_reason_decision";
                    Directory.CreateDirectory(cacheDir);
                }

                m_zipFile = cacheDir + @"\" + requestNumber + ".zip";
                m_jsonFile = cacheDir + @"\" + requestNumber + ".json";
                m_extraPath = cacheDir + @"\" + requestNumber;

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
                            CAppDocContRefusalReasonDecision applicant = JsonConvert.DeserializeObject<CAppDocContRefusalReasonDecision>(jpo.m_json);
                            m_result = applicant.result;
                            switch (m_result.statusCode)
                            {
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
                                    // jsonファイルを削除
                                    if (File.Exists(m_jsonFile))
                                        System.IO.File.Delete(m_jsonFile);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
            }
            try
            {
                if (File.Exists(m_zipFile))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(m_zipFile);
                    DateTime dt = DateTime.Now;
                    Account ac = new Account();
                    if (dt.AddDays(-ac.m_cacheEffective).Date <= fi.LastWriteTime.Date)
                    {
                        // 同名の展開パスが有れば前もって削除
                        if (System.IO.Directory.Exists(m_extraPath))
                        {
                            //System.IO.Directory.Delete(m_extraPath, true);
                        }
                        else
                        {
                            System.IO.Compression.ZipFile.ExtractToDirectory(m_zipFile, m_extraPath, System.Text.Encoding.GetEncoding("shift_jis"));
                        }
                        m_files = System.IO.Directory.EnumerateFiles(m_extraPath, "*.xml", System.IO.SearchOption.AllDirectories);
                        if (m_files == null)
                        {
                            m_error = e_CACHE;
                        } else
                        {
                            m_error = e_NONE;
                        }
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
            }
            using (JpoHttp jpo = new JpoHttp())
            {
                NetworkState networkState = new NetworkState();
                while (DateTime.Now.Second % 6 != networkState.m_i64macaddress % 6)
                {
                    System.Threading.Thread.Sleep(100);
                }

                jpo.get(Properties.Settings.Default.url + @"/app_doc_cont_refusal_reason_decision/" + requestNumber, a_access_token);
                while (DateTime.Now.Second % 6 == networkState.m_i64macaddress % 6)
                {
                    System.Threading.Thread.Sleep(100);
                }
                if (jpo.m_error == jpo.e_NONE)
                {
                    try
                    {
                        CAppDocContRefusalReasonDecision applicant = JsonConvert.DeserializeObject<CAppDocContRefusalReasonDecision>(jpo.m_json);
                        m_result = applicant.result;
                        File.WriteAllText(m_jsonFile, jpo.m_json);
                        if (System.IO.Directory.Exists(m_jsonFile))
                            System.IO.Directory.Delete(m_extraPath, true);
                        m_error = e_CONTENT;
                        return;
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.ToString());
                    }
                    try
                    {
                        File.WriteAllBytes(m_zipFile, jpo.m_content);
                        if (System.IO.Directory.Exists(m_extraPath))
                            System.IO.Directory.Delete(m_extraPath, true);


                        System.IO.Compression.ZipFile.ExtractToDirectory(m_zipFile, m_extraPath, System.Text.Encoding.GetEncoding("shift_jis"));
                        m_files = System.IO.Directory.EnumerateFiles(m_extraPath, "*.xml", System.IO.SearchOption.AllDirectories);
                        if (m_files == null)
                        {
                            m_error = e_CACHE;
                        }
                        else
                        {
                            m_error = e_NONE;
                        }
                    }
                    catch (Exception ex)
                    {
                        m_error = e_ZIPFILE;
                    }
                    // jsonファイルを削除
                    if (File.Exists(m_jsonFile))
                        System.IO.File.Delete(m_jsonFile);


                }
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
        // ~AppDocContRefusalReasonDecision()
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
