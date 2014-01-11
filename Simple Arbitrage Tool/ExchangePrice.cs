using Lostics.NCryptoExchange;
using Lostics.NCryptoExchange.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lostics.SimpleArbitrageTool
{
    /// <summary>
    /// Represents a price for a currency, that can be traded directly on an exchange.
    /// </summary>
    public sealed class ExchangePrice : MarketPrice
    {
        private readonly IExchange exchange;
        private decimal? ask;
        private decimal? bid;

        public ExchangePrice(IExchange exchange, NCryptoExchange.Model.Market market)
        {
            this.exchange = exchange;
            this.Market = market;
        }

        public async Task UpdatePriceAsync()
        {
            UpdatePrice(await this.Exchange.GetMarketDepth(this.Market.MarketId));
        }

        public void UpdatePrice(Book marketDepth)
        {
            if (marketDepth.Bids.Count == 0)
            {
                this.bid = null;
            }
            else
            {
                this.bid = marketDepth.Bids[0].Price;
            }

            if (marketDepth.Asks.Count == 0)
            {
                this.ask = null;
            }
            else
            {
                this.ask = marketDepth.Asks[0].Price;
            }
        }

        public override decimal? Ask { get { return this.ask; } }
        public override decimal? Bid { get { return this.bid; } }
        public override IExchange Exchange { get { return this.exchange; } }
        public Market Market { get; private set; }
        public override string MarketLabel { get { return this.Market.Label; } }
        public override bool IsTradeable { get { return true; } }
    }
}
