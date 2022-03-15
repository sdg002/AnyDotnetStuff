using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var someOfTheExpectedResults = new string[]
            {
                "https://www.bbc.co.uk/sport",
                "https://www.bbc.co.uk/weather",
            };

            var crawler = new SingleThreadedWebSiteCrawler(
                this.CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                htmlParser,
                httpClient);

            //Act
            var urlToCrawl = "http://www.bbc.co.uk/";
            var crawlResults = await crawler.Run(urlToCrawl, maxPagesToSearch);

            //Assert
            crawlResults.Should().HaveCountGreaterThan(maxPagesToSearch);

            someOfTheExpectedResults.ToList().ForEach(link =>
            {
                crawlResults
                .Any(r => r.AbsoluteLink.ToString().Trim('/') == link)
                .Should()
                .BeTrue("The link {0} was not found in the crawl results", link);
            });
        }

        [TestMethod]
        public async Task When_rxglobal_Is_Crawled()
        {
            //Arrange
            var htmlParser = new HtmlAgilityParser();
            var httpClient = new HttpClient();
            var maxPagesToSearch = 20;
            var someOfTheExpectedResults = new string[]
            {
                "https://rxglobal.com/rx-korea",
                "https://rxglobal.com/rx-brazil",
            };

            var crawler = new SingleThreadedWebSiteCrawler(
                this.CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                htmlParser,
                httpClient);

            //Act
            var urlToCrawl = "https://rxglobal.com";
            var crawlResults = await crawler.Run(urlToCrawl, maxPagesToSearch);

            //Assert
            crawlResults.Should().HaveCountGreaterThan(maxPagesToSearch);

            someOfTheExpectedResults.ToList().ForEach(link =>
            {
                crawlResults
                .Any(r => r.AbsoluteLink.ToString().Trim('/') == link)
                .Should()
                .BeTrue("The link {0} was not found in the crawl results", link);
            });
        }

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