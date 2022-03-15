using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebsiteCrawler.UnitTests
{
    [TestClass]
    public class WebsiteCrawlerUnitTests
    {
        [TestMethod]
        public void When_HttpError_Then_Retry_ShouldBe_Attempted()
        {
            //TODO retry unit test
        }

        [TestMethod]
        public void When_Levels_Exceeded_Limit_Then_Crawler_Must_Stop()
        {
            //TODO levels
        }

        [TestMethod]
        public void When_Page_Has_BookMarks_Then_Crawler_Must_Ignore_BookMarks()
        {
            //TODO boomarks
        }

        [TestMethod]
        public void When_Page_Has_EmptyHyperlinks_Then_Crawler_Must_Ignore_EmptyHyperlinks()
        {
            //TODO empty hyperlinks
        }

        [TestMethod]
        public void When_Page_Has_No_Links_Then_Zero_Links_ShouldBe_Returned()
        {
            //TODO empty page unit test
        }
    }
}