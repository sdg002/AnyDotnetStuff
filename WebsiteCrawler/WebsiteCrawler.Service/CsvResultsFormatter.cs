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
        //public void WriteResults(Stream output)
        //{
        //    var swriter = new StreamWriter(output);
        //    swriter.AutoFlush = true;
        //    for (int i = 0; i < 10; i++)
        //    {
        //        swriter.WriteLine($"Current date time is {DateTime.Now}");
        //    }
        //}

        public void WriteResults(Stream output, List<SearchResult> searchResults)
        {
            var swriter = new StreamWriter(output);
            swriter.AutoFlush = true;
            throw new NotImplementedException();
        }
    }
}