using System;
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
       static public double print(double d) {
            Console.WriteLine("( ﾟДﾟ)　" + d);
            return d;
        }
        static public bool print(bool d)
        {
            Console.WriteLine("( ﾟДﾟ)　"+d);
            return d;
        }
        static public Delegate print(Delegate lambda)
        {
            Console.WriteLine("( ﾟДﾟ)　" + lambda);
            return lambda;
        }
    }
}
