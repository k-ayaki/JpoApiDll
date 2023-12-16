using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
                this.m_error = this.e_NONE;
                this.m_result = JsonConvert.DeserializeObject<CResult>(this.m_result_json);
                this.m_cache_result = JsonConvert.DeserializeObject<CResult>(this.m_result_json);

                string[] dirs = szUri.Split('/');
                this.m_requestNumber = dirs[dirs.Length - 1];

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
                this.m_zipFile = this.m_cachePath + @"\" + this.m_requestNumber + ".zip";
                this.m_extractPath = this.m_cachePath + @"\" + this.m_requestNumber;
                if (this.isCache() == this.e_CONTENT)
                {
                    return;
                }
                this.readCache();
                if (this.m_error == this.e_NONE || this.m_error == this.e_CONTENT)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                ;
            }

            if (this.m_access_token.Length > 0)
            {
                this.read(szUri, this.m_access_token);
            }
            else
            {
                this.m_error = this.e_ACCOUNT;
            }
        }
        // キャッシュの存在チェック
        private int isCache()
        {
            try
            {
                this.m_error = this.e_NONE;
                if (File.Exists(this.m_zipFile))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(this.m_zipFile);
                    DateTime dt = DateTime.Now;
                    Account ac = new Account();
                    if (dt.AddDays(-ac.m_cacheEffective).Date <= fi.LastWriteTime.Date)
                    {
                        this.m_json = File.ReadAllText(this.m_zipFile);
                        CJpo cjpo = JsonConvert.DeserializeObject<CJpo>(this.m_json);
                        this.m_result = cjpo.result;
                        this.m_cache_result = cjpo.result;
                        switch (this.m_cache_result.statusCode)
                        {
                            case "107": // 該当するデータがありません。
                            case "108": // 該当する書類実体がありません。
                            case "111": // 提供対象外の案件番号のため取得できませんでした。
                                this.m_error = this.e_CONTENT;
                                break;
                            case "203": // 1日のアクセス上限を超過したため閲覧を制限します。
                                this.m_error = this.e_SERVER;
                                // jsonファイルを削除
                                if (System.IO.File.Exists(this.m_zipFile))
                                    File.Delete(this.m_zipFile);
                                break;
                            case "204": // パラメーターの入力された値に問題があります。
                            case "208": // 「タブ文字、,、 :、|」の文字は利用できません。
                            case "210": // 無効なトークンです。
                                this.m_error = this.e_CONTENT;
                                break;
                            case "212": // 無効な認証情報です。
                            case "301": // 指定された特許情報取得APIのURLは存在しません。
                                this.m_error = this.e_NETWORK;
                                // jsonファイルを削除
                                if (System.IO.File.Exists(this.m_zipFile))
                                    File.Delete(this.m_zipFile);
                                break;

                            case "302": // 処理が時間内に終了しないため、タイムアウトになりました。
                                this.m_error = this.e_TIMEOVER;
                                // jsonファイルを削除
                                if (System.IO.File.Exists(this.m_zipFile))
                                    File.Delete(this.m_zipFile);
                                break;
                            case "303": // アクセスが集中しています。
                                this.m_error = this.e_SERVER;
                                // jsonファイルを削除
                                if (System.IO.File.Exists(this.m_zipFile))
                                    File.Delete(this.m_zipFile);
                                break;
                            case "400": // 無効なリクエストです。
                            case "999": // 想定外のエラーが発生しました。
                            default:
                                this.m_error = this.e_NETWORK;
                                // jsonファイルを削除
                                if (System.IO.File.Exists(this.m_zipFile))
                                    File.Delete(this.m_zipFile);
                                break;
                        }
                        return this.m_error;
                    }
                    else
                    {
                        this.m_error = this.e_CACHE;
                    }
                }
                else
                {
                    this.m_error = this.e_CACHE;
                }
                return this.m_error;
            }
            catch (Exception ex)
            {
                this.m_error = this.e_CACHE;
                this.m_json = "";
                return this.m_error;
            }
        }

        // キャッシュの読み込み
        private int readCache()
        {
            try
            {
                if (File.Exists(this.m_zipFile))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(this.m_zipFile);
                    DateTime dt = DateTime.Now;
                    Account ac = new Account();
                    if (dt.AddDays(-ac.m_cacheEffective).Date <= fi.LastWriteTime.Date)
                    {
                        // 同名の展開パスが有れば前もって削除
                        if (System.IO.Directory.Exists(this.m_extractPath))
                        {
                            //System.IO.Directory.Delete(m_extractPath, true);
                        }
                        else
                        {
                            System.IO.Compression.ZipFile.ExtractToDirectory(this.m_zipFile, this.m_extractPath, System.Text.Encoding.GetEncoding("shift_jis"));
                        }
                        this.m_files = System.IO.Directory.EnumerateFiles(this.m_extractPath, "*.xml", System.IO.SearchOption.AllDirectories);
                        if (this.m_files == null)
                        {
                            this.m_error = this.e_CACHE;
                        }
                        else
                        {
                            this.m_error = this.e_NONE;
                        }
                        return this.m_error;
                    }
                    else
                    {
                        this.m_error = this.e_CACHE;
                    }
                }
                else
                {
                    this.m_error = this.e_CACHE;
                }
                return m_error;
            }
            catch (Exception ex)
            {
                this.m_error = this.e_CACHE;
                return this.m_error;
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
                        this.m_result = cjpo.result;
                        File.WriteAllText(this.m_zipFile, jpo.m_json);
                        switch (this.m_result.statusCode)
                        {
                            case "100": // 正常終了
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
                            case "212": // 無効な認証情報です。
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
                        return;
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.ToString());
                    }
                    try
                    {
                        File.WriteAllBytes(this.m_zipFile, jpo.m_content);
                        if (System.IO.Directory.Exists(this.m_extractPath))
                            System.IO.Directory.Delete(this.m_extractPath, true);


                        System.IO.Compression.ZipFile.ExtractToDirectory(this.m_zipFile, this.m_extractPath, System.Text.Encoding.GetEncoding("shift_jis"));
                        this.m_files = System.IO.Directory.EnumerateFiles(this.m_extractPath, "*.xml", System.IO.SearchOption.AllDirectories);
                        if (this.m_files == null)
                        {
                            this.m_error = this.e_CACHE;
                        }
                        else
                        {
                            this.m_error = this.e_NONE;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.m_error = this.e_CACHE;
                    }
                }
                else
                {
                    this.m_error = this.e_NETWORK;
                }
                jpo.Dispose();
                return;
            }
            this.m_error = this.e_NETWORK;
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
