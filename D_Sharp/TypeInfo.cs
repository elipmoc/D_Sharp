using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D_Sharp
{
    class DelegateHelper
    {
        //デリゲートの型を分解する
        public static Type[] GetTypesFromDelegate(Type funcType)
        {
            if (funcType.IsGenericType)
            {
                return funcType.GetGenericArguments();
            }
            return null;
        }
    }
}
