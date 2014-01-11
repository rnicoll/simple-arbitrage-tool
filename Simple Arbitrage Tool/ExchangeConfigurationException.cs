using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lostics.SimpleArbitrageBot
{
    public abstract class ExchangeConfigurationException : Exception
    {
        public ExchangeConfigurationException(string msg)
            : base(msg)
        {
        }
    }
}
