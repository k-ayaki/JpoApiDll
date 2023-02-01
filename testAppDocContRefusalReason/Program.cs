using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using JpoApi;

namespace testAppDocContRefusalReason
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);
            NetworkState networkState = new NetworkState();

            Console.WriteLine("■拒絶理由通知実体情報取得");

            //            string[] docNumbers = { "2020000001", "2010002165", "2006106644", "2014060127", "2014089742" };
            string[] docNumbers = { "2012000313" };
            foreach (string docNumber in docNumbers)
            {
                using (AppDocContRefusalReason tj5 = new AppDocContRefusalReason(docNumber, at.m_access_token.access_token))
                {
                    if (tj5.m_error == tj5.e_CONTENT)
                    {
                        Console.WriteLine("ステータスコード：" + tj5.m_result.statusCode);
                        Console.WriteLine("エラーメッセージ：" + tj5.m_result.errorMessage);
                        Console.WriteLine("残アクセス数：" + tj5.m_result.remainAccessCount);
                    }
                    else
                    if (tj5.m_error == tj5.e_NONE)
                    {
                        foreach (string f in tj5.m_files)
                        {
                            Console.WriteLine(f);

                            string curDir = System.IO.Directory.GetCurrentDirectory();
                            Xml2Word xml2word = new Xml2Word(f, docNumber, curDir, 20,15,30,25);
                        }
                    }
                }
            }
            Console.WriteLine("hello,world\n");
            string buff = Console.ReadLine();
        }
    }
}
