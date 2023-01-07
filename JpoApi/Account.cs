using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JpoApi
{
    public class Account : IDisposable
    {
        [DllImport("kernel32.dll")]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        private bool disposedValue;

        public string m_id {
            set
            {
                WriteString("特許情報取得API", "id", value);
            }
            get
            {
                return GetString("特許情報取得API", "id");
            }
        }
        public string m_password {
            set
            {
                WriteString("特許情報取得API", "password", value);
            }
            get
            {
                return GetString("特許情報取得API", "password");
            }
        }
        public string m_path {
            set
            {
                WriteString("特許情報取得API", "path", value);
            }
            get
            {
                return GetString("特許情報取得API", "path");
            }
        }
        public int m_cacheEffective
        {
            set
            {
                WriteString("特許情報取得API", "cacheEffective", value.ToString());
            }
            get
            {
                return GetInt("特許情報取得API", "cacheEffective");
            }
        }

        public string m_iniFilePath { get; set; }
        public string m_iniFileDir { get; set; }

        public Account()
        {
            m_iniFileDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\ayaki\jpoapi";
            if(Directory.Exists(m_iniFileDir)==false)
            {
                Directory.CreateDirectory(m_iniFileDir);
            }
            m_iniFilePath = m_iniFileDir +  @"\account.ini";
        }
        /// <summary>
        /// Ini ファイルから文字列を取得します。
        /// </summary>
        /// <param name="section">セクション名</param>
        /// <param name="key">項目名</param>
        /// <param name="defaultValue">値が取得できない場合の初期値</param>
        /// <returns></returns>
        public string GetString(string section, string key, string defaultValue = "")
        {
            var sb = new StringBuilder(1024);
            var r = GetPrivateProfileString(section, key, defaultValue, sb, (uint)sb.Capacity, m_iniFilePath);
            return sb.ToString();
        }
        /// <summary>
        /// Ini ファイルから整数を取得します。
        /// </summary>
        /// <param name="section">セクション名</param>
        /// <param name="key">項目名</param>
        /// <param name="defaultValue">値が取得できない場合の初期値</param>
        /// <returns></returns>
        public int GetInt(string section, string key, int defaultValue = 0)
        {
            return (int)GetPrivateProfileInt(section, key, defaultValue, m_iniFilePath);
        }
        /// <summary>
        /// Ini ファイルに文字列を書き込みます。
        /// </summary>
        /// <param name="section">セクション名</param>
        /// <param name="key">項目名</param>
        /// <param name="value">書き込む値</param>
        /// <returns></returns>
        public bool WriteString(string section, string key, string value)
        {
            return WritePrivateProfileString(section, key, value, m_iniFilePath);
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
        // ~Account()
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
