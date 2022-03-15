using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WebsiteCrawler.Service;

namespace WebsiteCrawler.UnitTests
{
    [TestClass]
    public class WebsiteCrawlerUnitTests
    {
        private HtmlAgilityParser _htmlParser;

        [TestInitialize]
        public void Init()
        {
            _htmlParser = new HtmlAgilityParser();
        }

        [TestMethod]
        public async Task When_HttpError_Then_Retry_ShouldBe_Attempted()
        {
            //Arrange
            Mock<HttpMessageHandler> handlerMock = Mocks.CreateHttpMessageHandlerMock(
                @"<html>HTML without some internal achor tags  <a href='link1'>Link 1<a> <a href='#bookmark2'>Bookmark<a></html>",
                HttpStatusCode.GatewayTimeout,
                "text/html");

            var httpClient = new HttpClient(handlerMock.Object);
            var site = "http://www.contoso.com";

            var crawler = new SingleThreadedWebSiteCrawler(
                CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                _htmlParser,
                httpClient);

            //Act
            var crawlerResults = await crawler.Run(site, 5);

            //Assert
            crawlerResults.Should().HaveCount(0);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(SingleThreadedWebSiteCrawler.RetryAttempts + 1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        [Ignore("Work in progress")]
        public void When_MaxPages_Exceeded_Limit_Then_Crawler_Must_Stop()
        {
            //TODO Test for an upper limit on how many pages to scan
        }

        [TestMethod]
        public async Task When_Page_ContentType_IsNot_Html_Then_Crawler_Must_Return_Zero_Hyperlinks()
        {
            //Arrange
            Mock<HttpMessageHandler> handlerMock = Mocks.CreateHttpMessageHandlerMock(
                @"<html>HTML without any anchor tags  <a href='somelink.html'></a></html>",
                HttpStatusCode.OK,
                "text/plain");

            var httpClient = new HttpClient(handlerMock.Object);
            var site = "http://www.sitewithnolinks.com";

            var crawler = new SingleThreadedWebSiteCrawler(
                CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                _htmlParser,
                httpClient);

            //Act
            var crawlerResults = await crawler.Run(site, 5);

            //Assert
            crawlerResults.Should().BeEmpty();
        }

        [TestMethod]
        public async Task When_Page_Has_BookMarks_Then_Crawler_Must_Ignore_BookMarks()
        {
            //Arrange
            Mock<HttpMessageHandler> handlerMock = Mocks.CreateHttpMessageHandlerMock(
                @"<html>HTML without some internal achor tags  <a href='link1'>Link 1<a> <a href='#bookmark2'>Bookmark<a></html>",
                HttpStatusCode.OK,
                "text/html");

            var httpClient = new HttpClient(handlerMock.Object);
            var site = "http://www.contoso.com";

            var crawler = new SingleThreadedWebSiteCrawler(
                CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                _htmlParser,
                httpClient);

            var expectedResults = new string[]
            {
                "http://www.contoso.com/link1",
            };

            //Act
            var crawlerResults = await crawler.Run(site, 5);

            //Assert
            crawlerResults.Should().HaveCount(1);
            expectedResults.ToList().ForEach(link =>
            {
                crawlerResults
                .Any(r => r.AbsoluteLink.ToString().Trim('/') == link)
                .Should()
                .BeTrue("The link {0} was not found in the crawl results", link);
            });
        }

        [TestMethod]
        public async Task When_Page_Has_External_Links_Then_Crawler_Must_Ignore_External_Hyperlinks()
        {
            //Arrange
            Mock<HttpMessageHandler> handlerMock = Mocks.CreateHttpMessageHandlerMock(
                @"<html>HTML without some internal achor tags  <a href='link1'>Link 1<a> <a href='http://www.cnn.com'>External link<a></html>",
                HttpStatusCode.OK,
                "text/html");

            var httpClient = new HttpClient(handlerMock.Object);
            var site = "http://www.contoso.com";

            var crawler = new SingleThreadedWebSiteCrawler(
                CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                _htmlParser,
                httpClient);

            var expectedResults = new string[]
            {
                "http://www.contoso.com/link1",
            };

            //Act
            var crawlerResults = await crawler.Run(site, 5);

            //Assert
            crawlerResults.Should().HaveCount(1);
            expectedResults.ToList().ForEach(link =>
            {
                crawlerResults
                .Any(r => r.AbsoluteLink.ToString().Trim('/') == link)
                .Should()
                .BeTrue("The link {0} was not found in the crawl results", link);
            });
        }

        [TestMethod]
        public async Task When_Page_Has_Internal_Links_Then_Crawler_Must_Return_Hyperlinks()
        {
            //Arrange
            Mock<HttpMessageHandler> handlerMock = Mocks.CreateHttpMessageHandlerMock(
                @"<html>HTML without some internal achor tags  <a href='link1'>Link 1<a> <a href='link2'>Link 2<a></html>",
                HttpStatusCode.OK,
                "text/html");

            var httpClient = new HttpClient(handlerMock.Object);
            var site = "http://www.contoso.com";

            var crawler = new SingleThreadedWebSiteCrawler(
                CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                _htmlParser,
                httpClient);

            var expectedResults = new string[]
            {
                "http://www.contoso.com/link1",
                "http://www.contoso.com/link2",
            };

            //Act
            var crawlerResults = await crawler.Run(site, 5);

            //Assert
            crawlerResults.Should().HaveCount(2);
            expectedResults.ToList().ForEach(link =>
            {
                crawlerResults
                .Any(r => r.AbsoluteLink.ToString().Trim('/') == link)
                .Should()
                .BeTrue("The link {0} was not found in the crawl results", link);
            });
        }

        [TestMethod]
        public async Task When_Page_Has_MailToLinks_Then_Crawler_Must_Ignore_MailTo()
        {
            //Arrange
            Mock<HttpMessageHandler> handlerMock = Mocks.CreateHttpMessageHandlerMock(
                @"<html>HTML without some internal achor tags  <a href='link1'>Link 1<a> <a href='mailto:johndoe@contoso.com'>Send mail<a></html>",
                HttpStatusCode.OK,
                "text/html");

            var httpClient = new HttpClient(handlerMock.Object);
            var site = "http://www.contoso.com";

            var crawler = new SingleThreadedWebSiteCrawler(
                CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                _htmlParser,
                httpClient);

            var expectedResults = new string[]
            {
                "http://www.contoso.com/link1",
            };

            //Act
            var crawlerResults = await crawler.Run(site, 5);

            //Assert
            crawlerResults.Should().HaveCount(1);
            expectedResults.ToList().ForEach(link =>
            {
                crawlerResults
                .Any(r => r.AbsoluteLink.ToString().Trim('/') == link)
                .Should()
                .BeTrue("The link {0} was not found in the crawl results", link);
            });
        }

        [TestMethod]
        public async Task When_Page_Has_No_Hyperlinks_Then_Crawler_Must_Return_Zero_Hyperlinks()
        {
            //Arrange
            Mock<HttpMessageHandler> handlerMock = Mocks.CreateHttpMessageHandlerMock(
                @"<html>HTML without any anchor tags</html>",
                HttpStatusCode.OK,
                "text/html");

            var httpClient = new HttpClient(handlerMock.Object);
            var site = "http://www.sitewithnolinks.com";

            var crawler = new SingleThreadedWebSiteCrawler(
                CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                _htmlParser,
                httpClient);

            //Act
            var crawlerResults = await crawler.Run(site, 5);

            //Assert
            crawlerResults.Should().BeEmpty();
        }

        [TestMethod]
        public async Task When_Page_Has_TelephoneNumberLinks_Then_Crawler_Must_Ignore_MailTo()
        {
            //Arrange
            Mock<HttpMessageHandler> handlerMock = Mocks.CreateHttpMessageHandlerMock(
                @"<html>HTML without some internal achor tags  <a href='link1'>Link 1<a> <a href='tel:12345678'>Call us<a></html>",
                HttpStatusCode.OK,
                "text/html");

            var httpClient = new HttpClient(handlerMock.Object);
            var site = "http://www.contoso.com";

            var crawler = new SingleThreadedWebSiteCrawler(
                CreateOutputWindowLogger<SingleThreadedWebSiteCrawler>(),
                _htmlParser,
                httpClient);

            var expectedResults = new string[]
            {
                "http://www.contoso.com/link1",
            };

            //Act
            var crawlerResults = await crawler.Run(site, 5);

            //Assert
            crawlerResults.Should().HaveCount(1);
            expectedResults.ToList().ForEach(link =>
            {
                crawlerResults
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