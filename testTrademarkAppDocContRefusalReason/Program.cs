﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using JpoApi;

namespace testTrademarkAppDocContRefusalReason
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);
            NetworkState networkState = new NetworkState();

            Console.WriteLine("■商標拒絶理由通知実体情報取得");

            //            string[] docNumbers = { "2020000001", "2010002165", "2006106644", "2014060127", "2014089742" };
            string[] docNumbers = { "2021010721" };
            foreach (string docNumber in docNumbers)
            {
                using (TrademarkAppDocContRefusalReason tj = new TrademarkAppDocContRefusalReason(docNumber, at.m_access_token.access_token))
                {
                    if (tj.m_error == tj.e_NONE)
                    {
                        foreach (string f in tj.m_files)
                        {
                            Console.WriteLine(f);

                            Xml2Word xml2word = new Xml2Word(f, docNumber);
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
