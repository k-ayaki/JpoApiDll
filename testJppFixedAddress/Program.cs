using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JpoApi;

namespace testJppFixedAddress
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            JppFixedAddress tj4 = new JppFixedAddress("2020008423", at.m_access_token.access_token);
            Console.WriteLine("■特許J-PlatPat固定アドレス取得API　2020008423");
            Console.WriteLine("ステータスコード：" + tj4.m_result.statusCode);
            Console.WriteLine("エラーメッセージ：" + tj4.m_result.errorMessage);
            Console.WriteLine("残アクセス数：" + tj4.m_result.remainAccessCount);
            if (tj4.m_error == tj4.e_NONE)
            {
                Console.WriteLine("url：" + tj4.m_data.url);
            }
            string t = Console.ReadLine();
        }
    }
}
