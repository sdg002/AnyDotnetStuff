using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.TradingAggregator.Host
{
    public class TradingAggregatorOptions
    {
        public int DelayBetweenExecutions { get; set; }
        public string ReportsRepositoryFolder { get; set; }
    }
}