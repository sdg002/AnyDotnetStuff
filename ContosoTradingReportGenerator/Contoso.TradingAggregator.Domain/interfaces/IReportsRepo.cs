using Contoso.TradingAggregator.Domain.entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.TradingAggregator.Domain.interfaces
{
    /// <summary>
    /// A simple abstraction over the repository where reports are held.
    /// The primary motivation for this interface is to have a way to find out
    /// if a report for a specified time period has already been generated
    /// </summary>
    public interface IReportsRepo
    {
        /// <summary>
        /// Returns a list of all report names
        /// </summary>
        /// <returns></returns>
        Task<List<AggregatedReportBase>> GetReports();

        /// <summary>
        /// Saves a report into the repository
        /// </summary>
        /// <param name="reportDateTime">The report contains trade aggregations upto this specified date time</param>
        /// <param name="contents">The raw contents of the file in CSV format</param>
        /// <returns></returns>
        Task SaveReport(DateTime reportDateTime, string contents);
    }
}