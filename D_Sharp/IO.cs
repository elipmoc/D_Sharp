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

    class IOWrapExpr
    {
        static public Expression Wrap(Expression expr)
        {
            var ioConstructorInfo=
                typeof(IO<>).MakeGenericType(expr.Type).
                    GetConstructor(new Type[] { Expression.GetDelegateType(expr.Type)}) ;
            return Expression.New(ioConstructorInfo,Expression.Lambda(expr));
        }
    }
}
