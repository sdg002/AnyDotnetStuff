using Dawn;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contoso.TradingAggregator.Domain.entity
{
    /// <summary>
    /// Represents the summation of all Trades for a specified hourly period (1-24). The period start
    /// </summary>
    public class AggregatedTradePosition
    {
        public AggregatedTradePosition(int period, double volume)
        {
            Guard.Argument(period, nameof(Period)).GreaterThan(0).LessThan(25);
            this.Period = period;
            this.Volume = volume;
            this.Hour = (this.Period == 1) ? (23) : (this.Period - 2);
            this.Minute = 0;
        }

        /// <summary>
        /// Gets the Hour of the local time
        /// </summary>
        public int Hour { get; protected set; }

        /// <summary>
        /// Gets the Minute of the local time
        /// </summary>
        public int Minute { get; protected set; }

        /// <summary>
        /// Gets/sets the period of trade.
        /// </summary>
        /// <example>
        /// The time from 23:00 to 23:59 is period 1
        /// The time from 22:00 to 22:29 is period 24
        /// </example>
        public int Period { get; }

        public double Volume { get; }
    }
}