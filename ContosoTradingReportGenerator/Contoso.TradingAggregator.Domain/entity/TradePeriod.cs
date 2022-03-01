using System;
using System.Collections.Generic;
using System.Text;

namespace Contoso.TradingAggregator.Domain.entity
{
    public class TradePeriod
    {
        public TradePeriod(int period, double volume)
        {
            Period = period;
            Volume = volume;
        }

        /// <summary>
        /// Gets the hours of the day for this trade position
        /// </summary>
        public int Period { get; }

        /// <summary>
        /// Gets the trading volume for this position
        /// </summary>
        public double Volume { get; }

        public override string ToString()
        {
            return $"Period={this.Period}   Volume={this.Volume}";
        }
    }
}