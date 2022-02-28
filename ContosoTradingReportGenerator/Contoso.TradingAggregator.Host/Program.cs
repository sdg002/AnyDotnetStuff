using Contoso.TradingAggregator.Domain.infrastructure;
using Contoso.TradingAggregator.Domain.interfaces;
using Contoso.TradingAggregator.Domain.services;
using Contoso.TradingAggregator.Jobs;
using Contoso.TradingConnector;
using Contoso.TradingReportsRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Contoso.TradingAggregator.Host
{
    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    /*
                     * Replace with a centralized logging system such as Application Insights (if on Azure)
                     * https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger#console-application
                     */
                    services.AddLogging(builder => builder.AddLog4Net());

                    services.AddHostedService<RecurringAggregatorJob>();
                    services.AddTransient<ICsvGenerator, SimpleCsvGenerator>();
                    services.AddSingleton<TradingAggregatorJobConfig>(sp =>
                    {
                        var configuration = sp.GetService<IConfiguration>();
                        var jobConfig = new TradingAggregatorJobConfig();
                        configuration.Bind(jobConfig);
                        return jobConfig;
                    });

                    services.AddSingleton<IReportsRepo>(sp =>
                    {
                        return new NetworkFileShareReportsRepository(
                            sp.GetService<TradingAggregatorJobConfig>().ReportsFolder,
                            sp.GetService<ILogger<NetworkFileShareReportsRepository>>()
                            );
                    });

                    services.AddSingleton<ITradingService, FakeTradingConnector>();
                    services.AddSingleton<IClock, Clock>();
                });

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}