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
            if (tj.m_error == tj.e_NONE)
            {

                if (tj != null && tj.m_result != null)
                {
                    Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                    Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                    Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);

                    if (tj.m_data == null)
                    {
                        for (int i = 0; i < tj.m_data.applicantAttorney.Length; i++)
                        {
                            Console.WriteLine("\t申請人識別番号：" + tj.m_data.applicantAttorney[i].applicantAttorneyCd);
                            Console.WriteLine("\t申請人名称：" + tj.m_data.applicantAttorney[i].name);
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
            string buff = Console.ReadLine();
        }
    }
}
