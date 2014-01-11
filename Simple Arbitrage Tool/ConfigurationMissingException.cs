using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lostics.SimpleArbitrageBot
{
    public sealed class ConfigurationMissingException : ExchangeConfigurationException
    {
        public ConfigurationMissingException(string msg) : base(msg)
        {
        }
    }
}
