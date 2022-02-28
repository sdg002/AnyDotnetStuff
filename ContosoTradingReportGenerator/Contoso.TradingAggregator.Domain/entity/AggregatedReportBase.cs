using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.TradingAggregator.Domain.entity
{
    public abstract class AggregatedReportBase
    {
        /// <summary>
        /// Gets the Date when the report was generated
        /// </summary>
        public DateTime ReportDate { get; protected set; }

        /// <summary>
        /// Gets the period number for which this report repsents an aggregation
        /// </summary>
        /// <example>
        /// 1 for 23 hrs. 2 for 0-1am, 3 for 1-2am
        /// </example>
        public int ReportPeriod { get; protected set; }

        /// <summary>
        /// Fetches the raw contents of the report
        /// </summary>
        /// <returns></returns>
        public abstract Task<string> GetContents();
    }
}