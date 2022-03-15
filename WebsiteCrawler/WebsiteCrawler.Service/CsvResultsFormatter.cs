using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebsiteCrawler.Infrastructure.entity;
using WebsiteCrawler.Infrastructure.interfaces;

namespace WebsiteCrawler.Service
{
    public class CsvResultsFormatter : ICrawlerResultsFormatter
    {
        public void WriteResults(Stream output, List<SearchResult> searchResults)
        {
            var swriter = new StreamWriter(output);
            swriter.AutoFlush = true;
            swriter.WriteLine($"Url");
            foreach (var result in searchResults)
            {
                swriter.WriteLine($"{result.AbsoluteLink}");
            }
        }
    }
}