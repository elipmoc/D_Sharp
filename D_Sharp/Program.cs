﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            : 識別子,"=",式
            ;
        式
            : 項 , { "+" || "-" , 項 }
            ;

        項
            :[-] ,　因子 { "*" || "/", 因子 }
            ;

        因子
            : 実数 || 組み込み関数呼び出し || 変数 || ラムダ呼び出し|| ラムダ定義 ||( "(" , 式 , ")" )
            ;

        ラムダ呼び出し
            :ラムダ定義,"(",")"
            ;

        ラムダ定義
            :"(",")","{",式,"}"
            ;

        組み込み関数呼び出し
            :識別子,"(",引数,")"
            ;
       
        変数
            :識別子
            ;

        識別子
            :(a-z)+
            ;

        引数
            :[式 , {"," , 式}]
            ;
               
        */

        static void Main(string[] args)
        {
            while (true)
            {
                var tokenStream = LexicalAnalyzer.Lexicalanalysis(Console.ReadLine());
                if (tokenStream == null)
                {
                    Console.WriteLine("token error!!");
                    continue;
                }
                
                /*//デバッグ用
                for (int i = 0; i < tokenStream.Size; i++)
                    tokenStream[i].DebugPrint();*/


                var func=CreateTree.CreateStatement(tokenStream);
                if (func == null)
                {
                    Console.WriteLine("Tree error!!");
                    continue;
                }

                func();
            }
        }
    }
}
