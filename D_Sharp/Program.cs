using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.IO;


namespace D_Sharp
{
    namespace test
    {
        class C
        {
            public void Do() { Console.WriteLine("aaaa"); }
        }

        class B
        {
            public C c = new C();
            public C GetC() { return c; }
        }

        class A
        {

            public B b = new B();
            public B GetB() { return b; }
        }
    }

    class Program
    {
        //拡張BNF記法
        //　[]は　0回か1回を表し
        //  {}を0回以上の繰り返し
        //　+を1回以上の繰り返しとする


        /*
        文
            : import | 式 | 変数宣言
            ;

        変数宣言
            : [型指定子3],識別子,"=",式
            ;

        import
            : "import" ,文字列
            ;

        式
            :do構文
            ;

        do構文
            : Netクラスnew | ( "do" , ( 変数束縛 | 式 )+ )
            ;

        変数束縛 
            :識別子 , "<-" , 式 
            ; 

        Netクラスnew
            :( "new" , クラス名 , "(",引数,")" ) | キャスト
            ;

        キャスト
            : ( "(" , 型種類 , ")", キャスト )| let_in
            ;

        let_in
            :("let" , 変数宣言,{ "," , 変数宣言}, "in" ,式) | 条件演算子
            ;

        条件演算子　
            :バインド,["?",式,":",式]
            ;

        バインド
            :等しい演算子 { ">>=" 等しい演算子} 
            ;

        等しい演算子
            :足し算 , {( "==" | "<=" | ">=" | "<" | ">" | "!=" ),足し算}
            ;

        足し算
            : 項 , { "+" | "-" , 項 }
            ;
            
        項
            :[-] ,　Netクラスのアクセス { "*" | "/", Netクラスのアクセス }
            ;

        Netクラスのアクセス
            :ラムダ呼び出し , { ".", Netクラスメンバメソッド呼び出し | Netクラスプロパティメソッド呼び出し }
            ;

        Netクラスメンバメソッド呼び出し
            : 識別子 , "(",引数,")"
            ;

        Netクラスプロパティ取得
            : 識別子 , {"=" , 式}
            ;

        ラムダ呼び出し
            :因子,{"(",引数,")"}
            ;

        因子
            : 整数| 実数 |文字 | 文字列 |
              リスト | Netクラス静的メソッド呼び出し | 組み込み関数呼び出し |Netクラス静的プロパティ呼び出し |
              グローバル変数|　ローカル変数 | ラムダ定義 | ( "(" , 式 , ")" )
            ;

        文字
            : "'" , 任意の一文字 , "'"
            ;

        文字列
            : " " ", 任意の文字列 , " " "
            ;

        リスト
            : "[" , 数列表記リスト | リスト中身 ,"]"
            ;
        
        リスト中身
            : 式 , { "," , 式}
            ;
        
        数列表記リスト
            :式, [ "," , 式]　".." , [式]
            ;

        Netクラス静的メソッド呼び出し
            :クラス名 , "." , 識別子 , "(",引数,")"
            ;
        
        Netクラス静的プロパティ呼び出し
            :クラス名 , "." , 識別子
            ;


        ラムダ定義
            :[型指定子3],"(",引数定義,")","{",式,"}"
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
            :( (型種類|クラス名|IO型) | "[", 型指定子 , "]" ) ,{ "[" , "]" }
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

        IO型
            : "IO" , "<" , 型指定子 , ">"
            ;

            Net関数のオーバーロード選択法則
            （優先度順）

            0:完全一致
            1:ジェネリック完全一致
            2:暗黙キャスト可能一致
            3:アップキャスト可能一致
            4：ジェネリックアップキャスト可能一致

        */

        static void Main(string[] args)
        {
           Action<int> gg = System.Console.WriteLine;
            var a=new System.Windows.Forms.Form();
            var label = new System.Windows.Forms.Label();
            label.Text = "HelloWorld";
            label.Size = new System.Drawing.Size(170,60);
            label.Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold);
            a.Controls.Add(label);
           //System.Windows.Forms.Application.Run(a);
            //import "System.Windows.Forms" "Version = 4.0.0.0, Culture = neutral, PublicKeyToken = b77a5c561934e089"
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
