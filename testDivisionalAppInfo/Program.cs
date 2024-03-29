﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JpoApi;

namespace testDivisionalAppInfo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            DivisionalAppInfo tj = new DivisionalAppInfo("2007035937", at.m_access_token.access_token);
            Console.WriteLine("■分割出願情報取得　2007035937");
            if (tj.m_error == tj.e_NONE)
            {
                Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                if(tj.m_data != null) 
                {
                    Console.WriteLine("出願番号：" + tj.m_data.applicationNumber);
                    Console.WriteLine("原出願情報");
                    Console.WriteLine("\t原出願番号：" + tj.m_data.parentApplicationInformation.parentApplicationNumber);
                    Console.WriteLine("\t出願日：" + tj.m_data.parentApplicationInformation.filingDate);
                    Console.WriteLine("\t");
                    for (int i = 0; i < tj.m_data.divisionalApplicationInformation.Length; i++)
                    {
                        Console.WriteLine("分割出願群情報");
                        Console.WriteLine("\t出願番号：" + tj.m_data.divisionalApplicationInformation[i].applicationNumber);
                        Console.WriteLine("\t公開番号：" + tj.m_data.divisionalApplicationInformation[i].publicationNumber);
                        Console.WriteLine("\t公開番号（西暦変換）：" + tj.m_data.divisionalApplicationInformation[i].ADPublicationNumber);
                        Console.WriteLine("\t公表番号：" + tj.m_data.divisionalApplicationInformation[i].nationalPublicationNumber);
                        Console.WriteLine("\t公表番号（西暦変換）：" + tj.m_data.divisionalApplicationInformation[i].ADNationalPublicationNumber);
                        Console.WriteLine("\t登録番号：" + tj.m_data.divisionalApplicationInformation[i].registrationNumber);
                        Console.WriteLine("\t国際出願番号：" + tj.m_data.divisionalApplicationInformation[i].internationalApplicationNumber);
                        Console.WriteLine("\t国際公開番号：" + tj.m_data.divisionalApplicationInformation[i].internationalPublicationNumber);
                        Console.WriteLine("\t抹消識別：" + tj.m_data.divisionalApplicationInformation[i].erasureIdentifier);
                        Console.WriteLine("\t存続期間満了年月日：" + tj.m_data.divisionalApplicationInformation[i].expireDate);
                        Console.WriteLine("\t本権利消滅日：" + tj.m_data.divisionalApplicationInformation[i].disappearanceDate);
                        Console.WriteLine("\t分割出願の世代：" + tj.m_data.divisionalApplicationInformation[i].divisionalGeneration);
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
            string t = Console.ReadLine();
        }
    }
}
