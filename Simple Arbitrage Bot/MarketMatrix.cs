using Lostics.NCryptoExchange;
using Lostics.NCryptoExchange.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lostics.SimpleArbitrageBot
{
    public class MarketMatrix
    {
        private readonly Dictionary<string, int> currencyIndices;

        // The following arrays are ordered by base currency, then quote currency

        private readonly List<Market>[,] markets;
        private readonly List<Price>[,] prices;

        public MarketMatrix(Dictionary<AbstractExchange, List<Market>> markets)
        {
            string[] currencies = GetIndividualCurrencies(markets);
            this.currencyIndices = new Dictionary<string, int>();

            this.prices = new List<Price>[currencies.Length, currencies.Length];
            for (int currencyIdx = 0; currencyIdx < currencies.Length; currencyIdx++)
            {
                currencyIndices[currencies[currencyIdx]] = currencyIdx;
                this.prices[currencyIdx, currencyIdx] = new List<Price>()
                {
                    new IdentityPrice()
                };
            }

            this.markets = GetMarketsMatrix(this.currencyIndices, markets);
        }

        private static List<Market>[,] GetMarketsMatrix(Dictionary<string, int> currencies,
            Dictionary<AbstractExchange, List<Market>> markets)
        {
            List<Market>[,] marketMatrix = new List<Market>[currencies.Count, currencies.Count];

            foreach (List<Market> exchangeMarkets in markets.Values)
            {
                foreach (Market market in exchangeMarkets)
                {
                    List<Market> marketList = marketMatrix[currencies[market.BaseCurrencyCode], currencies[market.QuoteCurrencyCode]];

                    if (null == marketList)
                    {
                        marketList = new List<Market>();
                        marketMatrix[currencies[market.BaseCurrencyCode], currencies[market.QuoteCurrencyCode]] = marketList;
                    }
                    marketList.Add(market);
                }
            }

            return marketMatrix;
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

        public abstract class Price
        {
            public abstract bool IsValid { get; }
            public abstract bool IsTradeable { get; }
        }

        public class IdentityPrice : Price
        {
            public bool IsValid { get { return false; } }
            public bool IsTradeable { get { return false; } }
        }
    }
}
