using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JpoApi;

namespace testOpdFamily
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            OpdFamily tj = new OpdFamily("application", "JP.2020008423.A", at.m_access_token.access_token);
            Console.WriteLine("■OPD書類一覧取得API　JP.2020008423.A");
            if (tj.m_error == tj.e_NONE)
            {
                Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                if (tj.m_resultXML.familyData != null)
                {
                    for (int i = 0; i < tj.m_resultXML.familyData.familyLists.Count; i++)
                    {
                        Console.WriteLine("国：" + tj.m_resultXML.familyData.familyLists[i].country);
                        for (int j=0; j< tj.m_resultXML.familyData.familyLists[i].familyItemLists.Count; j++)
                        {
                            Console.WriteLine("日時：" + tj.m_resultXML.familyData.familyLists[i].familyItemLists[j].applicationNumber.date);
                            Console.WriteLine("文書番号：" + tj.m_resultXML.familyData.familyLists[i].familyItemLists[j].applicationNumber.documentNumber);
                        }
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
