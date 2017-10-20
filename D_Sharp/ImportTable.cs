using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D_Sharp
{
    class ImportTable
    {
        private static Dictionary<string, string> importMap=new Dictionary<string, string>();

        public static bool AddImport(string _namespace,string assemblyName)
        {
            if (importMap.ContainsKey(_namespace) == true)
                return false;
            importMap.Add(_namespace, assemblyName);
            return true;
        }

        public static string GetImport(string _namespace)
        {
            if (importMap.ContainsKey(_namespace) == false)
                return null;
            return importMap[_namespace];
        }

    }
}
