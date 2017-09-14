using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace D_Sharp
{
    class LexicalAnalyzer
    {
        //字句解析実行
        static public TokenStream Lexicalanalysis(string str)
        {
            var tokenlist = new List<Token>();
            var num = new Regex(@"^\d+(\.\d+)?");
            var symbol = new Regex(@"^((double)|(->)|(::)|[\+\-\*\/{}\(\)=,])");
            var Identifier = new Regex(@"^[a-z]+");
            var GlobalVariable = new Regex(@"^g_[a-z]+");
            Match match;
            while (str.Length!=0)
            {
                if ((match = num.Match(str)).Success)
                    tokenlist.Add(new Token(match.Value,TokenType.Double));
                else if ((match = symbol.Match(str)).Success)
                    tokenlist.Add(new Token(match.Value, TokenType.symbol));
                else if ((match = GlobalVariable.Match(str)).Success)
                    tokenlist.Add(new Token(match.Value, TokenType.GlobalVariable));
                else if ((match = Identifier.Match(str)).Success)
                    tokenlist.Add(new Token(match.Value, TokenType.Identifier));
                else
                    return null;
                str=str.Substring(match.Length);
            }

            return new TokenStream(tokenlist);
        }
    }
}