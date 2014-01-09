using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lostics.SimpleArbitrageBot
{
    public class ArbitrageOpportunity
    {
        private readonly string label;
        private readonly MarketPrice lowestAsk;
        private readonly MarketPrice highestBid;

        public ArbitrageOpportunity(string label,
            MarketPrice lowestAsk, MarketPrice highestBid)
        {
            this.label = label;
            this.lowestAsk = lowestAsk;
            this.highestBid = highestBid;
        }

        public override string ToString()
        {
            return label + ": Buy at "
            + lowestAsk.Ask + " on "
            + lowestAsk.ExchangeLabel + ", sell at "
            + highestBid.Bid + " on "
            + highestBid.ExchangeLabel;
        }
    }
}
