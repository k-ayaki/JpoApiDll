using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JpoApi;

namespace testAppProgress
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);


            AppProgress tj = new AppProgress("2014560041", at.m_access_token.access_token);
            //AppProgress tj = new AppProgress("2023036507", at.m_access_token.access_token);
            Console.WriteLine("■特許経過情報取得　2014560041");
            if(tj.m_error == tj.e_NONE)
            {
                if (tj.m_result != null)
                {
                    Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                    Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                    Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                    if (tj.m_data != null)
                    {
                        Console.WriteLine("出願番号：" + tj.m_data.applicationNumber);
                        Console.WriteLine("発明の名称：" + tj.m_data.inventionTitle);
                        for (int i = 0; i < tj.m_data.applicantAttorney.Length; i++)
                        {
                            Console.WriteLine("\t申請人識別番号：" + tj.m_data.applicantAttorney[i].applicantAttorneyCd);
                            Console.WriteLine("\t申請人氏名・名称：" + tj.m_data.applicantAttorney[i].name);
                            Console.WriteLine("\t繰返番号：" + tj.m_data.applicantAttorney[i].repeatNumber);
                            Console.WriteLine("\t出願人・代理人識別：" + tj.m_data.applicantAttorney[i].applicantAttorneyClass);
                        }
                        Console.WriteLine("出願日：" + tj.m_data.filingDate);
                        Console.WriteLine("公開番号：" + tj.m_data.publicationNumber);
                        Console.WriteLine("公開番号（西暦変換）：" + tj.m_data.ADPublicationNumber);
                        Console.WriteLine("公表番号：" + tj.m_data.nationalPublicationNumber);
                        Console.WriteLine("公表番号（西暦変換）：" + tj.m_data.ADNationalPublicationNumber);
                        Console.WriteLine("公開日：" + tj.m_data.publicationDate);
                        Console.WriteLine("登録番号：" + tj.m_data.registrationNumber);
                        Console.WriteLine("登録日：" + tj.m_data.registrationDate);
                        Console.WriteLine("国際出願番号：" + tj.m_data.internationalApplicationNumber);
                        Console.WriteLine("国際公開番号：" + tj.m_data.internationalPublicationNumber);
                        Console.WriteLine("国際公開日：" + tj.m_data.internationalPublicationDate);
                        Console.WriteLine("抹消識別：" + tj.m_data.erasureIdentifier);
                        Console.WriteLine("存続期間満了年月日：" + tj.m_data.expireDate);
                        Console.WriteLine("本権利消滅日：" + tj.m_data.disappearanceDate);
                        for (int i = 0; i < tj.m_data.priorityRightInformation.Length; i++)
                        {
                            Console.WriteLine("\t");
                            Console.WriteLine("\tパリ条約に基づく優先権出願番号：" + tj.m_data.priorityRightInformation[i].parisPriorityApplicationNumber);
                            Console.WriteLine("\tパリ条約に基づく優先権主張日：" + tj.m_data.priorityRightInformation[i].parisPriorityDate);
                            Console.WriteLine("\tパリ条約に基づく優先権国コード：" + tj.m_data.priorityRightInformation[i].parisPriorityCountryCd);
                            Console.WriteLine("\t国内優先権四法コード：" + tj.m_data.priorityRightInformation[i].nationalPriorityLawCd);
                            Console.WriteLine("\t国内優先権出願番号：" + tj.m_data.priorityRightInformation[i].nationalPriorityApplicationNumber);
                            Console.WriteLine("\t国内優先権国際出願番号：" + tj.m_data.priorityRightInformation[i].nationalPriorityInternationalApplicationNumber);
                            Console.WriteLine("\t国内優先権主張日：" + tj.m_data.priorityRightInformation[i].nationalPriorityDate);
                        }
                        Console.WriteLine("原出願情報");
                        Console.WriteLine("\t原出願番号：" + tj.m_data.parentApplicationInformation.parentApplicationNumber);
                        Console.WriteLine("\t出願日：" + tj.m_data.parentApplicationInformation.filingDate);
                        Console.WriteLine("\t");
                        for (int i = 0; i < tj.m_data.divisionalApplicationInformation.Length; i++)
                        {
                            Console.WriteLine("分割出願群情報");
                            Console.WriteLine("\t出願番号：" + tj.m_data.divisionalApplicationInformation[i].applicationNumber);
                            Console.WriteLine("\t公開番号：" + tj.m_data.divisionalApplicationInformation[i].publicationNumber);
                            Console.WriteLine("\t公開番号（西暦変換）：" + tj.m_data.divisionalApplicationInformation[i].ADPublicationNumber);
                            Console.WriteLine("\t公表番号：" + tj.m_data.divisionalApplicationInformation[i].nationalPublicationNumber);
                            Console.WriteLine("\t公表番号（西暦変換）：" + tj.m_data.divisionalApplicationInformation[i].ADNationalPublicationNumber);
                            Console.WriteLine("\t登録番号：" + tj.m_data.divisionalApplicationInformation[i].registrationNumber);
                            Console.WriteLine("\t国際出願番号：" + tj.m_data.divisionalApplicationInformation[i].internationalApplicationNumber);
                            Console.WriteLine("\t国際公開番号：" + tj.m_data.divisionalApplicationInformation[i].internationalPublicationNumber);
                            Console.WriteLine("\t抹消識別：" + tj.m_data.divisionalApplicationInformation[i].erasureIdentifier);
                            Console.WriteLine("\t存続期間満了年月日：" + tj.m_data.divisionalApplicationInformation[i].expireDate);
                            Console.WriteLine("\t本権利消滅日：" + tj.m_data.divisionalApplicationInformation[i].disappearanceDate);
                            Console.WriteLine("\t分割出願の世代：" + tj.m_data.divisionalApplicationInformation[i].divisionalGeneration);
                        }
                        for (int i = 0; i < tj.m_data.bibliographyInformation.Length; i++)
                        {
                            Console.WriteLine("書類一覧（書誌）");
                            Console.WriteLine("\t番号種別：" + tj.m_data.bibliographyInformation[i].numberType);
                            Console.WriteLine("\t番号：" + tj.m_data.bibliographyInformation[i].number);
                            for (int j = 0; j < tj.m_data.bibliographyInformation[i].documentList.Length; j++)
                            {
                                Console.WriteLine("\t書類一覧");
                                Console.WriteLine("\t\t受付日・発送日・作成日：" + tj.m_data.bibliographyInformation[i].documentList[j].legalDate);
                                Console.WriteLine("\t\tIB書類フラグ：" + tj.m_data.bibliographyInformation[i].documentList[j].irirFlg);
                                Console.WriteLine("\t\t書類実体有無：" + tj.m_data.bibliographyInformation[i].documentList[j].availabilityFlag);
                                Console.WriteLine("\t\t中間書類コード：" + tj.m_data.bibliographyInformation[i].documentList[j].documentCode);
                                Console.WriteLine("\t\t書類名：" + tj.m_data.bibliographyInformation[i].documentList[j].documentDescription);
                                Console.WriteLine("\t\t書類番号：" + tj.m_data.bibliographyInformation[i].documentList[j].documentNumber);
                                Console.WriteLine("\t\tバージョン番号：" + tj.m_data.bibliographyInformation[i].documentList[j].versionNumber);
                                Console.WriteLine("\t\t書類識別：" + tj.m_data.bibliographyInformation[i].documentList[j].documentSeparator);
                                Console.WriteLine("\t\tページ数：" + tj.m_data.bibliographyInformation[i].documentList[j].numberOfPages);
                                Console.WriteLine("\t\tドキュメントサイズ：" + tj.m_data.bibliographyInformation[i].documentList[j].sizeOfDocument);
                            }
                        }
                    }
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
