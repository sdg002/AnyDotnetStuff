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
            serviceCollection.AddLogging(builder => builder.AddLog4Net("log4net.config"));

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
            Console.WriteLine($"Found {results.Count} sites in the Url:'{arg.Url}', after searching a maximum of {arg.MaxSites} sites");
        }
    }
}