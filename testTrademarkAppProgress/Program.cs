using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace testTrademarkAppProgress
{
    internal class Program
    {
        static void Main(string[] args)
        {


            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            TrademarkAppProgress tj4 = new TrademarkAppProgress("2018009480", at.m_access_token.access_token);
            Console.WriteLine("■商標経過情報取得　2018009480");
            Console.WriteLine("ステータスコード：" + tj4.m_result.statusCode);
            Console.WriteLine("エラーメッセージ：" + tj4.m_result.errorMessage);
            Console.WriteLine("残アクセス数：" + tj4.m_result.remainAccessCount);
            if (tj4.m_error == tj4.e_NONE)
            {
                Console.WriteLine("出願番号：" + tj4.m_data.applicationNumber);
                for (int i = 0; i < tj4.m_data.applicantAttorney.Length; i++)
                {
                    Console.WriteLine("\t申請人識別番号：" + tj4.m_data.applicantAttorney[i].applicantAttorneyCd);
                    Console.WriteLine("\t申請人氏名・名称：" + tj4.m_data.applicantAttorney[i].name);
                    Console.WriteLine("\t繰返番号：" + tj4.m_data.applicantAttorney[i].repeatNumber);
                    Console.WriteLine("\t出願人・代理人識別：" + tj4.m_data.applicantAttorney[i].applicantAttorneyClass);
                }
                Console.WriteLine("出願日：" + tj4.m_data.filingDate);
                Console.WriteLine("公開日：" + tj4.m_data.publicationDate);
                Console.WriteLine("登録番号：" + tj4.m_data.registrationNumber);
                Console.WriteLine("登録日：" + tj4.m_data.registrationDate);
                Console.WriteLine("抹消識別：" + tj4.m_data.erasureIdentifier);
                Console.WriteLine("存続期間満了年月日：" + tj4.m_data.expireDate);
                Console.WriteLine("本権利消滅日：" + tj4.m_data.disappearanceDate);
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
                for (int i = 0; i < tj4.m_data.priorityRightInformation.Length; i++)
                {
                    Console.WriteLine("\t");
                    Console.WriteLine("\tパリ条約に基づく優先権出願番号：" + tj4.m_data.priorityRightInformation[i].parisPriorityApplicationNumber);
                    Console.WriteLine("\tパリ条約に基づく優先権主張日：" + tj4.m_data.priorityRightInformation[i].parisPriorityDate);
                    Console.WriteLine("\tパリ条約に基づく優先権国コード：" + tj4.m_data.priorityRightInformation[i].parisPriorityCountryCd);
                }
                Console.WriteLine("原出願情報");
                Console.WriteLine("\t原出願番号：" + tj4.m_data.parentApplicationInformation.parentApplicationNumber);
                Console.WriteLine("\t原出願の出願日：" + tj4.m_data.parentApplicationInformation.filingDate);
                Console.WriteLine("\t");
                for (int i = 0; i < tj4.m_data.bibliographyInformation.Length; i++)
                {
                    Console.WriteLine("書類一覧（書誌）");
                    Console.WriteLine("\t番号種別：" + tj4.m_data.bibliographyInformation[i].numberType);
                    Console.WriteLine("\t番号：" + tj4.m_data.bibliographyInformation[i].number);
                    for (int j = 0; j < tj4.m_data.bibliographyInformation[i].documentList.Length; j++)
                    {
                        Console.WriteLine("\t書類一覧");
                        Console.WriteLine("\t\t受付日・発送日・作成日：" + tj4.m_data.bibliographyInformation[i].documentList[j].legalDate);
                        Console.WriteLine("\t\tIB書類フラグ：" + tj4.m_data.bibliographyInformation[i].documentList[j].irirFlg);
                        Console.WriteLine("\t\t書類実体有無：" + tj4.m_data.bibliographyInformation[i].documentList[j].availabilityFlag);
                        Console.WriteLine("\t\t中間書類コード：" + tj4.m_data.bibliographyInformation[i].documentList[j].documentCode);
                        Console.WriteLine("\t\t書類名：" + tj4.m_data.bibliographyInformation[i].documentList[j].documentDescription);
                        Console.WriteLine("\t\t書類番号：" + tj4.m_data.bibliographyInformation[i].documentList[j].documentNumber);
                        Console.WriteLine("\t\tバージョン番号：" + tj4.m_data.bibliographyInformation[i].documentList[j].versionNumber);
                        Console.WriteLine("\t\t書類識別：" + tj4.m_data.bibliographyInformation[i].documentList[j].documentSeparator);
                        Console.WriteLine("\t\tページ数：" + tj4.m_data.bibliographyInformation[i].documentList[j].numberOfPages);
                        Console.WriteLine("\t\tドキュメントサイズ：" + tj4.m_data.bibliographyInformation[i].documentList[j].sizeOfDocument);
                    }
                }
            }
            else
            {

            }
            string t = Console.ReadLine();
        }
    }
}
