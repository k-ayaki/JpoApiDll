using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JpoApi;

namespace SetAccount
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            Console.WriteLine("特許情報取得APIのアカウント設定 :");
            Console.WriteLine("id :");
            ac.m_id = Console.ReadLine();
            Console.WriteLine("password :");
            ac.m_password = Console.ReadLine();
            Console.WriteLine("Path :");
            ac.m_path = Console.ReadLine();
        }
    }
}
