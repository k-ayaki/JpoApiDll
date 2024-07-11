using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JpoApi;

namespace testOpdJpDocCont
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            //            OpdJpDocCont tj = new OpdJpDocCont("JP.2015500001.A/Abstract_61539913647_JP", at.m_access_token.access_token);
            //OpdJpDocCont tj = new OpdJpDocCont("JP.2007550210.A/Written_Amendment_51201606245_JP", at.m_access_token.access_token);
            //string[] docNumbers = new string[] { "Written_Opinion_50800695453_JP", "Written_Amendment_50800695455_JP",
            //    "Written_Opinion_50802750705_JP", "Written_Amendment_50802750709_JP", };
            /*
            string[] docNumbers = new string[] { "Description_51201919328_JP" };

            string fileNumber = "JP.2012199157.A";

            foreach(string docNumber in docNumbers)
            {
                OpdJpDocCont tj2 = new OpdJpDocCont(fileNumber,
                    docNumber,
                    at.m_access_token.access_token);
                Console.WriteLine("■OPDJP書類実体取得API:" + fileNumber + "/" + docNumber);
                if (tj2.m_error == tj2.e_NONE)
                {
                    Console.WriteLine("ステータスコード：" + tj2.m_result.statusCode);
                    Console.WriteLine("エラーメッセージ：" + tj2.m_result.errorMessage);
                    Console.WriteLine("残アクセス数：" + tj2.m_result.remainAccessCount);
                    if (tj2.m_files != null)
                    {
                        foreach (string filePath in tj2.m_files)
                        {
                            Console.WriteLine(filePath);
                            DocCont docCont = new DocCont(filePath, docNumber);
                        }
                    }
                }
            }
            */
            string fileNumber = "JP.2012199157.A";
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
                        Console.WriteLine("日時：" + tj.m_resultXML.documentListData.documentLists[i].legalDate);
                        Console.WriteLine("id：" + tj.m_resultXML.documentListData.documentLists[i].original.id);

                        OpdJpDocCont tj2 = new OpdJpDocCont(fileNumber, 
                            tj.m_resultXML.documentListData.documentLists[i].original.id, 
                            at.m_access_token.access_token);
                        Console.WriteLine("■OPDJP書類実体取得API:" + fileNumber  + "/" + tj.m_resultXML.documentListData.documentLists[i].original.id);
                        if (tj2.m_error == tj2.e_NONE)
                        {
                            Console.WriteLine("ステータスコード：" + tj2.m_result.statusCode);
                            Console.WriteLine("エラーメッセージ：" + tj2.m_result.errorMessage);
                            Console.WriteLine("残アクセス数：" + tj2.m_result.remainAccessCount);
                            if (tj2.m_files != null)
                            {
                                foreach (string filePath in tj2.m_files)
                                {
                                    Console.WriteLine(filePath);
                                    DocCont docCont = new DocCont(filePath, tj.m_resultXML.documentListData.documentLists[i].original.id, tj.m_resultXML.documentListData.documentLists[i].legalDate);
                                }
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
            string t = Console.ReadLine();
        }
    }
}
