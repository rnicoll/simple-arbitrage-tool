using Lostics.NCryptoExchange.CoinsE;
using Lostics.NCryptoExchange.Cryptsy;
using Lostics.NCryptoExchange.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lostics.SimpleArbitrageBot
{
    public class Core
    {
        public const string COINS_E_CONFIGURATION_FILENAME = "coins_e.conf";
        public const string CRYPTSY_CONFIGURATION_FILENAME = "cryptsy.conf";

        public static void Main()
        {
            using (CryptsyExchange cryptsy = CryptsyExchange.GetExchange(FindCryptsyConfigurationFile()))
            {
                using (CoinsEExchange coinsE = CoinsEExchange.GetExchange(FindCoinsEConfigurationFile()))
                {
                    DoTrading(cryptsy, coinsE);
                }
            }
        }

        private static void DoTrading(CryptsyExchange cryptsy, CoinsEExchange coinsE)
        {
            const int totalCurrencies = 10;
            IEnumerable<string> cryptsyCurrencies = MarketAnalyser.GetHighVolumeCurrencies(cryptsy, totalCurrencies).Result;
            IEnumerable<string> coinsECurrencies = MarketAnalyser.GetHighVolumeCurrencies(coinsE, totalCurrencies).Result;
            HashSet<string> currencies = new HashSet<string>(
                cryptsyCurrencies.Concat(coinsECurrencies)
            );

            Console.WriteLine("High volume currencies: ");
            foreach (string currencyCode in currencies)
            {
                Console.WriteLine(currencyCode);
            }

            Console.WriteLine("\nDone");
            Console.ReadKey();
        }

        private static FileInfo FindCoinsEConfigurationFile()
        {
            return new FileInfo(Path.Combine(GetConfigurationDirectory().FullName, COINS_E_CONFIGURATION_FILENAME));
        }

        private static DirectoryInfo GetConfigurationDirectory()
        {
            return new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent;
        }

        private static FileInfo FindCryptsyConfigurationFile()
        {
            return new FileInfo(Path.Combine(GetConfigurationDirectory().FullName, CRYPTSY_CONFIGURATION_FILENAME));
        }
    }
}
