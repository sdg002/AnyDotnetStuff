using Contoso.TradingAggregator.Domain.entity;
using Contoso.TradingAggregator.Domain.extensions;
using Contoso.TradingAggregator.Domain.infrastructure;
using Contoso.TradingAggregator.Domain.interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Contoso.TradingAggregator.Jobs
{
    public class RecurringAggregatorJob : Microsoft.Extensions.Hosting.BackgroundService
    {
        private const int ReportExecutionTimingThresholdSeconds = 60;
        private readonly IClock _clock;
        private readonly ICsvGenerator _csvGenerator;
        private readonly ILogger<RecurringAggregatorJob> _logger;
        private readonly IReportsRepo _reportsRepo;
        private readonly ITradingService _tradingConnector;

        public RecurringAggregatorJob(
            ILogger<RecurringAggregatorJob> logger,
            IReportsRepo reportsRepo,
            ITradingService tradingConnector,
            IClock clock,
            TradingAggregatorJobConfig config,
            ICsvGenerator csvGenerator)
        {
            this._logger = logger;
            this._reportsRepo = reportsRepo;
            this._tradingConnector = tradingConnector;
            this._clock = clock;
            this.MaximumDelayBetweenConsecutiveReportExtractions = config.MaximumSecondsDelayBetweenConsecutiveReporExtrations;
            this._csvGenerator = csvGenerator;

            this.DelayBetweenConsecutiveAttempts = config.SecondsDelayBetweenConsecutiveAttempts;
        }

        private int DelayBetweenConsecutiveAttempts { get; set; }
        private int MaximumDelayBetweenConsecutiveReportExtractions { get; set; }

        public List<AggregatedTradePosition> CalculateAggregatedTrades(List<TradePosition> trades)
        {
            var groupTradesByPeriod = trades.SelectMany(t => t.Periods).GroupBy(tp => tp.Period).ToList();
            var aggregatedPositions = groupTradesByPeriod.Select(g => new AggregatedTradePosition(g.Key, g.Sum(tp => tp.Volume)));
            return aggregatedPositions.OrderBy(ag => ag.Period).ToList();
        }

        public async Task ExecuteEarliestReportGenerationAsync()
        {
            _logger.LogInformation($"Executing job {_clock.GetCurrentTime()}");

            DateTime currentClockTime = _clock.GetCurrentTime();
            DateTime startOfReportingDay = GetStartOfReportingDay();
            DateTime endOfReportingDay = GetEndOfReportingDay();

            var alreadyGeneratedReports = (await _reportsRepo.GetReports())
                .Where(rpt => rpt.ReportDate > startOfReportingDay)
                .OrderBy(rpt => rpt.ReportDate)
                .ToList();

            _logger.LogInformation($"Found {alreadyGeneratedReports.Count} reports in the repository. Most recent being generated at {alreadyGeneratedReports.LastOrDefault()?.ReportDate}");

            List<DateTime> desiredReportExecutionTimings = GenerateTableOfDesiredReportExecutionTimings(startOfReportingDay, endOfReportingDay);

            var firstMissingExecutionTiming = desiredReportExecutionTimings
                .SkipWhile(d => IsDesiredExecutionTimingAlreadyGenerated(d, alreadyGeneratedReports))
                .FirstOrDefault();

            if (firstMissingExecutionTiming >= currentClockTime)
            {
                _logger.LogInformation($"All reports generated for the given reporting date. Current time:{_clock.GetCurrentTime()}");
                return;
            }

            _logger.LogInformation($"Going to fetch all trades for the date: {firstMissingExecutionTiming}");
            var trades = await _tradingConnector.GetTradesAsync(firstMissingExecutionTiming);
            _logger.LogInformation($"Found {trades.Count} trades for the date: {firstMissingExecutionTiming}");

            List<AggregatedTradePosition> aggregatedPositions = CalculateAggregatedTrades(trades);
            _logger.LogInformation($"The report will have {aggregatedPositions.Count} lines of data");
            string csvContents = CreateAggregatedCsvReport(aggregatedPositions);

            _logger.LogInformation($"The report generated at time {firstMissingExecutionTiming} will be saved");
            await _reportsRepo.SaveReport(firstMissingExecutionTiming, csvContents);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteEarliestReportGenerationAsync();
                _logger.LogInformation($"Waiting for execution, {this.DelayBetweenConsecutiveAttempts} seconds");
                await Task.Delay(this.DelayBetweenConsecutiveAttempts * 1000, stoppingToken);
            }
        }

        private bool ApproximateDateTimeMatch(DateTime dateTime1, DateTime dateTime2)
        {
            return Math.Abs((dateTime1 - dateTime2).TotalSeconds) < ReportExecutionTimingThresholdSeconds;
        }

        private string CreateAggregatedCsvReport(List<AggregatedTradePosition> trades)
        {
            return _csvGenerator.GenerateCsv(trades);
        }

        private List<DateTime> GenerateTableOfDesiredReportExecutionTimings(DateTime startOfReportingDay, DateTime endOfReportingDay)
        {
            _logger.LogInformation($"Generating table of expected report timings using the Max permitted delay between consecutive reports:{MaximumDelayBetweenConsecutiveReportExtractions}");
            var desiredReportExecutionTimings = new List<DateTime>();

            while (true)
            {
                if (!desiredReportExecutionTimings.Any())
                {
                    desiredReportExecutionTimings.Add(startOfReportingDay.AddSeconds(MaximumDelayBetweenConsecutiveReportExtractions));
                }
                else
                {
                    desiredReportExecutionTimings.Add(desiredReportExecutionTimings.Last().AddSeconds(MaximumDelayBetweenConsecutiveReportExtractions));
                }

                if (desiredReportExecutionTimings.Last() >= endOfReportingDay)
                {
                    break;
                }
            }
            desiredReportExecutionTimings.ForEach(timing => _logger.LogInformation($"{timing}"));
            return desiredReportExecutionTimings;
        }

        private DateTime GetEndOfReportingDay()
        {
            return GetStartOfReportingDay().AddDays(+1).Date.AddHours(23).AddMinutes(59);
        }

        private DateTime GetStartOfReportingDay()
        {
            var currentDateTime = _clock.GetCurrentTime();
            return (currentDateTime.Hour >= 23) ? (currentDateTime.Date.AddHours(23)) : (currentDateTime.Date.AddDays(-1).AddHours(23));
        }

        /// <summary>
        /// Returns True, if there is already a report for the specified execution time.
        /// </summary>
        /// <param name="desiredExecutionTiming">Execution time </param>
        /// <param name="alreadyGeneratedReports">Reports from the reports repository</param>
        /// <returns>True/False</returns>
        private bool IsDesiredExecutionTimingAlreadyGenerated(
            DateTime desiredExecutionTiming,
            List<AggregatedReportBase> alreadyGeneratedReports)
        {
            if (!alreadyGeneratedReports.Any())
            {
                return false;
            }
            return alreadyGeneratedReports.Any(r => ApproximateDateTimeMatch(r.ReportDate, desiredExecutionTiming));
        }
    }
}