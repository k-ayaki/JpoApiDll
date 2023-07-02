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
    public class CiteDocInfo : IDisposable
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
        public CData m_data { get; set; }
        public CResult m_result { get; set; }           // APIの結果
        public class CNonPatentDoc  // 非特許文献情報
        {
            public string draftDate { get; set; }       // 起案日
            public string citationType { get; set; }    // 種別
            public string documentType { get; set; }    // 文献分類
            public string authorName { get; set; }      // 著者/翻訳者名
            public string paperTitle { get; set; }      // 論文名/タイトル
            public string publicationName { get; set; } // 刊行物名
            public string issueCountryCd { get; set; }  // 発行国コード
            public string publisher { get; set; }       // 発行所／発行者
            public string issueDate { get; set; }       // 発行／受入年月日日
            public string issueDateType { get; set; }   // 年月日フラグ
            public string issueNumber { get; set; }     // 版数／巻／号数
            public string citationPages { get; set; }   // 引用頁
        }
        public class CPatentDoc // 特許文献情報
        {
            public string draftDate { get; set; }       // 起案日
            public string citationType { get; set; }    // 種別
            public string documentNumber { get; set; }  // 文献番号
        }
        public class CData
        {
            public string applicationNumber { get; set; }   // 出願番号
            public CPatentDoc[] patentDoc { get; set; }     // 特許文献情報
            public CNonPatentDoc[] nonPatentDoc { get; set; }   // 非特許文献情報
        }
        public class CResult
        {
            public string statusCode { get; set; }  // ステータスコード
            public string errorMessage { get; set; }    // エラーメッセージ
            public string remainAccessCount { get; set; }   // 残アクセス数
            public CData data { get; set; }     // 詳細情報データ

        }
        private class CCitedDocInfo
        {
            public CResult result { get; set; }
        }
        public string m_jsonFile { get; set; }
        public string m_json { get; set; }
        public CiteDocInfo(string applicationNumber, string a_access_token)
        {
            try
            {
                m_error = e_NONE;
                Cache jsonCache = new Cache(a_access_token);
                this.m_json = jsonCache.GetJson("api/patent/v1/cite_doc_info/" + applicationNumber);
                this.m_jsonFile = jsonCache.m_jsonFilePath;
                this.m_error = jsonCache.m_error;

                CCitedDocInfo jsonObj = JsonConvert.DeserializeObject<CCitedDocInfo>(m_json);
                this.m_result = jsonObj.result;
                this.m_data = jsonObj.result.data;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                ;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                ;
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

