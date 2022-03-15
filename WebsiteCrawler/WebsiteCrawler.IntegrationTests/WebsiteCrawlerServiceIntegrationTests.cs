using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebsiteCrawler.Service;

namespace WebsiteCrawler.IntegrationTests
{
    [TestClass]
    public class WebsiteCrawlerServiceIntegrationTests
    {
        [TestMethod]
        public async Task When_bbccouk_Is_Crawled()
        {
            //Arrange
            var htmlParser = new HtmlAgilityParser();
            var httpClient = new HttpClient();
            var maxPagesToSearch = 20;
            var expectedResults = new string[]
            {
                "https://www.bbc.co.uk/sport",
                "https://www.bbc.co.uk/weather",
            };

            var crawler = new SingleThreadedWebSiteCrawler(
                this.CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                htmlParser,
                httpClient, maxPagesToSearch);

            //Act
            var urlToCrawl = "http://www.bbc.co.uk/";
            var crawlResults = await crawler.Run(urlToCrawl);

            //Assert
            crawlResults.Should().HaveCountGreaterThan(maxPagesToSearch);

            expectedResults.ToList().ForEach(link =>
            {
                crawlResults
                .Any(r => r.AbsoluteLink.ToString().Trim('/') == link)
                .Should()
                .BeTrue("The link {0} was not found in the crawl results", link);
            });
        }

        //TODO Add integration test for https://rxglobal.com

        /// <summary>
        /// Helps you view logging results in the Output Window of Visual Studio
        /// </summary>
        private ILogger<T> CreateOutputWindowLogger<T>()
        {
            var serviceProvider = new ServiceCollection().AddLogging(builder => builder.AddDebug()).BuildServiceProvider();
            return serviceProvider.GetService<ILogger<T>>();
        }
    }
}