using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JpoApi;
using JpoApi.trademark;

namespace testTrademarkJppFixedAddress
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            TrademarkJppFixedAddress tj4 = new TrademarkJppFixedAddress("2018009480", at.m_access_token.access_token);
            Console.WriteLine("■商標J-PlatPat固定アドレス取得API　2018009480");
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
