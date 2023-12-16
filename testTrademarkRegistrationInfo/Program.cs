using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testTrademarkRegistrationInfo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            TrademarkRegistrationInfo tj4 = new TrademarkRegistrationInfo("2018009480", at.m_access_token.access_token);
            Console.WriteLine("■商標登録情報取得　2018009480");
            Console.WriteLine("ステータスコード：" + tj4.m_result.statusCode);
            Console.WriteLine("エラーメッセージ：" + tj4.m_result.errorMessage);
            Console.WriteLine("残アクセス数：" + tj4.m_result.remainAccessCount);
            if (tj4.m_error == tj4.e_NONE)
            {
                Console.WriteLine("出願番号：" + tj4.m_data.applicationNumber);
                Console.WriteLine("出願日：" + tj4.m_data.filingDate);
                Console.WriteLine("登録番号：" + tj4.m_data.registrationNumber);
                Console.WriteLine("登録日：" + tj4.m_data.registrationDate);
                Console.WriteLine("査定日：" + tj4.m_data.decisionDate);
                Console.WriteLine("審決日：" + tj4.m_data.appealTrialDecisiondDate);
                Console.WriteLine("\t");
                for (int i = 0; i < tj4.m_data.rightPersonInformation.Length; i++)
                {
                    Console.WriteLine("\t権利者コード：" + tj4.m_data.rightPersonInformation[i].rightPersonCd);
                    Console.WriteLine("\t権利者氏名・名称：" + tj4.m_data.rightPersonInformation[i].rightPersonName);
                }
                Console.WriteLine("存続期間満了年月日：" + tj4.m_data.expireDate);
                Console.WriteLine("次期年金納付期限：" + tj4.m_data.nextPensionPaymentDate);
                Console.WriteLine("最終納付年分：" + tj4.m_data.lastPaymentYearly);
                Console.WriteLine("本権利抹消識別：" + tj4.m_data.erasureIdentifier);
                Console.WriteLine("本権利抹消日：" + tj4.m_data.disappearanceDate);
                Console.WriteLine("更新日付：" + tj4.m_data.updateDate);
                Console.WriteLine("商標：" + tj4.m_data.trademarkForDisplay);

                foreach (KeyValuePair<string, string> kvp in tj4.m_data.transliteration)
                {
                    Console.WriteLine("称呼：Key: {0}, Value: {1}", kvp.Key, kvp.Value);
                }
                foreach (KeyValuePair<string, string> kvp in tj4.m_data.viennaClass)
                {
                    Console.WriteLine("区分：Key: {0}, Value: {1}", kvp.Key, kvp.Value);
                }
                for (int i = 0; i < tj4.m_data.goodsServiceInformation.Length; i++)
                {
                    Console.WriteLine("\t");
                    Console.WriteLine("\t商品又は役務の名称：" + tj4.m_data.goodsServiceInformation[i].goodsServiceName);
                    Console.WriteLine("\t商品又は役務の区分：" + tj4.m_data.goodsServiceInformation[i].goodsServiceClass);
                    Console.WriteLine("\t商品又は役務の類似群：" + tj4.m_data.goodsServiceInformation[i].similarCode);
                }

            }

            string t = Console.ReadLine();

        }
    }
}
