﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace D_Sharp
{
    //組み込み関数
    static class builti_in_functions
    {

        static public T print<T>(T t)
        {
            Console.WriteLine("( ﾟДﾟ)　" + t);
            return t;
        }

        //配列添え字アクセス
        static public T get<T>(T[] t,double index)
        {
            return t[(int)index];
        }

        //配列要素数ゲット
        static public double getlen<T>(T[] t)
        {
            return t.Count();
        }

        //配列取り出し
        static public T[] take<T>(T[] t,double range)
        {
            return t.Take((int)range).ToArray();
        }
    }
}
