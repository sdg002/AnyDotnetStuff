using System;

namespace WebsiteCrawler.Service.entity
{
    /// <summary>
    /// Defines a single URL which will be crawled for further discover
    /// </summary>
    internal class SearchJob
    {
        public SearchJob(string url, int level)
        {
            this.Url = url;
            this.Level = level;
            this.Uri = new Uri(url);
        }

        /// <summary>
        /// Gets or sets the Level at which the URL was found. E.g. 0 if the link belongs on the starting link
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// Gets the Uri model from the raw text based Url
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Gets or sets the fully qualified URL which will be searched
        /// </summary>
        public string Url { get; private set; }

        public override string ToString()
        {
            return $"Level={this.Level} Url={this.Url}";
        }
    }
}