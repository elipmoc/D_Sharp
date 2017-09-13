using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace D_Sharp
{
    static class VariableTable
    {
        static Dictionary<string,ConstantExpression> table =new Dictionary<string, ConstantExpression>();
        static public ConstantExpression Find(string name)
        {
            if (table.ContainsKey(name) == false) return null;
            return table[name];
        }

        static public void Register<T>(string name, T value)
        {
            table[name] = Expression.Constant(value);
        }
    }
}
