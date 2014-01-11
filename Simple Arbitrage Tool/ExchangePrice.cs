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

        public ExchangePrice(IExchange exchange, NCryptoExchange.Model.Market market)
        {
            this.exchange = exchange;
            this.Market = market;
        }

        public async Task UpdatePriceAsync()
        {
            this.MarketDepth = await this.Exchange.GetMarketDepth(this.Market.MarketId);
        }

        public override decimal? Ask
        {
            get {
                if (this.MarketDepth.Asks.Count > 0) {
                    return this.MarketDepth.Asks[0].Price;
                } else {
                    return null;
                }
            }
        }
        public override decimal? Bid
        {
            get
            {
                if (this.MarketDepth.Bids.Count > 0)
                {
                    return this.MarketDepth.Bids[0].Price;
                }
                else
                {
                    return null;
                }
            }
        }

        public Book MarketDepth { get; set; }
        public override IExchange Exchange { get { return this.exchange; } }
        public Market Market { get; private set; }
        public override string MarketLabel { get { return this.Market.Label; } }
        public override bool IsTradeable { get { return true; } }
    }
}
