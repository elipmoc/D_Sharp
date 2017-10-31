using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace D_Sharp
{
    //IOモナド
    class IO<OUT>
    {

        readonly Func<OUT> action;

        public IO(Func<OUT> action)
        {
            this.action = action;
        }

        public OUT Get()
        {
            return action();
        }
    }

    class IOMakeExpr
    {
        //式をIOで包む
        static public Expression Wrap(Expression expr)
        {
            var ioConstructorInfo=
                typeof(IO<>).MakeGenericType(expr.Type).
                    GetConstructor(new Type[] { Expression.GetDelegateType(expr.Type)}) ;
            return Expression.New(ioConstructorInfo,Expression.Lambda(expr));
        }

        //IO型を実行する
        static public Expression DoIO(Expression expr)
        {
            if (expr.Type.IsGenericType &&
               expr.Type.GetGenericTypeDefinition() == typeof(IO<>))
            {
                var methodInfo = expr.Type.GetMethod("Get");
                return Expression.Call(expr, methodInfo);
            }
            return null;
        }


    }
}
