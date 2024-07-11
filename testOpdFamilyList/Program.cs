using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testOpdFamilyList
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            OpdFamilyList tj = new OpdFamilyList("application", "JP.2015500001.A", at.m_access_token.access_token);
            Console.WriteLine("■OPD書類一覧取得API　JP.2015500001.A");
            if (tj.m_error == tj.e_NONE)
            {
                Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                if (tj.m_resultXML.familyListData != null)
                {
                    for (int i = 0; i < tj.m_resultXML.familyListData.familyLists.Count; i++)
                    {
                        Console.WriteLine("ＩＤ：" + tj.m_resultXML.familyListData.familyLists[i].id);
                        Console.WriteLine("出願番号：" + tj.m_resultXML.familyListData.familyLists[i].applicationNumber.documentNumber);
                        Console.WriteLine("出願日：" + tj.m_resultXML.familyListData.familyLists[i].applicationNumber.date);
                        for (int j=0; j < tj.m_resultXML.familyListData.familyLists[i].publicationNumberList.Count; j++)
                        {
                            Console.WriteLine("公開番号：" + tj.m_resultXML.familyListData.familyLists[i].publicationNumberList[j].documentNumber);
                            Console.WriteLine("公開日：" + tj.m_resultXML.familyListData.familyLists[i].publicationNumberList[j].date);
                        }
                        for (int k=0; k< tj.m_resultXML.familyListData.familyLists[i].registrationNumberList.Count; k++)
                        {
                            Console.WriteLine("登録番号：" + tj.m_resultXML.familyListData.familyLists[i].registrationNumberList[k].documentNumber);
                            Console.WriteLine("登録日：" + tj.m_resultXML.familyListData.familyLists[i].registrationNumberList[k].date);
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
