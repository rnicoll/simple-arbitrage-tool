using Lostics.NCryptoExchange;
using Lostics.NCryptoExchange.Model;
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

        public MarketMatrix(Dictionary<AbstractExchange, List<Market>> markets)
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
                    this.prices[baseCurrencyIdx, quoteCurrencyIdx] = new List<MarketPrice>();
                }

                this.prices[baseCurrencyIdx, baseCurrencyIdx].Add(new IdentityPrice());
            }

            // Insert placeholders for currency pairs which can be traded
            // directly.
            foreach (AbstractExchange exchange in markets.Keys) {
                foreach (Market market in markets[exchange])
                {
                    int baseCurrencyIdx = this.currencyIndices[market.BaseCurrencyCode];
                    int quoteCurrencyIdx = this.currencyIndices[market.QuoteCurrencyCode];

                    this.prices[baseCurrencyIdx, quoteCurrencyIdx].Add(
                        new ExchangePrice()
                        {
                            Exchange = exchange,
                            Market = market
                        }
                    );
                }
            }
        }

        private static string[] GetIndividualCurrencies(Dictionary<AbstractExchange, List<Market>> validMarkets)
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
            this.UpdateAllPrices();

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
                            string label = this.currencyCodes[baseCurrencyIdx] + "/"
                                + this.currencyCodes[quoteCurrencyIdx];
                            opportunities.Add(new ArbitrageOpportunity(label, lowestAsk, highestBid));
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

            // Start the data fetch running in parallel
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
                        tasks.Add(marketPrice.UpdatePrice());
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
