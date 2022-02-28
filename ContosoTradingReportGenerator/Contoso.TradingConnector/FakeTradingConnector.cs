using Contoso.TradingAggregator.Domain.entity;
using Contoso.TradingAggregator.Domain.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.TradingConnector
{
    public class FakeTradingConnector : ITradingService
    {
        public FakeTradingConnector()
        {
        }

        /// <summary>
        /// Return all trades that have taken place starting from 11PM the previous day till now (including current hour)
        /// </summary>
        /// <param name="date">The current clock time (should be in GMT)</param>
        /// <returns></returns>
        public Task<List<TradePosition>> GetTradesAsync(DateTime date)
        {
            var random = new Random();
            var noOfDifferentCompaniesTraded = random.Next(3, 10);
            var results = new List<TradePosition>();

            for (int company = 0; company < noOfDifferentCompaniesTraded; company++)
            {
                int endingPeriod = 0;
                if (date.Hour >= 23)
                {
                    endingPeriod = 1;
                }
                else
                {
                    endingPeriod = date.Hour + 2;
                }
                var trades = new List<TradePeriod>();
                for (int period = 1; period <= endingPeriod; period++)
                {
                    trades.Add(new TradePeriod(period, random.Next(0, 1000)));
                }
                results.Add(new TradePosition(date, trades));
            }

            return Task.FromResult<List<TradePosition>>(results);
        }
    }
}