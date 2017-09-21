using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

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
                case "%":
                    return Expression.Modulo(left, right);
                case "==":
                    return Expression.Equal(left, right);
                case "!=":
                    return Expression.NotEqual(left, right);
                case "<=":
                    return Expression.LessThanOrEqual(left, right);
                case ">=":
                    return Expression.GreaterThanOrEqual(left,right);
                case "<":
                    return Expression.LessThan(left, right);
                case ">":
                    return Expression.GreaterThan(left, right);
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
            Type[] types;
            /*   if ((types = CreateTypeSpecifier3(tokenst)) == null)
               {
                   tokenst.Rollback(checkPoint);
                   return null;
               }*/
            types = CreateTypeSpecifier3(tokenst);
            string variableName;
            if (tokenst.Get().TokenType == TokenType.GlobalVariable)
            {
                variableName = tokenst.Get().Str;
                if (VariableTable.Find(variableName) == false && types != null)
                {
                    if (types.Count() == 1)
                        VariableTable.Register(variableName, types[0]);
                    else if (types[0] == typeof(void))
                        VariableTable.Register(variableName, Expression.GetDelegateType(types.Skip<Type>(1).ToArray()));
                    else
                        VariableTable.Register(variableName, Expression.GetDelegateType(types));
                }
                tokenst.Next();
                if (tokenst.Get().Str == "=")
                {
                    tokenst.Next();
                    var expr = CreateSiki(tokenst, types);
                    if (expr != null)
                    {
                        if (VariableTable.Find(variableName) == false && types == null)
                        {
                            VariableTable.Register(variableName, expr.Type);
                        }
                        var methodInfo = typeof(VariableTable).GetMethod("SetValue").MakeGenericMethod(expr.Type);
                        return Expression.Call(methodInfo, Expression.Constant(variableName), expr);
                    }

                }
                VariableTable.Remove(variableName);

            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //式
        static Expression CreateSiki(TokenStream tokenst, Type[] argTypes=null)
        {
            Expression expr;
            if ((expr = CreateJyoukenEnzan(tokenst,argTypes)) != null)
                return expr;
            return null;
        }

        //条件演算子
        static Expression CreateJyoukenEnzan(TokenStream tokenst, Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            Expression expr;
            if ((expr = CreateTasizan(tokenst,argTypes)) != null)
            {
                if(tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == "?")
                {
                    Expression left, right;
                    tokenst.Next();
                    if ((left = CreateJyoukenEnzan(tokenst,argTypes)) != null)
                    {
                        if (tokenst.Get().Str == ":")
                        {
                            tokenst.Next();
                            if ((right = CreateJyoukenEnzan(tokenst,argTypes)) != null)
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
        static Expression CreateTasizan(TokenStream tokenst, Type[] argTypes)
        {
            var checkPoint=tokenst.NowIndex;
            Expression left;
            if ((left = CreateHitosi(tokenst,argTypes)) != null)
            {
                Expression right;
                string op;
                while (tokenst.NowIndex < tokenst.Size && (tokenst.Get().Str == "+" || tokenst.Get().Str == "-"))
                {
                    op = tokenst.Get().Str;
                    tokenst.Next();
                    if ((right = CreateHitosi(tokenst,argTypes)) == null)
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
        static Expression CreateHitosi(TokenStream tokenst, Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            Expression left;
            if ((left = CreateKou(tokenst,argTypes)) != null)
            {
                Expression right;
                string op;
                while (tokenst.NowIndex < tokenst.Size &&( 
                    tokenst.Get().Str == "=="|| tokenst.Get().Str == "<=" ||
                    tokenst.Get().Str == ">=" || tokenst.Get().Str == "<" ||
                    tokenst.Get().Str == ">"||tokenst.Get().Str == "!=" 
                    ))
                {
                    op = tokenst.Get().Str;
                    tokenst.Next();
                    if ((right = CreateKou(tokenst,argTypes)) == null)
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
        static Expression CreateKou(TokenStream tokenst, Type[] argTypes)
        {
            var checkPoint=tokenst.NowIndex;
            bool signedFlag = false;
            Expression left;
            if (tokenst.Get().Str == "-")
            {
                tokenst.Next();
                signedFlag = true;
            }
            if ((left = CreateLambdaCall(tokenst,argTypes)) != null)
            {
                Expression right;
                string op;
                while (tokenst.NowIndex < tokenst.Size && (tokenst.Get().Str == "*" || tokenst.Get().Str == "/" || tokenst.Get().Str=="%"))
                {
                    op = tokenst.Get().Str;
                    tokenst.Next();
                    if ((right = CreateLambdaCall(tokenst,argTypes)) == null)
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
        static Expression CreateInsi(TokenStream tokenst,Type[] argTypes)
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
            //リスト
            else if ((expr=CreateList(tokenst,argTypes))!=null)
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
            else if ((expr=CreateLambdaDefinition(tokenst,argTypes))!=null)
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

        //リスト
        static Expression CreateList(TokenStream tokenst,Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            if (tokenst.Get().Str == "[")
            {
                tokenst.Next();
                var array = CreateListNakami(tokenst,argTypes);
                if (array != null)
                {
                    if (tokenst.Get().Str == "]")
                    {
                        tokenst.Next();
                        return array;
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return null;

        }

        //リスト中身
        static Expression CreateListNakami(TokenStream tokenst,Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            Expression expr;
            List<Expression> exprList=new List<Expression>();
            if ((expr = CreateSiki(tokenst,new[] { argTypes!=null?argTypes[0].GetElementType():null })) != null)
            {
                var type = expr.Type;
                exprList.Add(expr);
                while (tokenst.Get().Str == ",")
                {
                    tokenst.Next();
                    if ((expr = CreateSiki(tokenst, new[] { type })) != null)
                    {
                        exprList.Add(expr);
                    }
                    else
                    {
                        tokenst.Rollback(checkPoint);
                        return null;
                    }

                }
                return Expression.NewArrayInit(type, exprList.ToArray());
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
                    if ((args=CreateArgs(tokenst,null)) != null)
                    {
                        if (tokenst.Get().Str == ")")
                        {
                            MethodInfo methodInfo;
                            MethodInfo makeGeneric;
                            methodInfo = typeof(builti_in_functions).GetMethod(funcName);
                            if (funcName == "get"||funcName=="getlen"||
                                funcName=="take"|| funcName == "merge"||
                                funcName=="printlist" ||funcName=="tail"||
                                funcName=="head" || funcName=="last"|| funcName=="drop")
                            {
                                makeGeneric = methodInfo.MakeGenericMethod(args[0].Type.GetElementType());
                            }
                            else
                            {
                                makeGeneric = methodInfo.MakeGenericMethod(args.Select(x => x.Type).ToArray());
                            }
                            if (makeGeneric != null)
                            {
                                tokenst.Next();
                               return Expression.Call(makeGeneric, args);
                            }
                        }
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //引数
        static List<Expression> CreateArgs(TokenStream tokenst,Type[] argTypes)
        {
           
            var checkPoint=tokenst.NowIndex;
            int argsIndex=0;
            Expression expr;
            if ((expr = CreateSiki(tokenst,argTypes==null?null:new[] { argTypes[argsIndex++] })) != null)
            {
                List<Expression> args=new List<Expression>();
                args.Add(expr);
                while (tokenst.Get().Str==",")
                {
                    tokenst.Next();
                    expr = CreateSiki(tokenst,argTypes == null ? null : new[] { argTypes[argsIndex++] });
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
        static Expression CreateLambdaCall(TokenStream tokenst,Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;

            //直接書かれたラムダ
            /*   var lambdadef = CreateLambdaDefinition(tokenst,argTypes);
               if (lambdadef != null) ;*/

            Expression expr;

            /*
                        //グローバル変数のラムダ
                        if (tokenst.Get().TokenType == TokenType.GlobalVariable &&
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
                        }*/
            if ((expr = CreateInsi(tokenst, argTypes)) != null){
                
            }
            else
            {
                tokenst.Rollback(checkPoint);
                return null;
            }

            List<Expression> args;

            //引数確認
            while (tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == "(")
            {
                tokenst.Next();
                
                if ((args=CreateArgs(tokenst,DelegateHelper.GetTypesFromDelegate(expr.Type)))!= null)
                {
                    if (tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == ")")
                    {
                        tokenst.Next();
                        expr= Expression.Invoke(expr,args);
                    }
                    else
                    {
                        tokenst.Rollback(checkPoint);
                        return null;
                    }
                }
                else
                {
                    tokenst.Rollback(checkPoint);
                    return null;
                }
            }
            return expr;
        }

        //ラムダ定義
        static Expression CreateLambdaDefinition(TokenStream tokenst, Type[] argTypes)
        {
            var checkPoint=tokenst.NowIndex;
            LocalVariableTable.In();
            if (argTypes != null && tokenst.Get().Str=="(")
            {
                tokenst.Next();
                List<ParameterExpression> argsDecl;
                if (argTypes.Count() == 1)
                    argTypes = DelegateHelper.GetTypesFromDelegate(argTypes[0]);
                if ((argsDecl = CreateArgsDeclaration(tokenst,argTypes)) != null)
                {
                    if (tokenst.Get().Str == ")")
                    {
                        tokenst.Next();
                        if (tokenst.Get().Str == "{")
                        {
                            tokenst.Next();
                            var body = CreateSiki(tokenst,argTypes.Skip(argTypes.Count()-1).ToArray());
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
        static List<ParameterExpression> CreateArgsDeclaration(TokenStream tokenst,Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;

            //引数のインデックス
            int argIndex=0;

            if (tokenst.Get().TokenType==TokenType.Identifier)
            {
                List<ParameterExpression> args = new List<ParameterExpression>();
                var parameter= Expression.Parameter(argTypes[argIndex++], tokenst.Get().Str);
                LocalVariableTable.Register(tokenst.Get().Str, parameter);
                args.Add(parameter);
                tokenst.Next();
                while (tokenst.Get().Str == ",")
                {
                    tokenst.Next();
                    if (tokenst.Get().TokenType == TokenType.Identifier)
                    {
                        parameter = Expression.Parameter(argTypes[argIndex++], tokenst.Get().Str);
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
        static Type[] CreateTypeSpecifier(TokenStream tokenst)
        {
            var checkPoint=tokenst.NowIndex;
            Type type;
            if ((type=CreateTypeSpecifier2(tokenst)) != null)
            {
                if (tokenst.Get().Str == "::" || tokenst.Get().Str == "]" || tokenst.Get().Str=="[")
                {
                    return new[] { type };
                }
                if (tokenst.Get().Str == "->")
                {
                    List<Type> types = new List<Type>();
                    types.Add(type);
                    while (tokenst.Get().Str == "->")
                    {
                        tokenst.Next();
                        if ((type = CreateTypeSpecifier2(tokenst)) != null)
                        {
                            types.Add(type);
                        }
                        else
                        {
                            tokenst.Rollback(checkPoint);
                            return null;
                        }
                    }
                    if (tokenst.Get().Str == "::" || tokenst.Get().Str == "]" || tokenst.Get().Str == "[")
                    {
                        return types.ToArray();
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //型指定子2
        static Type CreateTypeSpecifier2(TokenStream tokenst)
        {
            var checkPoint=tokenst.NowIndex;
            Type type=null;

            // [ 型指定子 ]
            if (tokenst.Get().Str == "[")
            {
                Type[] types;
                tokenst.Next();
                if ((types = CreateTypeSpecifier(tokenst)) != null)
                {
                    if (tokenst.Get().Str == "]")
                    {
                        tokenst.Next();
                        if (types.Count() == 1)
                            type= types[0];
                        else if (types[0] == typeof(void))
                            type= Expression.GetDelegateType(types.Skip(1).ToArray());
                        else
                            type= Expression.GetDelegateType(types);
                    }
                }
            }
            //型種類
            else if ((type = CreateType(tokenst.Get().Str)) != null)
            {
                tokenst.Next();
            }
            if (type != null)
            {
                while (true)
                {
                    if (tokenst.Get().Str == "[")
                    {
                        tokenst.Next();
                        if (tokenst.Get().Str == "]")
                        {
                            tokenst.Next();
                            type = type.MakeArrayType();
                        }
                        else
                        {
                            tokenst.Rollback(checkPoint);
                            return null;
                        }
                    }
                    else
                        break;
                }
            }
            if (type == null)
            {
                tokenst.Rollback(checkPoint);
                return null;
            }
            return type;

        }

        //型指定子3
        static Type[] CreateTypeSpecifier3(TokenStream tokenst)
        {
            var checkPoint = tokenst.NowIndex;
            var types = CreateTypeSpecifier(tokenst);
            if (types != null && tokenst.Get().Str == "::")
            {
                tokenst.Next();
                return types;
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
