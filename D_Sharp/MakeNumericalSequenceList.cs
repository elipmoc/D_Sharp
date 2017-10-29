using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections;

namespace D_Sharp
{

    //数列表記リストのヘルパクラス
    static class MakeNumericalSequenceList
    {
        //無限数列リスト作成
        public static InfinityList<T> MakeInfinityList<T>(T begin,T delta)
        {
            return new InfinityList<T>(begin, delta);
        }
    }



    //無限数列リストのクラス
    class InfinityList<T>:IEnumerable<T>
    {

        static public  int Make() { return 0; }

        readonly Func<T, T> AddFunc;
        readonly T begin;
        T seek;

        public InfinityList(T begin,T delta)
        {
            this.begin = begin;
            var x = Expression.Parameter(typeof(T));
            AddFunc = 
                Expression.Lambda<Func<T,T>>( 
                    Expression.Add(x, Expression.Constant(delta))
                    ,x).Compile();


        }

        public IEnumerator<T> GetEnumerator()
        {
            seek = begin;
            while (true)
            {
                yield return seek;
                seek = AddFunc(seek);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
