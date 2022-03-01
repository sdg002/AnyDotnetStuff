using Contoso.TradingAggregator.Domain.entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Contoso.TradingAggregator.Domain.interfaces
{
    public interface ICsvGenerator
    {
        string GenerateCsv(List<AggregatedTradePosition> trades);
    }
}