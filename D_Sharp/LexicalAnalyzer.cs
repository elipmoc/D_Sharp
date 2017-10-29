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
            var comment = new Regex(@"\/\*[^(\*\/)]*\*\/");
            var space = new Regex(@"^[ \t]+");
            var num_double = new Regex(@"^\d+\.\d+");
            var num_int = new Regex(@"^\d+");
            var character = new Regex(@"^(?:').(?:')");
            var string_ = new Regex("^\"[^\"]*\"");
            var symbol = new Regex(@"^((>>=)|(\.\.)|(\.)|(@)|(new)|(int)|(in)|(let)|(unit)|(void)|(double)|(bool)|(->)|(::)|(==)|(<=)|(>=)|(!=)|(\+\+)|[<>\[\]:\?\+\-%\*\/{}\(\)=,])");
            var Identifier = new Regex(@"^([a-z]|[A-Z]|[0-9]|_)+");
//            var GlobalVariable = new Regex(@"^g_[a-z]+");
            Match match;
            while (str.Length!=0)
            {
                if ((match = space.Match(str)).Success) ;
                else if ((match = comment.Match(str)).Success) ;
                else if ((match = num_double.Match(str)).Success)
                    tokenlist.Add(new Token(match.Value, TokenType.Double));
                else if ((match = num_int.Match(str)).Success)
                    tokenlist.Add(new Token(match.Value, TokenType.Int));
                else if ((match = character.Match(str)).Success)
                    tokenlist.Add(new Token(match.Value, TokenType.Charcter));
                else if ((match = string_.Match(str)).Success)
                    tokenlist.Add(new Token(match.Value, TokenType.String));
                else if ((match = symbol.Match(str)).Success)
                    tokenlist.Add(new Token(match.Value, TokenType.symbol));
              //  else if ((match = GlobalVariable.Match(str)).Success)
                //    tokenlist.Add(new Token(match.Value, TokenType.GlobalVariable));
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