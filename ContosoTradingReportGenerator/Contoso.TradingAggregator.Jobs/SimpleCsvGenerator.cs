using Contoso.TradingAggregator.Domain.entity;
using Contoso.TradingAggregator.Domain.interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Contoso.TradingAggregator.Jobs
{
    public class SimpleCsvGenerator : ICsvGenerator
    {
        public string GenerateCsv(List<AggregatedTradePosition> trades)
        {
            var rows = trades.Select(t => new { LocalTime = $"{t.Hour:D2}:{t.Minute:D2}", t.Volume });
            using (var memory = new MemoryStream())
            using (var writer = new StreamWriter(memory))
            using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(rows);
                csv.Flush();
                return System.Text.Encoding.UTF8.GetString(memory.ToArray());
            }
        }
    }
}