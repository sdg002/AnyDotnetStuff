using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteCrawler.Service.entity
{
    /// <summary>
    /// Defines a single URL which will be crawled for further discover
    /// </summary>
    internal class SearchJob
    {
        /// <summary>
        /// Gets or sets the Level at which the URL was found. E.g. 0 if the link belongs on the starting link
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified URL which will be searched
        /// </summary>
        public string Url { get; set; }

        public override string ToString()
        {
            return $"Level={this.Level} Url={this.Url}";
        }
    }
}