using Contoso.TradingAggregator.Domain.interfaces;
using Contoso.TradingReportsRepository;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Contoso.TradingAggregator.UnitTests
{
    [TestClass]
    public class NetworkFileShareReportsRepositoryTests
    {
        private string _baseFolder = null;

        [TestCleanup]
        public void Cleanup()
        {
            if (System.IO.Directory.Exists(_baseFolder))
            {
                System.IO.Directory.Delete(_baseFolder, true);
            }
        }

        [TestInitialize]
        public void Init()
        {
            _baseFolder = System.IO.Path.Join(System.Environment.GetEnvironmentVariable("TEMP"), $"contoso-reports-{Guid.NewGuid()}");
            System.IO.Directory.CreateDirectory(_baseFolder);
        }

        [TestMethod]
        public async Task When_CSV_FilesAvailable_ShouldReturn_Reports()
        {
            //Arrange
            var repo = new NetworkFileShareReportsRepository(_baseFolder, NullLogger<NetworkFileShareReportsRepository>.Instance);
            var expectedDate1 = new DateTime(2022, 12, 25, 08, 33, 0);
            var expectedPeriod1 = 10;

            var expectedDate2 = new DateTime(2023, 02, 28, 12, 01, 0);
            var expectedPeriod2 = 14;

            System.IO.File.WriteAllText(_baseFolder + "\\PowerPosition_20221225_0833.csv", "some dummy csv contents");
            System.IO.File.WriteAllText(_baseFolder + "\\PowerPosition_20230228_1201.csv", "some dummy csv contents");
            System.IO.File.WriteAllText(_baseFolder + "\\SomeNonCsvFile.txt", "some other file contents");

            //Act
            var reports = await repo.GetReports();

            //Assert
            reports.Should().HaveCount(2);
            reports[0].ReportDate.Should().Be(expectedDate1);
            reports[0].ReportPeriod.Should().Be(expectedPeriod1);

            reports[1].ReportDate.Should().Be(expectedDate2);
            reports[1].ReportPeriod.Should().Be(expectedPeriod2);
        }

        [TestMethod]
        public async Task When_NoFilesAvailabale_ShouldReturn_Zero_Reports()
        {
            //Arrange
            var repo = new NetworkFileShareReportsRepository(_baseFolder, NullLogger<NetworkFileShareReportsRepository>.Instance);

            //Act
            var reports = await repo.GetReports();

            //Assert
            reports.Should().BeEmpty();
        }

        [TestMethod]
        public async Task When_SaveReport_Then_NewFile_MustBe_Created()
        {
            //Arrange
            var repo = new NetworkFileShareReportsRepository(_baseFolder, NullLogger<NetworkFileShareReportsRepository>.Instance);
            var expectedDate = new DateTime(2030, 12, 25, 09, 05, 0);

            //Act
            await repo.SaveReport(expectedDate, "some contents");

            //Assert
            var reports = await repo.GetReports();
            reports.Should().HaveCount(1);
            reports[0].ReportDate.Should().Be(expectedDate);

            var reportFiles = System.IO.Directory.GetFiles(_baseFolder, "*.csv");
            reportFiles.Should().HaveCount(1);
            System.IO.Path.GetFileName(reportFiles[0]).Should().Be("PowerPosition_20301225_0905.csv");
        }
    }
}