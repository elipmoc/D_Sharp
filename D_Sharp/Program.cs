using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.IO;

namespace D_Sharp
{

    class Hoge
    {

        public static int a() { return 1; }
    }
    class Program
    {
        //拡張BNF記法
        //　[]は　0回か1回を表し
        //  {}を0回以上の繰り返し
        //　+を1回以上の繰り返しとする


        /*
        文
            : 式 | 変数宣言
            ;

        変数宣言
            : [型指定子3],識別子,"=",式
            ;

        式
            :キャスト
            ;

        キャスト
            : ( "(" , 型種類 , ")", キャスト )| let_in
            ;

        let_in
            :("let" , 変数宣言,{ "," , 変数宣言}, "in" ,式) | 条件演算子
            ;

        条件演算子　
            :等しい演算子,["?",式,":",式]
            ;

        等しい演算子
            :足し算 , {( "==" | "<=" | ">=" | "<" | ">" | "!=" ),足し算}
            ;

        足し算
            : 項 , { "+" | "-" , 項 }
            ;
            
        項
            :[-] ,　ラムダ呼び出し { "*" | "/", ラムダ呼び出し }
            ;

        ラムダ呼び出し
            :因子,{"(",引数,")"}
            ;

        因子
            : 整数| 実数 |文字 | 文字列 |
              リスト | Netクラス静的メソッド呼び出し | 組み込み関数呼び出し | 
              グローバル変数|　ローカル変数 | ラムダ定義 | ( "(" , 式 , ")" )
            ;

        文字
            : "'" , 任意の一文字 , "'"
            ;

        文字列
            : " " ", 任意の文字列 , " " "
            ;

        リスト
            : "[" , リスト中身 ,"]"
            ;
        
        リスト中身
            : 式 , { "," , 式}
            ;

        Netクラス静的メソッド呼び出し
            :クラス名 , "." , 識別子 , "(",引数,")"
            ;


        ラムダ定義
            :"(",引数定義,")","{",式,"}"
            ;

        組み込み関数呼び出し
            :識別子,"(",引数,")"
            ;
       
        グローバル変数 とローカル変数
            :識別子
            ;

        識別子
            :("a"-"z")+ | ("A"-"Z")+ | ("0"-"9")+ | "_"
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
            :"double" | "void" | "bool" | "char" | "int" | "unit"
            ;

        クラス名
            :識別子 , { "@" , 識別子}
            ;
            
        */

        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding =Encoding.Unicode;

            Console.Clear();

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
