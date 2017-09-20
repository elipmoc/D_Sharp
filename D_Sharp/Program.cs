using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace D_Sharp
{
    class Program
    {
        //拡張BNF記法
        //　[]は　0回か1回を表し
        //  {}を0回以上の繰り返し
        //　+を1回以上の繰り返しとする


        /*
        文
            : 式 || 変数宣言
            ;

        変数宣言
            : [型指定子3],識別子,"=",式
            ;

        式
            :条件演算子
            ;

        条件演算子　
            :足し算,{"?",条件演算子,":",条件演算子}
            ;

        足し算
            : 等しい演算子 , { "+" || "-" , 等しい演算子 }
            ;

        等しい演算子
            :項{"==",項}
            ;

        項
            :[-] ,　因子 { "*" || "/", 因子 }
            ;

        因子
            : 実数 || リスト || 組み込み関数呼び出し || グローバル変数||　ローカル変数 || ラムダ呼び出し|| ラムダ定義 ||( "(" , 式 , ")" )
            ;
        
        リスト
            : "[" , リスト中身 ,"]"
            ;
        
        リスト中身
            : 式 , { "," , 式}
            ;

        ラムダ呼び出し
            :ラムダ定義|ラムダ格納変数,"(",引数,")"
            ;

        ラムダ定義
            :"(",引数定義,")","{",式,"}"
            ;

        組み込み関数呼び出し
            :識別子,"(",引数,")"
            ;
       
        グローバル変数
            :"g_",識別子
            ;

        識別子
            :(a-z)+
            ;

        引数
            :[式 , {"," , 式}]
            ;

        引数定義
            :[識別子 , {"," , 識別子}]
            ;

        型指定子3
            :型指定子 , "::"
            ; 
           
        型指定子2
            :型種類 | "[", 型指定子 , "]"  
            ;


        型指定子
            :型指定子2 | , {"->",型指定子2}
            ;

        型種類
            :"double" | "void"|"bool"
            ;
               
        */

        static void Main(string[] args)
        {

          
            while (true)
            {

                try
                {
                    Console.Write("!(^^)!　");
                    var tokenStream = LexicalAnalyzer.Lexicalanalysis(Console.ReadLine());
                    if (tokenStream == null)
                    {
                        Console.WriteLine("( ;∀;)　token error!!");
                        continue;
                    }

                    //デバッグ用
                    /*for (int i = 0; i < tokenStream.Size; i++)
                        tokenStream[i].DebugPrint();*/


                    var func = CreateTree.CreateStatement(tokenStream);
                    if (func == null)
                    {
                        Console.WriteLine("( ;∀;)　Tree error!!");
                        continue;
                    }
                    func();
                }
                catch (Exception except) {
                    Console.WriteLine(except.Message);
                    continue;
                }

               
            }
        }
    }
}
