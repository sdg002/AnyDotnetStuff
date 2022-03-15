using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebsiteCrawler.Infrastructure.extensions;

namespace WebsiteCrawler.UnitTests.ExtensionTests
{
    [TestClass]
    public class UrlExtensionsTests
    {
        [TestMethod]
        [DataRow("http://www.bbc.co.uk/sports/football", "//premier.html", "http://www.bbc.co.uk/sports/football/premier.html")]
        [DataRow("http://www.bbc.co.uk/sports", "/football/premier.html", "http://www.bbc.co.uk/sports/football/premier.html")]
        [DataRow("http://www.bbc.co.uk/", "weather.html", "http://www.bbc.co.uk/weather.html")]
        [DataRow("http://www.bbc.co.uk", "weather.html", "http://www.bbc.co.uk/weather.html")]
        [DataRow("http://www.bbc.co.uk", "/weather.html", "http://www.bbc.co.uk/weather.html")]
        public void When_Valid_Url_Then_CombineMustProduce_AbsoluteUrl(string parentUrl, string childLink, string expectedNewAbsoluteUrl)
        {
            //Arrange

            //Act
            var actualNewAbsoluteUrl = parentUrl.Combine(childLink);

            //Assert
            actualNewAbsoluteUrl.ToString().Should().Be(expectedNewAbsoluteUrl);
        }

        [TestMethod]
        [DataRow("https://rxglobal.com/foo/press-kit?field_press_kit_type_target_id=All&amp;page=1", "https://rxglobal.com/foo/")]
        [DataRow("https://rxglobal.com/press-kit?field_press_kit_type_target_id=All&amp;page=1", "https://rxglobal.com/")]
        [DataRow("http://foo.com", "http://foo.com")]
        [DataRow("http://foo.com/baz.html", "http://foo.com/")]
        [DataRow("http://foo.com/baz", "http://foo.com/")]
        [DataRow("http://foo.com/baz/", "http://foo.com/")]
        [DataRow("http://foo.com/baz/baz1/", "http://foo.com/baz/")]
        [DataRow("http://foo.com/bar/baz.html", "http://foo.com/bar/")]
        public void When_Valid_Url_Then_GetParent_MustReturn_Parent(string inputUrl, string expectedUrl)
        {
            var actualParent = (new Uri(inputUrl)).GetParentUriString();

            //Assert
            actualParent.Should().Be(expectedUrl);
        }
    }
}