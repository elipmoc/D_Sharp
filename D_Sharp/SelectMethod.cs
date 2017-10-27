using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace D_Sharp
{
    public class GG
    {
        public static void fool<T>(T t) { }
      /*  public static void fool() { }
        public static void fool(Program.G i) { }
        public static void fool(System.Int16 i) { }
        public static void fool(System.Int32 i) { }
        public static void fool(System.Int64 i) { }
        public static void fool(System.UInt16 i) { }
        public static void fool(System.UInt32 i) { }
        public static void fool(System.UInt64 i) { }
        public static void fool(System.Single i) { }
        public static void fool<T>(T i, T i2) { }*/
        public static void fool<T>(List<T> i) { }

      //  public static void fool<T>(T i) { }
       // public static void fool<T>(T[] i) { }
    }

    class SelectMethod
    {
       

        //疑似タプル
        struct Tuple
        {
            public MethodInfo methodInfo;
            public　IEnumerable<ParamsPriority> paramsPrioritys;
        }

       


        static public MethodInfo Select(Type classType,string funcName,BindingFlags bindingFlags,Type[] types)
        {
            var typesLength = types.Length;
            if (classType == null)
                return null;
            var methodInfos = 
                classType.GetMethods(bindingFlags)
                    .Where(f=> {
                        if(f.Name==funcName && f.GetParameters().Length==typesLength)
                            return true;
                        return false;
                    });

            var tuples = methodInfos.Select(
                m => {
                    var parameters = m.GetParameters();
                    Tuple tuple;
                    tuple.methodInfo = m;
                    tuple.paramsPrioritys =
                        parameters.
                            Zip(types, (param, type) =>
                                TypeCheck.GetParamsPriority(type, param.ParameterType));

                    return tuple;
                }).Where(tuple=>!tuple.paramsPrioritys.Contains(null));

            var tempTuple= new Tuple{ paramsPrioritys=null};
            int tempSum = 0;

            foreach (var it in tuples)
            {
                
                if(tempTuple.paramsPrioritys==null)
                {
                    tempTuple = it;
                    continue;
                }

                int sum = 0;
                sum = it.paramsPrioritys.Zip(tempTuple.paramsPrioritys,
                    (paramsPriority, tParamsPriority) => paramsPriority.CompareTo(tParamsPriority))
                    .Sum();

                if (sum == tempSum)
                    return null;
                else if (sum > tempSum)
                {
                    tempTuple = it;
                    tempSum = sum;
                }
            }
            if (tempTuple.methodInfo == null)
                return null;
            if (tempTuple.methodInfo.IsGenericMethod)
            {

                Type genericType = new SelectGenericParams().Select(tempTuple.methodInfo.GetParameters(), types);
                if (genericType != null)
                    return tempTuple.methodInfo.MakeGenericMethod(genericType);
                return null;
            }
            return tempTuple.methodInfo;
        }
    }
}
