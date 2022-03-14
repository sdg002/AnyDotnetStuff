using System;

namespace WebsiteCrawler.Executable
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var formatter = new WebsiteCrawler.Service.CsvResultsFormatter();
            formatter.WriteResults(Console.OpenStandardOutput(), new System.Collections.Generic.List<Infrastructure.entity.SearchResult>());

            //TODO Create DI and add dependencies

            //TODO Write class to capture command line arguments
            /*
             *
             Webrawler.exe -url http://somesite.com	-levels 10
             */

            //TODO Parse command line arguments
        }
    }
}

//TODO finish Readme