using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.IO;

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
            :項{( "==" || "<=" || ">=" || "<" || ">" || "!=" ),項}
            ;

        項
            :[-] ,　ラムダ呼び出し { "*" || "/", ラムダ呼び出し }
            ;

        ラムダ呼び出し
            :因子,{"(",引数,")"}
            ;

        因子
            : 実数 || リスト || 組み込み関数呼び出し || グローバル変数||　ローカル変数 || ラムダ定義 ||( "(" , 式 , ")" )
            ;
        
        リスト
            : "[" , リスト中身 ,"]"
            ;
        
        リスト中身
            : 式 , { "," , 式}
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
            :( 型種類 | "[", 型指定子 , "]" ) ,{ "[" , "]" }
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

            //ファイルからプログラム読み込み
            StreamReader sr =
                new StreamReader("main.ds", Encoding.GetEncoding("Shift_JIS"));

            string line,buf;
            while (true)
            {
                line = "";
                while ((buf = sr.ReadLine()) != null)
                {
                    line += buf;
                    Console.Write("!(^^)!　");
                    Console.WriteLine(buf);

                    if (line.Count()>0&&line[line.Count() - 1] == ';')
                    {

                        break;
                    }
                }
                if (buf == null) break;
                Interpreter.ReadLine(line);
            }
            sr.Close();
            string str;
            while (true)
            {
                str = "";
                while (true)
                {
                    Console.Write("!(^^)!　");
                    str += Console.ReadLine();
                    if (str.Count() > 0 && str[str.Count() - 1] == ';')
                        break;
                }
                Interpreter.ReadLine(str);
               

               
            }
        }
    }
}
