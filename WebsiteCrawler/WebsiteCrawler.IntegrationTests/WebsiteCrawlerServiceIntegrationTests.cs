using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebsiteCrawler.Service;

namespace WebsiteCrawler.IntegrationTests
{
    [TestClass]
    public class WebsiteCrawlerServiceIntegrationTests
    {
        [TestMethod]
        public void MyTestMethod()
        {
            try
            {
                var u = new Uri("rx-middle-east");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            //TODO use Console logger or Output logger for integration tests
            var htmlParser = new HtmlAgilityParser();
            var httpClient = new HttpClient();
            var crawler = new SingleThreadedWebSiteCrawler(
                NullLogger<SingleThreadedWebSiteCrawler>.Instance,
                htmlParser,
                httpClient);

            var results = await crawler.Run("https://rxglobal.com/");

            results.Should().HaveCountGreaterThanOrEqualTo(1);
        }
    }
}