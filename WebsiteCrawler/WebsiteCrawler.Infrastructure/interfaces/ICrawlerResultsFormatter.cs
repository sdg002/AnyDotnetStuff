using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebsiteCrawler.Infrastructure.entity;

namespace WebsiteCrawler.Infrastructure.interfaces
{
    public interface ICrawlerResultsFormatter
    {
        //TODO design the interface results formatter
        void WriteResults(Stream output, List<SearchResult> searchResults);
    }
}