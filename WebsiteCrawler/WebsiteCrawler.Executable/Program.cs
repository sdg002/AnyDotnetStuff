using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebsiteCrawler.Infrastructure.interfaces;
using WebsiteCrawler.Service;

namespace WebsiteCrawler.Executable
{
    internal static class Program
    {
        private static IServiceProvider _provider;

        private static ServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IWebSiteCrawler, SingleThreadedWebSiteCrawler>();
            serviceCollection.AddTransient<IHtmlParser, HtmlAgilityParser>();
            serviceCollection.AddTransient<HttpClient>();
            serviceCollection.AddLogging(builder => builder.AddConsole());  //TODO Change to nlog
            serviceCollection.AddTransient<ICrawlerResultsFormatter, CsvResultsFormatter>();
            return serviceCollection.BuildServiceProvider();
        }

        private static void DisplayCommandLineArguments(string[] args)
        {
            Console.WriteLine("Arguments:");
            Console.WriteLine("--------------");
            for (int index = 0; index < args.Length; index++)
            {
                Console.WriteLine($"{index}\t\t\t{args[index]}");
            }
            Console.WriteLine("--------------");
        }

        private static async Task Main(string[] args)
        {
            try
            {
                DisplayCommandLineArguments(args);
                _provider = ConfigureServices();

                await Parser.Default.ParseArguments<CmdLineArgumentModel>(args).WithParsedAsync(Run);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task Run(CmdLineArgumentModel arg)
        {
            var crawler = _provider.GetService<IWebSiteCrawler>();
            var results = await crawler.Run(arg.Url, arg.MaxSites);
            var formatter = _provider.GetService<ICrawlerResultsFormatter>();
            var stdOut = Console.OpenStandardOutput();
            stdOut.Flush();
            formatter.WriteResults(stdOut, results);
        }
    }
}

/*
 *
 Webrawler.exe -url http://somesite.com	-maxpages 10
 */

//TODO finish Readme

/*
             var formatter = new WebsiteCrawler.Service.CsvResultsFormatter();
            formatter.WriteResults(Console.OpenStandardOutput(), new System.Collections.Generic.List<Infrastructure.entity.SearchResult>());

 */