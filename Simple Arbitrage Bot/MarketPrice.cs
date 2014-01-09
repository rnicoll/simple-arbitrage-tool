using Lostics.NCryptoExchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lostics.SimpleArbitrageBot
{
    public abstract class MarketPrice
    {
        public abstract Task UpdatePrice();

        public abstract decimal? Ask { get; set;  }
        public abstract decimal? Bid { get; set;  }
        public abstract bool IsTradeable { get; }
    }
}
