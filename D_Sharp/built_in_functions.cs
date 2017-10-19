﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace D_Sharp
{

    //voidのようなもの
    struct Unit {
    }

    //組み込み関数
    static class built_in_functions
    {
        static public char[] scanf()
        {

           return Console.ReadLine().ToArray();
        }

        static public double random()
        {
   

            return new System.Random().NextDouble();
        }

        static public T print<T>(T t)
        {
            Console.WriteLine("( ﾟДﾟ)　" + t);
            return t;
        }

        static public T printtype<T>(T t)
        {
            print(typeof(T));
            return t;
        }

        static public Unit tounit<T>(T t)
        {
            return new Unit();
        }

        static public char[] tostring<T>(T t)
        {
            
            return t.ToString().ToArray();
        }

        //配列添え字アクセス
        static public string arrayToString<T>(char[] t)
        {
            return new string(t);
        }

        //配列添え字アクセス
        static public T get<T>(T[] t,int index)
        {
            return t[index];
        }

        //配列要素数ゲット
        static public int getlen<T>(T[] t)
        {
            return t.Count();
        }

        //配列取り出し
        static public T[] take<T>(T[] t,int range)
        {
            return t.Take(range).ToArray();
        }

        //配列を先頭からｎ個取り出す
        static public T[] drop<T>(T[] t, int range)
        {
            return t.Skip(range).ToArray();
        }

        //配列結合
        static public T[] merge<T>(T[]a,T[] b)
        {
            var list = a.ToList();
            list.AddRange(b);
            return list.ToArray();
        }

        //1番目を取り出し
        static public T head<T>(T[] t)
        {
            return t[0];
        }

        //最後を取り出し
        static public T last<T>(T[] t)
        {
            return t[t.Count()-1];
        }

        //二番目以降を取り出し
        static public T[] tail<T>(T[] t)
        {
           return t.Skip(1).ToArray();
        }

        //配列に配列を挿入
        static public T[] insert<T>(T[] a, T[] b,int index)
        {
            var list = b.Take(index).ToList();
            list.AddRange(a);
            list.AddRange(b.Skip(index));
            return list.ToArray();
        }

        //配列表示
        static public T[] printlist<T>(T[] t)
        {
            string str="[";
            foreach(var item in t)
            {
                str+=item+",";
            }
            str=(str.Length==1?str:str.Remove(str.Length - 1,1))+"]";
            print(str);
            return t;
        }

        //文字列表示
        static public T[] printstr<T>(T[] t)
        {
            string str="";
            foreach (var item in t)
            {
                str += item;
            }
            print(str);
            return t;
        }
    }
}