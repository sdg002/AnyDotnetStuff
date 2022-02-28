using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contoso.TradingAggregator.Domain.entity
{
    /// <summary>
    /// Represents all trades for a specific trade position grouped on an hourly basis.
    /// </summary>
    public class TradePosition
    {
        public TradePosition(DateTime date, IEnumerable<TradePeriod> periods)
        {
            this.Date = date;
            this.Periods = periods.ToList();
        }

        /// <summary>
        /// The date of the trade position
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        /// Gets all trade volumes on a hourly basis starting from the 11PM the previous day
        /// </summary>
        public List<TradePeriod> Periods { get; }

        public override string ToString()
        {
            return $"Date={Date}    Periods={this.Periods?.Count}";
        }
    }
}