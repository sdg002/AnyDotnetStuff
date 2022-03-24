using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteCrawler.Executable
{
    public class CmdLineArgumentModel
    {
        [Option("maxsites", Required = true, HelpText = "An upper limit on the number of sub-sites to search.Example:30. This number is used as a stopping criteria which is very useful for deeply nested web sites.")]
        public int MaxSites { get; set; }

        [Option("url", Required = true, HelpText = "The URL of the site to search. Example: https://www.cnn.com")]
        public string Url { get; set; }
    }
}