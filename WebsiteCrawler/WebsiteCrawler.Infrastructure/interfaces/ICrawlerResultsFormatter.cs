using System.Collections.Generic;
using System.IO;
using WebsiteCrawler.Infrastructure.entity;

namespace WebsiteCrawler.Infrastructure.interfaces
{
    /// <summary>
    /// Converts the results of the crawler to the desired output. Example: CSV, JSON
    /// </summary>
    public interface ICrawlerResultsFormatter
    {
        void WriteResults(Stream output, List<SearchResult> searchResults);
    }
}