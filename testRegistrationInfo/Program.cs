using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testRegistrationInfo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            RegistrationInfo tj = new RegistrationInfo("2020008423", at.m_access_token.access_token);
            Console.WriteLine("■特許登録情報取得　2020008423");
            if (tj.m_error == tj.e_NONE)
            {

                Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                if (tj.m_data != null)
                {
                    Console.WriteLine("出願番号：" + tj.m_data.applicationNumber);
                    Console.WriteLine("出願日：" + tj.m_data.filingDate);
                    Console.WriteLine("登録番号：" + tj.m_data.registrationNumber);
                    Console.WriteLine("登録日：" + tj.m_data.registrationDate);
                    Console.WriteLine("査定日：" + tj.m_data.decisionDate);
                    Console.WriteLine("審決日：" + tj.m_data.appealTrialDecisiondDate);
                    Console.WriteLine("\t");
                    for (int i = 0; i < tj.m_data.rightPersonInformation.Length; i++)
                    {
                        Console.WriteLine("\t権利者コード：" + tj.m_data.rightPersonInformation[i].rightPersonCd);
                        Console.WriteLine("\t権利者氏名・名称：" + tj.m_data.rightPersonInformation[i].rightPersonName);
                    }
                    Console.WriteLine("発明の名称：" + tj.m_data.inventionTitle);
                    Console.WriteLine("請求項の数：" + tj.m_data.numberOfClaims);
                    Console.WriteLine("存続期間満了年月日：" + tj.m_data.expireDate);
                    Console.WriteLine("次期年金納付期限：" + tj.m_data.nextPensionPaymentDate);
                    Console.WriteLine("最終納付年分：" + tj.m_data.lastPaymentYearly);
                    Console.WriteLine("本権利抹消識別：" + tj.m_data.erasureIdentifier);
                    Console.WriteLine("本権利抹消日：" + tj.m_data.disappearanceDate);
                    Console.WriteLine("更新日付：" + tj.m_data.updateDate);
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
            string t = Console.ReadLine();
        }
    }
}
