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
        public static async Task<IEnumerable<string>> GetHighVolumeCurrencies<M, O>(AbstractExchange<M, O> exchange, int totalCurrencies)
            where M : MarketId
            where O : OrderId
        {
            List<Market<M>> markets = await exchange.GetMarkets();

            markets = markets.Where(x => x.QuoteCurrencyCode.Equals("BTC")).ToList();
            markets.Sort((m1, m2) => m2.Statistics.Volume24H.CompareTo(m1.Statistics.Volume24H));

            HashSet<string> currencies = new HashSet<string>();

            foreach (Market<M> market in markets)
            {
                currencies.Add(market.BaseCurrencyCode);

                if (currencies.Count >= totalCurrencies)
                {
                    break;
                }
            }

            return currencies;
        }
    }
}
