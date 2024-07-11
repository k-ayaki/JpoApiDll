using DocumentFormat.OpenXml.Bibliography;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

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

        [XmlRoot("api-data", Namespace = "https://www.jpo.go.jp")]
        public class XApiData
        {
            [XmlElement("statusCode", IsNullable = true)]
            public string statusCode { get; set; }

            [XmlElement("errorMessage", IsNullable = true)]
            public string errorMessage { get; set; }

            [XmlElement("remainAccessCount", IsNullable = true)]
            public string remainAccessCount { get; set; }
        }
        public XApiData m_resultXml { get; set; }       // APIの結果

        public string m_responseFilePath { get; set; }  // json 
        public string m_response { get; set; }
        private string m_access_token { get; set; }
        private string m_fileNumber { get; set; }
        public Cache(string a_access_token)
        {
            this.m_access_token = a_access_token;
            this.m_response = "";
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
                this.m_fileNumber = dirs[dirs.Length - 1];

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

                this.m_responseFilePath = m_cachePath + "\\" + this.m_fileNumber + ".json";
                int iRet = this.isCache();
                if (iRet == this.e_CONTENT || iRet == this.e_NONE)
                {
                    return this.m_response;
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
            if (this.m_access_token.Length > 0)
            {
                this.read(szUri, this.m_access_token);
            }
            else
            {
                this.m_error = this.e_ACCOUNT;
            }
            return m_response;
        }
        // json キャッシュの存在チェック
        private int isCache()
        {
            try
            {
                if (File.Exists(this.m_responseFilePath))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(this.m_responseFilePath);
                    DateTime dt = DateTime.Now;
                    Account ac = new Account();
                    if (dt.AddDays(-ac.m_cacheEffective).Date <= fi.LastWriteTime.Date)
                    {
                        using (JpoHttp jpo = new JpoHttp())
                        {
                            jpo.m_response = File.ReadAllText(this.m_responseFilePath);
                            CJpo jpoObj = JsonConvert.DeserializeObject<CJpo>(jpo.m_response);
                            this.m_result = jpoObj.result;
                            this.m_cache_result = jpoObj.result;
                            this.m_error = libStatus(this.m_cache_result.statusCode);
                            this.m_response = jpo.m_response;
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
            }
            catch (Exception ex)
            {
                this.m_error = this.e_CACHE;
            }
            return this.m_error;
        }
        public string GetXml(string szUri)
        {
            try
            {
                string[] dirs = szUri.Split('/');
                this.m_fileNumber = dirs[dirs.Length - 1];

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

                this.m_responseFilePath = m_cachePath + "\\" + this.m_fileNumber + ".json";
                int iRet = this.isCache();
                if (iRet == this.e_CACHE)
                {
                    if (File.Exists(this.m_responseFilePath))
                    {
                        File.Delete(this.m_responseFilePath);
                    }
                    this.m_responseFilePath = m_cachePath + "\\" + this.m_fileNumber + ".xml";
                    iRet = this.isXmlCache();
                    if (iRet == this.e_CONTENT || iRet == this.e_NONE)
                    {
                        return this.m_response;
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
            if (this.m_access_token.Length > 0)
            {
                this.read(szUri, this.m_access_token);
            }
            else
            {
                this.m_error = this.e_ACCOUNT;
            }
            return m_response;
        }

        // json キャッシュの存在チェック
        private int isXmlCache()
        {
            try
            {
                if (File.Exists(this.m_responseFilePath))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(this.m_responseFilePath);
                    DateTime dt = DateTime.Now;
                    Account ac = new Account();
                    if (dt.AddDays(-ac.m_cacheEffective).Date <= fi.LastWriteTime.Date)
                    {
                        using (JpoHttp jpo = new JpoHttp())
                        {
                            jpo.m_response = File.ReadAllText(this.m_responseFilePath);
                            XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(XApiData));
                            TextReader reader = new StringReader(jpo.m_response);
                            XmlReaderSettings settings = new XmlReaderSettings();
                            settings.IgnoreWhitespace = true;
                            settings.IgnoreProcessingInstructions = true;
                            settings.IgnoreComments = true;
                            XmlReader xmlReader = XmlReader.Create(reader, settings);
                            XApiData xjpo = (XApiData)serializer.Deserialize(xmlReader);

                            this.m_error = libStatus(xjpo.statusCode);
                            this.m_response = jpo.m_response;
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
            }
            catch(Exception ex)
            {
                this.m_error = this.e_CACHE;
            }
            return this.m_error;
        }
        public int libStatus(string wStatusCode)
        {
            switch (wStatusCode)
            {
                case "100":
                    return this.e_NONE;
                case "107": // 該当するデータがありません。
                case "108": // 該当する書類実体がありません。
                case "111": // 提供対象外の案件番号のため取得できませんでした。
                    return this.e_CONTENT;
                case "203": // 1日のアクセス上限を超過したため閲覧を制限します。
                    return this.e_SERVER;
                case "204": // パラメーターの入力された値に問題があります。
                case "208": // 「タブ文字、,、 :、|」の文字は利用できません。
                case "210": // 無効なトークンです。
                    return this.e_CONTENT;
                case "212": // 無効な認証情報です。
                case "301": // 指定された特許情報取得APIのURLは存在しません。
                    return this.e_NETWORK;
                case "302": // 処理が時間内に終了しないため、タイムアウトになりました。
                    return this.e_TIMEOVER;
                case "303": // アクセスが集中しています。
                    return this.e_SERVER;
                case "400": // 無効なリクエストです。
                case "999": // 想定外のエラーが発生しました。
                    return this.e_NETWORK;
                default:
                    break;
            }
            return 0;
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
                        if (jpo.m_response.Substring(0,1) == "{")
                        {
                            CJpo jpoObj = JsonConvert.DeserializeObject<CJpo>(jpo.m_response);
                            this.m_result = jpoObj.result;

                            this.m_responseFilePath = m_cachePath + "\\" + this.m_fileNumber + ".json";
                            File.WriteAllText(m_responseFilePath, jpo.m_response);
                            this.m_error = libStatus(this.m_result.statusCode);
                        }
                        else if(jpo.m_response.Substring(0,1) == "<")
                        {
                            XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(XApiData));
                            TextReader reader = new StringReader(jpo.m_response);
                            XmlReaderSettings settings = new XmlReaderSettings();
                            settings.IgnoreWhitespace = true;
                            settings.IgnoreProcessingInstructions = true;
                            settings.IgnoreComments = true;
                            XmlReader xmlReader = XmlReader.Create(reader, settings);
                            this.m_resultXml = (XApiData)serializer.Deserialize(xmlReader);

                            this.m_responseFilePath = m_cachePath + "\\" + this.m_fileNumber + ".xml";
                            File.WriteAllText(m_responseFilePath, jpo.m_response);
                            this.m_error =  libStatus(this.m_resultXml.statusCode);
                        }
                    }
                    else
                    {
                        this.m_error = jpo.m_error;
                    }
                    this.m_response = jpo.m_response;
                    jpo.Dispose();
                    return;
                }
                this.m_error = e_NETWORK;
            }
            catch (Exception ex)
            {
                this.m_error = this.e_CACHE;
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
