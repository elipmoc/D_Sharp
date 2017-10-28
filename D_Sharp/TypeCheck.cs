using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D_Sharp
{
    public class ParamsPriority:IComparable<ParamsPriority>
    {
       public enum MatchKind
        {
            //型一致
            TypeMatch=0,
            //ジェネリック型一致
            GenericTypeMatch,
            //暗黙的キャストによる一致
            ImplicitCastMatch,
            //アップキャストによる一致
            UpCastMatch,
            //ジェネリック型のアップキャストによる一致
            GenericTypeUpCastMatch,
            //オブジェクト型への一致
            ObjectMatch,
        }
        public MatchKind matchkind{get;set;}
        //アップキャストの深度
        public int upCastNest = 0;

        //型の具体性
        public int concreteness = 0;

        //暗黙的キャストの優先順位
        public int implicitCastPriority = 0;

        //優先順位比較
        //0で優先順位が等しい,もしくは判別不能
        //1でxのほうが優先順位が高い
        //-1でyのほうが優先順位が高い

        public int CompareTo(ParamsPriority y)
        {
            if (this.matchkind < y.matchkind)
                return 1;
            else if (this.matchkind > y.matchkind)
                return -1;
            switch (this.matchkind)
            {

                case MatchKind.TypeMatch:
                    return 0;

                case MatchKind.GenericTypeMatch:
                    if (this.concreteness > y.concreteness)
                        return 1;
                    else if (this.concreteness < y.concreteness)
                        return -1;
                    return 0;

                case MatchKind.ImplicitCastMatch:
                    if (this.implicitCastPriority > y.implicitCastPriority)
                        return 1;
                    else if (this.implicitCastPriority < y.implicitCastPriority)
                        return -1;
                    return 0;

                case MatchKind.UpCastMatch:
                    if (this.upCastNest < y.upCastNest)
                        return 1;
                    else if (this.upCastNest > y.upCastNest)
                        return -1;
                    return 0;
                case MatchKind.ObjectMatch:
                    return 0;
            }

            throw new Exception("error!");
        }
    }

    public class TypeCheck
    {

        //型が等しかったらtrue
        static public ParamsPriority IsComplete(Type t1,Type t2)
        {
            if (t1 == t2)
            {
                var paramsPriority = new ParamsPriority();
                paramsPriority.matchkind = ParamsPriority.MatchKind.TypeMatch;
                return paramsPriority;
            }
            return null;
        }

        //アップキャストができるなら親の最小の深さを返す
        //ちがうなら-1
        static public int IsUpCast(Type t1, Type t2)
        {
            
            if (t1.BaseType != null)
            {
                if (t1.BaseType == t2)
                    return 1;
               var upCastNest= IsUpCast(t1.BaseType, t2);
                if (upCastNest != 0)
                    return upCastNest+1;
            }

            foreach(var interfaceType in t1.GetInterfaces())
            {
                if (interfaceType == t2)
                    return 1;
                var upCastNest = IsUpCast(interfaceType, t2);
                if (upCastNest != 0)
                    return upCastNest+1;
            }

            return 0;
        }


        //ジェネリックの判定
        //型の具体性を返す
        //失敗で0を返す
        static public int IsGeneric(Type t1,Type t2)
        {

            if (t2.IsGenericParameter)
                return 1;

            if (t1 == t2)
                return 2;

            if (t1.IsArray && t2.IsArray && t1.GetArrayRank() == t2.GetArrayRank())
            {
                var hoge = IsGeneric(t1.GetElementType(), t2.GetElementType());
                if (hoge == 0)
                    return 0;
                return hoge + 1;
            }

            if (t1.IsGenericType&& t2.IsGenericType&& t1.GetGenericTypeDefinition() == t2.GetGenericTypeDefinition())
            {
                var genericParams1= t1.GetGenericArguments();
                var genericParams2 = t2.GetGenericArguments();
                if (genericParams1.Length != genericParams2.Length)
                    return 0;

                int sum = 1;
                for(int i=0; i < genericParams1.Length; i++)
                {
                    var hoge = IsGeneric(genericParams1[i], genericParams2[i]);
                    if (hoge==0)
                        return 0;
                    sum += hoge;
                }
                return sum;
            }
            return 0;
        }

        public struct GenericUpCastInfo {
            public int upCastNest;
            public Type upCastedType;
        }

        //ジェネリック型を含むアップキャストの判定
        static public GenericUpCastInfo? GetGenericUpCastInfo(Type t1,Type t2)
        {
            if (t2.IsGenericType == false)
                return null;

            if (t1.BaseType != null)
            {
                if (t1.BaseType.IsGenericType && t1.BaseType.GetGenericTypeDefinition() == t2.GetGenericTypeDefinition())
                    return
                        new GenericUpCastInfo
                        { upCastNest = 1,upCastedType= t1.BaseType };
                var genericUpCastInfo = GetGenericUpCastInfo(t1.BaseType, t2);
                if (genericUpCastInfo != null)
                    return new GenericUpCastInfo
                    { upCastNest = genericUpCastInfo.Value.upCastNest+1,upCastedType=genericUpCastInfo.Value.upCastedType };
            }

            foreach (var interfaceType in t1.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == t2.GetGenericTypeDefinition())
                    return
                       new GenericUpCastInfo
                       { upCastNest = 1, upCastedType = interfaceType};
                var genericUpCastInfo = GetGenericUpCastInfo(interfaceType, t2);
                if (genericUpCastInfo != null)
                    return new GenericUpCastInfo
                    { upCastNest = genericUpCastInfo.Value.upCastNest + 1, upCastedType = genericUpCastInfo.Value.upCastedType };
            }

            return null;
        }

        //暗黙的型変換ができるならオーバーロードした際の優先順位を返す
        //ただし、primitive型のみ
        //アップキャストは対象外
        static public int IsImplicitCast(Type t1, Type t2)
        {
            if (t1.IsPrimitive && t2.IsPrimitive)
            {
                TypeCode t1code = Type.GetTypeCode(t1);
                TypeCode t2code = Type.GetTypeCode(t2);

                switch (t1code)
                {
                    case TypeCode.Char:
                        switch (t2code)
                        {
                            case TypeCode.UInt16: return 15;
                            case TypeCode.UInt32: return 13;
                            case TypeCode.Int32: return 14;
                            case TypeCode.UInt64: return 11;
                            case TypeCode.Int64: return 12;
                            case TypeCode.Single: return 10;
                            case TypeCode.Double: return 9;
                            default: return 0;
                        }

                    case TypeCode.Byte:
                        switch (t2code)
                        {
                            case TypeCode.UInt16: return 14;
                            case TypeCode.Int16: return 15;
                            case TypeCode.UInt32: return 12;
                            case TypeCode.Int32: return 13;
                            case TypeCode.UInt64: return 10;
                            case TypeCode.Int64: return 11;
                            case TypeCode.Single: return 9;
                            case TypeCode.Double: return 8;
                            default: return 0;
                        }

                    case TypeCode.SByte:
                        switch (t2code)
                        {
                            case TypeCode.Int16: return 15;
                            case TypeCode.Int32: return 14;
                            case TypeCode.Int64: return 13;
                            case TypeCode.Single: return 12;
                            case TypeCode.Double: return 11;
                            default: return 0;
                        }

                    case TypeCode.UInt16:
                        switch (t2code)
                        {
                            case TypeCode.UInt32: return 14;
                            case TypeCode.Int32: return 15;
                            case TypeCode.UInt64: return 12;
                            case TypeCode.Int64: return 13;
                            case TypeCode.Single: return 11;
                            case TypeCode.Double: return 10;
                            default: return 0;
                        }

                    case TypeCode.Int16:
                        switch (t2code)
                        {
                            case TypeCode.Int32: return 15;
                            case TypeCode.Int64: return 14;
                            case TypeCode.Single: return 13;
                            case TypeCode.Double: return 12;
                            default: return 0;
                        }

                    case TypeCode.UInt32:
                        switch (t2code)
                        {
                            case TypeCode.UInt64: return 14;
                            case TypeCode.Int64: return 15;
                            case TypeCode.Single: return 13;
                            case TypeCode.Double: return 12;
                            default: return 0;
                        }

                    case TypeCode.Int32:
                        switch (t2code)
                        {
                            case TypeCode.Int16: return 15;
                            case TypeCode.UInt16: return 14;
                            case TypeCode.UInt32: return 13;
                            case TypeCode.Int64: return 12;
                            case TypeCode.UInt64: return 11;
                            case TypeCode.Single: return 10;
                            case TypeCode.Double: return 9;
                            default: return 0;
                        }

                    case TypeCode.UInt64:
                        switch (t2code)
                        {
                            case TypeCode.Single: return 15;
                            case TypeCode.Double: return 14;
                            default: return 0;
                        }

                    case TypeCode.Int64:
                        switch (t2code)
                        {
                            case TypeCode.UInt64: return 15;
                            case TypeCode.Single: return 14;
                            case TypeCode.Double: return 13;
                            default: return 0;
                        }

                    case TypeCode.Single:
                        switch (t2code)
                        {
                            case TypeCode.Double: return 15;
                            default: return 0;
                        }
                }
            }
            return 0;
        }



        static public ParamsPriority GetParamsPriority(Type t1, Type t2)
        {
            ParamsPriority paramsPriority;

            if ((paramsPriority = IsComplete(t1, t2)) != null)
                return paramsPriority;

            var implicitCastPriority = IsImplicitCast(t1, t2);
            if (implicitCastPriority != 0)
            {
                paramsPriority = new ParamsPriority();
                paramsPriority.implicitCastPriority=implicitCastPriority;
                paramsPriority.matchkind = ParamsPriority.MatchKind.ImplicitCastMatch;
                return paramsPriority;
            }

            
            var upCastNest = IsUpCast(t1, t2);
            if (upCastNest != 0)
            {
                paramsPriority = new ParamsPriority();
                paramsPriority.upCastNest = upCastNest;
                paramsPriority.matchkind = ParamsPriority.MatchKind.UpCastMatch;
                return paramsPriority;
            }
            var concreteness = IsGeneric(t1, t2);
            if(concreteness!=0)
            {
                paramsPriority = new ParamsPriority();
                paramsPriority.matchkind = ParamsPriority.MatchKind.GenericTypeMatch;
                paramsPriority.concreteness = concreteness;
                return paramsPriority;
            }
            var genericUpCastInfo= GetGenericUpCastInfo(t1, t2);
            if (genericUpCastInfo != null)
            {
                paramsPriority = new ParamsPriority();
                paramsPriority.upCastNest =
                    genericUpCastInfo.Value.upCastNest;
                paramsPriority.matchkind = 
                    ParamsPriority.MatchKind.GenericTypeUpCastMatch;
                return paramsPriority;
            }

            if(t2==typeof(object))
            {
                paramsPriority = new ParamsPriority();
                paramsPriority.matchkind =
                    ParamsPriority.MatchKind.ObjectMatch;
                return paramsPriority;
            }

            return null;
        }

    }
}
