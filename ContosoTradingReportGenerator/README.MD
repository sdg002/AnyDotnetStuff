
# Block Diagram


![blockdiagram!](docs/images/block_diagram.png "blockdiagram")


---


# Possible solution approaches


## 1-Naive solution - A single command line executable written in procedural style
- A single .NET Core executable written in a procedural style
- The EXE polls the trading system at desired intervals


## 2-A more robust solution  - A componentized ASP.NET worker process deployable to Azure

- ASP.NET Core worker process as the outer shell
- Split into testable components and abstractions
- Poll the trading system at desired intervals


## 3-Non coded approaches, like Azure Datafactory/Synapse pipelines
- Sql Server Integration services 
- Azure Data factory / Azure Synapse pipelines

---

# Solution overview

The solution has been implemented using a *componentized ASP.NET Worker process* approach


## ASP.NET Core Worker class

The core scheduler is implemented as an inheritance from Microsoft's `BackgroundService` class. 
The virtual method `ExecuteAsync` is designed to keep polling using a configured delay.
```
public class RecurringAggregatorJob : Microsoft.Extensions.Hosting.BackgroundService
{
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteEarliestReportGenerationAsync();
                _logger.LogInformation($"Waiting for execution, {this.DelayBetweenConsecutiveAttempts} seconds");
                await Task.Delay(this.DelayBetweenConsecutiveAttempts * 1000, stoppingToken);
            }
        }
}
```
- The class `RecurringAggregatorJob` is desinged to poll the trading system at configured intervals
- The configuration is read from `appSettings.json`
- Dependencies like logger, trading connector and reports repository are wired through Dependency Injection

Structure of appSettings.json is as below:
```
{
  "ReportsFolder": "c:\\truetemp\\pitl_challenge\\",
  "MaximumSecondsDelayBetweenConsecutiveReporExtrations": 100,
  "SecondsDelayBetweenConsecutiveAttempts": 30

```
The settings are serialized during DI initialization:
```
                    services.AddSingleton<TradingAggregatorJobConfig>(sp =>
                    {
                        var configuration = sp.GetService<IConfiguration>();
                        var jobConfig = new TradingAggregatorJobConfig();
                        configuration.Bind(jobConfig);
                        return jobConfig;
                    });

```

## Trading systems connector
The trading system is abstracted through the interface `ITradingService`. There are 2 implementations provided
- **FakeTradingConnector**-Generates a random sequence of trades and used for testing the system
- **LiveTradingConnector**-Connects to the live system.

![tradingcomponent!](docs/images/trading_component.png "tradingcomponent")


## File based reports repository
The reports repository is abstracted through the interface `IReportsRepo` and the class `AggregatedReportBase` .
For this exercise a simple file based repository has been implemented in the class `NetworkFileShareReportsRepository`

![reportsrepo!](docs/images/reportsrepo_component.png "reportsrepo")



# How to produce CSV content?
The popular 3rd party nuget package `CsvHelper` has been used. Refer class `SimpleCsvGenerator` which hides the implementation details

![csv!](docs/images/csvgenerator_component.png "csv")



# Logging
For this solution, the popular logging library `log4net` has been used. This was primarily for simplicity. 

```
                    /*
                     * Replace with a centralized logging system such as Application Insights (if on Azure)
                     * https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger#console-application
                     */
                    services.AddLogging(builder => builder.AddLog4Net());

```
The loggers are injected via dependency injection. Therefore replacing with a centralized logging like **Application Insights** should be fairly easy.

Sample DI code for Application Insights below:
https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger#console-application


# How to ensure that no scheduled report generation has been missed?
Consider the case of a temporary outage. This could be a case of a container instance failing or an Azure web job instance not responding. To handle such situations, the following logic has been employed in the class `RecurringAggregatorJob.cs`
1. Wake up
1. Query the reports repository for all reports that have been generated so far
1. Compute a table of desired report run timings
1. Compare the desired run timings with the reports that have been already generated
1. Find missing reports
1. Generate the missing reports
1. Wait for a configured delay
3. Repeat the above

So essentially, the report generation plays a catch up. To make this successful, it is important that the the report polling should run at a frequency higher than the report extraction frequency.
Example:
If we expect a report to be generated every X minutes, then polling delay should be X/2 minutes (as an example).

## Caveats
The above approach works robustly if the report generation task is not a very long one.


# Understanding the accompanying code
## Running the EXE
- The main executable is implemented in the project **Contoso.TradingAggregator.Host** project
- The configuration file **appsettings.json** has the path to the folder used as a reports repository
- The configuration file also controls the report generation interval and polling interval

## Running the unit tests

Implemented in the project **Contoso.TradingAggregator.UnitTests**
![csv!](docs/images/unittests_green.png "csv")

## List of projects

- **Contoso.TradingAggregator.Domain** - Core domain classes and interfaces
- **Contoso.TradingAggregator.TradingConnector** - Implements a random trade generator for demonstration and a live connector using Contoso's proprietary assembly
- **Contoso.TradingAggregator.TradingReportsRepository** - Implements a simple file system based reports repsitory
- **Contoso.TradingAggregator.Jobs** - The ASP.NET Core inheritance from `BackgroundService` class and CSV generation component

# Deploying to Azure
The existing solution can be very easily deployed to Azure as a Web job.
Please refer my project on Github for Azure CLI/PowerShell sample code.
https://github.com/sdg002/AnyDotnetStuff/tree/master/DemoWebAppWithCiCd
- Create an Azure resource group
- Create an Azure App Service plan
- Create a Azure App
- Deploy the DLLs 

# How can we change the implementation of the reports repository?
![reportsrepo!](docs/images/reportsrepo_component.png "reportsrepo")

Consider the hypothetical example of using an Azure Blob storage as a reports repository.
- Implement the interface **IReportsRepo**  in a new class called **AzureBlogReportsRepo** (as an example)
- Implement the abstract class **AggregatedReportBase** in a new class called **AzureBlogReportBase**. 
- Change the initialization in dependency injection 
- Provide the neccessary app settings.

```
                    services.AddSingleton<IReportsRepo>(sp =>
                    {
                        return new NetworkFileShareReportsRepository(
                            sp.GetService<TradingAggregatorJobConfig>().ReportsFolder,
                            sp.GetService<ILogger<NetworkFileShareReportsRepository>>()
                            );
                    });

```

# Alternative Scheduling approaches
Not in the scope of this solution. We could think of moving away from a polling based approach to a system which is trigerred by an external scheduling engine.
Such a system offers the advantage of complex schedules and load balancing.
- Hangfire
- Quartz.net
- Azure Deferred messages


