using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace D_Sharp
{
    class AssemblyTable
    {
        private static Dictionary<string, Assembly> asmMap = new Dictionary<string, Assembly>();

        public static bool AddAsm(string path)
        {
            var asm = Assembly.LoadFrom(path);
            if (asmMap.ContainsKey(asm.FullName) == true)
                return false;
            asmMap.Add(asm.FullName, asm);
            return true;
        }

        public static Type GetClassType(string className)
        {
            Type classType=null;
            foreach(var asm in asmMap.Values)
            {
                classType = asm.GetType(className);
                if (classType != null)
                    return classType;
            }
            return null;
        }
    }
}
