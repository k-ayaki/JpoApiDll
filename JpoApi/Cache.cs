using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpoApi
{
    public class Cache : IDisposable
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
        public string m_cachePath { get; set; }
        public CResult m_cache_result { get; set; }     // APIキャッシュの結果
        public CResult m_result { get; set; }           // APIの結果

        private string m_result_json = "{\r\n    \"statusCode\": \"\",\r\n    \"errorMessage\": \"\",\r\n    \"remainAccessCount\": \"\"\r\n  }\r\n";

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
        public string m_json { get; set; }
        private string m_access_token { get; set; }
        public Cache(string a_access_token)
        {
            this.m_access_token = a_access_token;
            this.m_json = "";
            this.m_cachePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
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
        public string GetJson(string szUri)
        {
            try
            {
                string[] dirs = szUri.Split('/');
                string fileNumber = dirs[dirs.Length - 1];

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
                this.m_error = this.e_NONE;
                this.m_result = JsonConvert.DeserializeObject<CResult>(this.m_result_json);
                this.m_cache_result = JsonConvert.DeserializeObject<CResult>(this.m_result_json);

                this.m_jsonFilePath = m_cachePath + "\\" + fileNumber + ".json";
                int iRet = isCache();
                if (iRet == e_CONTENT || iRet == this.e_NONE)
                {
                    return this.m_json;
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
            if (m_access_token.Length > 0)
            {
                read(szUri, this.m_access_token);
            }
            else
            {
                this.m_error = this.e_ACCOUNT;
            }
            return m_json;
        }
        // キャッシュの存在チェック
        private int isCache()
        {
            if (File.Exists(this.m_jsonFilePath))
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(this.m_jsonFilePath);
                DateTime dt = DateTime.Now;
                Account ac = new Account();
                if (dt.AddDays(-ac.m_cacheEffective).Date <= fi.LastWriteTime.Date)
                {
                    using (JpoHttp jpo = new JpoHttp())
                    {
                        jpo.m_json = File.ReadAllText(this.m_jsonFilePath);
                        CJpo jpoObj = JsonConvert.DeserializeObject<CJpo>(jpo.m_json);
                        this.m_result = jpoObj.result;
                        this.m_cache_result = jpoObj.result;

                        switch (this.m_cache_result.statusCode)
                        {
                            case "100":
                                this.m_error = this.e_NONE;
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
                            case "400": // 無効なリクエストです。
                            case "999": // 想定外のエラーが発生しました。
                                this.m_error = this.e_NETWORK;
                                break;
                            default:
                                break;
                        }
                        this.m_json = jpo.m_json;
                        jpo.Dispose();
                        return this.m_error;
                    }
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
        private void read(string szUri, string a_access_token)
        {
            try
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

                    jpo.get(Properties.Settings.Default.at_url + @"/" + szUri, a_access_token);

                    System.IO.File.SetCreationTime(ac.m_iniFilePath, DateTime.Now);

                    if (jpo.m_error == jpo.e_NONE)
                    {

                        CJpo jpoObj = JsonConvert.DeserializeObject<CJpo>(jpo.m_json);
                        this.m_result = jpoObj.result;

                        File.WriteAllText(m_jsonFilePath, jpo.m_json);
                        switch (this.m_result.statusCode)
                        {
                            case "100":
                                this.m_error = this.e_NONE;
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
                            case "400": // 無効なリクエストです。
                            case "999": // 想定外のエラーが発生しました。
                            default:
                                this.m_error = this.e_CONTENT;
                                break;
                        }
                    }
                    else
                    {
                        m_error = jpo.m_error;
                    }
                    m_json = jpo.m_json;
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
        // ~Cache()
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
