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
using System.Net.Http.Headers;
using System.Net.Http;
using JpoApi;

namespace JpoApi
{
    public class JpoHttp : IDisposable
    {
        private bool disposedValue;

        public int m_error;
        public readonly int e_NONE = 0x00000000;
        public readonly int e_NETWORK = 0x00000001;
        public readonly int e_SERVER = 0x00000002;
        public readonly int e_TIMEOVER = 0x00000004;

        public readonly int i_BUFLEN = 8192;

        public byte[] m_buf = new byte[8192];
        public string m_json;
        public byte[] m_content = new byte[0];
        public int m_statusCode;
        public JpoHttp()
        {
            m_statusCode = 0;
            m_error = e_NONE;
            m_json = "";
        }
        public void get(string a_url, string accessToken = "")
        {
            try
            {
                m_error = e_NONE;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(a_url);
                req.Method = "GET";
                if(accessToken.Length > 0)
                {
                    req.Headers.Add("Authorization", "Bearer " + accessToken);
                }
                req.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
                //サーバーからの応答を受信するためのHttpWebResponseを取得
                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    m_statusCode = (int)res.StatusCode;
                    //応答データを受信するためのStreamを取得
                    using (Stream st = res.GetResponseStream())
                    {
                        // 一時ファイルに書き込み
                        using (MemoryStream ms = new MemoryStream())
                        {
                            st.CopyTo(ms);
                            m_content = ms.ToArray();
                            //文字コードを指定して、バイト配列を変換
                            m_json = System.Text.Encoding.UTF8.GetString(m_content);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == System.Net.WebExceptionStatus.NameResolutionFailure)
                {
                    m_error = e_NETWORK;
                    return;
                }
                else
                {
                    m_error = e_SERVER;
                    return;
                }
            }
        }
        public void getBinary(string a_url, string a_access_token, string a_strFile)
        {
            try
            {
                m_error = e_NONE;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(a_url);
                req.Method = "GET";
                req.Headers.Add("Authorization", "Bearer " + a_access_token);
                req.UserAgent = @"Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1866.237 Safari/537.36";
                //サーバーからの応答を受信するためのHttpWebResponseを取得
                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    //応答データを受信するためのStreamを取得
                    using (Stream st = res.GetResponseStream())
                    {
                        // メモリストリームに書き込み
                        // https://atmarkit.itmedia.co.jp/fdotnet/dotnettips/985memstream/memstream.html
                        using (MemoryStream ms = new MemoryStream())
                        {
                            st.CopyTo(ms);
                            m_content = ms.ToArray();
                            m_json = System.Text.Encoding.UTF8.GetString(m_content);
                        }
                    }
                }
                return;
            }
            catch (WebException ex)
            {
                if (ex.Status == System.Net.WebExceptionStatus.NameResolutionFailure)
                {
                    m_error = e_NETWORK;
                    return;
                }
                else
                {
                    m_error = e_SERVER;
                    return;
                }
            }
        }
        public void post(string a_url, byte[] a_postDataBytes)
        {
            try
            {
                m_error = e_NONE;
                //文字コードを指定する
                System.Text.Encoding enc = System.Text.Encoding.GetEncoding("UTF-8");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(a_url);
                ((HttpWebRequest)req).UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";

                req.Method = "POST";
                req.Credentials = CredentialCache.DefaultCredentials;
                req.ContentType = "application/x-www-form-urlencoded";
                //POST送信するデータの長さを指定
                req.ContentLength = a_postDataBytes.Length;

                //データをPOST送信するためのStreamを取得
                using (System.IO.Stream reqStream = req.GetRequestStream())
                {
                    //送信するデータを書き込む
                    reqStream.Write(a_postDataBytes, 0, a_postDataBytes.Length);
                    reqStream.Close();

                    //サーバーからの応答を受信するためのWebResponseを取得
                    using (System.Net.WebResponse res = req.GetResponse())
                    {
                        //応答データを受信するためのStreamを取得
                        using (System.IO.Stream resStream = res.GetResponseStream())
                        {
                            //受信して表示
                            using (System.IO.StreamReader sr = new System.IO.StreamReader(resStream, enc))
                            {
                                m_json = sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == System.Net.WebExceptionStatus.NameResolutionFailure)
                {
                    m_error = e_NETWORK;
                    return;
                }
                else
                {
                    m_error = e_SERVER;
                    return;
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
        // ~httpClass()
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
