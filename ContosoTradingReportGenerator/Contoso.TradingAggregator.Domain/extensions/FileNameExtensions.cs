using System;
using System.Collections.Generic;
using System.Text;

namespace Contoso.TradingAggregator.Domain.extensions
{
    public static class FileNameExtensions
    {
        /// <summary>
        /// Converts a string like 'PowerPosition_YYYYMMDD_HHMM.csv' into a DateTime by parsing the year,month,day,hour and minute componetns
        /// </summary>
        /// <param name="reportFileName">Name of the file name</param>
        /// <returns>A System.DateTime</returns>
        public static DateTime? ToDateTime(this string reportFileName)
        {
            var frags = reportFileName.Split('_', '.');
            if (frags.Length != 4)
            {
                return null;
            }

            try
            {
                return DateTime.ParseExact(frags[1] + "_" + frags[2], "yyyyMMdd_HHmm", null);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a DateTime to a format PowerPosition_YYYYMMDD_HHMM.csv
        /// </summary>
        /// <param name="reportDate">A DateTime</param>
        /// <returns></returns>
        public static string ToReportFileName(this DateTime reportDate)
        {
            return string.Format("PowerPosition_{0}.csv", reportDate.ToString("yyyyMMdd_HHmm"));
        }
    }
}