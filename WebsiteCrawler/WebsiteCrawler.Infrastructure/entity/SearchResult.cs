using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteCrawler.Infrastructure.entity
{
    public class SearchResult
    {
        public SearchResult()
        {
        }

        public SearchResult(string parentUrl, string childUrl, int level)
        {
            this.ChildPageUrl = childUrl;
            this.ParentPageUrl = parentUrl;
            this.Level = level;
        }

        public string ChildPageUrl { get; set; }
        public int Level { get; set; }
        public string ParentPageUrl { get; set; }

        public override string ToString()
        {
            return $"Parent={this.ParentPageUrl}    Child={this.ChildPageUrl}   Level={this.Level}";
        }
    }
}