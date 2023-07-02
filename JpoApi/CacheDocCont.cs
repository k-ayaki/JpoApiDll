using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpoApi
{
    public class CacheDocCont : IDisposable
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
        public readonly int e_ACCOUNT = 0x00000040;

        public string m_cachePath { get; set; }
        public CResult m_cache_result { get; set; }     // APIキャッシュの結果
        public CResult m_result { get; set; }           // APIの結果

        private string m_result_json = "{\r\n  \"result\": {\r\n    \"statusCode\": \"\",\r\n    \"errorMessage\": \"\",\r\n    \"remainAccessCount\": \"\"\r\n  }\r\n}\r\n";
        public IEnumerable<string> m_files { get; set; }
        public class CResult
        {
            public string statusCode { get; set; }      // ステータスコード
            public string errorMessage { get; set; }    // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数

        }
        private class CJpo
        {
            public CResult result { get; set; }
        }
        public string m_jsonFilePath { get; set; }
        public string m_zipFile { get; set; }
        public string m_extractPath { get; set; }
        public string m_json { get; set; }
        private string m_access_token { get; set; }
        private string m_requestNumber { get; set; }
        public CacheDocCont(string a_access_token)
        {
            this.m_access_token = a_access_token;
            this.m_json = "";
            this.m_cachePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            this.m_requestNumber = "";
            string[] dirs = @"\ayaki\jpoapi".Split('\\');
            foreach (string dir in dirs)
            {
                if (dir.Length == 0) continue;

                this.m_cachePath += "\\" + dir;
                if (Directory.Exists(this.m_cachePath) == false)
                {
                    Directory.CreateDirectory(this.m_cachePath);
                }
            }
        }

        public void GetZipXml(string szUri)
        {
            try
            {
                this.m_error = e_NONE;
                this.m_result = JsonConvert.DeserializeObject<CResult>(m_result_json);
                this.m_cache_result = JsonConvert.DeserializeObject<CResult>(m_result_json);

                string[] dirs = szUri.Split('/');
                m_requestNumber = dirs[dirs.Length - 1];

                var dirList = new List<string>();
                dirList = dirs.ToList();
                dirList.RemoveAt(dirs.Length - 1);

                foreach (string dir in dirList)
                {
                    this.m_cachePath += "\\" + dir;
                    if (Directory.Exists(this.m_cachePath) == false)
                    {
                        Directory.CreateDirectory(this.m_cachePath);
                    }
                }
                this.m_zipFile = this.m_cachePath + @"\" + m_requestNumber + ".zip";
                this.m_extractPath = this.m_cachePath + @"\" + m_requestNumber;
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
                ;
            }

            if (m_access_token.Length > 0)
            {
                read(szUri, m_access_token);
            }
            else
            {
                m_error = e_ACCOUNT;
            }
        }
        // キャッシュの存在チェック
        private int isCache()
        {
            try
            {
                m_error = e_NONE;
                if (File.Exists(m_zipFile))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(m_zipFile);
                    DateTime dt = DateTime.Now;
                    Account ac = new Account();
                    if (dt.AddDays(-ac.m_cacheEffective).Date <= fi.LastWriteTime.Date)
                    {
                        this.m_json = File.ReadAllText(m_zipFile);
                        CJpo cjpo = JsonConvert.DeserializeObject<CJpo>(this.m_json);
                        m_result = cjpo.result;
                        m_cache_result = cjpo.result;
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
                                if (System.IO.File.Exists(m_zipFile))
                                    File.Delete(m_zipFile);
                                break;
                        }
                        return m_error;
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
                m_error = e_CACHE;
                this.m_json = "";
                return m_error;
            }
        }

        // キャッシュの読み込み
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
                        if (System.IO.Directory.Exists(m_extractPath))
                        {
                            //System.IO.Directory.Delete(m_extractPath, true);
                        }
                        else
                        {
                            System.IO.Compression.ZipFile.ExtractToDirectory(m_zipFile, m_extractPath, System.Text.Encoding.GetEncoding("shift_jis"));
                        }
                        m_files = System.IO.Directory.EnumerateFiles(m_extractPath, "*.xml", System.IO.SearchOption.AllDirectories);
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
                m_error = e_CACHE;
                return m_error;
            }
        }
        private void read(string szUri, string a_access_token)
        {
            using (JpoHttp jpo = new JpoHttp())
            {
                Account ac = new Account();
                DateTime dt = System.IO.File.GetCreationTime(ac.m_iniFilePath);
                TimeSpan ts1 = DateTime.Now - dt;
                while (ts1.TotalSeconds <= 6)
                {
                    System.Threading.Thread.Sleep(100);
                    ts1 = DateTime.Now - dt;
                }

                jpo.get(Properties.Settings.Default.at_url + "/" + szUri, a_access_token);
                System.IO.File.SetCreationTime(ac.m_iniFilePath, DateTime.Now);

                if (jpo.m_error == jpo.e_NONE)
                {
                    try
                    {
                        CJpo cjpo = JsonConvert.DeserializeObject<CJpo>(jpo.m_json);
                        m_result = cjpo.result;
                        File.WriteAllText(m_zipFile, jpo.m_json);
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
                        if (System.IO.Directory.Exists(m_extractPath))
                            System.IO.Directory.Delete(m_extractPath, true);


                        System.IO.Compression.ZipFile.ExtractToDirectory(m_zipFile, m_extractPath, System.Text.Encoding.GetEncoding("shift_jis"));
                        m_files = System.IO.Directory.EnumerateFiles(m_extractPath, "*.xml", System.IO.SearchOption.AllDirectories);
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
        // ~CacheDocCont()
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
