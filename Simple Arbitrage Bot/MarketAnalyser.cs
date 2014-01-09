using Lostics.NCryptoExchange;
using Lostics.NCryptoExchange.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lostics.SimpleArbitrageBot
{
    public class MarketAnalyser
    {
        public static Dictionary<AbstractExchange, List<Market>> GetHighVolumeMarkets(List<AbstractExchange> exchanges)
        {
            const int totalCurrencies = 10;
            Dictionary<AbstractExchange, Task<List<Market>>> allMarkets = new Dictionary<AbstractExchange, Task<List<Market>>>();

            // Start fetching markets for all exchanges
            foreach (AbstractExchange exchange in exchanges)
            {
                allMarkets.Add(exchange, exchange.GetMarkets());
            }

            // Find the most popular currencies
            Dictionary<string, decimal> currenciesByVolume = new Dictionary<string, decimal>();

            foreach (AbstractExchange exchange in exchanges)
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

                    // Only count volume against BTC, but make sure we log all currencies anyway
                    if (market.QuoteCurrencyCode.Equals("BTC")) {
                        volume += market.Statistics.Volume24H;
                    }

                    currenciesByVolume[market.BaseCurrencyCode] = volume;
                }
            }
                
            // Sort the highest volumes to determine cut-off
            List<decimal> highestVolumes = currenciesByVolume.Values.ToList();
                
            highestVolumes.Sort((v1, v2) => v2.CompareTo(v1));

            decimal cutOff = (decimal)0.00000000;

            if (highestVolumes.Count > totalCurrencies)
            {
                cutOff = highestVolumes[totalCurrencies - 1];
            }

            // Generate value for BTC
            currenciesByVolume["BTC"] = currenciesByVolume.Values.Sum();
                
            Dictionary<AbstractExchange, List<Market>> validMarkets = new Dictionary<AbstractExchange, List<Market>>();

            foreach (AbstractExchange exchange in exchanges)
            {
                List<Market> markets = allMarkets[exchange].Result
                    .Where(x => currenciesByVolume[x.BaseCurrencyCode] >= cutOff)
                    .Where(x => currenciesByVolume[x.QuoteCurrencyCode] >= cutOff)
                    .ToList();
                validMarkets.Add(exchange, markets);
            }

            return validMarkets;
        }
    }
}
