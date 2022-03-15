using System.Collections.Generic;

namespace WebsiteCrawler.Infrastructure.interfaces
{
    /// <summary>
    /// Parses a HTML document
    /// </summary>
    public interface IHtmlParser
    {
        /// <summary>
        /// Returns all hyper link URLs from the given HTML document
        /// </summary>
        /// <param name="html">The HTML content to scan</param>
        /// <returns></returns>
        List<string> GetLinks(string htmlContent);
    }
}