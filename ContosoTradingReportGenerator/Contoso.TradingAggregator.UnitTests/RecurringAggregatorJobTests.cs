using Contoso.TradingAggregator.Domain.entity;
using Contoso.TradingAggregator.Domain.extensions;
using Contoso.TradingAggregator.Domain.infrastructure;
using Contoso.TradingAggregator.Domain.interfaces;
using Contoso.TradingAggregator.Jobs;
using Contoso.TradingReportsRepository;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contoso.TradingAggregator.UnitTests
{
    [TestClass]
    public class RecurringAggregatorJobTests
    {
        private static readonly string FakeCsvContents = "fake csv contents";
        private readonly List<AggregatedReport> _fakeReportsList = new List<AggregatedReport>();
        private readonly List<TradePosition> _fakeTradePositions = new List<TradePosition>();
        private Mock<ICsvGenerator> _csvGenerator;
        private DateTime _currentClockTime = DateTime.MinValue;
        private IClock _fakeClock;

        private IReportsRepo _reportsRepository;
        private ITradingService _tradingService;

        [TestInitialize]
        public void Init()
        {
            var mockClock = new Mock<IClock>();
            mockClock.Setup(x => x.GetCurrentTime()).Returns(() => { return _currentClockTime; });
            _fakeClock = mockClock.Object;

            var mockReportsRepo = new Mock<IReportsRepo>();
            mockReportsRepo
                .Setup(x => x.GetReports()).ReturnsAsync(() => _fakeReportsList.Cast<AggregatedReportBase>().ToList());

            mockReportsRepo
                .Setup(x => x.SaveReport(It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback((DateTime reportDate, string contents) =>
                {
                    var report = new AggregatedReport(reportDate, reportDate.ConvertDateToPeriod(), reportDate.ToReportFileName());
                    _fakeReportsList.Add(report);
                });
            _reportsRepository = mockReportsRepo.Object;

            var mockTradingService = new Mock<ITradingService>();
            mockTradingService
                .Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
                .ReturnsAsync((DateTime dt) => { return _fakeTradePositions.ToList(); });
            _tradingService = mockTradingService.Object;

            _csvGenerator = new Mock<ICsvGenerator>();
            _csvGenerator.Setup(x => x.GenerateCsv(It.IsAny<List<AggregatedTradePosition>>())).Returns(FakeCsvContents);
        }

        [TestMethod]
        public void The_Job_Must_Aggregate_The_Trades_Correctly()
        {
            //Arrange
            var job = new RecurringAggregatorJob(
                null,
                null,
                null,
                null,
                new TradingAggregatorJobConfig { MaximumSecondsDelayBetweenConsecutiveReporExtrations = 0 },
                null);

            //Act
            var dateOfTradePosition = new DateTime(2025, 12, 25, 23, 0, 0);
            var fakeTrades = new List<TradePosition>
            {
                new TradePosition(dateOfTradePosition, new TradePeriod[] { new TradePeriod(1, 100), new TradePeriod(2, 20) }),
                new TradePosition(dateOfTradePosition, new TradePeriod[] { new TradePeriod(1, 200), new TradePeriod(2, 120) })
            };
            var aggregatedTrades = job.CalculateAggregatedTrades(fakeTrades);

            //Assert
            aggregatedTrades.Count.Should().Be(2);

            aggregatedTrades[0].Period.Should().Be(1);
            aggregatedTrades[0].Hour.Should().Be(23);
            aggregatedTrades[0].Volume.Should().Be(300);

            aggregatedTrades[1].Period.Should().Be(2);
            aggregatedTrades[1].Hour.Should().Be(0);
            aggregatedTrades[1].Volume.Should().Be(140);
        }

        [TestMethod]
        public async Task When_Job_Runs_And_NoReports_HaveBeenExtracted_Then_The_Job_Must_Generate_The_Report_For_2300()
        {
            //Arrange
            var fewMinutesPast2300 = new DateTime(2025, 12, 25, 23, 15, 0);
            _currentClockTime = fewMinutesPast2300;

            int minutesOfDelayBetweenExecution = 10;
            var expectedDateTimeForFirstReport = new DateTime(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day, _currentClockTime.Hour, 0, 0)
                .AddMinutes(minutesOfDelayBetweenExecution);

            //Act
            var job = new RecurringAggregatorJob(
                NullLogger<RecurringAggregatorJob>.Instance,
                _reportsRepository,
                _tradingService,
                _fakeClock,
                new TradingAggregatorJobConfig { MaximumSecondsDelayBetweenConsecutiveReporExtrations = minutesOfDelayBetweenExecution * 60 },
                _csvGenerator.Object);
            await job.ExecuteEarliestReportGenerationAsync();

            //Assert
            _fakeReportsList.Count.Should().Be(1);
            _fakeReportsList[0].ReportDate.Should().Be(expectedDateTimeForFirstReport);
            _csvGenerator.Verify(m => m.GenerateCsv(It.IsAny<List<AggregatedTradePosition>>()), Times.Once);
        }

        [TestMethod]
        public async Task When_Job_Runs_And_Scheduled_ReportExtracts_HaveBeen_Missed_Then_Job_Must_Generate_MissedOut_Reports()
        {
            //Arrange
            var thirtyMinutesPast12AM = new DateTime(2025, 12, 26, 01, 30, 0);
            _currentClockTime = thirtyMinutesPast12AM;
            int minutesOfDelayBetweenExecution = 20;

            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day - 1, 23, 00));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day - 1, 23, 20));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day - 1, 23, 40));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day, 00, 00));
            //We have missed the report run for 00:20
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day, 00, 40));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day, 01, 00));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day, 01, 20));

            var expectedReportDateTime = new DateTime(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day, 00, 20, 0);

            var countOfReportsBeforeExecution = _fakeReportsList.Count;

            //Act
            var job = new RecurringAggregatorJob(
                NullLogger<RecurringAggregatorJob>.Instance,
                _reportsRepository,
                _tradingService,
                _fakeClock,
                new TradingAggregatorJobConfig { MaximumSecondsDelayBetweenConsecutiveReporExtrations = minutesOfDelayBetweenExecution * 60 },
                _csvGenerator.Object);
            await job.ExecuteEarliestReportGenerationAsync();

            //Assert
            _fakeReportsList.Count.Should().Be(countOfReportsBeforeExecution + 1);
            _fakeReportsList.Any(rpt => rpt.ReportDate == expectedReportDateTime).Should().BeTrue();
            _csvGenerator.Verify(m => m.GenerateCsv(It.IsAny<List<AggregatedTradePosition>>()), Times.Once);
        }

        [TestMethod]
        public async Task When_JobRuns_And_All_Scheduled_Reports_HaveBeen_Generated_Then_NoNewReport_MustBe_Generated()
        {
            //Arrange
            var thirtyMinutesPast1AM = new DateTime(2025, 12, 26, 01, 30, 0);
            _currentClockTime = thirtyMinutesPast1AM;
            int minutesOfDelayBetweenExecution = 20;

            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day - 1, 23, 00));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day - 1, 23, 20));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day - 1, 23, 40));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day, 00, 00));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day, 00, 20));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day, 00, 40));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day, 01, 00));
            _fakeReportsList.Add(CreateFakeReport(_currentClockTime.Year, _currentClockTime.Month, _currentClockTime.Day, 01, 20));

            var countOfReportsBeforeExecution = _fakeReportsList.Count;

            //Act
            var job = new RecurringAggregatorJob(
                NullLogger<RecurringAggregatorJob>.Instance,
                _reportsRepository,
                _tradingService,
                _fakeClock,
                new TradingAggregatorJobConfig { MaximumSecondsDelayBetweenConsecutiveReporExtrations = minutesOfDelayBetweenExecution * 60 },
                _csvGenerator.Object);
            await job.ExecuteEarliestReportGenerationAsync();

            //Assert
            _fakeReportsList.Count.Should().Be(countOfReportsBeforeExecution);
            _csvGenerator.Verify(m => m.GenerateCsv(It.IsAny<List<AggregatedTradePosition>>()), Times.Never);
        }

        private AggregatedReport CreateFakeReport(int year, int month, int day, int hour, int minute)
        {
            var date = new DateTime(year, month, day, hour, minute, 0);
            var fakeReport = new AggregatedReport(date, date.ConvertDateToPeriod(), date.ToReportFileName());
            return fakeReport;
        }
    }
}