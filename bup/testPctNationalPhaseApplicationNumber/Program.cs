using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testPctNationalPhaseApplicationNumber
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            PctNationalPhaseApplicationNumber tj4 = new PctNationalPhaseApplicationNumber("international_application", "JP2019011858", at.m_access_token.access_token);
            Console.WriteLine("■特許PCT出願の日本国内移行後の出願番号取得API　JP2019011858");
            Console.WriteLine("ステータスコード：" + tj4.m_result.statusCode);
            Console.WriteLine("エラーメッセージ：" + tj4.m_result.errorMessage);
            Console.WriteLine("残アクセス数：" + tj4.m_result.remainAccessCount);
            if (tj4.m_error == tj4.e_NONE)
            {
                Console.WriteLine("出願番号：" + tj4.m_data.applicationNumber);
            }
            string t = Console.ReadLine();
        }
    }
}
