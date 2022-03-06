using Contoso.TradingAggregator.Domain.entity;
using Contoso.TradingAggregator.Domain.extensions;
using Contoso.TradingAggregator.Domain.infrastructure;
using Contoso.TradingAggregator.Domain.interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
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
        public const int RetryAttempts = 3;
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
            this.MaximumSecondsDelayBetweenConsecutiveReportExtractions = config.MaximumSecondsDelayBetweenConsecutiveReporExtrations;
            this._csvGenerator = csvGenerator;
        }

        private int MaximumSecondsDelayBetweenConsecutiveReportExtractions { get; set; }

        public List<AggregatedTradePosition> CalculateAggregatedTrades(List<TradePosition> trades)
        {
            var groupTradesByPeriod = trades.SelectMany(t => t.Periods).GroupBy(tp => tp.Period).ToList();
            var aggregatedPositions = groupTradesByPeriod.Select(g => new AggregatedTradePosition(g.Key, g.Sum(tp => tp.Volume)));
            return aggregatedPositions.OrderBy(ag => ag.Period).ToList();
        }

        /// <summary>
        /// Extracts the earlies pending report
        /// </summary>
        /// <returns>The count of pending reports, i.e. reports should have been already generated at the given clock time</returns>
        public async Task<int> ExecuteEarliestReportGenerationAsync()
        {
            DateTime currentClockTime = _clock.GetCurrentTime();
            DateTime startOfReportingDay = GetStartOfReportingDay();
            DateTime endOfReportingDay = GetEndOfReportingDay();

            _logger.LogInformation($"Executing job {currentClockTime}, startOfReportingDay={startOfReportingDay} , endOfReportingDay={endOfReportingDay}");

            AsyncRetryPolicy retryPolicy = CreateExponentialBackoffPolicy();

            var allExistingReports = await retryPolicy
                .ExecuteAsync<List<AggregatedReportBase>>(() => _reportsRepo.GetReports());

            var alreadyGeneratedReports = allExistingReports
                .Where(rpt => rpt.ReportDate > startOfReportingDay)
                .OrderBy(rpt => rpt.ReportDate)
                .ToList();

            _logger
                .LogInformation($"Found {alreadyGeneratedReports.Count} reports in the repository. Most recent being generated at '{alreadyGeneratedReports.LastOrDefault()?.ReportDate}'");

            List<DateTime> desiredReportExecutionTimings =
                GenerateTableOfDesiredReportExecutionTimings(startOfReportingDay, endOfReportingDay);

            var allReportsWhichHaveMissedOrNotYetGenerated = desiredReportExecutionTimings
                .SkipWhile(d => IsDesiredExecutionTimingAlreadyGenerated(d, alreadyGeneratedReports))
                .ToList();

            var allReportsWhichAreAlreadyDue = allReportsWhichHaveMissedOrNotYetGenerated
                .Where(d => d <= currentClockTime).ToList();

            if (allReportsWhichAreAlreadyDue.Count == 0)
            {
                _logger.LogInformation($"All reports generated for the given reporting date. Current time:{_clock.GetCurrentTime()}");
                return 0;
            }

            _logger.LogInformation($"There are {allReportsWhichAreAlreadyDue.Count} reports which are already due for generation");
            var firstMissingExecutionTiming = allReportsWhichAreAlreadyDue.First();

            _logger.LogInformation($"Going to fetch all trades for the date: {firstMissingExecutionTiming}");

            var trades = await retryPolicy
                .ExecuteAsync<List<TradePosition>>(() => _tradingConnector.GetTradesAsync(firstMissingExecutionTiming));

            _logger.LogInformation($"Found {trades.Count} trades for the date: {firstMissingExecutionTiming}");

            List<AggregatedTradePosition> aggregatedPositions = CalculateAggregatedTrades(trades);
            _logger.LogInformation($"The report will have {aggregatedPositions.Count} lines of data");
            string csvContents = CreateAggregatedCsvReport(aggregatedPositions);

            _logger.LogInformation($"The report generated at time {firstMissingExecutionTiming} will be saved");

            await retryPolicy.ExecuteAsync(() => _reportsRepo.SaveReport(firstMissingExecutionTiming, csvContents));

            return (allReportsWhichAreAlreadyDue.Count - 1);
        }

        public DateTime GetEndOfReportingDay()
        {
            return GetStartOfReportingDay().AddDays(+1).Date.AddHours(22).AddMinutes(59);
        }

        public DateTime GetStartOfReportingDay()
        {
            var currentDateTime = _clock.GetCurrentTime();
            return (currentDateTime.Hour >= 23) ? (currentDateTime.Date.AddHours(23)) : (currentDateTime.Date.AddDays(-1).AddHours(23));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    int pendingReports = await ExecuteEarliestReportGenerationAsync();
                    if (pendingReports == 0)
                    {
                        var delayMilliseconds = this.MaximumSecondsDelayBetweenConsecutiveReportExtractions * 1000;
                        _logger.LogInformation($"No pending report. Waiting for execution, {delayMilliseconds} ms");
                        await Task.Delay(delayMilliseconds, stoppingToken);
                    }
                    else
                    {
                        _logger.LogInformation($"{pendingReports} pending reports in the queue. Not going to wait ");
                        await Task.Delay(0, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception while calling method {nameof(ExecuteEarliestReportGenerationAsync)}");
                }
            }
        }

        private static AsyncRetryPolicy CreateExponentialBackoffPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                RetryAttempts,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
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
            _logger.LogInformation($"Generating table of expected report timings using the Max permitted delay between consecutive reports:{MaximumSecondsDelayBetweenConsecutiveReportExtractions} seconds");
            var desiredReportExecutionTimings = new List<DateTime>();

            while (true)
            {
                if (!desiredReportExecutionTimings.Any())
                {
                    desiredReportExecutionTimings.Add(startOfReportingDay.AddSeconds(MaximumSecondsDelayBetweenConsecutiveReportExtractions));
                }
                else
                {
                    desiredReportExecutionTimings.Add(desiredReportExecutionTimings.Last().AddSeconds(MaximumSecondsDelayBetweenConsecutiveReportExtractions));
                }

                if (desiredReportExecutionTimings.Last() >= endOfReportingDay)
                {
                    break;
                }
            }
            _logger.LogInformation($"Generated {desiredReportExecutionTimings.Count} execution timings");
            _logger.LogInformation($"First execution time in the current reporting day:{desiredReportExecutionTimings.First()}");
            _logger.LogInformation($"Last execution time in the current reporting day:{desiredReportExecutionTimings.Last()}");
            return desiredReportExecutionTimings;
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