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
        private readonly List<FileSystemAggregatedReport> _fakeReportsList = new List<FileSystemAggregatedReport>();
        private readonly List<TradePosition> _fakeTradePositions = new List<TradePosition>();
        private Mock<ICsvGenerator> _csvGenerator;
        private DateTime _currentClockTime = DateTime.MinValue;
        private IClock _fakeClock;

        private Mock<IReportsRepo> _mockReportsRepository;
        private Mock<ITradingService> _mockTradingService;

        [TestInitialize]
        public void Init()
        {
            var mockClock = new Mock<IClock>();
            mockClock.Setup(x => x.GetCurrentTime()).Returns(() => { return _currentClockTime; });
            _fakeClock = mockClock.Object;

            _mockReportsRepository = new Mock<IReportsRepo>();
            _mockReportsRepository
                .Setup(x => x.GetReports()).ReturnsAsync(() => _fakeReportsList.Cast<AggregatedReportBase>().ToList());

            _mockReportsRepository
                .Setup(x => x.SaveReport(It.IsAny<DateTime>(), It.IsAny<string>()))
                .Callback((DateTime reportDate, string contents) =>
                {
                    var report = new FileSystemAggregatedReport(reportDate, reportDate.ConvertDateToPeriod(), reportDate.ToReportFileName());
                    _fakeReportsList.Add(report);
                });

            _mockTradingService = new Mock<ITradingService>();
            _mockTradingService
                .Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
                .ReturnsAsync((DateTime dt) => { return _fakeTradePositions.ToList(); });

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
        [DataRow("2025-12-25T05:30", "2025-12-24T23:00", "2025-12-25T22:59")]
        [DataRow("2025-12-25T22:30", "2025-12-24T23:00", "2025-12-25T22:59")]
        [DataRow("2025-12-25T23:30", "2025-12-25T23:00", "2025-12-26T22:59")]
        public void The_StartOfReportingDay_And_EndOfReportingDay_MustBe_CalculatedCorrectly(
            string isoCurrentClockTime,
            string isoExpectedStartOfReportingDay,
            string isoExpectedEndOfReportingDay)
        {
            //Arrange
            _currentClockTime = DateTime.Parse(isoCurrentClockTime);
            var expectedStartOfReportingDate = DateTime.Parse(isoExpectedStartOfReportingDay);
            var expectedEndOfReportingDate = DateTime.Parse(isoExpectedEndOfReportingDay);

            var job = new RecurringAggregatorJob(
                null,
                null,
                null,
                _fakeClock,
                new TradingAggregatorJobConfig { MaximumSecondsDelayBetweenConsecutiveReporExtrations = 0 },
                null);

            //Act
            var actualStartOfReportingDate = job.GetStartOfReportingDay();
            var actualEndOfReportingDate = job.GetEndOfReportingDay();

            //Assert
            actualStartOfReportingDate.Should().Be(expectedStartOfReportingDate);
            actualEndOfReportingDate.Should().Be(expectedEndOfReportingDate);
        }

        [TestMethod]
        public async Task When_Job_Runs_And_Exception_WasThrown_By_ReportsRepository_Then_Exception_MustBe_PropagatedUpTheStack()
        {
            //Arrange
            var expectedExceptionMessage = $"some error while querying Trading service at {DateTime.Now}";

            var mockReportsRepo = new Mock<IReportsRepo>();
            mockReportsRepo
                .Setup(x => x.GetReports()).ReturnsAsync(() => throw new Exception(expectedExceptionMessage));

            _currentClockTime = DateTime.Now;
            int minutesOfDelayBetweenExecution = 20;

            //Act
            var job = new RecurringAggregatorJob(
                NullLogger<RecurringAggregatorJob>.Instance,
                mockReportsRepo.Object,
                _mockTradingService.Object,
                _fakeClock,
                new TradingAggregatorJobConfig { MaximumSecondsDelayBetweenConsecutiveReporExtrations = minutesOfDelayBetweenExecution * 60 },
                _csvGenerator.Object);

            Func<Task> act = async () => await job.ExecuteEarliestReportGenerationAsync();

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(expectedExceptionMessage);

            _mockTradingService.Verify(m => m.GetTradesAsync(It.IsAny<DateTime>()), Times.Never);

            mockReportsRepo.Verify(m => m.GetReports(), Times.AtLeast(RecurringAggregatorJob.RetryAttempts));
            mockReportsRepo.Verify(m => m.SaveReport(It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task When_Job_Runs_And_Exception_WasThrown_By_TradingService_Then_Exception_MustBe_PropagatedUpTheStack()
        {
            //Arrange
            var expectedExceptionMessage = $"some error while querying Trading service at {DateTime.Now}";
            var mockTradingService = new Mock<ITradingService>();
            mockTradingService
                .Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
                .ReturnsAsync((DateTime dt) => { throw new Exception(expectedExceptionMessage); });

            _currentClockTime = DateTime.Now;
            int minutesOfDelayBetweenExecution = 20;

            //Act
            var job = new RecurringAggregatorJob(
                NullLogger<RecurringAggregatorJob>.Instance,
                _mockReportsRepository.Object,
                mockTradingService.Object,
                _fakeClock,
                new TradingAggregatorJobConfig { MaximumSecondsDelayBetweenConsecutiveReporExtrations = minutesOfDelayBetweenExecution * 60 },
                _csvGenerator.Object);

            Func<Task> act = async () => await job.ExecuteEarliestReportGenerationAsync();

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(expectedExceptionMessage);
            mockTradingService.Verify(m => m.GetTradesAsync(It.IsAny<DateTime>()), Times.AtLeast(RecurringAggregatorJob.RetryAttempts));

            _mockReportsRepository.Verify(m => m.GetReports(), Times.Once);
            _mockReportsRepository.Verify(m => m.SaveReport(It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);
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
                _mockReportsRepository.Object,
                _mockTradingService.Object,
                _fakeClock,
                new TradingAggregatorJobConfig { MaximumSecondsDelayBetweenConsecutiveReporExtrations = minutesOfDelayBetweenExecution * 60 },
                _csvGenerator.Object);
            int pendingReports = await job.ExecuteEarliestReportGenerationAsync();

            //Assert
            pendingReports.Should().Be(0);
            _fakeReportsList.Count.Should().Be(1);
            _fakeReportsList[0].ReportDate.Should().Be(expectedDateTimeForFirstReport);
            _csvGenerator.Verify(m => m.GenerateCsv(It.IsAny<List<AggregatedTradePosition>>()), Times.Once);

            _mockTradingService.Verify(m => m.GetTradesAsync(It.IsAny<DateTime>()), Times.Once);

            _mockReportsRepository.Verify(m => m.GetReports(), Times.Once);
            _mockReportsRepository.Verify(m => m.SaveReport(It.IsAny<DateTime>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task When_Job_Runs_And_Scheduled_ReportExtracts_HaveBeen_Missed_Then_Job_Must_Generate_MissedOut_Report()
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
                _mockReportsRepository.Object,
                _mockTradingService.Object,
                _fakeClock,
                new TradingAggregatorJobConfig { MaximumSecondsDelayBetweenConsecutiveReporExtrations = minutesOfDelayBetweenExecution * 60 },
                _csvGenerator.Object);
            int pendingReports = await job.ExecuteEarliestReportGenerationAsync();

            //Assert
            pendingReports.Should().BeGreaterThan(0);
            _fakeReportsList.Count.Should().Be(countOfReportsBeforeExecution + 1);
            _fakeReportsList.Any(rpt => rpt.ReportDate == expectedReportDateTime).Should().BeTrue();
            _csvGenerator.Verify(m => m.GenerateCsv(It.IsAny<List<AggregatedTradePosition>>()), Times.Once);

            _mockTradingService.Verify(m => m.GetTradesAsync(It.IsAny<DateTime>()), Times.Once);

            _mockReportsRepository.Verify(m => m.GetReports(), Times.Once);
            _mockReportsRepository.Verify(m => m.SaveReport(It.IsAny<DateTime>(), It.IsAny<string>()), Times.Once);
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
                _mockReportsRepository.Object,
                _mockTradingService.Object,
                _fakeClock,
                new TradingAggregatorJobConfig { MaximumSecondsDelayBetweenConsecutiveReporExtrations = minutesOfDelayBetweenExecution * 60 },
                _csvGenerator.Object);
            var pendingReports = await job.ExecuteEarliestReportGenerationAsync();

            //Assert
            pendingReports.Should().Be(0);
            _fakeReportsList.Count.Should().Be(countOfReportsBeforeExecution);
            _csvGenerator.Verify(m => m.GenerateCsv(It.IsAny<List<AggregatedTradePosition>>()), Times.Never);

            _mockTradingService.Verify(m => m.GetTradesAsync(It.IsAny<DateTime>()), Times.Never);

            _mockReportsRepository.Verify(m => m.GetReports(), Times.Once);
            _mockReportsRepository.Verify(m => m.SaveReport(It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);
        }

        private FileSystemAggregatedReport CreateFakeReport(int year, int month, int day, int hour, int minute)
        {
            var date = new DateTime(year, month, day, hour, minute, 0);
            var fakeReport = new FileSystemAggregatedReport(date, date.ConvertDateToPeriod(), date.ToReportFileName());
            return fakeReport;
        }
    }
}