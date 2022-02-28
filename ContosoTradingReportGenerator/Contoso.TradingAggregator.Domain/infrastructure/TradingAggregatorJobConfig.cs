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

        /// <summary>
        /// Gets or sets the duration for which the job will pause before the next extraction.
        /// This number should be smaller than the value <see cref="MaximumSecondsDelayBetweenConsecutiveReporExtrations"/> so that the
        /// job engine can has enough allowance to do a catch up in case of outage.
        /// </summary>
        public int SecondsDelayBetweenConsecutiveAttempts { get; set; }
    }
}