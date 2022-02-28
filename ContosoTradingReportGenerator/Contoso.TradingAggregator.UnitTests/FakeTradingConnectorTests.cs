using Contoso.TradingAggregator.Domain.interfaces;
using Contoso.TradingConnector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace Contoso.TradingAggregator.UnitTests
{
    [TestClass]
    public class FakeTradingConnectorTests
    {
        [TestMethod]
        [DataRow(05, 33, 7)]
        [DataRow(23, 01, 1)]
        [DataRow(22, 30, 24)]
        public async Task When_GetTradesAsync_Should_Return_All_Trades_From_11pm_Till_Current_Time(int currentHour, int currentMinute, int expectedMaxPeriod)
        {
            //Arrange
            var now = new DateTime(2025, 12, 25, currentHour, currentMinute, 0);

            //Act
            var tradingConnector = new FakeTradingConnector();
            var trades = await tradingConnector.GetTradesAsync(now);

            //Assert
            var minPeriod = trades.SelectMany(t => t.Periods).Min(p => p.Period);
            minPeriod.Should().Be(1);

            var maxPeriod = trades.SelectMany(t => t.Periods).Max(p => p.Period);
            maxPeriod.Should().Be(expectedMaxPeriod);
        }
    }
}