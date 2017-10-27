using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace D_Sharp
{
    struct SelectGenericParams
    {
        private Type genericType;
        public Type Select(ParameterInfo[] paramTypes, Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                GetGenericType(paramTypes[i].ParameterType, types[i]);
            }
            return genericType;
        }

        private void GetGenericType(Type param ,Type type)
        {
            if (param.IsGenericParameter)
            {
                if (genericType == null)
                    genericType = type;
                if (genericType != type)
                {
                    if (TypeCheck.IsImplicitCast(genericType, type) != 0)
                    {
                        genericType = type;
                    }
                }
            }
            else if (param.IsArray && param.GetElementType().IsGenericParameter)
            {
                if (genericType == null)
                    genericType = type.GetElementType();
                if (genericType != type.GetElementType())
                {
                    if (TypeCheck.IsImplicitCast(genericType, type.GetElementType()) != 0)
                    {
                        genericType = type.GetElementType();
                    }
                }
            }
            else if (param.IsGenericType)
            {

                var paramNestList = param.GetGenericArguments();
                Type[] typeNestList;
                if (type.IsGenericType && param.GetGenericTypeDefinition() == type.GetGenericTypeDefinition())
                    typeNestList = type.GetGenericArguments();
                else
                    typeNestList=
                        TypeCheck.GetGenericUpCastInfo(type, param).Value.upCastedType.GetGenericArguments();
                for (int i = 0; i < typeNestList.Length; i++)
                {
                    GetGenericType(paramNestList[i],typeNestList[i]);
                }
            }
        }
    }
}
