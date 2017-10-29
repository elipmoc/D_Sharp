using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D_Sharp
{
    //IOモナド
    class IO<OUT>
    {

        readonly Func<OUT> action;

        public IO(Func<OUT> action)
        {
            this.action = action;
        }

        public OUT Get()
        {
            return action();
        }
    }
}
