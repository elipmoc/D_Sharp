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
                if((body=CreateVariableDeclaration(tokenst))==null)
                    return null;
            if (tokenst.NowIndex < tokenst.Size)
                return null;
            return Expression.Lambda<Action>(body).Compile();
        }

        //変数宣言の解析
        static Expression CreateVariableDeclaration(TokenStream tokenst)
        {
            var checkPoint=tokenst.NowIndex;
            string variableName;
            if (tokenst.Get().TokenType == TokenType.Identifier)
            {
                variableName = tokenst.Get().Str;
                tokenst.Next();
                if (tokenst.Get().Str == "=")
                {
                    tokenst.Next();
                    var expr = CreateSiki(tokenst);
                    if (expr != null)
                    {
                        if (VariableTable.Find(variableName) == null)
                        {
                            var genericFunc = typeof(VariableTable).GetMethod("Register");
                            return Expression.Call(
                                genericFunc.MakeGenericMethod(new[] {expr.Type}),
                                Expression.Constant(variableName),
                                expr
                            );

                        }
                    }

                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //式
        static Expression CreateSiki(TokenStream tokenst)
        {
            var checkPoint=tokenst.NowIndex;
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
                        tokenst.Rollback(checkPoint);
                        return null;
                    }
                    left =
                         CreateBinaryOperator(left, right, op);
                }
                return left;
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //項
        static Expression CreateKou(TokenStream tokenst)
        {
            var checkPoint=tokenst.NowIndex;
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
                        tokenst.Rollback(checkPoint);
                        return null;
                    }
                    left =
                         CreateBinaryOperator(left, right, op);
                }
                return left;
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //因子
        static Expression CreateInsi(TokenStream tokenst)
        {
            var checkPoint = tokenst.NowIndex;
            Expression expr;
            //実数
            if (tokenst.Get().TokenType == TokenType.Double)
            {
                var constant_double = Expression.Constant(tokenst.Get().GetDouble());
                tokenst.Next();
                return constant_double;
            }
            //ラムダ呼び出し
            else if ((expr = CreateLambdaCall(tokenst)) != null)
            {
                return expr;
            }
            //組み込み関数呼び出し
            else if ((expr = CreateFunctionCall(tokenst)) != null)
            {
                return expr;
            }
            //変数
            else if (
                tokenst.Get().TokenType==TokenType.Identifier &&
                (expr=VariableTable.Find(tokenst.Get().Str))!=null
                )
            {
                tokenst.Next();
                return expr;
            }
            //ラムダ定義
            else if ((expr=CreateLambdaDefinition(tokenst))!=null)
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
            tokenst.Rollback(checkPoint);
            return null;
        }

        //組み込み関数呼び出し
        static Expression CreateFunctionCall(TokenStream tokenst)
        {
            var checkPoint=tokenst.NowIndex;
            if (tokenst.Get().TokenType == TokenType.Identifier)
            {
                string funcName = tokenst.Get().Str;
                tokenst.Next();
                if (tokenst.NowIndex<tokenst.Size && tokenst.Get().Str == "(")
                {
                    tokenst.Next();
                    List<Expression> args;
                    if ((args=CreateArgs(tokenst)) != null)
                    {
                        if (tokenst.Get().Str == ")")
                        {
                            
                            var methodInfo=typeof(builti_in_functions).GetMethod(funcName, args.Select(x => x.Type).ToArray());
                            if (methodInfo != null)
                            {
                                tokenst.Next();
                               return Expression.Call(methodInfo, args);
                            }
                        }
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //関数の引数
        static List<Expression> CreateArgs(TokenStream tokenst)
        {
           
            var checkPoint=tokenst.NowIndex;
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
                        tokenst.Rollback(checkPoint);
                        return null;
                    }
                    args.Add(expr);
                }
                return args;
            }
            else
                return new List<Expression>();
        }

        //ラムダ呼び出し
        static Expression CreateLambdaCall(TokenStream tokenst)
        {
            var checkPoint=tokenst.NowIndex;
            //直接書かれたラムダ
            var lambdadef = CreateLambdaDefinition(tokenst);
            if (lambdadef != null)
            {
                if (tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == "(")
                {
                    tokenst.Next();
                    if (tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == ")")
                    {
                        tokenst.Next();
                        return Expression.Invoke(lambdadef);
                    }
                }
            }
            else
            {

                //ラムダが変数に格納されてた場合
                Expression expr;
                if (tokenst.Get().TokenType == TokenType.Identifier &&
                   (expr = VariableTable.Find(tokenst.Get().Str)) != null)
                {
                    tokenst.Next();
                    if (tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == "(")
                    {
                        tokenst.Next();
                        if (tokenst.Get().Str == ")")
                        {
                            tokenst.Next();
                            return Expression.Invoke(expr);
                        }
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //ラムダ定義
        static Expression CreateLambdaDefinition(TokenStream tokenst)
        {
            var checkPoint=tokenst.NowIndex;
            if (tokenst.Get().Str=="(")
            {
                tokenst.Next();
                if (tokenst.Get().Str==")")
                {
                    tokenst.Next();
                    if (tokenst.Get().Str == "{")
                    {
                        tokenst.Next();
                        var body = CreateSiki(tokenst);
                        if (body!=null&&tokenst.Get().Str=="}")
                        {
                            tokenst.Next();
                            return Expression.Lambda(body);
                        }
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

    }
}
