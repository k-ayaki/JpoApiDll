using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testOpdGlobalDocCont
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);


            //string[] docNumbers = new string[] { "Written_Opinion_50800695453_JP", "Written_Amendment_50800695455_JP", 
            //    "Written_Opinion_50802750705_JP", "Written_Amendment_50802750709_JP", };
            //string fileNumber = "JP.2007035937.A";
            string[] docNumbers = new string[] { "Description_12200910015_JP", };
            string fileNumber = "JP.2022091892.A";
            foreach ( string docNumber in docNumbers)
            {
                OpdGlobalDocCont tj2 = new OpdGlobalDocCont(fileNumber,
                    docNumber,
                    at.m_access_token.access_token);
                Console.WriteLine("■書類：" + docNumber);
                if (tj2.m_error == tj2.e_NONE)
                {
                    Console.WriteLine("ステータスコード：" + tj2.m_result.statusCode);
                    Console.WriteLine("エラーメッセージ：" + tj2.m_result.errorMessage);
                    Console.WriteLine("残アクセス数：" + tj2.m_result.remainAccessCount);
                    if (tj2.m_pdfFile.Length > 0)
                    {
                        Console.WriteLine("pdf：" + tj2.m_pdfFile);
                    }
                }
            }
            /*
            OpdGlobalDocList tj = new OpdGlobalDocList(fileNumber, at.m_access_token.access_token);
            Console.WriteLine("■OPD書類一覧取得API　" + fileNumber);
            if (tj.m_error == tj.e_NONE)
            {
                Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                if (tj.m_resultXML.documentListData != null)
                {
                    Console.WriteLine("発明の名称：" + tj.m_resultXML.documentListData.bibliographic.original.inventionTitle);
                    Console.WriteLine("出願人名：" + tj.m_resultXML.documentListData.bibliographic.original.applicant.lastName);
                    for (int i = 0; i < tj.m_resultXML.documentListData.documentLists.Count; i++)
                    {
                        Console.WriteLine("書類名：" + tj.m_resultXML.documentListData.documentLists[i].original.documentDescription);
                        Console.WriteLine("id：" + tj.m_resultXML.documentListData.documentLists[i].original.id);
                        OpdGlobalDocCont tj2 = new OpdGlobalDocCont(fileNumber,
                            tj.m_resultXML.documentListData.documentLists[i].original.id, 
                            at.m_access_token.access_token);
                        Console.WriteLine("■書類：" + tj.m_resultXML.documentListData.documentLists[i].original.id);
                        if (tj2.m_error == tj2.e_NONE)
                        {
                            Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                            Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                            Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                            if (tj2.m_pdfFile.Length > 0)
                            {
                                Console.WriteLine("pdf：" + tj2.m_pdfFile);
                            }
                        }
                    }
                }
                Console.WriteLine("\t終了しました。");
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
            */
            string t = Console.ReadLine();
        }
    }
}
