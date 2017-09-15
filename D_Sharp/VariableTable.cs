using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace D_Sharp
{
    class GlobalVariable
    {
        object value;
        Type type;
        public void SetValue(object value)
        {
            this.value = value;
        }

        public void SetType(Type type)
        {
            this.type = type;
        }

        public T GetValue<T>()
        {
            return (T)value;
        }

        public new Type GetType()
        { return type; }

    }

    static class VariableTable
    {
        static Dictionary<string, GlobalVariable> table =new Dictionary<string,GlobalVariable>();
        static public bool Find(string name)
        {
            if (table.ContainsKey(name) == false) return false;
            return true;
        }

        static public Type GetType(string name)
        {
            return table[name].GetType();
        }

        static public T Get<T>(string name)
        {
            return table[name].GetValue<T>();
        }

        static public void Remove(string name)
        {
            table.Remove(name);
        }

        static public void SetValue<T>(string name,T value)
        {
            table[name].SetValue(value);
        }

        static public void Register(string name, Type type,object value=null)
        {
            var variable = new GlobalVariable();
            variable.SetType(type);
            variable.SetValue(value);
            table[name] =variable;
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
