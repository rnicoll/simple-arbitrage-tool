using Lostics.NCryptoExchange;
using Lostics.NCryptoExchange.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lostics.SimpleArbitrageBot
{
    /// <summary>
    /// Represents a price for a currency, that can be traded directly on an exchange.
    /// </summary>
    public class ExchangePrice : MarketPrice
    {
        public override async Task UpdatePrice()
        {
            Book marketOrders = await this.Exchange.GetMarketOrders(this.Market.MarketId);

            if (marketOrders.Bids.Count == 0)
            {
                this.Bid = null;
            }
            else
            {
                this.Bid = marketOrders.Bids[0].Price;
            }

            if (marketOrders.Asks.Count == 0)
            {
                this.Ask = null;
            }
            else
            {
                this.Ask = marketOrders.Asks[0].Price;
            }
        }

        public override decimal? Ask { get; set; }
        public override decimal? Bid { get; set; }
        public AbstractExchange Exchange { get; set; }
        public override string ExchangeLabel { get { return Exchange.Label; } }
        public Market Market { get; set; }
        public override bool IsTradeable { get { return true; } }
    }
}
