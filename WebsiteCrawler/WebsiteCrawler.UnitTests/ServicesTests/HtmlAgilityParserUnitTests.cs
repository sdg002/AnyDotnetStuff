using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebsiteCrawler.Service;

namespace WebsiteCrawler.UnitTests
{
    [TestClass]
    public class HtmlAgilityParserUnitTests
    {
        [TestMethod]
        public void When_Empty_Html_Then_Zero_Links_ShouldBe_Returned()
        {
            //Arrange
            var htmlParser = new HtmlAgilityParser();

            //Act
            var links = htmlParser.GetLinks("");

            //Assert
            links.Should().BeEmpty();
        }

        [TestMethod]
        public void When_Html_AnchorTags_Then_Links_ShouldBe_Returned()
        {
            //Arrange
            var htmlParser = new HtmlAgilityParser();

            //Act
            var links = htmlParser.GetLinks("<html> <body>Body of the HTML has no anchor elements</body> </html>");

            //Assert
            links.Should().BeEmpty();
        }

        [TestMethod]
        public void When_Html_HasNoAnchorTags_Then_Zero_Links_ShouldBe_Returned()
        {
            //Arrange
            var htmlParser = new HtmlAgilityParser();

            //Act
            var links = htmlParser.GetLinks("<html> <body>Body of the HTML one anchor element. <a href='www.somesite.com'>Click here</a>  </body> </html>");

            //Assert
            links.Should().HaveCount(1);
            links[0].Should().Be("www.somesite.com");
        }
    }
}