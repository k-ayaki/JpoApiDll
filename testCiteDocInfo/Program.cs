using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testCiteDocInfo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            //CiteDocInfo tj = new CiteDocInfo("2015500069", at.m_access_token.access_token);
            CiteDocInfo tj = new CiteDocInfo("2020000001", at.m_access_token.access_token);
            Console.WriteLine("■特許引用文献情報取得　2020000001");
            if (tj.m_error == tj.e_NONE)
            {
                Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                if (tj.m_data != null)
                {
                    Console.WriteLine("出願番号：" + tj.m_data.applicationNumber);
                    for (int i = 0; i < tj.m_data.patentDoc.Length; i++)
                    {
                        Console.WriteLine("特許文献情報");
                        Console.WriteLine("\t起案日：" + tj.m_data.patentDoc[i].draftDate);
                        Console.WriteLine("\t種別：" + tj.m_data.patentDoc[i].citationType);
                        Console.WriteLine("\t文献番号：" + tj.m_data.patentDoc[i].documentNumber);
                    }
                    for (int i = 0; i < tj.m_data.nonPatentDoc.Length; i++)
                    {
                        Console.WriteLine("非特許文献情報");
                        Console.WriteLine("\t起案日：" + tj.m_data.nonPatentDoc[i].draftDate);
                        Console.WriteLine("\t種別：" + tj.m_data.nonPatentDoc[i].citationType);
                        Console.WriteLine("\t文献分類：" + tj.m_data.nonPatentDoc[i].documentType);
                        Console.WriteLine("\t著者/翻訳者名：" + tj.m_data.nonPatentDoc[i].authorName);
                        Console.WriteLine("\t論文名/タイトル：" + tj.m_data.nonPatentDoc[i].paperTitle);
                        Console.WriteLine("\t刊行物名：" + tj.m_data.nonPatentDoc[i].publicationName);
                        Console.WriteLine("\t発行国コード：" + tj.m_data.nonPatentDoc[i].issueCountryCd);
                        Console.WriteLine("\t発行所／発行者：" + tj.m_data.nonPatentDoc[i].publisher);
                        Console.WriteLine("\t発行／受入年月日日：" + tj.m_data.nonPatentDoc[i].issueDate);
                        Console.WriteLine("\t年月日フラグ：" + tj.m_data.nonPatentDoc[i].issueDateType);
                        Console.WriteLine("\t版数／巻／号数：" + tj.m_data.nonPatentDoc[i].issueNumber);
                        Console.WriteLine("\t引用頁：" + tj.m_data.nonPatentDoc[i].citationPages);
                    }
                }
            }
            else if (tj.m_error == tj.e_NETWORK)
            {
                Console.WriteLine("\tネットワークエラーです。");
            }
            else if (tj.m_error == tj.e_SERVER)
            {
                Console.WriteLine("\tサーバエラーです。");
            }
            else if (tj.m_error == tj.e_TIMEOVER)
            {
                Console.WriteLine("\tタイムオーバーエラーです。");
            }
            else if (tj.m_error == tj.e_CONTENT)
            {
                Console.WriteLine("\t内容のエラーです。");
            }
            else if (tj.m_error == tj.e_ZIPFILE)
            {
                Console.WriteLine("\tZIPの解凍エラーです。");
            }
            else if (tj.m_error == tj.e_CACHE)
            {
                Console.WriteLine("\tキャッシュエラーです。");
            }
            else if (tj.m_error == tj.e_ACCOUNT)
            {
                Console.WriteLine("\tアカウントのエラーです。");
            }
            string buff = Console.ReadLine();
        }
    }
}
