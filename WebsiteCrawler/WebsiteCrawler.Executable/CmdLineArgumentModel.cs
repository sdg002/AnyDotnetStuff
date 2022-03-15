using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteCrawler.Executable
{
    public class CmdLineArgumentModel
    {
        [Option("maxsites", Required = true, HelpText = "An upper limit on the number of sites to search. Example: 30")]
        public int MaxSites { get; set; }

        [Option("url", Required = true, HelpText = "The URL of the site to search")]
        public string Url { get; set; }
    }
}