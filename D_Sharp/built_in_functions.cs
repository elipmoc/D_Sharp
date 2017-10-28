using System;
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

        static public Unit Test<T>(IEnumerable<T> i)
        {
            foreach (var item in i)
                Console.WriteLine(item);
            return new Unit();
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
        static public string arrayToString(char[] t)
        {
            return new string(t);
        }

        //配列添え字アクセス
        static public T get<T>(IEnumerable<T> t,int index)
        {
            return t.ElementAt(index);
        }

        //配列要素数ゲット
        static public int getlen<T>(IEnumerable<T> t)
        {
            return t.Count();
        }

        //配列取り出し
        static public IEnumerable<T> take<T>(IEnumerable<T> t,int range)
        {
            return t.Take(range);
        }

        //配列を先頭からｎ個取り出す
        static public IEnumerable<T> drop<T>(IEnumerable<T> t, int range)
        {
            return t.Skip(range);
        }

        //配列結合
        static public IEnumerable<T> merge<T>(IEnumerable<T>a,IEnumerable<T> b)
        {
            return a.Concat(b);
        }

        //1番目を取り出し
        static public T head<T>(IEnumerable<T> t)
        {
            return t.First();
        }

        //最後を取り出し
        static public T last<T>(IEnumerable<T> t)
        {
            return t.Last();
        }

        //二番目以降を取り出し
        static public IEnumerable<T> tail<T>(IEnumerable<T> t)
        {
           return t.Skip(1);
        }

        //配列に配列を挿入
        static public IEnumerable<T> insert<T>(IEnumerable<T> a, IEnumerable<T> b,int index)
        {
            var list = b.Take(index).ToList();
            list.AddRange(a);
            list.AddRange(b.Skip(index));
            return list;
        }

        //配列表示
        static public IEnumerable<T> printlist<T>(IEnumerable<T> t)
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
        static public IEnumerable<char> printstr(IEnumerable<char> t)
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
