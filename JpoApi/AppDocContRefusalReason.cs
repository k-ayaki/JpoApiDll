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


namespace JpoApi
{
    // 特許拒絶理由通知書
    public class AppDocContRefusalReason : IDisposable    
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
        public string m_cacheDir { get; set; }
        public string m_zipFile { get; set; }
        public string m_jsonFile { get; set; }
        public string m_extraPath { get; set; }
        public IEnumerable<string> m_files { get; set; }
        public CResult m_cache_result { get; set; }     // APIキャッシュの結果
        public CResult m_result { get; set; }           // APIの結果

        private string m_result_json = "{\r\n  \"result\": {\r\n    \"statusCode\": \"\",\r\n    \"errorMessage\": \"\",\r\n    \"remainAccessCount\": \"\"\r\n  }\r\n}\r\n";
        public class CResult
        {
            public string statusCode { get; set; }          // ステータスコード
            public string errorMessage { get; set; }        // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数
        }
        private class CAppDocContRefusalReason
        {
            public CResult result { get; set; }
        }
        public AppDocContRefusalReason(string cacheDir = null)
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
                    m_cacheDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\ayaki\jpoapi\app_doc_cont_refusal_reason";
                    if (Directory.Exists(m_cacheDir) == false)
                    {
                        Directory.CreateDirectory(m_cacheDir);
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());

            }
        }
        public AppDocContRefusalReason(string requestNumber, string a_access_token, string cacheDir = null)
        {
            try
            {
                m_error = e_NONE;
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
                    m_cacheDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\ayaki\jpoapi\app_doc_cont_refusal_reason";
                    if (Directory.Exists(m_cacheDir) == false)
                    {
                        Directory.CreateDirectory(m_cacheDir);
                    }
                }
                // Zipファイル
                m_zipFile = m_cacheDir + @"\" + requestNumber + ".zip";
                m_jsonFile = m_cacheDir + @"\" + requestNumber + ".json";
                // xmlの展開ディレクトリ
                m_extraPath = m_cacheDir + @"\" + requestNumber;

                if (isCache() == e_CONTENT)
                {
                    return;
                }
                readCache();
                if (m_error == e_NONE || m_error == e_CONTENT)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());

            }
            read(requestNumber, a_access_token);
        }

        public void readContent(string requestNumber, string a_access_token)
        {
            try
            {
                m_error = e_NONE;
                // Zipファイル
                m_zipFile = m_cacheDir + @"\" + requestNumber + ".zip";
                m_jsonFile = m_cacheDir + @"\" + requestNumber + ".json";
                // xmlの展開ディレクトリ
                m_extraPath = m_cacheDir + @"\" + requestNumber;

                if (isCache() == e_CONTENT)
                {
                    return;
                }
                readCache();
                if (m_error == e_NONE || m_error == e_CONTENT)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());

            }
            read(requestNumber, a_access_token);
        }

        // キャッシュの存在チェック
        private int isCache()
        {
            try
            {
                m_error = e_NONE;
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
                            CAppDocContRefusalReason applicant = JsonConvert.DeserializeObject<CAppDocContRefusalReason>(jpo.m_json);
                            m_result = applicant.result;
                            m_cache_result = applicant.result;
                            switch (m_cache_result.statusCode)
                            {
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
                                    // jsonファイルを削除
                                    if (File.Exists(m_jsonFile))
                                        System.IO.File.Delete(m_jsonFile);
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
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                m_error = e_CACHE;
                return m_error;
            }
        }

        // キャッシュされたZIPファイルを展開
        private int readCache()
        {
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
                        }
                        else
                        {
                            m_error = e_NONE;
                        }
                        return m_error;
                    }
                }
                else
                {
                    m_error = e_CACHE;
                }
                return m_error;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                m_error = e_CACHE;
                return m_error;
            }
        }
        private void read(string requestNumber, string a_access_token)
        {
            using (JpoHttp jpo = new JpoHttp())
            {
                NetworkState networkState = new NetworkState();
                while (DateTime.Now.Second % 6 != networkState.m_i64macaddress % 6)
                {
                    System.Threading.Thread.Sleep(100);
                }
                jpo.get(Properties.Settings.Default.url + @"/app_doc_cont_refusal_reason/" + requestNumber, a_access_token);
                while (DateTime.Now.Second % 6 == networkState.m_i64macaddress % 6)
                {
                    System.Threading.Thread.Sleep(100);
                }
                if (jpo.m_error == jpo.e_NONE)
                {
                    try
                    {
                        CAppDocContRefusalReason applicant = JsonConvert.DeserializeObject<CAppDocContRefusalReason>(jpo.m_json);
                        m_result = applicant.result;
                        File.WriteAllText(m_jsonFile, jpo.m_json);
                        m_error = e_CONTENT;
                        return;
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.ToString());
                    }
                    try
                    {
                        // キャッシュファイル(zip)の書込み
                        File.WriteAllBytes(m_zipFile, jpo.m_content);
                        // 同名の展開パスが有れば前もって削除
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
                        m_error = e_CACHE;
                    }
                    // jsonファイルを削除
                    if (File.Exists(m_jsonFile))
                        System.IO.File.Delete(m_jsonFile);
                }
                else
                {
                    m_error = e_NETWORK;
                }
                jpo.Dispose();
                return;
            }
            m_error = e_NETWORK;
            return;
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
        // ~AppDocContRefusalReason()
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
