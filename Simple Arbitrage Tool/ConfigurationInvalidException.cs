using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lostics.SimpleArbitrageBot
{
    public sealed class ConfigurationInvalidException : ExchangeConfigurationException
    {
        public ConfigurationInvalidException(string msg) : base(msg)
        {
        }
    }
}
