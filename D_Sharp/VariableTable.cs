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

    static class LocalVariableTable
    {
        static int nest = -1;
        static List<Dictionary<string, ParameterExpression>> table = new List<Dictionary<string, ParameterExpression>>();
        static public void In()
        {
            nest++;
            table.Add(new Dictionary<string, ParameterExpression>());
        }

        static public void Out()
        {
            table.RemoveAt(nest);
            nest--;

        }

        static public ParameterExpression Find(string name)
        {
            if (nest < 0) return null;
            if (table[nest].ContainsKey(name) == false) return null;
            return table[nest][name];
        }

        static public void Register(string name, ParameterExpression parameter)
        {
            table[nest][name] = parameter;
        }
    }
}
