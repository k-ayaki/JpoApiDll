using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JpoApi;

namespace testInpitRss
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            Console.WriteLine("■拒絶理由通知実体情報取得");
            string[] docNumbers = { "2017157494", "2013250562", "2010013051", "2006106644", "2014089742" };
            foreach (string docNumber in docNumbers)
            {
                InpitRss ir = new InpitRss(docNumber);
                Console.WriteLine(docNumber + " pubDate：" + ir.m_szDate);
                Console.WriteLine("statusCode:" + ir.m_statusCode);
                Console.WriteLine(ir.m_rss);
                ir.Dispose();
            }
            string buff = Console.ReadLine();
        }
    }
}
