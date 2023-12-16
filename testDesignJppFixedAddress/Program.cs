using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testDesignJppFixedAddress
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            DesignJppFixedAddress tj4 = new DesignJppFixedAddress("2022012584", at.m_access_token.access_token);
            Console.WriteLine("■意匠J-PlatPat固定アドレス取得API　2022012584");
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
