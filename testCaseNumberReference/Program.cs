using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testCaseNumberReference
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            //CaseNumberReference tj = new CaseNumberReference("application", "2020008423", at.m_access_token.access_token);
            CaseNumberReference tj = new CaseNumberReference("application", "2023056033", at.m_access_token.access_token);
            Console.WriteLine("■特許番号参照 application 2020008423");
            if (tj.m_error == tj.e_NONE)
            {
                Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                if (tj.m_data != null)
                {
                    Console.WriteLine("出願番号：" + tj.m_data.applicationNumber);
                    Console.WriteLine("公開番号：" + tj.m_data.publicationNumber);
                    Console.WriteLine("公表番号：" + tj.m_data.nationalPublicationNumber);
                    Console.WriteLine("登録番号：" + tj.m_data.registrationNumber);
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

            string buff = Console.ReadLine();
        }
    }
}
