using Lostics.NCryptoExchange;
using Lostics.NCryptoExchange.Model;
using Lostics.NCryptoExchange.Vircurex;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lostics.SimpleArbitrageTool
{
    public class MarketMatrix
    {
        private readonly Dictionary<string, int> currencyIndices = new Dictionary<string, int>();
        private readonly Dictionary<int, string> currencyCodes = new Dictionary<int, string>();

        // The following arrays are ordered by base currency, then quote currency

        private readonly List<MarketPrice>[,] prices;

        public MarketMatrix(Dictionary<IExchange, List<Market>> markets)
        {
            string[] currencies = GetIndividualCurrencies(markets);

            // Construct the empty prices array, and fill in prices for exchanging
            // a currency with itself.
            this.prices = new List<MarketPrice>[currencies.Length, currencies.Length];
            for (int baseCurrencyIdx = 0; baseCurrencyIdx < currencies.Length; baseCurrencyIdx++)
            {
                currencyIndices[currencies[baseCurrencyIdx]] = baseCurrencyIdx;
                currencyCodes[baseCurrencyIdx] = currencies[baseCurrencyIdx];

                for (int quoteCurrencyIdx = 0; quoteCurrencyIdx < currencies.Length; quoteCurrencyIdx++)
                {
                    if (baseCurrencyIdx != quoteCurrencyIdx)
                    {
                        this.prices[baseCurrencyIdx, quoteCurrencyIdx] = new List<MarketPrice>();
                    }
                }
            }

            // Insert placeholders for currency pairs which can be traded
            // directly.
            foreach (IExchange exchange in markets.Keys) {
                foreach (Market market in markets[exchange])
                {
                    int baseCurrencyIdx = this.currencyIndices[market.BaseCurrencyCode];
                    int quoteCurrencyIdx = this.currencyIndices[market.QuoteCurrencyCode];

                    this.prices[baseCurrencyIdx, quoteCurrencyIdx].Add(
                        new ExchangePrice(exchange, market)
                    );
                }
            }
        }

        public void AddIndirectExchanges(params string[] currencyCodeRoute)
        {
            if (currencyCodeRoute.Length < 1)
            {
                return;
            }

            Dictionary<IExchange, ExchangePrice[]> routes = new Dictionary<IExchange, ExchangePrice[]>();

            for (int currencyIdx = 1; currencyIdx < currencyCodeRoute.Length; currencyIdx++)
            {
                foreach (MarketPrice price in this.GetPrices(currencyCodeRoute[currencyIdx - 1], currencyCodeRoute[currencyIdx]))
                {
                    ExchangePrice exchangePrice = price as ExchangePrice;
                    ExchangePrice[] routePrices;

                    if (null == exchangePrice)
                    {
                        // Cannot route through non-exchange prices
                        continue;
                    }

                    if (!routes.TryGetValue(exchangePrice.Exchange, out routePrices))
                    {
                        routePrices = new ExchangePrice[currencyCodeRoute.Length - 1];
                        routes[exchangePrice.Exchange] = routePrices;
                    }

                    routePrices[currencyIdx - 1] = exchangePrice;
                }
            }

            List<MarketPrice> routedMarket = this.GetPrices(currencyCodeRoute[0], currencyCodeRoute[currencyCodeRoute.Length - 1]);

            foreach (IExchange exchange in routes.Keys)
            {
                bool fullRoute = true;
                ExchangePrice[] route = routes[exchange];

                // Check if we have steps for each part of the route.
                for (int routeIdx = 0; routeIdx < route.Length; routeIdx++) {
                    if (route[routeIdx] == null) {
                        fullRoute = false;
                        break;
                    }
                }

                if (fullRoute)
                {
                    routedMarket.Add(new IndirectPrice(route));
                }
            }
        }

        /// <summary>
        /// Fetches available markets for the given base and quote currency pair.
        /// </summary>
        /// <param name="baseCurrencyCode"></param>
        /// <param name="quoteCurrencyCode"></param>
        /// <returns></returns>
        public List<MarketPrice> GetPrices(string baseCurrencyCode, string quoteCurrencyCode)
        {
            int baseCurrencyIdx;
            int quoteCurrencyIdx;

            if (!this.currencyIndices.TryGetValue(baseCurrencyCode, out baseCurrencyIdx)
                || !this.currencyIndices.TryGetValue(quoteCurrencyCode, out quoteCurrencyIdx))
            {
                return new List<MarketPrice>();
            }

            if (baseCurrencyIdx == quoteCurrencyIdx)
            {
                throw new ArgumentException("Base and quote currency codes cannot be the same; got \""
                    + baseCurrencyCode + "\" and \""
                    + quoteCurrencyCode + "\" respectively.");
            }

            return this.prices[baseCurrencyIdx, quoteCurrencyIdx];
        }

        private static string[] GetIndividualCurrencies(Dictionary<IExchange, List<Market>> validMarkets)
        {
            HashSet<string> currencies = new HashSet<string>();

            foreach (List<Market> markets in validMarkets.Values)
            {
                foreach (Market market in markets)
                {
                    currencies.Add(market.BaseCurrencyCode);
                    currencies.Add(market.QuoteCurrencyCode);
                }
            }

            return currencies.ToArray();
        }

        public List<ArbitrageOpportunity> GetArbitrageOpportunities()
        {
            List<ArbitrageOpportunity> opportunities = new List<ArbitrageOpportunity>();
            int currencyCount = prices.GetLength(0);

            // Start the data fetch running in parallel
            for (int baseCurrencyIdx = 0; baseCurrencyIdx < currencyCount; baseCurrencyIdx++)
            {
                for (int quoteCurrencyIdx = 0; quoteCurrencyIdx < currencyCount; quoteCurrencyIdx++)
                {
                    if (baseCurrencyIdx == quoteCurrencyIdx)
                    {
                        continue;
                    }

                    MarketPrice highestBid = null;
                    MarketPrice lowestAsk = null;

                    foreach (MarketPrice marketPrice in this.prices[baseCurrencyIdx, quoteCurrencyIdx])
                    {
                        if (marketPrice.Bid != null)
                        {
                            if (highestBid == null
                                || marketPrice.Bid > highestBid.Bid)
                            {
                                highestBid = marketPrice;
                            }
                        }
                        if (marketPrice.Ask != null)
                        {
                            if (lowestAsk == null
                                || marketPrice.Ask < lowestAsk.Ask)
                            {
                                lowestAsk = marketPrice;
                            }
                        }
                    }

                    if (null != highestBid
                        && null != lowestAsk)
                    {
                        if (highestBid.Bid > lowestAsk.Ask
                            && !highestBid.Equals(lowestAsk))
                        {
                            opportunities.Add(new ArbitrageOpportunity(lowestAsk, highestBid));
                        }
                    }
                }
            }

            return opportunities;
        }

        public void UpdateAllPrices()
        {
            if (prices.Length == 0)
            {
                return;
            }

            List<Task> tasks = new List<Task>();
            int currencyCount = prices.GetLength(0);

            Dictionary<MarketId, ExchangePrice> vircurexPrices = new Dictionary<MarketId, ExchangePrice>();
            HashSet<string> vircurexQuoteCurrencyCodes = new HashSet<string>();
            VircurexExchange vircurex = null;

            // Start the data fetch running in parallel; non-Vircurex first
            for (int baseCurrencyIdx = 0; baseCurrencyIdx < currencyCount; baseCurrencyIdx++)
            {
                for (int quoteCurrencyIdx = 0; quoteCurrencyIdx < currencyCount; quoteCurrencyIdx++)
                {
                    if (baseCurrencyIdx == quoteCurrencyIdx)
                    {
                        continue;
                    }

                    foreach (MarketPrice marketPrice in this.prices[baseCurrencyIdx, quoteCurrencyIdx])
                    {
                        // Can only update prices on markets which are directly tradable; other markets
                        // infer their prices from the underlying exchange prices.
                        // As such, ignore any non-exchange-price types.
                        ExchangePrice exchangePrice = marketPrice as ExchangePrice;

                        if (null == exchangePrice)
                        {
                            continue;
                        }

                        if (exchangePrice.Exchange is VircurexExchange)
                        {
                            VircurexMarketId marketId = new VircurexMarketId(currencyCodes[baseCurrencyIdx],
                                currencyCodes[quoteCurrencyIdx]);

                            vircurexQuoteCurrencyCodes.Add(marketId.QuoteCurrencyCode);
                            vircurexPrices[marketId] = exchangePrice;
                            vircurex = (VircurexExchange)marketPrice.Exchange;
                        }
                        else
                        {
                            tasks.Add(exchangePrice.UpdatePriceAsync());
                        }
                    }
                }
            }

            // Perform data fetch for Vircurex currencies; these can be
            // done in a batch, so we do them once the rest of the data
            // requests are running
            foreach (string quoteCurrencyCode in vircurexQuoteCurrencyCodes)
            {
                Dictionary<MarketId, Book> books = vircurex.GetMarketOrdersAlt(quoteCurrencyCode).Result;

                foreach (MarketId marketId in books.Keys) {
                    ExchangePrice exchangePrice;

                    if (vircurexPrices.TryGetValue(marketId, out exchangePrice))
                    {
                        exchangePrice.MarketDepth = books[marketId];
                    }
                }
            }

            // Wait for all tasks to finish before we exit
            foreach (Task task in tasks)
            {
                task.Wait();
            }
        }
    }
}
