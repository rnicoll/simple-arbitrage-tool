using Lostics.NCryptoExchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lostics.SimpleArbitrageTool
{
    public abstract class MarketPrice
    {
        public override bool Equals(object obj)
        {
            if (!obj.GetType().Equals(this.GetType())) {
                return false;
            }

            MarketPrice other = (MarketPrice)obj;

            return this.ExchangeLabel.Equals(other.ExchangeLabel)
                && this.Ask == other.Ask
                && this.Bid == other.Bid;
        }

        public override int GetHashCode()
        {
            int hash = 1;

            hash = (hash * 31) + this.ExchangeLabel.GetHashCode();
            if (null != this.Bid)
            {
                hash = (hash * 31) + this.Bid.GetHashCode();
            }
            if (null != this.Ask)
            {
                hash = (hash * 31) + this.Ask.GetHashCode();
            }

            return hash;
        }

        public abstract Task UpdatePrice();

        public abstract decimal? Ask { get; set;  }
        public abstract decimal? Bid { get; set;  }
        public abstract string ExchangeLabel { get; }
        public abstract bool IsTradeable { get; }
    }
}
