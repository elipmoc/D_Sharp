using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D_Sharp
{
    class LexicalAnalyzer
    {
        //字句解析実行
        static public TokenStream Lexicalanalysis(string str)
        {
            var tokenlist = new List<Token>();
            return new TokenStream(tokenlist);
        }
    }
}