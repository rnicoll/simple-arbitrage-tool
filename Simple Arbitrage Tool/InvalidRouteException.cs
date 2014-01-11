using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lostics.SimpleArbitrageBot
{
    public class InvalidRouteException : Exception
    {
        public InvalidRouteException(string msg)
            : base(msg)
        {
        }
    }
}
