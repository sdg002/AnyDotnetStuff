using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contoso.TradingAggregator.Domain.extensions;
using FluentAssertions;

namespace Contoso.TradingAggregator.UnitTests.extensiontests
{
    [TestClass]
    public class PeriodExtensionsTests
    {
        [TestMethod]
        [DataRow("00:00", 2)]
        [DataRow("00:30", 2)]
        [DataRow("00:59", 2)]
        [DataRow("01:00", 3)]
        [DataRow("05:00", 7)]
        [DataRow("14:00", 16)]
        [DataRow("23:30", 1)]
        public void ConvertDateTimeToPeriod(string HHmm, int expectedPeriod)
        {
            var date = DateTime.ParseExact(HHmm, "HH:mm", null);

            int actualPeriod = date.ConvertDateToPeriod();

            actualPeriod.Should().Be(expectedPeriod);
        }

        [TestMethod]
        [DataRow(1, 23)]
        [DataRow(2, 0)]
        [DataRow(24, 22)]
        public void ConvertPeriodToDate(int period, int expectedHour)
        {
            var now = DateTime.Now;
            var date = period.ConvertPeriodToDate(now);

            date.Hour.Should().Be(expectedHour);
        }
    }
}