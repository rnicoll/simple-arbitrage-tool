using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lostics.SimpleArbitrageBot
{
    public sealed class IdentityPrice : MarketPrice
    {
        public override async Task UpdatePrice()
        {
        }

        public override decimal? Ask { get { return 1; } set { } }
        public override decimal? Bid { get { return 1; } set { } }
        public override string ExchangeLabel { get { return "Identity"; } }
        public override bool IsTradeable { get { return false; } }
    }
}
