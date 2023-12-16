using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testTrademarkCaseNumberReference
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            //CaseNumberReference tj = new CaseNumberReference("application", "2020008423", at.m_access_token.access_token);
            TrademarkCaseNumberReference tj = new TrademarkCaseNumberReference("application", "2018009480", at.m_access_token.access_token);
            Console.WriteLine("■商標番号参照 application 2018009480");
            if (tj.m_error == tj.e_NONE)
            {
                Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                Console.WriteLine("出願番号：" + tj.m_result.data.applicationNumber);
                Console.WriteLine("登録番号：" + tj.m_result.data.registrationNumber);
            }
            string buff = Console.ReadLine();
        }
    }
}
