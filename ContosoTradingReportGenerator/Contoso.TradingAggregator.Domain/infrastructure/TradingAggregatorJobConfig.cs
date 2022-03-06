using System;
using System.Collections.Generic;
using System.Text;

namespace Contoso.TradingAggregator.Domain.infrastructure
{
    /// <summary>
    /// The configuration options for the trading aggregator job
    /// </summary>
    public class TradingAggregatorJobConfig
    {
        /// <summary>
        /// Get/sets the maximum permitted seconds of interval between consecutive report extractions.
        /// No two consecutive report extractions should exceed this specified limit
        /// </summary>
        public int MaximumSecondsDelayBetweenConsecutiveReporExtrations { get; set; }

        /// <summary>
        /// Get/sets the path to the network folder where reports will be delivered. This is used by the <c></c>
        /// </summary>
        public string ReportsFolder { get; set; }
    }
}