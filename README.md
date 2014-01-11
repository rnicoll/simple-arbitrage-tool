simple-arbitrage-tool
=====================

Very simple arbitrage tool for cryptocurrencies. Searches a number of different exchanges
for trade prices where there's potential for profit via arbitrage, by comparing prices
across the different exchanges.

This is not designed as a ready-to-use package, but instead primarily exists as a base
for others to form their own tools, and as a worked example for my cryptocurrency
exchange API abstraction library.

To compile, you'll need a copy of https://github.com/rnicoll/ncrypto-currency-exchange
It's strongly suggested that you review the code and understand the underlying analysis
process. The application runs as a console tool at the moment.

A summary of the analysis process is provided below for reference:

1. Find currencies with a high trading volume against BTC.
2. For the top dozen or so currencies, find all markets involving those currencies.
3. Get prices for all of those markets, across all available exchanges.
4. Infer prices for currencies, doing indirect trading (i.e. trading BTC/DOGE via LTC instead of directly).
5. Compare prices, and look for cases where lowest ask is below the highest bid price.
6. Display best prices where lowest ask is below highest bid.

The listed trades are **only intended as suggestions**; you should understand trading, the
risks and costs in doing so, etc. and do your own analysis before actually performing
any trade.

This document, the enclosed code, and its output are not intended as investment advice.

