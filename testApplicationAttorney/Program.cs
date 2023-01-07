using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace testApplicationAttorney
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            ApplicantAttorney tj = new ApplicantAttorney("株式会社日立製作所", at.m_access_token.access_token);
            Console.WriteLine("■特許申請人コード取得 株式会社日立製作所");
            Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
            Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
            Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
            if (tj.m_error == tj.e_NONE)
            {
                for (int i = 0; i < tj.m_data.applicantAttorney.Length; i++)
                {
                    Console.WriteLine("\t申請人識別番号：" + tj.m_data.applicantAttorney[i].applicantAttorneyCd);
                    Console.WriteLine("\t申請人名称：" + tj.m_data.applicantAttorney[i].name);
                }
            }
            string buff = Console.ReadLine();
        }
    }
}
