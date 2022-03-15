using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using WebsiteCrawler.Infrastructure.entity;

namespace WebsiteCrawler.UnitTests
{
    [TestClass]
    public class CsvResultsFormatterTests
    {
        [TestMethod]
        public void When_SearchResults_AreFormatted_Then_CSV_MustBe_Produced_And_ShouldContain_The_Urls()
        {
            var formatter = new Service.CsvResultsFormatter();
            var searchResults = new List<SearchResult>();
            searchResults.Add(new SearchResult
            {
                AbsoluteLink = new Uri("http://www.contoso.com/")
            });
            searchResults.Add(new SearchResult
            {
                AbsoluteLink = new Uri("http://www.bbc.com/")
            });

            string rawText = null;
            using (var memory = new MemoryStream())
            {
                formatter.WriteResults(memory, searchResults);
                rawText = System.Text.Encoding.UTF8.GetString(memory.ToArray());
            }
            var lines = rawText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            lines.Should().HaveCount(3);
            lines[0].Should().Be("Url");
            lines[1].Should().Be("http://www.contoso.com/");
            lines[2].Should().Be("http://www.bbc.com/");
        }
    }
}