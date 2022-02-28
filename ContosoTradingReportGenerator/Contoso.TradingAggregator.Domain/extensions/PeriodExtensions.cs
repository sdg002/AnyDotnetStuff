using System;
using System.Collections.Generic;
using System.Text;

namespace Contoso.TradingAggregator.Domain.extensions
{
    public static class PeriodExtensions
    {
        public static DateTime ConvertPeriodToDate(this int period, DateTime clockTime)
        {
            if (period == 1)
            {
                var previousDay = clockTime.AddDays(-1);
                return new DateTime(previousDay.Year, previousDay.Month, previousDay.Day, 23, 59, 59);
            }
            else
            {
                return new DateTime(clockTime.Year, clockTime.Month, clockTime.Day, period - 2, 59, 59);
            }
        }

        public static int ConvertDateToPeriod(this DateTime date)
        {
            return (date.Hour >= 23) ? 1 : (date.Hour + 2);
        }
    }
}