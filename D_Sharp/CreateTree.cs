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
                case "==":
                    return Expression.Equal(left, right);
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

        //グローバル変数宣言の解析
        static Expression CreateVariableDeclaration(TokenStream tokenst)
        {
            var checkPoint = tokenst.NowIndex;
            Type type;
            if ((type = CreateTypeSpecifier(tokenst)) == null)
            {
                tokenst.Rollback(checkPoint);
                return null;
            }
            string variableName;
            if (tokenst.Get().TokenType == TokenType.GlobalVariable)
            {
                variableName = tokenst.Get().Str;
                if (VariableTable.Find(variableName) == false)
                {
                    VariableTable.Register(variableName, type);
                    tokenst.Next();
                    if (tokenst.Get().Str == "=")
                    {
                        tokenst.Next();
                        var expr = CreateSiki(tokenst);
                        if (expr != null)
                        {
                            var methodInfo = typeof(VariableTable).GetMethod("SetValue").MakeGenericMethod(expr.Type);
                            return Expression.Call(methodInfo,Expression.Constant(variableName),expr);
                        }

                    }
                    VariableTable.Remove(variableName);
                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //式
        static Expression CreateSiki(TokenStream tokenst)
        {
            Expression expr;
            if ((expr = CreateJyoukenEnzan(tokenst)) != null)
                return expr;
            return null;
        }

        //条件演算子
        static Expression CreateJyoukenEnzan(TokenStream tokenst)
        {
            var checkPoint = tokenst.NowIndex;
            Expression expr;
            if ((expr = CreateTasizan(tokenst)) != null)
            {
                if(tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == "?")
                {
                    Expression left, right;
                    tokenst.Next();
                    if ((left = CreateJyoukenEnzan(tokenst)) != null)
                    {
                        if (tokenst.Get().Str == ":")
                        {
                            tokenst.Next();
                            if ((right = CreateJyoukenEnzan(tokenst)) != null)
                            {
                                return Expression.Condition(expr, left, right);
                            }
                        }
                    };
                    tokenst.Rollback(checkPoint);
                    return null;
                }
                return expr;
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //足し算
        static Expression CreateTasizan(TokenStream tokenst)
        {
            var checkPoint=tokenst.NowIndex;
            Expression left;
            if ((left = CreateHitosi(tokenst)) != null)
            {
                Expression right;
                string op;
                while (tokenst.NowIndex < tokenst.Size && (tokenst.Get().Str == "+" || tokenst.Get().Str == "-"))
                {
                    op = tokenst.Get().Str;
                    tokenst.Next();
                    if ((right = CreateHitosi(tokenst)) == null)
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

        //等しい演算子
        static Expression CreateHitosi(TokenStream tokenst)
        {
            var checkPoint = tokenst.NowIndex;
            Expression left;
            if ((left = CreateKou(tokenst)) != null)
            {
                Expression right;
                string op;
                while (tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == "==")
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
            bool signedFlag = false;
            Expression left;
            if (tokenst.Get().Str == "-")
            {
                tokenst.Next();
                signedFlag = true;
            }
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
                if(signedFlag)
                    return Expression.Multiply(Expression.Convert(Expression.Constant(-1),left.Type),left);
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
            //グローバル変数
            else if (
                tokenst.Get().TokenType==TokenType.GlobalVariable &&
                VariableTable.Find(tokenst.Get().Str)!=false)
                
            {
                var type = VariableTable.GetType(tokenst.Get().Str);
                var methodInfo = typeof(VariableTable).GetMethod("Get").MakeGenericMethod(type);
                expr = Expression.Call(methodInfo,Expression.Constant(tokenst.Get().Str));
                tokenst.Next();
                return expr;
            }
            //ローカル変数
            else if ((expr=CreateLocalVariableExpr(tokenst))!=null) {
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

        //引数
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
            var checkPoint = tokenst.NowIndex;
            //直接書かれたラムダ
            var lambdadef = CreateLambdaDefinition(tokenst);
            if (lambdadef != null) ;
            //グローバル変数のラムダ
            else if (tokenst.Get().TokenType == TokenType.GlobalVariable &&
                  (VariableTable.Find(tokenst.Get().Str)) ==true)
            {
                var type = VariableTable.GetType(tokenst.Get().Str);
                var methodInfo = typeof(VariableTable).GetMethod("Get").MakeGenericMethod(type);
                lambdadef = Expression.Call(methodInfo,Expression.Constant( tokenst.Get().Str));
                tokenst.Next();
            }
            //ローカル変数のラムダ
            else if ((lambdadef = CreateLocalVariableExpr(tokenst)) != null)
            {
            }
            else
            {
                tokenst.Rollback(checkPoint);
                return null;
            }
            
            //引数確認
            if (tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == "(")
            {
                tokenst.Next();
                List<Expression> args;
                if ((args=CreateArgs(tokenst))!= null)
                {
                    if (tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == ")")
                    {
                        tokenst.Next();
                        return Expression.Invoke(lambdadef,args);
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
            LocalVariableTable.In();
            if (tokenst.Get().Str=="(")
            {
                tokenst.Next();
                List<ParameterExpression> argsDecl;
                if ((argsDecl = CreateArgsDeclaration(tokenst)) != null)
                {
                    if (tokenst.Get().Str == ")")
                    {
                        tokenst.Next();
                        if (tokenst.Get().Str == "{")
                        {
                            tokenst.Next();
                            var body = CreateSiki(tokenst);
                            if (body != null && tokenst.Get().Str == "}")
                            {
                                tokenst.Next();
                                LocalVariableTable.Out();
                                return Expression.Lambda(body,argsDecl);
                            }
                        }
                    }
                }
            }
            LocalVariableTable.Out();
            tokenst.Rollback(checkPoint);
            return null;
        }

        //引数宣言
        static List<ParameterExpression> CreateArgsDeclaration(TokenStream tokenst)
        {
            var checkPoint = tokenst.NowIndex;
            if (tokenst.Get().TokenType==TokenType.Identifier)
            {
                List<ParameterExpression> args = new List<ParameterExpression>();
                var parameter= Expression.Parameter(typeof(double), tokenst.Get().Str);
                LocalVariableTable.Register(tokenst.Get().Str, parameter);
                args.Add(parameter);
                tokenst.Next();
                while (tokenst.Get().Str == ",")
                {
                    tokenst.Next();
                    if (tokenst.Get().TokenType == TokenType.Identifier)
                    {
                        parameter = Expression.Parameter(typeof(double), tokenst.Get().Str);
                        LocalVariableTable.Register(tokenst.Get().Str, parameter);
                        args.Add(parameter);
                        tokenst.Next();
                    }
                    else
                    {
                        tokenst.Rollback(checkPoint);
                    }
                }
                return args;
            }
            else
                return new List<ParameterExpression>();
        }

        //ローカル変数の取得
        static Expression CreateLocalVariableExpr(TokenStream tokenst)
        {
            var checkPoint= tokenst.NowIndex;
            if (tokenst.Get().TokenType == TokenType.Identifier)
            {
                var expr=LocalVariableTable.Find(tokenst.Get().Str);
                if (expr != null)
                {
                    tokenst.Next();
                    return expr;
                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //型指定子
        static Type CreateTypeSpecifier(TokenStream tokenst)
        {
            var checkPoint=tokenst.NowIndex;
            Type type;
            if ((type=CreateType(tokenst.Get().Str)) != null)
            {
                tokenst.Next();
                if (tokenst.Get().Str == "::")
                {
                    tokenst.Next();
                    return type;
                }
                if (tokenst.Get().Str == "->")
                {
                    List<Type> types = new List<Type>();
                    types.Add(type);
                    while (tokenst.Get().Str == "->")
                    {
                        if (types[types.Count - 1] == typeof(void))
                            types.RemoveAt(types.Count - 1);
                        tokenst.Next();
                        if ((type = CreateType(tokenst.Get().Str)) != null)
                        {
                            types.Add(type);
                            tokenst.Next();
                        }
                        else
                        {
                            tokenst.Rollback(checkPoint);
                            return null;
                        }
                    }
                    if (tokenst.Get().Str == "::")
                    {
                        tokenst.Next();
                        return Expression.GetDelegateType(types.ToArray());
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        static Type CreateType(string typeName)
        {
            switch (typeName)
            {
                case"double":
                    return typeof(double);
                case "bool":
                    return typeof(bool);
                case "void":
                    return typeof(void);
            }
            return null;
        }

    }
}
