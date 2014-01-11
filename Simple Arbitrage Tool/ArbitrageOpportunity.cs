using System;
using System.Collections.Generic;
using System.Globalization;
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
            decimal difference = (decimal)(highestBid.Bid - lowestAsk.Ask);
            decimal percentImprovement = difference / (decimal)lowestAsk.Ask * 100;

            return "Buy "
            + lowestAsk.MarketLabel + " at "
            + lowestAsk.Ask + " on "
            + lowestAsk.ExchangeLabel + ", sell "
            + highestBid.MarketLabel + " at "
            + highestBid.Bid + " on "
            + highestBid.ExchangeLabel + " for "
            + percentImprovement.ToString("0.00", CultureInfo.InvariantCulture) + "% profit";
        }
    }
}
