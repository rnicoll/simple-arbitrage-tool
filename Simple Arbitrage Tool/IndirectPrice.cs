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

            IExchange referenceExchange = route[0].Exchange;
            HashSet<ExchangePrice> breadcrumbs = new HashSet<ExchangePrice>();
            Market previousMarket = null;

            foreach (ExchangePrice routeElement in route)
            {
                if (!routeElement.Exchange.Equals(referenceExchange))
                {
                    throw new InvalidRouteException("All exchanges in the price path must be the same.");
                }

                if (breadcrumbs.Contains(routeElement))
                {
                    throw new InvalidRouteException("Route is cyclic; have seen the market \""
                        + routeElement.Market + "\" once already in the route.");
                }

                // TODO: Consider inverting the next route element, if it has a valid pair.
                if (null != previousMarket
                    && !previousMarket.QuoteCurrencyCode.Equals(routeElement.Market.BaseCurrencyCode))
                {
                    throw new InvalidRouteException("Route does not connect directly; cannot go from \""
                        + previousMarket + " to \""
                        + routeElement.Market + "\".");
                }

                breadcrumbs.Add(routeElement);
                previousMarket = routeElement.Market;
            }
        }

        public ExchangePrice[] Route { get; private set; }

        public override decimal? Ask
        {
            get
            {
                decimal ask = 1;

                foreach (ExchangePrice routeElement in this.Route)
                {
                    if (null == routeElement.Ask)
                    {
                        return null;
                    }

                    ask *= (decimal)routeElement.Ask;
                }

                return ask;
            }
        }

        public override decimal? Bid
        {
            get
            {
                decimal bid = 1;

                foreach (ExchangePrice routeElement in this.Route)
                {
                    if (null == routeElement.Bid)
                    {
                        return null;
                    }

                    bid *= (decimal)routeElement.Bid;
                }

                return bid;
            }
        }

        public override IExchange Exchange {
            get
            {
                return this.Route[0].Exchange;
            }
        }

        public Market Market { get; set; }
        public override string MarketLabel
        {
            get
            {
                StringBuilder label = new StringBuilder(this.Market.BaseCurrencyCode);

                foreach (ExchangePrice routeElement in this.Route) {
                    label.Append("/").Append(routeElement.Market.QuoteCurrencyCode);
                }

                return label.ToString();
            }
        }
        public override bool IsTradeable { get { return true; } }
    }
}
