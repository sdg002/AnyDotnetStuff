using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WebApplication1.Controllers;

namespace UnitTestProject1
{
    /// <summary>
    /// Toy unit tests for our Weather forecase API
    /// </summary>
    [TestClass]
    public class WeatherForecastControllerTests
    {
        [TestMethod]
        public void When_WeatherForecastController()
        {
            var logger = NullLogger<WeatherForecastController>.Instance;
            var api = new WebApplication1.Controllers.WeatherForecastController(logger);
            var forecasts = api.Get().ToList();

            //Assert
            Assert.IsTrue(forecasts.Count > 2);

            foreach (var forecast in forecasts)
            {
                Assert.AreEqual(forecast.TemperatureF, (forecast.TemperatureC * 9 / 5) + 32, 1);
            }
        }
    }
}