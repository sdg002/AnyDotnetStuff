using Contoso.TradingAggregator.Domain.entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.TradingAggregator.Domain.interfaces
{
    public interface ITradingService
    {
        Task<List<TradePosition>> GetTradesAsync(System.DateTime date);
    }
}