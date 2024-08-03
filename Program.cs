using AzureFunctionForEmailAndPDF;
using CfrpAzureFunction;
using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        var hostingEnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        config.AddAzureAppConfiguration(options =>
            options
                .Connect(Environment.GetEnvironmentVariable("MyAzureAppConfig"))//this is set in your local environment settings and in the function. It is the connection string to AppConfig in Azure
                //Configure a global refresh variable for this application
                .ConfigureRefresh(refresh =>
                {
                    refresh.Register(Constants.ConfigAppServiceName + ":SETTINGS:SENTINEL",
                            refreshAll: true)
                        .SetCacheExpiration(new TimeSpan(0, 5, 0));
                })
                // Load global setting
                .Select("Global.*", LabelFilter.Null)
                // Load configuration values with no label.  AKA prod
                .Select(Constants.ConfigAppServiceName + ".*", LabelFilter.Null)
                // Override with any configuration values specific to current hosting env
                // Stack the changes per https://docs.microsoft.com/en-us/azure/azure-app-configuration/howto-best-practices
                // How to set env variables https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-3.1#determining-the-environment-at-runtime
                .Select("Global.*", hostingEnvironmentName)
                .Select(Constants.ConfigAppServiceName + ".*",
                    hostingEnvironmentName));
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
        services.AddScoped<IHtmlToPdfHelper, HtmlToPdfHelper>();
    })
    .Build();

host.Run();
