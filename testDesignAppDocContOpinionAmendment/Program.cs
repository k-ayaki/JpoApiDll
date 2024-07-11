﻿using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testDesignAppDocContOpinionAmendment
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string app_base_path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            Console.WriteLine("■意匠申請書類実体情報取得");
            string curDir = System.IO.Directory.GetCurrentDirectory();

            //string[] docNumbers = new string[] { "2015515485", "2012193763", "2012199157", "2015532525", "2015539540", "2015515940", "2015550024", "2012001505", "2012019353", "2009004798", "2006106644" };
            string[] docNumbers = new string[] { "2020021161" };
            foreach (string docNumber in docNumbers)
            {
                DesignAppProgressSimple tj0 = new DesignAppProgressSimple(docNumber, at.m_access_token.access_token);
                if (tj0.m_error == tj0.e_NONE)
                {
                    DesignAppDocContOpinionAmendment tj = new DesignAppDocContOpinionAmendment(docNumber, at.m_access_token.access_token);
                    if (tj.m_error == tj.e_NONE)
                    {
                        foreach (string filePath in tj.m_files)
                        {
                            Console.WriteLine(filePath);
                            Console.WriteLine(tj.documentNumber(filePath));
                            DesignAppProgressSimple.CDocumentList documentList = tj0.cDocumentList(tj.documentNumber(filePath));
                            if (documentList != null)
                            {
                                Console.WriteLine(documentList.legalDate);
                                Xml2Word xml2html11 = new Xml2Word(filePath, docNumber, app_base_path + ApplicationName, documentList.legalDate);
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
                        if (tj.m_result != null)
                        {
                            Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                            Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                            Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                        }

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
                }
            }
            Console.WriteLine("hello,world\n");
            string buff = Console.ReadLine();

        }
    }
}
