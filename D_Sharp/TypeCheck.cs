using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D_Sharp
{
    class TypeCheck
    {

        //型が等しかったらtrue
        static public bool IsComplete(Type t1,Type t2)
        {
            return t1 == t2;
        }

        //アップキャストができるなら親の最小の深さを返す
        //ちがうなら-1
        static private int? IsUpCast(Type t1, Type t2)
        {
            int count = 0;
            var baseType = t1.BaseType;
            while (baseType != null)
            {
                count++;
                if (baseType == t2)
                    return count;
                baseType = baseType.BaseType;
            }
            return null;
        }

        //ジェネリックの判定
        static public int? IsGeneric(Type t1,Type t2)
        {
            if (t2.IsGenericParameter)
                return 18;
            if (t1.IsArray && t2.IsArray && t1.GetArrayRank() == t2.GetArrayRank() && t2.GetElementType().IsGenericParameter)
                return 19;
            if (t1 == t2)
                return 20;
            if (t1.IsGenericType&& t2.IsGenericType&& t1.GetGenericTypeDefinition() == t2.GetGenericTypeDefinition())
            {
                var genericParams1= t1.GetGenericArguments();
                var genericParams2 = t2.GetGenericArguments();
                if (genericParams1.Length != genericParams2.Length)
                    return null;

                int sum = 0;
                for(int i=0; i < genericParams1.Length; i++)
                {
                    var hoge = IsGeneric(genericParams1[i], genericParams2[i]);
                    if (hoge==null)
                        return null;
                    sum += 1 + (int)hoge;
                }
                return sum;
            }
            return null;
        }


        //暗黙的型変換ができるならオーバーロードした際の優先順位を返す
        //ただし、primitive型のみ
        //アップキャストは対象外
        static public int? IsImplicitCast(Type t1, Type t2)
        {
            if (t1 == t2)
                return 10000;

            if (t1.IsPrimitive && t2.IsPrimitive)
            {
                TypeCode t1code = Type.GetTypeCode(t1);
                TypeCode t2code = Type.GetTypeCode(t2);

                if (t1code == TypeCode.Char)
                    switch (t2code)
                    {
                        case TypeCode.UInt16: return 15;
                        case TypeCode.UInt32: return 13;
                        case TypeCode.Int32: return 14;
                        case TypeCode.UInt64: return 11;
                        case TypeCode.Int64: return 12;
                        case TypeCode.Single: return 10;
                        case TypeCode.Double: return 9;
                        default: return null;
                    }

                if (t1code == TypeCode.Byte)
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
                        default: return null;
                    }

                if (t1code == TypeCode.SByte)
                    switch (t2code)
                    {
                        case TypeCode.Int16: return 15;
                        case TypeCode.Int32: return 14;
                        case TypeCode.Int64: return 13;
                        case TypeCode.Single: return 12;
                        case TypeCode.Double: return 11;
                        default: return null;
                    }

                if (t1code == TypeCode.UInt16)
                    switch (t2code)
                    {
                        case TypeCode.UInt32: return 14;
                        case TypeCode.Int32: return 15;
                        case TypeCode.UInt64: return 12;
                        case TypeCode.Int64: return 13;
                        case TypeCode.Single: return 11;
                        case TypeCode.Double: return 10;
                        default: return null;
                    }

                if (t1code == TypeCode.Int16)
                    switch (t2code)
                    {
                        case TypeCode.Int32: return 15;
                        case TypeCode.Int64: return 14;
                        case TypeCode.Single: return 13;
                        case TypeCode.Double: return 12;
                        default: return null;
                    }

                if (t1code == TypeCode.UInt32)
                    switch (t2code)
                    {
                        case TypeCode.UInt64: return 14;
                        case TypeCode.Int64: return 15;
                        case TypeCode.Single: return 13;
                        case TypeCode.Double: return 12;
                        default: return null;
                    }

                if (t1code == TypeCode.Int32)
                    switch (t2code)
                    {
                        case TypeCode.Int16: return 15;
                        case TypeCode.UInt16: return 14;
                        case TypeCode.UInt32: return 13;
                        case TypeCode.Int64: return 12;
                        case TypeCode.UInt64: return 11;
                        case TypeCode.Single: return 10;
                        case TypeCode.Double: return 9;
                        default: return null;
                    }

                if (t1code == TypeCode.UInt64)
                    switch (t2code)
                    {
                        case TypeCode.Single: return 15;
                        case TypeCode.Double: return 14;
                        default: return null;
                    }

                if (t1code == TypeCode.Int64)
                    switch (t2code)
                    {
                        case TypeCode.UInt64: return 15;
                        case TypeCode.Single: return 14;
                        case TypeCode.Double: return 13;
                        default: return null;
                    }

                if (t1code == TypeCode.Single)
                    switch (t2code)
                    {
                        case TypeCode.Double: return 15;
                        default: return null;
                    }
            }
            var count = IsUpCast(t1, t2);
            return count == null ? IsGeneric(t1,t2) : -count;
        }

    }
}
