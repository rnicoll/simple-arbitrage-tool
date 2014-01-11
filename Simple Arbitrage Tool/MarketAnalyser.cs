using Lostics.NCryptoExchange;
using Lostics.NCryptoExchange.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lostics.SimpleArbitrageTool
{
    public class MarketAnalyser
    {
        public static Dictionary<IExchange, List<Market>> GetHighVolumeMarkets(List<IExchange> exchanges,
            string referenceCurrencyCode, int maxCurrencies)
        {
            Dictionary<IExchange, Task<List<Market>>> allMarkets = new Dictionary<IExchange, Task<List<Market>>>();

            // Start fetching markets for all exchanges
            foreach (IExchange exchange in exchanges)
            {
                allMarkets.Add(exchange, exchange.GetMarkets());
            }

            // Find the most popular currencies
            Dictionary<string, decimal> currenciesByVolume = GetCurrenciesByVolume(allMarkets, referenceCurrencyCode);
                
            // Sort the highest volumes to determine cut-off
            List<decimal> highestVolumes = currenciesByVolume.Values.ToList();
                
            highestVolumes.Sort((v1, v2) => v2.CompareTo(v1));

            decimal cutOff = (decimal)0.00000000;

            if (highestVolumes.Count > maxCurrencies)
            {
                cutOff = highestVolumes[maxCurrencies - 1];
            }
                
            Dictionary<IExchange, List<Market>> validMarkets = new Dictionary<IExchange, List<Market>>();

            foreach (IExchange exchange in exchanges)
            {
                List<Market> markets = allMarkets[exchange].Result
                    .Where(x => currenciesByVolume[x.BaseCurrencyCode] >= cutOff)
                    .Where(x => currenciesByVolume[x.QuoteCurrencyCode] >= cutOff)
                    .ToList();
                validMarkets.Add(exchange, markets);
            }

            return validMarkets;
        }

        /// <summary>
        /// Breaks down the available currencies by volume traded against a single
        /// reference currency.
        /// </summary>
        /// <param name="allMarkets">A mapping from exchange to a list of markets</param>
        /// <returns>A mapping from currency code, to volume traded.</returns>
        private static Dictionary<string, decimal> GetCurrenciesByVolume(Dictionary<IExchange, Task<List<Market>>> allMarkets,
            string referenceCurrencyCode)
        {
            Dictionary<string, decimal> currenciesByVolume = new Dictionary<string, decimal>();

            foreach (IExchange exchange in allMarkets.Keys)
            {
                List<Market> markets = allMarkets[exchange].Result;

                foreach (Market market in markets)
                {
                    decimal volume;

                    if (currenciesByVolume.ContainsKey(market.BaseCurrencyCode))
                    {
                        volume = currenciesByVolume[market.BaseCurrencyCode];
                    }
                    else
                    {
                        volume = (decimal)0.00000000;
                    }

                    // Only count volume against a single currency, but make sure we log all currencies anyway
                    if (market.QuoteCurrencyCode.Equals(referenceCurrencyCode))
                    {
                        // We have to convert volume in the base currency across to the quote currency
                        if (market.Statistics.LastTrade > (decimal)0.00000000)
                        {
                            volume += (market.Statistics.Volume24HBase * market.Statistics.LastTrade);
                        }
                    }

                    currenciesByVolume[market.BaseCurrencyCode] = volume;
                }
            }

            // Generate volume value for reference currency
            currenciesByVolume[referenceCurrencyCode] = currenciesByVolume.Values.Sum();

            return currenciesByVolume;
        }
    }
}
