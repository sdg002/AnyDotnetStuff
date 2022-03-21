using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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
        [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
        public async Task When_Site_Is_Crawled_Then_Actual_Results_Must_Match_Expected(string urlToCrawl, int maxPagesToCrawl, int expectedLinks, string[] someExpectedLinks)
        {
            //Arrange
            var htmlParser = new HtmlAgilityParser();
            var httpClient = new HttpClient();

            var crawler = new SingleThreadedWebSiteCrawler(
                this.CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                htmlParser,
                httpClient);

            //Act
            var crawlResults = await crawler.Run(urlToCrawl, maxPagesToCrawl);

            //Assert
            crawlResults.Should().HaveCountGreaterThan(expectedLinks);

            someExpectedLinks.ToList().ForEach(link =>
            {
                crawlResults
                .Any(r => r.AbsoluteLink.ToString().Trim('/') == link)
                .Should()
                .BeTrue("The link {0} was not found in the crawl results", link);
            });
        }

        internal static IEnumerable<object[]> GetTestData()
        {
            yield return CreateTestDataAsArray(
                "https://rxglobal.com",
                10, 50,
                "https://rxglobal.com/rx-australia",
                "https://rxglobal.com/rx-korea",
                "https://rxglobal.com/join-us",
                "https://rxglobal.com/leadership-team");

            yield return CreateTestDataAsArray(
                "https://www.premierleague.com/",
                10, 300,
                "https://www.premierleague.com/photos",
                "https://www.premierleague.com/players",
                "https://www.premierleague.com/news");

            yield return CreateTestDataAsArray(
                "https://www.bbc.co.uk/",
                11, 300,
                "https://www.bbc.co.uk/sport",
                "https://www.bbc.co.uk/weather",
                "https://www.bbc.co.uk/weather/search",
                "https://www.bbc.co.uk/weather/warnings/floods",
                "https://www.bbc.co.uk/tv/cbbc");
        }

        private static object[] CreateTestDataAsArray(
            string siteUrl,
            int maxPagesToCrawl,
            int expectedCountOfLinks,
            params string[] someExpectedLinks)
        {
            return new object[] {
                siteUrl,
                maxPagesToCrawl,
                expectedCountOfLinks,
                someExpectedLinks
            };
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