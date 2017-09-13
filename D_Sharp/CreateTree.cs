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

        //二項演算子の生成
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

        //文
        static public Action CreateStatement(TokenStream tokenst)
        {
            var body = CreateSiki(tokenst);
            if (body == null)
                return null;
            return Expression.Lambda<Action>(body).Compile();
        }

        //式
        static Expression CreateSiki(TokenStream tokenst)
        {
            tokenst.SetCheckPoint();
            Expression left;
            if ((left = CreateKou(tokenst)) != null)
            {
                Expression right;
                string op;
                while (tokenst.NowIndex < tokenst.Size && (tokenst.Get().Str == "+" || tokenst.Get().Str == "-"))
                {
                    op = tokenst.Get().Str;
                    tokenst.Next();
                    if ((right = CreateKou(tokenst)) == null)
                    {
                        tokenst.Rollback();
                        return null;
                    }
                    left =
                         CreateBinaryOperator(left, right, op);
                }
                return left;
            }
            tokenst.Rollback();
            return null;
        }

        //項
        static Expression CreateKou(TokenStream tokenst)
        {
            tokenst.SetCheckPoint();
            Expression left;
            if ((left = CreateInsi(tokenst)) != null)
            {
                Expression right;
                string op;
                while (tokenst.NowIndex < tokenst.Size && (tokenst.Get().Str == "*" || tokenst.Get().Str == "/"))
                {
                    op = tokenst.Get().Str;
                    tokenst.Next();
                    if ((right = CreateInsi(tokenst)) == null)
                    {
                        tokenst.Rollback();
                        return null;
                    }
                    left =
                         CreateBinaryOperator(left, right, op);
                }
                return left;
            }
            tokenst.Rollback();
            return null;
        }

        //因子
        static Expression CreateInsi(TokenStream tokenst)
        {
            tokenst.SetCheckPoint();
            Expression expr;
            //実数
            if (tokenst.Get().TokenType == TokenType.Double)
            {
                var constant_double = Expression.Constant(tokenst.Get().GetDouble());
                tokenst.Next();
                return constant_double;
            }
            //組み込み関数呼び出し
            else if ((expr=CreateFunctionCall(tokenst))!=null)
            {
                return expr;
            }
            // ( 式 ) 
            else if (tokenst.Get().Str == "(")
            {
                tokenst.Next();
                if ((expr = CreateSiki(tokenst)) != null)
                {
                    if (tokenst.Get().Str == ")")
                    {
                        tokenst.Next();
                        return expr;
                    }
                }
            }
            tokenst.Rollback();
            return null;
        }

        //組み込み関数呼び出し
        static Expression CreateFunctionCall(TokenStream tokenst)
        {
            tokenst.SetCheckPoint();
            if (tokenst.Get().TokenType == TokenType.Identifier)
            {
                string funcName = tokenst.Get().Str;
                tokenst.Next();
                if (tokenst.Get().Str == "(")
                {
                    tokenst.Next();
                    List<Expression> args;
                    if ((args=CreateArgs(tokenst)) != null)
                    {
                        if (tokenst.Get().Str == ")")
                        {
                            var methodInfo=typeof(builti_in_functions).GetMethod(funcName);
                            if (methodInfo != null)
                            {
                               return Expression.Call(methodInfo, args);
                            }
                        }
                    }
                }
            }
            tokenst.Rollback();
            return null;
        }

        //関数の引数
        static List<Expression> CreateArgs(TokenStream tokenst)
        {
           
            tokenst.SetCheckPoint();
            Expression expr;
            if ((expr = CreateSiki(tokenst)) != null)
            {
                List<Expression> args=new List<Expression>();
                args.Add(expr);
                while (tokenst.Get().Str==",")
                {
                    tokenst.Next();
                    expr = CreateSiki(tokenst);
                    if (expr == null)
                    {
                        tokenst.Rollback();
                        return null;
                    }
                    args.Add(expr);
                }
                return args;
            }
            else
                return new List<Expression>();
        }

    }
}
