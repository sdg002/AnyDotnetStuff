using Contoso.TradingAggregator.Domain.entity;
using Contoso.TradingAggregator.Domain.infrastructure;
using Contoso.TradingAggregator.Domain.interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.TradingConnector
{
    /// <summary>
    /// This is the pseudo-implementation which connects to the actual Trading system.
    /// I am not certain what configuration parameters/environment variables would be required. Example: Connection string
    /// But, it should be fairly easy to provide those values by extending the configuration class
    /// </summary>
    [ExcludeFromCodeCoverage()]
    public class LiveTradingConnector : ITradingService
    {
        private readonly TradingAggregatorJobConfig _config;
        private readonly Services.PowerService _powerService = null;

        public LiveTradingConnector(TradingAggregatorJobConfig config)
        {
            _powerService = new Services.PowerService();
            _config = config;
        }

        public async Task<List<TradePosition>> GetTradesAsync(DateTime date)
        {
            var trades = (await _powerService.GetTradesAsync(date)).ToList();
            var results = new List<TradePosition>();
            trades.ForEach(trade =>
            {
                var periods = trade
                .Periods
                .Select(tp => new TradePeriod(tp.Period, tp.Volume))
                .ToList();
                var tradePosition = new TradePosition(date, periods);
                results.Add(tradePosition);
            });
            return results;
        }
    }
}