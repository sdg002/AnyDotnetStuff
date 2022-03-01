using System;
using System.Collections.Generic;
using System.Text;

namespace Contoso.TradingAggregator.Domain.interfaces
{
    /// <summary>
    /// Mocks the system clock
    /// </summary>
    public interface IClock
    {
        DateTime GetCurrentTime();
    }
}