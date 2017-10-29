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
                case "++":
                    return Expression.Call(
                        typeof(built_in_functions).GetMethod("merge").MakeGenericMethod(left.Type.GetGenericArguments()[0]),
                        left, right);
            }
            return null;
        }

        //文
        static public Action CreateStatement(TokenStream tokenst)
        {
            Expression body;
            if (CreateImport(tokenst) == false)
            {
                body = CreateSiki(tokenst);
                if (body == null)
                    if ((body = CreateGlobalVariableDecl(tokenst)) == null)
                        return null;
                if (tokenst.NowIndex < tokenst.Size)
                    return null;
            }
            else
                body = Expression.Constant(0);
            return Expression.Lambda<Action>(body).Compile();
        }

        //Import
        static bool CreateImport(TokenStream tokenst)
        {
            var checkPoint = tokenst.NowIndex;
            if (tokenst.Get().Str == "import")
            {
                tokenst.Next();
                if (tokenst.Get().TokenType == TokenType.String)
                {
                    var _namespace = new string( tokenst.Get().Str.Take(tokenst.Get().Str.Length - 1).Skip(1).ToArray());
                    tokenst.Next();
                    if(tokenst.Get().TokenType == TokenType.String)
                    {
                        var assemblyInfo = new string(tokenst.Get().Str.Take(tokenst.Get().Str.Length - 1).Skip(1).ToArray());
                        if (ImportTable.AddImport(_namespace, _namespace + "," + assemblyInfo))
                        {
                            tokenst.Next();
                            return true;
                        }
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return false;
        }

        //グローバル変数宣言
        static Expression CreateGlobalVariableDecl(TokenStream tokenst)
        {
            var checkPoint = tokenst.NowIndex;
            Type[] types;
            types = CreateTypeSpecifier3(tokenst);
            string variableName;
            if (tokenst.Get().TokenType == TokenType.Identifier)
            {
                variableName = tokenst.Get().Str;
                if (VariableTable.Find(variableName) == false)
                {
                    if (types != null)
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
                }
                VariableTable.Remove(variableName);

            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //ローカル変数宣言
        static Expression CreateLocalVariableDecl(TokenStream tokenst)
        {
            var checkPoint = tokenst.NowIndex;
            Type[] types;
            types = CreateTypeSpecifier3(tokenst);
            string variableName;
            if (tokenst.Get().TokenType == TokenType.Identifier)
            {
                variableName = tokenst.Get().Str;
                if (LocalVariableTable.FindNowNest(variableName) == null && types != null)
                {
                    if (types.Count() == 1)
                        LocalVariableTable.Register(variableName, Expression.Parameter(types[0]));
                    else if (types[0] == typeof(void))
                        LocalVariableTable.Register(variableName, Expression.Parameter(Expression.GetDelegateType(types.Skip<Type>(1).ToArray())));
                    else
                        LocalVariableTable.Register(variableName, Expression.Parameter(Expression.GetDelegateType(types)));
                }
                tokenst.Next();
                if (tokenst.Get().Str == "=")
                {
                    tokenst.Next();
                    var expr = CreateSiki(tokenst, types);
                    if (expr != null)
                    {
                        if (LocalVariableTable.FindNowNest(variableName) == null && types == null)
                        {
                            LocalVariableTable.Register(variableName, Expression.Parameter(expr.Type));
                        }
                        Expression param= LocalVariableTable.FindNowNest(variableName);
                        return Expression.Assign(param, expr);
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //式
        static Expression CreateSiki(TokenStream tokenst, Type[] argTypes=null)
        {
            Expression expr;
            if ((expr = CreateNetClassNew(tokenst,argTypes)) != null)
                return expr;
            return null;
        }
        //Netクラスnew
        static Expression CreateNetClassNew(TokenStream tokenst,Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            if (tokenst.Get().Str == "new")
            {
              
                tokenst.Next();
                var classType = CreateNetClassType(tokenst);
                if (classType != null)
                {
                    if (tokenst.Get().Str == "(")
                    {
                        tokenst.Next();
                        var args=CreateArgs(tokenst, argTypes);
                        if (args != null && tokenst.Get().Str==")")
                        {
                            tokenst.Next();
                            var constructorInfo=
                                classType.GetConstructor(BindingFlags.Public|BindingFlags.Instance, null,CallingConventions.HasThis, args.Select(arg => arg.Type).ToArray(), null);
                            if (constructorInfo != null)
                            {
                                var paramT = constructorInfo.GetParameters().Select(param => param.ParameterType).ToArray();
                                return Expression.New(constructorInfo, args.Select((arg, i) => Expression.Convert(arg, paramT[i])));
                            }
                        }
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return CreateCast(tokenst, argTypes);
        }

        //キャスト
        static Expression CreateCast(TokenStream tokenst, Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            if (tokenst.Get().Str == "(")
            {
                tokenst.Next();
                Type type;
                if ((type = CreateType(tokenst.Get().Str)) != null)
                {
                    tokenst.Next();
                    if (tokenst.Get().Str == ")")
                    {
                        tokenst.Next();
                        Expression expr;
                        if ((expr = CreateCast(tokenst, null)) != null)
                        {
                            return Expression.Convert(expr, type);
                        }
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return CreateLetIn(tokenst, argTypes);
        }

        //let in
        static Expression CreateLetIn(TokenStream tokenst,Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            List<Expression> exprList=new List<Expression>();
            Expression expr;
            if (tokenst.Get().Str == "let") {
                LocalVariableTable.In();
                tokenst.Next();
                if ((expr=CreateLocalVariableDecl(tokenst)) != null)
                {
                    exprList.Add(expr);
                    while (tokenst.Get().Str == ",")
                    {
                        tokenst.Next();
                        if ((expr = CreateLocalVariableDecl(tokenst)) != null)
                            exprList.Add(expr);
                        else
                        {
                            tokenst.Rollback(checkPoint);
                            return null;
                        }

                    }
                    if (tokenst.Get().Str == "in")
                    {
                        tokenst.Next();
                        if ((expr = CreateSiki(tokenst,argTypes)) != null)
                        {
                            exprList.Add(expr);
                            expr=Expression.Block(LocalVariableTable.GetNowNestParamList(), exprList);
                            LocalVariableTable.Out();
                            return expr;
                        }
                    }
                }
            }
            else if((expr=CreateJyoukenEnzan(tokenst,argTypes))!=null){ return expr; }

            tokenst.Rollback(checkPoint);
            return null;
        }

        //条件演算子
        static Expression CreateJyoukenEnzan(TokenStream tokenst, Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            Expression expr;
            if ((expr = CreateHitosi(tokenst,argTypes)) != null)
            {
                if(tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == "?")
                {
                    Expression left, right;
                    tokenst.Next();
                    if ((left = CreateSiki(tokenst,argTypes)) != null)
                    {
                        if (tokenst.Get().Str == ":")
                        {
                            tokenst.Next();
                            if ((right = CreateSiki(tokenst,argTypes)) != null)
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

        //等しい演算子
        static Expression CreateHitosi(TokenStream tokenst, Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            Expression left;
            if ((left = CreateTasizan(tokenst,argTypes)) != null)
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
                    if ((right = CreateTasizan(tokenst,argTypes)) == null)
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

        //足し算
        static Expression CreateTasizan(TokenStream tokenst, Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            Expression left;
            if ((left = CreateKou(tokenst, argTypes)) != null)
            {
                Expression right;
                string op;
                while (tokenst.NowIndex < tokenst.Size && (tokenst.Get().Str == "+" || tokenst.Get().Str == "-" || tokenst.Get().Str == "++"))
                {
                    op = tokenst.Get().Str;
                    tokenst.Next();
                    if ((right = CreateKou(tokenst, argTypes)) == null)
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
            if ((left = CreateNetClassAccess(tokenst,argTypes)) != null)
            {
                Expression right;
                string op;
                while (tokenst.NowIndex < tokenst.Size && (tokenst.Get().Str == "*" || tokenst.Get().Str == "/" || tokenst.Get().Str=="%"))
                {
                    op = tokenst.Get().Str;
                    tokenst.Next();
                    if ((right = CreateNetClassAccess(tokenst,argTypes)) == null)
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
            //整数
            if (tokenst.Get().TokenType == TokenType.Int)
            {
                expr = Expression.Constant(tokenst.Get().GetInt());
                tokenst.Next();
                return expr;
            }
            //実数
            else if (tokenst.Get().TokenType == TokenType.Double)
            {
                expr = Expression.Constant(tokenst.Get().GetDouble());
                tokenst.Next();
                return expr;
            }
            //文字
            else if ((expr=CreateCharacter(tokenst)) != null)
            {
                return expr;
            }
            //文字列
            else if ((expr = CreateString(tokenst)) != null)
            {
                return expr;
            }
            //リスト
            else if ((expr=CreateList(tokenst,argTypes))!=null)
            {
                return expr;
            }
            //Netクラスの静的メソッド呼び出し
            else if ((expr = CreateNetClassStaticFunctionCall(tokenst)) != null)
            {
                return expr;
            }
            //組み込み関数呼び出し
            else if ((expr = CreateFunctionCall(tokenst)) != null)
            {
                return expr;
            }
            //Netクラスの静的プロパティ呼び出し
            else if ((expr = CreateNetClassStaticProperty(tokenst)) != null)
            {
                return expr;
            }
            //ローカル変数
            else if ((expr = CreateLocalVariableExpr(tokenst)) != null)
            {
                return expr;
            }
            //グローバル変数
            else if (
                tokenst.Get().TokenType==TokenType.Identifier &&
                VariableTable.Find(tokenst.Get().Str)!=false)
                
            {
                var type = VariableTable.GetType(tokenst.Get().Str);
                var methodInfo = typeof(VariableTable).GetMethod("Get").MakeGenericMethod(type);
                expr = Expression.Call(methodInfo,Expression.Constant(tokenst.Get().Str));
                tokenst.Next();
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

        //文字
        static Expression CreateCharacter(TokenStream tokenst)
        {
            if (tokenst.Get().TokenType == TokenType.Charcter)
            {
                var expr = Expression.Constant(tokenst.Get().Str[1]);
                tokenst.Next();
                return expr;
            }
            return null;
        }


        //文字列
        static Expression CreateString(TokenStream tokenst)
        {
            if (tokenst.Get().TokenType == TokenType.String)
            {
                string str=tokenst.Get().Str;
                Expression expr;
                if (str.Length == 0)
                    expr = Expression.NewArrayInit(typeof(char), Expression.Constant(""));
                else
                    expr = Expression.NewArrayInit(typeof(char), str.Take(str.Length - 1).Skip(1).Select(x => Expression.Constant(x)));
                tokenst.Next();
                return expr;
            }
            return null;
        }

        //リスト
        static Expression CreateList(TokenStream tokenst,Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            if (tokenst.Get().Str == "[")
            {
                tokenst.Next();
                var array = CreateSuretuHyoukiList(tokenst, argTypes);
                if (array != null)
                {
                    if (tokenst.Get().Str == "]")
                    {
                        tokenst.Next();
                        return array;
                    }
                }
                array = CreateListNakami(tokenst,argTypes);
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
            if ((expr = CreateSiki(tokenst, argTypes != null ? new[] { argTypes[0].GetElementType()}:null)) != null)
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
                return 
                    Expression.Convert( Expression.NewArrayInit(type, exprList.ToArray()),typeof(IEnumerable<>).MakeGenericType(type));
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //数列表記リスト
        static Expression CreateSuretuHyoukiList(TokenStream tokenst,Type[] argTypes) {
            var checkPoint = tokenst.NowIndex;
            Expression beginExpr;
            if ((beginExpr = CreateSiki(tokenst, argTypes != null ? new[] { argTypes[0].GetElementType() } : null)) != null)
            {
                var type = beginExpr.Type;
                if (tokenst.Get().Str == ",")
                {
                    Expression secondExpr;
                    tokenst.Next();
                    if ((secondExpr = CreateSiki(tokenst, new[] { type })) != null)
                    {
                        if (tokenst.Get().Str == "..")
                        {
                            tokenst.Next();
                            if (tokenst.Get().Str != "]")
                            {
                                var endExpr = CreateSiki(tokenst, new[] { type });
                                if (endExpr != null)
                                {
                                    var methodInfo =
                                    SelectMethod.Select
                                        (typeof(MakeNumericalSequenceList), "MakeRangeList", BindingFlags.Public | BindingFlags.Static, new Type[] { beginExpr.Type, secondExpr.Type,endExpr.Type });
                                    if (methodInfo != null)
                                    {
                                        return Expression.Call(methodInfo, beginExpr, Expression.Subtract(secondExpr, beginExpr),endExpr);
                                    }
                                }
                            }
                            else
                            {
                                var methodInfo =
                                    SelectMethod.Select
                                        (typeof(MakeNumericalSequenceList), "MakeInfinityList", BindingFlags.Public | BindingFlags.Static, new Type[] { beginExpr.Type, secondExpr.Type });
                                if (methodInfo != null)
                                {
                                    return Expression.Call(methodInfo, beginExpr, Expression.Subtract(secondExpr, beginExpr));
                                }
                            }
                        }
                    }
                }
             
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //Netクラスの静的メソッド呼び出し
        static Expression CreateNetClassStaticFunctionCall(TokenStream tokenst)
        { 
            var checkPoint=tokenst.NowIndex;
            var type = CreateNetClassType(tokenst);
            if (type != null)
            {
                if (tokenst.Get().Str == ".")
                {
                    tokenst.Next();
                    if (tokenst.Get().TokenType == TokenType.Identifier)
                    {
                        var funcName = tokenst.Get().Str;
                        tokenst.Next();
                        if (tokenst.NowIndex< tokenst.Size-1 && tokenst.Get().Str == "(")
                        {
                            tokenst.Next();
                            List<Expression> args;
                            if ((args = CreateArgs(tokenst, null)) != null)
                            {
                                if (tokenst.Get().Str == ")")
                                {
                                    var methodInfo=SelectMethod.Select(type,funcName,BindingFlags.Public|BindingFlags.Static, args.Select(arg=>arg.Type).ToArray());
                                    if (methodInfo != null)
                                    {
                                       var paramT= methodInfo.GetParameters().Select(param=>param.ParameterType).ToArray();
                                        tokenst.Next();
                                        var callExpr=Expression.Call(methodInfo, args.Select((arg, i) => Expression.Convert(arg, paramT[i])));

                                        if (methodInfo.ReturnType == typeof(void))
                                                return Expression.Block(
                                                    callExpr
                                                    ,Expression.Constant(new Unit())
                                                );
                                        return callExpr;
                                    }
                                }
                            }
                        }
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
                    if ((args=CreateArgs(tokenst,null)) != null)
                    {
                        if (tokenst.Get().Str == ")")
                        {
                            MethodInfo methodInfo;
                            methodInfo = 
                                SelectMethod.Select(typeof(built_in_functions),funcName ,BindingFlags.Static | BindingFlags.Public, args.Select(x => x.Type).ToArray());
                            if (methodInfo == null)
                            {
                                tokenst.Rollback(checkPoint);
                                return null;
                            }
                                tokenst.Next();
                            var paramT = methodInfo.GetParameters().Select(param => param.ParameterType).ToArray();
                            return Expression.Call(methodInfo, args.Select((arg, i) => Expression.Convert(arg, paramT[i])));
                        }
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //Netクラスの静的プロパティ呼び出し
        static Expression CreateNetClassStaticProperty(TokenStream tokenst)
        {
            var checkPoint = tokenst.NowIndex;
            var type = CreateNetClassType(tokenst);
            if (type != null)
            {
                if (tokenst.Get().Str == ".")
                {
                    tokenst.Next();
                    if (tokenst.Get().TokenType == TokenType.Identifier)
                    {
                        var propertyName = tokenst.Get().Str;
                        tokenst.Next();
                        var fieldInfo = type.GetField(propertyName, BindingFlags.Public | BindingFlags.Static);
                        if (fieldInfo != null)
                            return Expression.Field(null,fieldInfo);
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

        //Netクラスアクセス
        static Expression CreateNetClassAccess(TokenStream tokenst, Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;
            var expr = CreateLambdaCall(tokenst, argTypes);
            if (expr != null)
            {
                Expression expr2;
                while (tokenst.NowIndex<tokenst.Size-1 && tokenst.Get().Str == ".")
                {
                    tokenst.Next();
                    expr2=CreateMemberMethodCall(expr, tokenst, argTypes);
                    if (expr2 != null)
                        expr = expr2;
                    else
                        expr = CreateNetClassPropertyGet(expr, tokenst, argTypes);
                    if (expr == null)
                    {
                        tokenst.Rollback(checkPoint);
                        return null;
                    }
                }
                return expr;
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //Netクラスメンバメソッド呼び出し
        static Expression CreateMemberMethodCall(Expression expr,TokenStream tokenst,Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;

                List<Expression> args;
                string methodName;

                if (tokenst.Get().TokenType == TokenType.Identifier)
                {
                    methodName = tokenst.Get().Str;
                    tokenst.Next();
                if (tokenst.NowIndex<tokenst.Size-1 && tokenst.Get().Str == "(")
                {
                    tokenst.Next();
                    if ((args = CreateArgs(tokenst, DelegateHelper.GetTypesFromDelegate(expr.Type))) != null)
                    {
                        if (tokenst.NowIndex < tokenst.Size && tokenst.Get().Str == ")")
                        {
                            tokenst.Next();
                            var methodInfo =SelectMethod.Select(expr.Type,methodName, BindingFlags.Public | BindingFlags.Instance, args.Select(arg => arg.Type).ToArray());
                            var paramT = methodInfo.GetParameters().Select(param => param.ParameterType).ToArray();
                            expr = Expression.Call(expr, methodInfo, args.Select((arg, i) => Expression.Convert(arg, paramT[i])));
                            if (methodInfo.ReturnType == typeof(void))
                                return Expression.Block(
                                    expr
                                    , Expression.Constant(new Unit())
                                );
                            return expr;
                        }
                    }
                }
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //Netクラスプロパティ取得
        static Expression CreateNetClassPropertyGet(Expression expr, TokenStream tokenst, Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;

            string propertyOrFieldName;

            if (tokenst.Get().TokenType == TokenType.Identifier)
            {
                propertyOrFieldName = tokenst.Get().Str;
                tokenst.Next();
                    if (tokenst.NowIndex<tokenst.Size-1&& tokenst.Get().Str == "=")
                    {
                        tokenst.Next();
                        var expr2 = CreateSiki(tokenst, null);
                        if (expr2 == null)
                        {
                            tokenst.Rollback(checkPoint);
                            return null;
                        }
                        return Expression.Assign(Expression.PropertyOrField(expr, propertyOrFieldName),expr2);
                    }
                    else
                        return Expression.PropertyOrField(expr, propertyOrFieldName);
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

        //ラムダ呼び出し
        static Expression CreateLambdaCall(TokenStream tokenst,Type[] argTypes)
        {
            var checkPoint = tokenst.NowIndex;


            Expression expr;

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
            if (argTypes == null)
                return null;
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
                            type = typeof(IEnumerable<>).MakeGenericType(type);
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
                case "int":
                    return typeof(int);
                case"double":
                    return typeof(double);
                case "bool":
                    return typeof(bool);
                case "char":
                    return typeof(char);
                case "void":
                    return typeof(void);
                case "unit":
                    return typeof(Unit);
            }
            return null;
        }

        //クラス名
        static Type CreateNetClassType(TokenStream tokenst) {
            var checkPoint = tokenst.NowIndex;
            string _namespace="";
            string className = "";
            string tempName = "";
            if (tokenst.Get().TokenType == TokenType.Identifier)
            {
                className = tokenst.Get().Str;
                _namespace = className;
                tokenst.Next();
                while (tokenst.Get().Str =="@" )
                {
                   if(tempName!="") _namespace +="."+ tempName;
                    tokenst.Next();
                    if (tokenst.Get().TokenType != TokenType.Identifier)
                    {
                        tokenst.Rollback(checkPoint);
                        return null;
                    }
                    className +="."+ tokenst.Get().Str;
                    tempName= tokenst.Get().Str;
                    tokenst.Next();
                }
                string assemblyName = ImportTable.GetImport(_namespace);
                if (assemblyName != null)
                    className += "," + assemblyName;
                return Type.GetType(className);
            }
            tokenst.Rollback(checkPoint);
            return null;
        }

    }
}
