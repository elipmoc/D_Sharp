using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace D_Sharp
{
    static class CreateTree
    {

        static Expression CreateBinaryOperator(Expression left,Expression right,string str)
        {
            switch (str)
            {
                case "+":
                   return Expression.Add(left, right);
                case "-":
                    return Expression.Subtract(left, right);
                case "*":
                    return Expression.Multiply(left, right);
                case "/":
                    return Expression.Divide(left, right);
            }
            return null;
        }

        static public Func<double> CreateStatement(TokenStream tokenst)
        {
            var body = CreateSiki(tokenst);
            if (body == null)
                return null;
            return Expression.Lambda<Func<double>>(body).Compile();
        }

        static Expression CreateSiki(TokenStream tokenst)
        {
            tokenst.SetCheckPoint();
            Expression left;
            if ((left=CreateInsi(tokenst)) != null)
            {
                string op = tokenst.Get().Str;
                if (op== "+" ||op=="-")
                {
                    Expression right;
                    tokenst.Next();
                    if ((right = CreateInsi(tokenst)) != null)
                    {
                        Expression body =
                            CreateBinaryOperator(left,right,op);
                        while (tokenst.NowIndex<tokenst.Size && (tokenst.Get().Str=="+"||tokenst.Get().Str=="-"))
                        {
                            op = tokenst.Get().Str;
                            tokenst.Next();
                            if ((right = CreateInsi(tokenst)) == null)
                            {
                                tokenst.Rollback();
                                return null;
                            }
                            body =
                                 CreateBinaryOperator(body,right,op);
                        }
                        return body;
                    }
                }
            }
            tokenst.Rollback();
            return null;
        }

        static Expression CreateInsi(TokenStream tokenst)
        {
            tokenst.SetCheckPoint();
            if (tokenst.Get().TokenType == TokenType.Double)
            {
                var constant_double = Expression.Constant(tokenst.Get().GetDouble());
                tokenst.Next();
                return constant_double;
            }
            tokenst.Rollback();
            return null;
        }
    }
}
