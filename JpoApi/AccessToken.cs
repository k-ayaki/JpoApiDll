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
using System.Security.Cryptography;

namespace JpoApi
{
    public class AccessToken : IDisposable
    {
        public CAccessToken m_access_token 
        { 
            get
            {
                TimeSpan elapsedSpan = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
                if (elapsedSpan.Seconds >= 300)
                {
                    getAC();
                }
                return _m_access_token;
            }
            set
            {
                _m_access_token = value;
            }

        }
        public CAccessToken _m_access_token;
        public class CAccessToken
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public int refresh_expires_in { get; set; }
            public string refresh_token { get; set; }
            public string token_type { get; set; }
        }
        private const string m_default_json = "{\"access_token\":\"\",\"expires_in\":0,\"refresh_expires_in\":0,\"refresh_token\":\"\",\"token_type\":\"\"}";
        private bool disposedValue;

        private string m_id;
        private string m_password;
        private string m_authPath;

        private DateTime dt;
        public AccessToken(string aId, string aPassword, string a_authPath)
        {
            m_id = aId;
            m_password = aPassword;
            m_authPath = a_authPath;
            getAC();
        }

        private void getAC()
        {
            //文字コードを指定する
            System.Text.Encoding enc = System.Text.Encoding.GetEncoding("UTF-8");
            //POST送信する文字列を作成
            string postData = "grant_type=password"
                            + "&username=" + System.Web.HttpUtility.UrlEncode(m_id, enc)
                            + "&password=" + System.Web.HttpUtility.UrlEncode(m_password, enc);

            using (JpoHttp jpoHttp = new JpoHttp())
            {
                //バイト型配列に変換
                byte[] postDataBytes = System.Text.Encoding.ASCII.GetBytes(postData);
                jpoHttp.post(Properties.Settings.Default.at_url + m_authPath, postDataBytes);
                if (jpoHttp.m_error == jpoHttp.e_NONE)
                {
                    dt = DateTime.Now;
                    _m_access_token = JsonConvert.DeserializeObject<CAccessToken>(jpoHttp.m_json);
                }
                else
                {
                    dt = DateTime.Now.AddHours(-1);
                    _m_access_token = JsonConvert.DeserializeObject<CAccessToken>(m_default_json);
                }
                jpoHttp.Dispose();
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
        // ~AccessToken()
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
