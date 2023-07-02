using JpoApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testAppDocContOpinionAmendment
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            Console.WriteLine("■特許申請書類実体情報取得");
            string curDir = System.IO.Directory.GetCurrentDirectory();

            //string[] docNumbers = new string[] { "2015515485", "2012193763", "2012199157", "2015532525", "2015539540", "2015515940", "2015550024", "2012001505", "2012019353", "2009004798", "2006106644" };
            string[] docNumbers = new string[] { "2015515485" };
            foreach (string docNumber in docNumbers)
            {
                AppDocContOpinionAmendment tj5 = new AppDocContOpinionAmendment(docNumber, at.m_access_token.access_token);
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
                        Xml2Word xml2html11 = new Xml2Word(f, docNumber, curDir,20,15,20,15);
                    }
                }
            }
            Console.WriteLine("hello,world\n");
            string buff = Console.ReadLine();
        }
    }
}
