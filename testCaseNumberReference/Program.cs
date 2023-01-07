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

            CaseNumberReference tj = new CaseNumberReference("application", "2020008423", at.m_access_token.access_token);
            Console.WriteLine("■特許番号参照 application 2020008423");
            if (tj.m_error == tj.e_NONE)
            {
                Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                Console.WriteLine("出願番号：" + tj.m_result.data.applicationNumber);
                Console.WriteLine("公開番号：" + tj.m_result.data.publicationNumber);
                Console.WriteLine("公表番号：" + tj.m_result.data.nationalPublicationNumber);
                Console.WriteLine("登録番号：" + tj.m_result.data.registrationNumber);
            }
            string buff = Console.ReadLine();
        }
    }
}
