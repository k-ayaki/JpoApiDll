using JpoApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testOpdGlobalCiteClass
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Account ac = new Account();
            AccessToken at = new AccessToken(ac.m_id, ac.m_password, ac.m_path);

            OpdGlobalCiteClass tj = new OpdGlobalCiteClass("JP.2015500001.A", at.m_access_token.access_token);
            Console.WriteLine("■OPD書類一覧取得API　JP.2015500001.A");
            if (tj.m_error == tj.e_NONE)
            {
                Console.WriteLine("ステータスコード：" + tj.m_result.statusCode);
                Console.WriteLine("エラーメッセージ：" + tj.m_result.errorMessage);
                Console.WriteLine("残アクセス数：" + tj.m_result.remainAccessCount);
                if (tj.m_resultXML.citationAndClassificationData != null)
                {
                    Console.WriteLine("出願番号：" + tj.m_resultXML.citationAndClassificationData.reference.applicationNumber.documentNumber);
                    Console.WriteLine("公開番号：" + tj.m_resultXML.citationAndClassificationData.reference.publicationNumber.documentNumber);
                    Console.WriteLine("登録番号：" + tj.m_resultXML.citationAndClassificationData.reference.registrationNumber.documentNumber);

                    for (int i = 0; i < tj.m_resultXML.citationAndClassificationData.classification.ipc.Count;  i++)
                    {
                        Console.WriteLine("国際特許分類：" + tj.m_resultXML.citationAndClassificationData.classification.ipc[i]);
                    }
                    for (int i = 0; i < tj.m_resultXML.citationAndClassificationData.classification.originals.OriginalList.Count; i++)
                    {
                        Console.WriteLine("日本国特許分類：" + tj.m_resultXML.citationAndClassificationData.classification.originals.OriginalList[i].Scheme 
                            + " " + tj.m_resultXML.citationAndClassificationData.classification.originals.OriginalList[i].Value);
                    }
                    Console.WriteLine("特許文献");
                    for (int i = 0; i < tj.m_resultXML.citationAndClassificationData.citation.patentLiteratureLists.Count; i++)
                    {
                        Console.WriteLine("日時：" + tj.m_resultXML.citationAndClassificationData.citation.patentLiteratureLists[i].draftDate);
                        Console.WriteLine("公開番号：" + tj.m_resultXML.citationAndClassificationData.citation.patentLiteratureLists[i].publicationNumber.documentNumber);
                    }
                    Console.WriteLine("非特許文献");
                    for (int i = 0; i < tj.m_resultXML.citationAndClassificationData.citation.nonPatentLiteratureLists.Count; i++)
                    {
                        Console.WriteLine("日時：" + tj.m_resultXML.citationAndClassificationData.citation.nonPatentLiteratureLists[i].draftDate);
                        Console.WriteLine("題名：" + tj.m_resultXML.citationAndClassificationData.citation.nonPatentLiteratureLists[i].text);
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
