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

            CiteDocInfo tj4 = new CiteDocInfo("2015500069", at.m_access_token.access_token);
            Console.WriteLine("■特許引用文献情報取得　2015500069");
            Console.WriteLine("ステータスコード：" + tj4.m_result.statusCode);
            Console.WriteLine("エラーメッセージ：" + tj4.m_result.errorMessage);
            Console.WriteLine("残アクセス数：" + tj4.m_result.remainAccessCount);
            if (tj4.m_error == tj4.e_NONE)
            {
                Console.WriteLine("出願番号：" + tj4.m_data.applicationNumber);
                for (int i = 0; i < tj4.m_data.patentDoc.Length; i++)
                {
                    Console.WriteLine("特許文献情報");
                    Console.WriteLine("\t起案日：" + tj4.m_data.patentDoc[i].draftDate);
                    Console.WriteLine("\t種別：" + tj4.m_data.patentDoc[i].citationType);
                    Console.WriteLine("\t文献番号：" + tj4.m_data.patentDoc[i].documentNumber);
                }
                for (int i = 0; i < tj4.m_data.nonPatentDoc.Length; i++)
                {
                    Console.WriteLine("非特許文献情報");
                    Console.WriteLine("\t起案日：" + tj4.m_data.nonPatentDoc[i].draftDate);
                    Console.WriteLine("\t種別：" + tj4.m_data.nonPatentDoc[i].citationType);
                    Console.WriteLine("\t文献分類：" + tj4.m_data.nonPatentDoc[i].documentType);
                    Console.WriteLine("\t著者/翻訳者名：" + tj4.m_data.nonPatentDoc[i].authorName);
                    Console.WriteLine("\t論文名/タイトル：" + tj4.m_data.nonPatentDoc[i].paperTitle);
                    Console.WriteLine("\t刊行物名：" + tj4.m_data.nonPatentDoc[i].publicationName);
                    Console.WriteLine("\t発行国コード：" + tj4.m_data.nonPatentDoc[i].issueCountryCd);
                    Console.WriteLine("\t発行所／発行者：" + tj4.m_data.nonPatentDoc[i].publisher);
                    Console.WriteLine("\t発行／受入年月日日：" + tj4.m_data.nonPatentDoc[i].issueDate);
                    Console.WriteLine("\t年月日フラグ：" + tj4.m_data.nonPatentDoc[i].issueDateType);
                    Console.WriteLine("\t版数／巻／号数：" + tj4.m_data.nonPatentDoc[i].issueNumber);
                    Console.WriteLine("\t引用頁：" + tj4.m_data.nonPatentDoc[i].citationPages);
                }
            }
            string buff = Console.ReadLine();
        }
    }
}
