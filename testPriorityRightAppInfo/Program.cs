using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testPriorityRightAppInfo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            PriorityRightAppInfo tj = new PriorityRightAppInfo("2020008423", at.m_access_token.access_token);
            Console.WriteLine("■特許優先基礎出願情報　2020008423");
            if (tj.m_error == tj.e_NONE)
            {

                Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                if (tj.m_data != null)
                {
                    Console.WriteLine("出願番号：" + tj.m_data.applicationNumber);
                    for (int i = 0; i < tj.m_data.priorityRightInformation.Length; i++)
                    {
                        Console.WriteLine("\t");
                        Console.WriteLine("\tパリ条約に基づく優先権出願番号：" + tj.m_data.priorityRightInformation[i].parisPriorityApplicationNumber);
                        Console.WriteLine("\tパリ条約に基づく優先権主張日：" + tj.m_data.priorityRightInformation[i].parisPriorityDate);
                        Console.WriteLine("\tパリ条約に基づく優先権国コード：" + tj.m_data.priorityRightInformation[i].parisPriorityCountryCd);
                        Console.WriteLine("\t国内優先権四法コード：" + tj.m_data.priorityRightInformation[i].nationalPriorityLawCd);
                        Console.WriteLine("\t国内優先権出願番号：" + tj.m_data.priorityRightInformation[i].nationalPriorityApplicationNumber);
                        Console.WriteLine("\t国内優先権国際出願番号：" + tj.m_data.priorityRightInformation[i].nationalPriorityInternationalApplicationNumber);
                        Console.WriteLine("\t国内優先権主張日：" + tj.m_data.priorityRightInformation[i].nationalPriorityDate);
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
