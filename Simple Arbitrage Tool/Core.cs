using Lostics.NCryptoExchange;
using Lostics.NCryptoExchange.CoinsE;
using Lostics.NCryptoExchange.Cryptsy;
using Lostics.NCryptoExchange.Model;
using Lostics.NCryptoExchange.Vircurex;
using System;
using System.Collections.Generic;
using System.IO;

namespace Lostics.SimpleArbitrageTool
{
    public class Core
    {
        public const string COINS_E_CONFIGURATION_FILENAME = "coins_e.conf";
        public const string CRYPTSY_CONFIGURATION_FILENAME = "cryptsy.conf";
        public const string VIRCUREX_CONFIGURATION_FILENAME = "vircurex.conf";

        public static void Main()
        {
            using (CryptsyExchange cryptsy = CryptsyExchange.GetExchange(FindCryptsyConfigurationFile()))
            {
                using (CoinsEExchange coinsE = CoinsEExchange.GetExchange(FindCoinsEConfigurationFile()))
                {
                    using (VircurexExchange vircurex = new VircurexExchange())
                    {
                        DoAnalysis(new List<AbstractExchange>() {
                            cryptsy,
                            coinsE,
                            vircurex
                        });
                    }
                }
            }
        }

        private static void DoAnalysis(List<AbstractExchange> exchanges)
        {
            const int maxCurrencies = 12;
            Dictionary<AbstractExchange, List<Market>> validMarkets
                = MarketAnalyser.GetHighVolumeMarkets(exchanges, "BTC", maxCurrencies);
            MarketMatrix marketMatrix = new MarketMatrix(validMarkets);

            foreach (ArbitrageOpportunity opportunity in marketMatrix.GetArbitrageOpportunities()) {
                Console.WriteLine(opportunity.ToString());
            }

            Console.WriteLine("\nDone");
            Console.ReadKey();
        }

        private static FileInfo FindCoinsEConfigurationFile()
        {
            return new FileInfo(Path.Combine(GetConfigurationDirectory().FullName,
                COINS_E_CONFIGURATION_FILENAME));
        }

        private static DirectoryInfo GetConfigurationDirectory()
        {
            return new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent;
        }

        private static FileInfo FindCryptsyConfigurationFile()
        {
            return new FileInfo(Path.Combine(GetConfigurationDirectory().FullName,
                CRYPTSY_CONFIGURATION_FILENAME));
        }

        private static FileInfo FindVircurexConfigurationFile()
        {
            return new FileInfo(Path.Combine(GetConfigurationDirectory().FullName,
                VIRCUREX_CONFIGURATION_FILENAME));
        }
    }
}
