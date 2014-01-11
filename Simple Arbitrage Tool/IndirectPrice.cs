using Lostics.NCryptoExchange;
using Lostics.NCryptoExchange.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lostics.SimpleArbitrageBot;

namespace Lostics.SimpleArbitrageTool
{
    /// <summary>
    /// Represents a price for a currency, inferred through two or more different exchanges.
    /// </summary>
    public sealed class IndirectPrice : MarketPrice
    {
        public IndirectPrice(ExchangePrice[] setRoute)
        {
            AssertRouteValid(setRoute);

            this.Route = setRoute;
        }

        public static void AssertRouteValid(ExchangePrice[] route)
        {
            if (route.Length == 0)
            {
                throw new InvalidRouteException("Trading route between two currencies cannot be empty.");
            }
        }

        public ExchangePrice[] Route { get; private set; }

        public override decimal? Ask
        {
            get
            {
                return (decimal)0.0;
            }
        }
        public override decimal? Bid
        {
            get
            {
                return (decimal)0.0;
            }
        }
        public override IExchange Exchange {
            get
            {
                return this.Route[0].Exchange;
            }
        }
        public Market Market { get; set; }
        public override bool IsTradeable { get { return true; } }
    }
}
