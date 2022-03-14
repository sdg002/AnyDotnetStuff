using System;
using System.Collections.Generic;
using WebsiteCrawler.Infrastructure.interfaces;

namespace WebsiteCrawler.Service
{
    public class HtmlAgilityParser : IHtmlParser
    {
        public List<Uri> GetLinks(string html)
        {
            throw new NotImplementedException();
        }
    }
}