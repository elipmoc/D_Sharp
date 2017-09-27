using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D_Sharp
{
    //インタプリター
    class Interpreter
    {
        static public void ReadLine(string str)
        {
           // try
            //{

                var tokenStream = LexicalAnalyzer.Lexicalanalysis(str.Remove(str.Count() - 1, 1));
                if (tokenStream == null)
                {
                    Console.WriteLine("( ;∀;)　token error!!");
                    return;
                }

                //デバッグ用
              /*  for (int i = 0; i < tokenStream.Size; i++)
                    tokenStream[i].DebugPrint();*/


                var func = CreateTree.CreateStatement(tokenStream);
                if (func == null)
                {
                    Console.WriteLine("( ;∀;)　Tree error!!");
                    return;
                }
                func();
            /*}
            catch (Exception except)
            {
                Console.WriteLine(except.Message);
                return;
            }*/
        }
    }
}
