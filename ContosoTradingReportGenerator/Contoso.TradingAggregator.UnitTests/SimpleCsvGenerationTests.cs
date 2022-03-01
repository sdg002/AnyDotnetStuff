using Contoso.TradingAggregator.Domain.entity;
using Contoso.TradingAggregator.Jobs;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Contoso.TradingAggregator.UnitTests
{
    [TestClass]
    public class SimpleCsvGenerationTests
    {
        [TestMethod]
        public void When_3_Trades_Then_The_CSV_File_MustHave_4_Lines()
        {
            //Arrange
            var aggregatedTrades = new List<AggregatedTradePosition>
            {
                new AggregatedTradePosition(1, 111),
                new AggregatedTradePosition(2, 222),
                new AggregatedTradePosition(3, 333)
            };

            //Act
            var generator = new SimpleCsvGenerator();
            var actualOutput = generator.GenerateCsv(aggregatedTrades);

            //Assert
            var actualLines = actualOutput.Split("\r\n", System.StringSplitOptions.RemoveEmptyEntries);
            actualLines[0].Should().Be("LocalTime,Volume");
            actualLines.Length.Should().Be(4);
            actualLines[1].Should().Be("23:00,111");
            actualLines[2].Should().Be("00:00,222");
            actualLines[3].Should().Be("01:00,333");
        }

        [TestMethod]
        public void When_No_Trades_Then_OnlyHeaderRowMustBe_Present()
        {
            //Arrange
            var aggregatedTrades = new List<AggregatedTradePosition>();

            //Act
            var generator = new SimpleCsvGenerator();
            var actualOutput = generator.GenerateCsv(aggregatedTrades);

            //Assert
            var actualLines = actualOutput.Split("\r\n", System.StringSplitOptions.RemoveEmptyEntries);
            actualLines[0].Should().Be("LocalTime,Volume");
            actualLines.Length.Should().Be(1);
        }
    }
}