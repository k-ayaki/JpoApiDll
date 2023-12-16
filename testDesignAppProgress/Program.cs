using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JpoApi;

namespace testDesignAppProgress
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            DesignAppProgress tj4 = new DesignAppProgress("2022012584", at.m_access_token.access_token);
            Console.WriteLine("■意匠経過情報取得　2022012584");
            Console.WriteLine("ステータスコード：" + tj4.m_result.statusCode);
            Console.WriteLine("エラーメッセージ：" + tj4.m_result.errorMessage);
            Console.WriteLine("残アクセス数：" + tj4.m_result.remainAccessCount);
            if (tj4.m_error == tj4.e_NONE)
            {
                Console.WriteLine("出願番号：" + tj4.m_data.applicationNumber);
                Console.WriteLine("意匠に係る物品：" + tj4.m_data.designArticle);
                Console.WriteLine("意匠区分：" + tj4.m_data.designClass);
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
                Console.WriteLine("本意匠番号：" + tj4.m_data.principalDesignApplicationNumber);
                Console.WriteLine("抹消識別：" + tj4.m_data.erasureIdentifier);
                Console.WriteLine("存続期間満了年月日：" + tj4.m_data.expireDate);
                Console.WriteLine("本権利消滅日：" + tj4.m_data.disappearanceDate);
                for (int i = 0; i < tj4.m_data.priorityRightInformation.Length; i++)
                {
                    Console.WriteLine("\t");
                    Console.WriteLine("\tパリ条約に基づく優先権出願番号：" + tj4.m_data.priorityRightInformation[i].parisPriorityApplicationNumber);
                    Console.WriteLine("\tパリ条約に基づく優先権主張日：" + tj4.m_data.priorityRightInformation[i].parisPriorityDate);
                    Console.WriteLine("\tパリ条約に基づく優先権国コード：" + tj4.m_data.priorityRightInformation[i].parisPriorityCountryCd);
                }
                Console.WriteLine("原出願情報");
                Console.WriteLine("\t原出願のカテゴリ：" + tj4.m_data.parentApplicationInformation.parentApplicationCategory);
                Console.WriteLine("\t原出願の法区分：" + tj4.m_data.parentApplicationInformation.parentApplicationLawCode);
                Console.WriteLine("\t原出願番号：" + tj4.m_data.parentApplicationInformation.parentApplicationNumber);
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
