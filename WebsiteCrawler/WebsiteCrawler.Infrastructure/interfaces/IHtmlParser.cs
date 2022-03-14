using System;
using System.Collections.Generic;
using System.Text;

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
        /// <param name="html"></param>
        /// <returns></returns>
        List<Uri> GetLinks(string html);

        //TODO Change from Uri to string
    }
}