using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lostics.SimpleArbitrageTool
{
    public class ArbitrageOpportunity
    {
        private readonly MarketPrice lowestAsk;
        private readonly MarketPrice highestBid;

        public ArbitrageOpportunity(MarketPrice lowestAsk, MarketPrice highestBid)
        {
            this.lowestAsk = lowestAsk;
            this.highestBid = highestBid;
        }

        public override string ToString()
        {
            return "Buy "
            + lowestAsk.MarketLabel + " at "
            + lowestAsk.Ask + " on "
            + lowestAsk.ExchangeLabel + ", sell "
            + highestBid.MarketLabel + " at "
            + highestBid.Bid + " on "
            + highestBid.ExchangeLabel;
        }
    }
}
