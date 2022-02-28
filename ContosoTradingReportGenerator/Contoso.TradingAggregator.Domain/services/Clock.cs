using Contoso.TradingAggregator.Domain.interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contoso.TradingAggregator.Domain.services
{
    public class Clock : IClock
    {
        private const string BritishTimeZone = "Greenwich Standard Time";

        public DateTime GetCurrentTime()
        {
            var zoneInfo = System.TimeZoneInfo.FindSystemTimeZoneById(BritishTimeZone);
            var gmtClockTime = System.TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zoneInfo);
            return gmtClockTime;
        }
    }
}