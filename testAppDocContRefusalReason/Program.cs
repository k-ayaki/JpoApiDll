using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using JpoApi;
using static JpoApi.OpdGlobalDocList;

namespace testAppDocContRefusalReason
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string app_base_path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);
            NetworkState networkState = new NetworkState();

            Console.WriteLine("■拒絶理由通知実体情報取得");

            //string[] docNumbers = { "2012000313", "2020000001", "2010002165", "2006106644", "2014060127", "2014089742","2020162217" };
            //string[] docNumbers = { "2019051185" };
            string[] docNumbers = { "2020139282" };
            foreach (string docNumber in docNumbers)
            {
                AppProgressSimple tj0 = new AppProgressSimple(docNumber, at.m_access_token.access_token);
                if (tj0.m_error == tj0.e_NONE)
                {

                    using (AppDocContRefusalReason tj = new AppDocContRefusalReason(docNumber, at.m_access_token.access_token))
                    {
                        if (tj.m_error == tj.e_NONE && tj.m_files != null)
                        {
                            foreach (string filePath in tj.m_files)
                            {
                                Console.WriteLine(filePath);
                                Console.WriteLine(tj.documentNumber(filePath));
                                AppProgressSimple.CDocumentList documentList = tj0.cDocumentList(tj.documentNumber(filePath));
                                if (documentList != null)
                                {
                                    Console.WriteLine(documentList.legalDate);
                                    Xml2Word xml2word = new Xml2Word(filePath, docNumber, app_base_path + ApplicationName, documentList.legalDate);
                                    Console.WriteLine(xml2word.m_wordFilePath);
                                    Console.WriteLine(xml2word.m_provisions);
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
            }
            Console.WriteLine("hello,world\n");
            string buff = Console.ReadLine();
        }
    }
}
