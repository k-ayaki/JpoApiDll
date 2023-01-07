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

            PriorityRightAppInfo tj4 = new PriorityRightAppInfo("2020008423", at.m_access_token.access_token);
            Console.WriteLine("■特許優先基礎出願情報　2020008423");
            Console.WriteLine("ステータスコード：" + tj4.m_result.statusCode);
            Console.WriteLine("エラーメッセージ：" + tj4.m_result.errorMessage);
            Console.WriteLine("残アクセス数：" + tj4.m_result.remainAccessCount);
            if (tj4.m_error == tj4.e_NONE)
            {
                Console.WriteLine("出願番号：" + tj4.m_data.applicationNumber);
                for (int i = 0; i < tj4.m_data.priorityRightInformation.Length; i++)
                {
                    Console.WriteLine("\t");
                    Console.WriteLine("\tパリ条約に基づく優先権出願番号：" + tj4.m_data.priorityRightInformation[i].parisPriorityApplicationNumber);
                    Console.WriteLine("\tパリ条約に基づく優先権主張日：" + tj4.m_data.priorityRightInformation[i].parisPriorityDate);
                    Console.WriteLine("\tパリ条約に基づく優先権国コード：" + tj4.m_data.priorityRightInformation[i].parisPriorityCountryCd);
                    Console.WriteLine("\t国内優先権四法コード：" + tj4.m_data.priorityRightInformation[i].nationalPriorityLawCd);
                    Console.WriteLine("\t国内優先権出願番号：" + tj4.m_data.priorityRightInformation[i].nationalPriorityApplicationNumber);
                    Console.WriteLine("\t国内優先権国際出願番号：" + tj4.m_data.priorityRightInformation[i].nationalPriorityInternationalApplicationNumber);
                    Console.WriteLine("\t国内優先権主張日：" + tj4.m_data.priorityRightInformation[i].nationalPriorityDate);
                }
            }
            string t = Console.ReadLine();
        }
    }
}
