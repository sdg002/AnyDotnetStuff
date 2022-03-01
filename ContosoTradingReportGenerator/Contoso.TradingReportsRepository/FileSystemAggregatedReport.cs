using Contoso.TradingAggregator.Domain.entity;
using Dawn;
using System;
using System.Threading.Tasks;

namespace Contoso.TradingReportsRepository
{
    /// <summary>
    /// Represents file based aggregated Trading CSV report generated at a specified Date
    /// </summary>
    public class FileSystemAggregatedReport : AggregatedReportBase
    {
        private readonly string _absolutePathCsvFile;

        public FileSystemAggregatedReport(DateTime date, int period, string csvFile)
        {
            Guard.Argument(csvFile, nameof(csvFile)).NotEmpty("Absolute CSV file name expected");
            this.ReportDate = date;
            this.ReportPeriod = period;
            this._absolutePathCsvFile = csvFile;
        }

        public override Task<string> GetContents()
        {
            return Task.Run<string>(() => System.IO.File.ReadAllText(_absolutePathCsvFile));
        }

        public override string ToString()
        {
            return $"Date={ReportDate}  File={_absolutePathCsvFile}";
        }
    }
}