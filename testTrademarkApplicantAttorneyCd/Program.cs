using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testTrademarkApplicantAttorneyCd
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            TrademarkApplicantAttorneyCd tj2 = new TrademarkApplicantAttorneyCd("000001199", at.m_access_token.access_token);
            Console.WriteLine("■商標　申請人氏名・名称取得 000001199");
            Console.WriteLine("ステータスコード：" + tj2.m_result.statusCode);
            Console.WriteLine("エラーメッセージ：" + tj2.m_result.errorMessage);
            Console.WriteLine("残アクセス数：" + tj2.m_result.remainAccessCount);
            if (tj2.m_error == tj2.e_NONE)
            {
                Console.WriteLine("申請人名称：" + tj2.m_data.applicantAttorneyName);
            }
            string t = Console.ReadLine();

        }
    }
}
